using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using ZPrinterConfig.Controllers;

namespace ZPrinterConfig.WindowViewModele
{
    public class MainWindowViewModel : Core.BaseViewModel
    {
        private PrinterController Printer { get; } = new PrinterController();

        public string Host { get => App.Settings.GetValue("PrinterHost", ""); set => App.Settings.SetValue("PrinterHost", value); }
        public string Port { get => App.Settings.GetValue("PrinterPort", ""); set => App.Settings.SetValue("PrinterPort", value); }

        public class PrinterSetting : Core.BaseViewModel
        {
            public string Name { get; internal set; }
            public string WriteValue { get => App.Settings.GetValue(Name, Default); set => App.Settings.SetValue(Name, value); }
            public string ReadValue { get => _ReadValue; set => SetProperty(ref _ReadValue, value); }
            private string _ReadValue;
            public string Default { get; internal set; }
            public string Options { get; internal set; }
        }
        public ObservableCollection<PrinterSetting> Settings { get; } = new ObservableCollection<PrinterSetting>()
        {
            new PrinterSetting() { Name = "ezpl.reprint_mode", Default = "off", Options = "on, off" },
            new PrinterSetting() { Name = "ezpl.reprint_void", Default = "off", Options = "on, off, custom" },
            new PrinterSetting() { Name = "ezpl.reprint_void_length", Default = "203", Options = "1-32000" },
            new PrinterSetting() { Name = "ezpl.reprint_void_pattern", Default = "1", Options = "1-4" },
        };

        public string Status
        {
            get { return _Status; }
            set { SetProperty(ref _Status, value); }
        }
        private string _Status;

        public string ConnectButtonText { get => connectButtonText; set => SetProperty(ref connectButtonText, value); }
        private string connectButtonText = "Connect";
        public bool ConnectionState { get => connectionState; set => SetProperty(ref connectionState, value); }
        private bool connectionState;
        public string ConnectMessage { get => connectMessage; set { _ = SetProperty(ref connectMessage, value); } }
        private string connectMessage;
        public bool IsConnected
        {
            get => isConnected;
            set { SetProperty(ref isConnected, value); OnPropertyChanged("IsNotConnected"); }
        }
        private bool isConnected = false;
        public bool IsNotConnected { get => !isConnected; }

        public ICommand Read { get; }
        public ICommand ReadAll { get; }
        public ICommand Write { get; }
        public ICommand WriteAll { get; }

        public MainWindowViewModel()
        {
            Printer.SocketStateEvent += Printer_SocketStateEvent;

            Read = new Core.RelayCommand(ReadAction, c => true);
            ReadAll = new Core.RelayCommand(ReadAllAction, c => true);

            Write = new Core.RelayCommand(WriteAction, c => true);
            WriteAll = new Core.RelayCommand(WriteAllAction, c => true);
        }

        public string GetIPAddress()
        {
            string ip = null;
            if (Core.StaticUtils.Regex.CheckValidIP(Host))
            {
                ip = Host;
            }
            else
            {
                try
                {
                    var ipEntry = System.Net.Dns.GetHostEntry(Host);
                    if (ipEntry.AddressList.Count() > 0)
                        ip = ipEntry.AddressList[0].ToString();
                }
                catch (Exception ex)
                {
                    Status = ex.Message;
                }
            }

            return ip;
        }
        private void Printer_SocketStateEvent(SocketStates state, string message)
        {
            ConnectMessage = message;
        }

        private void ReadAction(object parameter)
        {
            string ip = GetIPAddress();
            if(ip == null)
            {
                ConnectMessage = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                PrinterSetting ps = (PrinterSetting)parameter;

                Printer.Send($"! U1 getvar \"{ps.Name}\"\r\n");
                string res = Printer.Recieve(1000);

                ps.ReadValue = res.Trim('\"', '\0');

                Printer.Disconnect();
            }
        }

        private void ReadAllAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                ConnectMessage = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                foreach(PrinterSetting ps in Settings)
                {
                    Printer.Send($"! U1 getvar \"{ps.Name}\"\r\n");
                    string res = Printer.Recieve(1000);

                    ps.ReadValue = res.Trim('\"', '\0');
                }

                Printer.Disconnect();
            }
        }

        private void WriteAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                ConnectMessage = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                PrinterSetting ps = (PrinterSetting)parameter;

                Printer.Send($"! U1 setvar \"{ps.Name}\" \"{ps.WriteValue}\"\r\n");

                ReadAction(parameter);
            }
        }

        private void WriteAllAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                ConnectMessage = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                foreach (PrinterSetting ps in Settings)
                {
                    Printer.Send($"! U1 setvar \"{ps.Name}\" \"{ps.WriteValue}\"\r\n");

                    Printer.Send($"! U1 getvar \"{ps.Name}\"\r\n");
                    string res = Printer.Recieve(1000);

                    ps.ReadValue = res.Trim('\"', '\0');
                }

                Printer.Disconnect();
            }
        }
    }
}
