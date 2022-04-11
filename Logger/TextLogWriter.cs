using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Logger
{
    /// <summary>
    /// TopDirectory\yyyy-MM-dd\logFileName_yyyy-MM-dd.xxx 로그 쓰기
    /// </summary>
    /// <remark>GetCurrentDirectoryPathInvoker를 사용하여 저장폴더 경로를 새로 정의 가능</remark>
    /// <remark>GetCurrentFileNameInvoker 사용하여 저장파일 이름을 새로 정의 가능</remark>
    /// 
    public class TextLogWriter
    {
        /// <summary>
        /// 동일 파일에 여러 쓰레드 접근 금지
        /// </summary>
        private static Dictionary<string, object> logLockObject = new Dictionary<string, object>();

        /// <summary>
        /// 로그 폴더 경로 재정의 Invoker
        /// </summary>
        /// <remarks>기본값: TopDirectory\yyyy-MM-dd\</remarks>
        public Func<string> GetCurrentDirectoryPathInvoker { private get; set; }

        /// <summary>
        /// 로그 파일 이름 재정의 Invoker
        /// </summary>
        /// <remarks>기본값: logFileName_yyyy-MM-dd</remarks>
        public Func<string> GetCurrentFileNameInvoker { private get; set; }

        /// <summary>
        /// 로그에 시간을 자동 기록할 때, 시간 형식
        /// </summary>
        /// <remarks>isWriteTime이 false이면 사용안함</remarks>
        public string WrittenTimeFormat { get; set; } = "yyyy-MM-dd HH:mm:ss.fff";

        /// <summary>
        /// 현재 로그 저장폴더 경로
        /// </summary>
        public string CurrentDirectory { get => GetCurrentDirectoryPathInvoker?.Invoke(); }

        /// <summary>
        /// 현재 로그 파일명(확장자 포함)
        /// </summary>
        public string CurrentLogFileNameWithExtension { get => GetCurrentFileNameInvoker?.Invoke(); }

        /// <summary>
        /// 로그 파일 최상위에 기록되는 헤더
        /// </summary>
        public string Header { get; }

        /// <summary>
        /// 현재 사용중인 인코딩
        /// </summary>
        /// <remarks>기본값: Encoding.Default</remarks>
        public Encoding Encoding { get; set; } = Encoding.Default;

        /// <summary>
        /// 항목 구분자 (다수 항목시)
        /// </summary>
        public string Splitor { get; }


        private readonly string topDirectory;
        private readonly string logFileName;
        private readonly string logFileExtension;
        private readonly bool isWriteTime;
        private readonly StringBuilder logBuilder;


        /// <summary>
        /// Text형식의 로그 파일 작성
        /// </summary>
        /// <param name="topDirectory">로그파일 저장 경로 (topDirectory\yyyy-MM-dd\)</param>
        /// <param name="logName">로그파일 저장명 (logName_yyyy-MM-dd)</param>
        /// <param name="extension">로그파일 확장명</param>
        /// <param name="splitor">Log Item별 구분자</param>
        /// <param name="headerItems">Header별 구분자</param>
        /// <param name="isWriteTime">Log기록 시, 시간 기록 (true시 헤더에 "DateTime" 자동추가)</param>
        /// <param name="capacity">내부적으로 사용되는 StringBuilder의 Capacity</param>
        public TextLogWriter(string topDirectory, string logName, string extension, string splitor, string[] headerItems, bool isWriteTime = true, int capacity = 1024)
        {
            this.topDirectory = topDirectory;
            this.logFileName = logName;
            this.logFileExtension = extension;
            this.Splitor = splitor;
            this.isWriteTime = isWriteTime;

            var tempHeader = (headerItems == null | headerItems.Length == 0) ? "Messages" : string.Join(splitor, headerItems);
            Header = (isWriteTime == true) ? $"{"DateTime"}{splitor}{tempHeader}" : tempHeader;

            this.logBuilder = new StringBuilder(capacity);

            this.GetCurrentDirectoryPathInvoker = GetCurrentDirecotry;
            this.GetCurrentFileNameInvoker = GetCurrentFileNameWithExtension;

            logLockObject[logName] = new object();
        }

        /// <summary>
        /// Log 기록 (항목1,항목2,...)
        /// </summary>
        /// <param name="contents">항목별 로그</param>
        public void WriteLine(params string[] contents)
        {
            if (contents?.Length > 0)
            {
                lock (logLockObject[logFileName])
                {
                    logBuilder.Clear();
                    var filepath = Path.Combine(CurrentDirectory, CurrentLogFileNameWithExtension);

                    if (File.Exists(filepath) == false)
                    {
                        logBuilder.AppendLine(Header);
                    }

                    if (isWriteTime)
                    {
                        logBuilder.Append(DateTime.Now.ToString(this.WrittenTimeFormat));
                        logBuilder.Append(Splitor);
                    }

                    foreach (var content in contents)
                    {
                        logBuilder.Append(content);
                        logBuilder.Append(Splitor);
                    }

                    using (var fs = new FileStream(filepath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
                    using (var sw = new StreamWriter(fs, Encoding))
                    {
                        // 마지막 content에 Splitor가 붙는것을 제거
                        sw.WriteLine(logBuilder.ToString(0, logBuilder.Length - Splitor.Length));
                    }
                }
            }
        }

        /// <summary>
        /// Default Current Directory Pattern
        /// </summary>
        /// <returns>Current Directory</returns>
        private string GetCurrentDirecotry()
        {
            return Path.Combine(topDirectory, $"{DateTime.Now.ToString("yyyy-MM-dd")}");
        }

        /// <summary>
        ///  Default Current FileName Pattern
        /// </summary>
        /// <returns>Current FileName</returns>
        private string GetCurrentFileNameWithExtension()
        {
            return $"{logFileName}_{ $"{DateTime.Now.ToString("yyyy-MM-dd")}"}.{logFileExtension}";
        }
    }
}
