using System;
using System.Linq;
using System.Runtime.InteropServices;

namespace SQLiteManager
{
    public class NetworkDrive : IDisposable
    {
        #region WinApi ==============================================================================

        private const int RESOURCE_CONNECTED = 0x00000001;
        private const int RESOURCE_GLOBALNET = 0x00000002;
        private const int RESOURCE_REMEMBERED = 0x00000003;
        private const int RESOURCETYPE_ANY = 0x00000000;
        private const int RESOURCETYPE_DISK = 0x00000001;
        private const int RESOURCETYPE_PRINT = 0x00000002;
        private const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
        private const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
        private const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
        private const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
        private const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
        private const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;
        private const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
        private const int RESOURCEUSAGE_CONTAINER = 0x00000002;
        private const int CONNECT_INTERACTIVE = 0x00000008;
        private const int CONNECT_PROMPT = 0x00000010;
        private const int CONNECT_REDIRECT = 0x00000080;
        private const int CONNECT_UPDATE_PROFILE = 0x00000001;
        private const int CONNECT_COMMANDLINE = 0x00000800;
        private const int CONNECT_CMD_SAVECRED = 0x00001000;
        private const int CONNECT_LOCALDRIVE = 0x00000100;

        /// <summary>
        /// https://docs.microsoft.com/en-us/windows/win32/debug/system-error-codes
        /// </summary>
        public class Error
        {
            public const int NO_ERROR = 0;
            public const int ACCESS_DENIED = 5;
            public const int BAD_NET_NAME = 67;
            public const int ALREADY_ASSIGNED = 85;
            public const int INVALID_PARAMETER = 87;
            public const int MORE_DATA = 234;
            public const int NO_MORE_ITEMS = 259;
            public const int INVALID_ADDRESS = 487;
            public const int BAD_DEVICE = 1200;
            public const int NO_NET_OR_BAD_PATH = 1203;
            public const int BAD_PROVIDER = 1204;
            public const int CANNOT_OPEN_PROFILE = 1205;
            public const int BAD_PROFILE = 1206;
            public const int EXTENDED_ERROR = 1208;
            public const int INVALID_PASSWORD = 1216;
            public const int MULTIPLE_CONNECTION = 1219;
            public const int NO_NETWORK = 1222;
            public const int CANCELLED = 1223;
            public const int LOGON_FAILURE = 1326;
            public const int OPEN_FILES = 2401;
            public const int DEVICE_IN_USE = 2404;
            public const int NOT_CONNECTED = 2250;
        }

        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(IntPtr hwndOwner, NetResource lpNetResource, string lpPassword, string lpUserID, int dwFlags, string lpAccessName, string lpBufferSize, string lpResult);

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection2(string lpName, int dwFlags, bool fForce);

        [StructLayout(LayoutKind.Sequential)]
        private class NetResource
        {
            public int Scope { get; set; }
            public int Type { get; set; }
            public int DisplayType { get; set; }
            public int Usage { get; set; }
            public string LocalName { get; set; }
            public string RemoteName { get; set; }
            public string Comment { get; set; }
            public string Provider { get; set; }
        }

        #endregion

        public string RemoteName { get; }
        public string User { get; }
        public string Password { get; }
        public bool Connected { get; private set; } = false;

        public NetworkDrive(string remoteName, string user, string password)
        {
            RemoteName = remoteName;
            User = user;
            Password = password;
        }

        public void Connect()
        {
            var resource = new NetResource()
            {
                Type = RESOURCETYPE_DISK,
                LocalName = null,
                RemoteName = RemoteName,
                Provider = null,
            };

            int result = WNetUseConnection(IntPtr.Zero, resource, Password, User, CONNECT_PROMPT, null, null, null);
            if (result != 0)
            {
                throw new Exception($"Error ({GetErrorName(result)})");
            }
            Connected = true;
        }

        public void Disconnect()
        {
            var result = WNetCancelConnection2(RemoteName, CONNECT_UPDATE_PROFILE, true);
            if (result != Error.NO_ERROR)
            {
                throw new Exception($"Error ({GetErrorName(result)})");
            }

            Connected = false;
        }

        public void Dispose()
        {
            if (Connected)
            {
                Disconnect();
            }
        }

        private string GetErrorName(int errorCode)
        {
            var fieldInfo = typeof(Error).GetFields()
                .FirstOrDefault(p => Convert.ToInt32(p.GetRawConstantValue()) == errorCode);
            return fieldInfo == null ? $"{errorCode.ToString()}" 
                : fieldInfo.Name;
        }
    }
}
