using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZPrinterConfig.ViewModels;

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
        public string Recieve(int timeout) => Socket.Receive(timeout);
        public string Recieve(int timeout, string terminator) => Socket.Receive(timeout, terminator);

        private void Socket_ExceptionEvent(object sender, EventArgs e)
        {
            SocketState = SocketStates.Closed;
            SocketStateEvent?.Invoke(SocketStates.Exception, ((Exception)sender).Message);
        }
        private void Socket_ConnectEvent()
        {
            SocketState = SocketStates.Open;
            SocketStateEvent?.Invoke(SocketStates.Open, "Open");
        }
        private void Socket_CloseEvent()
        {
            SocketState = SocketStates.Closed;
            SocketStateEvent?.Invoke(SocketStates.Closed, "Close");
        }
        private void Socket_MessageEvent(object sender, EventArgs e)
        {
            string message = (string)sender;


        }

        public List<PrinterParameter> GetAllSettings(string ip, string port)
        {
            List<PrinterParameter> settings = new List<PrinterParameter>();

            if (Connect(ip, port))
            {
                Socket.Send($"! U1 getvar \"all\"\r\n");

                foreach(var line in Socket.Receive(1000, "\"\"").Split('\n'))
                {
                    if(!line.TrimEnd('\r').EndsWith("."))
                        settings.Add(new PrinterParameter() { ParameterName = line.TrimEnd('\r') });
                }
                settings.Remove(settings.Last());

                Socket.Send($"! U1 getvar \"allcv\"\r\n");

                foreach (var ln in Socket.Receive(1000, "\"\"").Split('\n'))
                {
                    string line = ln.TrimEnd('\r');
                    if (!line.EndsWith("."))
                    {
                        if (line.Contains("Choices:"))
                        {
                            int ind = line.IndexOf(' ');
                            if(ind != -1)
                            {
                                string name = line.Substring(0, ind);
                                try
                                {
                                    name = name.TrimEnd(',');
                                    PrinterParameter setting = settings.First(x => x.ParameterName == name); 
                                    setting.Options = line.Substring(line.IndexOf("Choices:") + "Choices:".Length);
                                }
                                catch 
                                {

                                }


                            
                            }
                        }

                    }
                        
                }

                Disconnect();
            }

            return settings;
        }

    }
}
