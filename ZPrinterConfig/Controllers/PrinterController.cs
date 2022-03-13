using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPrinterConfig.Controllers
{
    public class PrinterController
    {
        private AsyncSocket.ASocketManager Socket { get; }

        public SocketStates SocketState { get; set; }
        public delegate void SocketStateEventDelegate(SocketStates state, string message);
        public event SocketStateEventDelegate SocketStateEvent;

        public bool IsConnected => Socket.IsConnected;

        public PrinterController()
        {
            Socket = new AsyncSocket.ASocketManager();
            Socket.CloseEvent += Socket_CloseEvent;
            Socket.ConnectEvent += Socket_ConnectEvent;
            Socket.ExceptionEvent += Socket_ExceptionEvent;
            Socket.MessageEvent += Socket_MessageEvent;
        }

        public void ConnectAsync(string ipAddress, string port)
        {
            if (!Socket.IsConnected)
            {
                SocketStateEvent?.Invoke(SocketStates.Trying, "Trying");
                Task.Run(() =>
                {
                    if (!Socket.Connect(ipAddress, int.Parse(port)))
                        SocketStateEvent?.Invoke(SocketStates.Exception, "Unable to connect!");
                });
            }
        }

        public bool Connect(string ipAddress, string port)
        {
            if (!Socket.IsConnected)
            {
                SocketStateEvent?.Invoke(SocketStates.Trying, "Trying");

                if (!Socket.Connect(ipAddress, int.Parse(port)))
                {
                    Disconnect();
                    SocketStateEvent?.Invoke(SocketStates.Exception, "Unable to connect!");
                    return false;
                }
            }
            return true;
        }

        public void Disconnect()
        {
            Socket.Close();
        }

        public void Send(string message) => Socket.Send(message);
        public string Recieve(int timeout) => System.Text.Encoding.UTF8.GetString(Socket.Receive(timeout));

        private void Socket_ExceptionEvent(object sender, EventArgs e)
        {
            SocketState = SocketStates.Closed;
            SocketStateEvent?.Invoke(SocketStates.Exception, ((Exception)sender).Message);
        }
        private void Socket_ConnectEvent(object sender, EventArgs e)
        {
            SocketState = SocketStates.Open;
            SocketStateEvent?.Invoke(SocketStates.Open, "Open");
        }
        private void Socket_CloseEvent(object sender, EventArgs e)
        {
            SocketState = SocketStates.Closed;
            SocketStateEvent?.Invoke(SocketStates.Closed, "Close");
        }
        private void Socket_MessageEvent(object sender, EventArgs e)
        {
            string message = (string)sender;


        }

    }
}
