using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCabz.Tasks
{
    public delegate void BlockStoppedEventHandler();
    /// <summary>
    /// 상속 후 'RunJob' 구현 필수
    /// 'Run' 후 'Add'로 Job을 추가해서 실행
    /// 'NextBlock'이 연결되어있으면 
    /// 끝난 Job을 다음 Block으로 자동으로 넘겨준다
    /// </summary>
    /// <remarks>JobCount == 0, WokrThread 대기</remarks>
    /// <remarks>JobCount > 0, WokrThread 진행</remarks>
    public abstract class Block<T> : IDisposable
    {
        /// <summary>
        /// Block 정지 이벤트
        /// </summary>
        public event BlockStoppedEventHandler BlockStopped;

        /// <summary>
        /// 이전 Block
        /// </summary>
        public Block<T> PreviousBlock { get; set; }

        /// <summary>
        /// 다음 Block
        /// </summary>
        public Block<T> NextBlock { get; set; }

        /// <summary>
        /// 현재 Queue에 대기중인 Job 수
        /// </summary>
        public int JobCount
        {
            get
            {
                lock (queueLock)
                {
                    return queue.Count;
                }
            }
        }

        /// <summary>
        /// Queue가 비어있는지 확인
        /// </summary>
        public bool IsEmptyJob
        {
            get
            {
                lock (queueLock)
                {
                    if (queue.Count == 0)
                    {
                        return true;
                    }
                    return false;
                }
            }
        }

        /// <summary>
        /// Block 실행 확인
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (runningLock)
                {
                    return _IsRunning;
                }
            }
            set
            {
                lock (runningLock)
                {
                    _IsRunning = value;
                    // 상태 변경 시, 기존 Job 제거
                    ClearJob();
                }
            }
        }
        private bool _IsRunning;

        private readonly Queue<T> queue = new Queue<T>();
        private readonly ManualResetEvent jobResetEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent queueResetEvent = new ManualResetEvent(false);
        private readonly object queueLock = new object();
        private readonly object runningLock = new object();
        private CancellationTokenSource tokenSource;
        private Task myTask;
        private bool isDisposed;


        /// <summary>
        /// Block 실행
        /// </summary>
        public async void Run()
        {
            if (IsRunning == false && isDisposed == false)
            {
                IsRunning = true;

                // Job Thread 대기해제
                jobResetEvent.Set();

                // Queue는 비어있으므로 대기
                queueResetEvent.Reset();

                // Thread 생성
                tokenSource = new CancellationTokenSource();
                myTask = Task.Run(WatchAndRunJob, tokenSource.Token);
                await myTask.ConfigureAwait(false);

                // Thread 종료
                tokenSource.Dispose();
                myTask.Dispose();

                IsRunning = false;

                BlockStopped?.Invoke();
            }
        }

        /// <summary>
        /// Block 실행 중단 (재실행 가능)
        /// </summary>
        public void Stop()
        {
            if (IsRunning == true)
            {
                tokenSource.Cancel();
                queueResetEvent.Set();
                jobResetEvent.Set();
            }
        }

        /// <summary>
        /// Work Thread에 Job 추가
        /// </summary>
        public void Add(T job)
        {
            if (IsRunning == true)
            {
                Enqueue(job);
            }
        }

        /// <summary>
        /// Work Thread 일시 중지
        /// (Add는 계속 실행)
        /// </summary>
        public void Suspend()
        {
            if (IsRunning == true)
            {
                jobResetEvent.Reset();
            }
        }

        /// <summary>
        /// Work Thread 재개
        /// </summary>
        public void Resume()
        {
            jobResetEvent.Set();
        }

        /// <summary>
        /// Queue에 저장된 Job 제거
        /// </summary>
        public void ClearJob()
        {
            lock (queueLock)
            {
                queueResetEvent.Reset();

                while (queue.Count > 0)
                {
                    queue.Dequeue();
                }
            }
        }

        /// <summary>
        /// Block 실행 중지 (재실행 안됨) 후,
        /// 'PreviousBlock/NextBlock' Block과의 연결을 해제한다
        /// </summary>
        public void Dispose()
        {
            isDisposed = true;
            Stop();

            queueResetEvent.Dispose();
            jobResetEvent.Dispose();

            // 앞 Block과 연결 해제
            if (PreviousBlock != null)
            {
                PreviousBlock.NextBlock = null;
            }

            // 뒤 Block과 연결 해제
            if (NextBlock != null)
            {
                NextBlock.PreviousBlock = null;
            }
        }


        /// <summary>
        /// Dequeue후 Queue가 비었으면, Thread 대기
        /// </summary>
        private bool TryDequeue(out T job)
        {
            lock (queueLock)
            {
                job = default;

                if (queue.Count > 0)
                {
                    job = queue.Dequeue();

                    // Job이 없으면 Thread 대기
                    if (IsEmptyJob == true)
                    {
                        queueResetEvent.Reset();
                    }
                    return true;
                }
                return false;
            }
        }


        private void Enqueue(T job)
        {
            if (IsRunning == false)
            {
                return;
            }

            lock (queueLock)
            {
                // Enqueue시 Thread 대기 해제
                queueResetEvent.Set();
                queue.Enqueue(job);
            }
        }

        /// <summary>
        /// Queue를 감시하고 Job이 있으면 진행
        /// </summary>
        private void WatchAndRunJob()
        {
            while (tokenSource.IsCancellationRequested == false)
            {

                try
                {
                    if (TryDequeue(out var job) == true)
                    {  // Job 실행
                        RunJob(job);

                        // 연결된 Block이 있으면 Job을 넘겨준다
                        if (NextBlock != null && IsRunning == true)
                        {
                            NextBlock.Add(job);
                        }
                    }
                    // Queue가 비었으면 Thread 대기
                    queueResetEvent.WaitOne();

                    // 보류 요청이 있으면 Thread 대기
                    jobResetEvent.WaitOne();
                }
                catch (ObjectDisposedException)
                {
                    // When ResetEventDisposed
                    break;
                }
            }
        }


        /// <summary>
        /// 실제로 실행될 함수
        /// </summary>
        /// <param name="Job"></param>
        protected abstract void RunJob(T Job);


        /// <summary>
        /// 두 Block을 연결한다
        /// </summary>
        /// <param name="runFlag">연결후 실행 Block</param>
        public static void Connect(Block<T> previous, Block<T> next, BlockActionFlags runFlag)
        {
            if (previous == null || next == null)
            {
                throw new NullReferenceException();
            }

            // Previous Block이 기존 연결이 있으면
            if (previous.NextBlock != null)
            {
                // 해당 연결 제거
                previous.NextBlock.PreviousBlock = null;
            }

            // Nest Block이 기존 연결이 있으면
            if (next.PreviousBlock != null)
            {
                // 해당 연결 제거
                next.PreviousBlock.NextBlock = null;
            }

            previous.NextBlock = next;
            next.PreviousBlock = previous;

            switch (runFlag)
            {
                case BlockActionFlags.None:
                    break;
                case BlockActionFlags.Previous:
                    previous.Run();
                    break;
                case BlockActionFlags.Next:
                    next.Run();
                    break;
                case BlockActionFlags.Both:
                    previous.Run();
                    next.Run();
                    break;
                default:
                    throw new InvalidCastException($"{runFlag.ToString()}");
            }
        }

        /// <summary>
        /// 두 Block을 연결해제 한다
        /// </summary>
        /// <param name="stopFlag">연결 해제 후 Block</param>
        public static void Disconnect(Block<T> previous, Block<T> next, BlockActionFlags stopFlag)
        {
            if (previous == null || next == null)
            {
                throw new NullReferenceException();
            }

            // Check Connection
            if (previous.NextBlock != next || next.PreviousBlock != previous)
            {
                throw new NotImplementedException("Not Connected");
            }

            previous.NextBlock = null;
            next.PreviousBlock = null;

            switch (stopFlag)
            {
                case BlockActionFlags.None:
                    break;
                case BlockActionFlags.Previous:
                    previous.Stop();
                    break;
                case BlockActionFlags.Next:
                    next.Stop();
                    break;
                case BlockActionFlags.Both:
                    previous.Stop();
                    next.Stop();
                    break;
                default:
                    throw new InvalidCastException($"{stopFlag.ToString()}");
            }
        }
    }
}
