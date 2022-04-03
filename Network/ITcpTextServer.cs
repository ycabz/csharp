using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace YCabz.Network
{
    public interface ITcpTextServer
    {
        Task<Socket> Accept(Socket listener);
        Socket Listen(int port, string ipString = "", int backLog = 1);
        Task<string> ReceiveText(Socket socket, Encoding encoding, int bufferMaxSize);
        Task SendText(Socket socket, Encoding encoding, string message, int bufferMaxSize);
    }
}