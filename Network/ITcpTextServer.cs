using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public interface ITcpTextServer
    {
        Encoding Encoding { get; set; }
        Task<Socket> Accept(Socket listener);
        Socket Listen(int port, string ipString = "");
        Task<string> ReceiveText(Socket socket, int bufferSize);
        Task SendText(Socket socket, string message, int bufferSize);
    }
}