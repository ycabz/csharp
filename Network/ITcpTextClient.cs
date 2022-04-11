using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public interface ITcpTextClient
    {
        Encoding Encoding { get; set; }
        Task<Socket> Connect(string ipString, int port);
        Task<string> ReceiveText(Socket socket, int bufferSize);
        Task SendText(Socket socket,  string message, int bufferSize);
    }
}