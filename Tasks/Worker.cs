using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace YCabz.Tasks
{
    /// <summary>
    /// Thread를 새로 만들어 작업 실행
    /// 'DoWork'이벤트에 처리방법을 알려주고
    /// 처리할 일들은 'Add'로 추가해준다
    /// </summary>
    /// <typeparam name="T">Job Type</typeparam>
    public class Worker<T> : IDisposable
    {
        public delegate void DoWorkEventHandler(T job);
        public delegate void WorkStoppedEventHander();

        /// <summary>
        /// Main Job Event
        /// 'AddJob'으로 Job을 추가하면 
        /// 'DoWork'으로 실행
        /// </summary>
        /// 
        public event DoWorkEventHandler DoWork;

        /// <summary>
        /// Main Thread 정지 후 이벤트
        /// </summary>
        public event WorkStoppedEventHander WorkerStopped;

        /// <summary>
        /// DelayTime JobEnd To JobStart
        /// </summary>
        public int DelayTimeMilliSecond { get; set; } = 50;

        /// <summary>
        /// Worker Thread State
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (_IsRunningLockOjb)
                {
                    return _IsRunning;
                }
            }
            private set
            {
                lock (_IsRunningLockOjb)
                {
                    _IsRunning = value; ;
                }
            }
        }
        private bool _IsRunning;
        private readonly object _IsRunningLockOjb = new object();

        /// <summary>
        /// 일시 정시 상태
        /// </summary>
        public bool IsPausing { get; private set; }

        /// <summary>
        /// 현재 큐에 대기중인 Job 개수
        /// </summary>
        public int CurrentJobCount { get => jobQueue.Count; }


        private readonly ConcurrentQueue<T> jobQueue = new ConcurrentQueue<T>();
        private CancellationTokenSource tokenSource;
        private ManualResetEvent resetEvent;
        private Task myTask;
        private bool isDisposeCalled;


        /// <summary>
        /// Worker Thread 실행
        /// </summary>
        public async void StartAsync()
        {
            if (IsRunning == false && isDisposeCalled == false)
            {
                IsRunning = true;

                resetEvent = new ManualResetEvent(true);
                tokenSource = new CancellationTokenSource();

                // start work thread
                try
                {
                    myTask = Task.Run(WatchAndWork, tokenSource.Token);
                    await myTask.ConfigureAwait(false);

                    if (isDisposeCalled == false)
                    {
                        // job stop event
                        WorkerStopped?.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    resetEvent.Dispose();
                    tokenSource.Dispose();
                    myTask.Dispose();

                    IsRunning = false;

                    while (jobQueue.IsEmpty == false)
                    {
                        jobQueue.TryDequeue(out var dummy);
                    }
                }
            }
        }

        /// <summary>
        /// Worker Thread에 Job을 추가한다
        /// </summary>
        /// <param name="job"></param>
        public void Add(T job)
        {
            if (IsRunning == true)
            {
                jobQueue.Enqueue(job);
            }

        }

        /// <summary>
        /// Worker Thead 정지 요청
        /// </summary>
        public void Stop()
        {
            tokenSource?.Cancel();
        }

        /// <summary>
        /// Work Thread 일시 중지
        /// </summary>
        public void Pause()
        {
            if (IsRunning)
            {
                resetEvent.Reset();
                IsPausing = true;
            }
        }

        /// <summary>
        /// 일시 정지된 Work Thread를 계속 실행
        /// </summary>
        public void Continue()
        {
            if (IsRunning)
            {
                resetEvent.Set();
                IsPausing = false;
            }
        }

        public void Dispose()
        {
            isDisposeCalled = true;

            resetEvent?.Set();
            tokenSource?.Cancel();
        }


        private void WatchAndWork()
        {
            while (tokenSource.IsCancellationRequested == false)
            {
                if (jobQueue.TryDequeue(out T job))
                {
                    DoWork?.Invoke(job);
                }

                Task.Delay(DelayTimeMilliSecond).Wait();

                // Suspend시, Thread 대기
                resetEvent.WaitOne();
            }
        }
    }
}

