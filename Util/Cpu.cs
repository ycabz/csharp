using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

using FileTime = System.Runtime.InteropServices.ComTypes.FILETIME;

namespace YCabz.Util
{
    public static class Cpu
    {
        [DllImport("Kernel32.dll"), SuppressUnmanagedCodeSecurity]
        private static extern int GetCurrentProcessorNumber();

        /// <summary>
        /// 현재 사용중이 CPU ID.
        /// </summary>
        public static int CurrentID { get => GetCurrentProcessorNumber(); }

        /// <summary>
        /// CPU Affinity(Masking)
        /// (0 ~ Max CPU Count - 1)
        /// </summary>
        /// <param name="usingCore">사용할 CPU 번호, null이면 모두 사용</param>
        /// <remarks>Max CPU Count를 넘으면 안된다</remarks>
        public static void SetAffinity(params uint[] usingCore)
        {
            // 매개변수가 없으면 All
            if (usingCore == null || usingCore.Length == 0)
            {
                usingCore = Enumerable.Range(0, Environment.ProcessorCount).Select(p => (uint)p).ToArray();
            }

            // 최대 Cpu수보다 설정된 Core번호가 크면 Error
            if (usingCore.Count(p => p >= Environment.ProcessorCount) > 0)
            {
                throw new ArgumentException("Cannot be greater than max number of cores");
            }

            var mask = 0;
            foreach (var core in usingCore)
            {
                mask |= 0x0001 << (int)core;
            }

            Process.GetCurrentProcess().ProcessorAffinity = (IntPtr)mask;
        }
    }
}
