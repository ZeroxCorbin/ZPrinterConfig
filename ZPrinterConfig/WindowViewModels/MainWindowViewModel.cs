using MahApps.Metro.Controls.Dialogs;
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

        public string Version => App.Version;

        private PrinterController Printer { get; } = new PrinterController();

        public string Host { get => App.Settings.GetValue("PrinterHost", ""); set => App.Settings.SetValue("PrinterHost", value); }
        public string Port { get => App.Settings.GetValue("PrinterPort", "9100"); set => App.Settings.SetValue("PrinterPort", value); }

        public string Status
        {
            get { return _Status; }
            set { SetProperty(ref _Status, value); }
        }
        private string _Status;

        public ObservableCollection<PrinterController.PrinterSetting> BVSettings { get; } = new ObservableCollection<PrinterController.PrinterSetting>()
        {
            new PrinterController.PrinterSetting() { Name= "Reprint Mode", ParameterName = "ezpl.reprint_mode", Default = "off", Options = "on, off" },
            new PrinterController.PrinterSetting() { Name= "Reprint Void", ParameterName = "ezpl.reprint_void", Default = "off", Options = "on, off, custom" },
            new PrinterController.PrinterSetting() { Name= "Reprint Void Length", ParameterName = "ezpl.reprint_void_length", Default = "203", Options = "1 - 32000" },
            new PrinterController.PrinterSetting() { Name= "Reprint Void Pattern", ParameterName = "ezpl.reprint_void_pattern", Default = "1", Options = "1 - 4" },
            new PrinterController.PrinterSetting() { Name= "Start Print Signal", ParameterName = "device.applicator.start_print_mode", Default = "level",  Options = "level, pulse" },
            new PrinterController.PrinterSetting() { Name= "Tear Off", ParameterName = "ezpl.tear_off", Default = "0",  Options = "-120 - 1200" },
            new PrinterController.PrinterSetting() { Name= "Print Mode", ParameterName = "media.printmode", Default = "tear off",  Options = "tear off" },
            new PrinterController.PrinterSetting() { Name= "Applicator", ParameterName = "device.applicator.end_print", Default = "1",  Options = "off, 1" },
        };
        public ObservableCollection<string> BVOperationTypes { get; } = new ObservableCollection<string>()
        {
            "Backup/Void Ribbon",
            "Backup/Void Direct",
            "Normal",
        };

        public string BVSelectedOperationType
        {
            get { string val = App.Settings.GetValue("BVSelectedOperationType", "Backup/Void Ribbon"); BVSelectedOperationType_Changed(val); return val; }
            set { App.Settings.SetValue("BVSelectedOperationType", value); BVSelectedOperationType_Changed(value); }
        }

        public ICommand BVRead { get; }
        public ICommand BVReadAll { get; }
        public ICommand BVWrite { get; }
        public ICommand BVWriteAll { get; }

        public ObservableCollection<PrinterController.PrinterSetting> Settings { get; } = new ObservableCollection<PrinterController.PrinterSetting>();

        
        public PrinterController.PrinterSetting NewSetting { get; } = new PrinterController.PrinterSetting();

        public ICommand GetAllSettings { get; }
        public ICommand AddSetting { get; }

        public ICommand Read { get; }
        public ICommand ReadAll { get; }
        public ICommand Write { get; }
        public ICommand WriteAll { get; }

        public MainWindowViewModel()
        {
            Printer.SocketStateEvent += Printer_SocketStateEvent;

            BVRead = new Core.RelayCommand(BVReadAction, c => true);
            BVReadAll = new Core.RelayCommand(BVReadAllAction, c => true);

            BVWrite = new Core.RelayCommand(BVWriteAction, c => true);
            BVWriteAll = new Core.RelayCommand(BVWriteAllAction, c => true);

            GetAllSettings = new Core.RelayCommand(GetAllSettingsAction, c => true);
            AddSetting = new Core.RelayCommand(AddSettingAction, c => true);

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
            Status = message;
        }


        private void BVReadAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                PrinterController.PrinterSetting ps = (PrinterController.PrinterSetting)parameter;

                Printer.Send($"! U1 getvar \"{ps.ParameterName}\" \r\n");
                string res = Printer.Recieve(1000);

                ps.ReadValue = res.Trim('\"', '\0');

                Printer.Disconnect();
            }
        }
        private void BVReadAllAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                foreach (PrinterController.PrinterSetting ps in BVSettings)
                {
                    Printer.Send($"! U1 getvar \"{ps.ParameterName}\" \r\n");
                    string res = Printer.Recieve(1000);

                    ps.ReadValue = res.Trim('\"', '\0');
                }

                Printer.Disconnect();
            }
        }

        private async void BVWriteAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (await DialogCoordinator.Instance.ShowMessageAsync(this, "Overwrite Parameter?", "Are you sure you want to overwrite the parameter?", MessageDialogStyle.AffirmativeAndNegative) != MessageDialogResult.Affirmative)
                return;

            if (Printer.Connect(ip, Port))
            {
                PrinterController.PrinterSetting ps = (PrinterController.PrinterSetting)parameter;

                //Printer.Send($"! U1 getvar \"\" \"\"\r\n");
                Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\"\r\n ");

                ReadAction(parameter);
            }
        }
        private void BVWriteAllAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                foreach (PrinterController.PrinterSetting ps in BVSettings)
                {
                    Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\" \r\n");

                    Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
                    string res = Printer.Recieve(1000);

                    ps.ReadValue = res.Trim('\"', '\0');
                }

                Printer.Disconnect();
            }
        }

        private void BVSelectedOperationType_Changed(string value)
        {
            if(value == "Backup/Void Ribbon")
            {
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_mode").Recommended = "on";
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_void").Recommended = "custom";
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_void_length").Recommended = "< 203";
                BVSettings.First(s => s.ParameterName == "device.applicator.start_print_mode").Recommended = "pulse";
                BVSettings.First(s => s.ParameterName == "ezpl.tear_off").Recommended = "< 200";
                BVSettings.First(s => s.ParameterName == "media.printmode").Recommended = "tear off";
                BVSettings.First(s => s.ParameterName == "device.applicator.end_print").Recommended = "1";
            }
            if (value == "Backup/Void Direct")
            {
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_mode").Recommended = "on";
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_void").Recommended = "on";
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_void_length").Recommended = "";
                BVSettings.First(s => s.ParameterName == "device.applicator.start_print_mode").Recommended = "pulse";
                BVSettings.First(s => s.ParameterName == "ezpl.tear_off").Recommended = "";
                BVSettings.First(s => s.ParameterName == "media.printmode").Recommended = "tear off";
                BVSettings.First(s => s.ParameterName == "device.applicator.end_print").Recommended = "1";
            }
            if (value == "Normal")
            {
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_mode").Recommended = "off";
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_void").Recommended = "off";
                BVSettings.First(s => s.ParameterName == "ezpl.reprint_void_length").Recommended = "";
                BVSettings.First(s => s.ParameterName == "device.applicator.start_print_mode").Recommended = "level";
                BVSettings.First(s => s.ParameterName == "ezpl.tear_off").Recommended = "";
                BVSettings.First(s => s.ParameterName == "media.printmode").Recommended = "tear off";
                BVSettings.First(s => s.ParameterName == "device.applicator.end_print").Recommended = "1";
            }
        }


        private void ReadAction(object parameter)
        {
            string ip = GetIPAddress();
            if(ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                PrinterController.PrinterSetting ps = (PrinterController.PrinterSetting)parameter;

                Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
                string res = Printer.Recieve(1000, "\"");

                ps.ReadValue = res.Trim('\"', '\0');

                Printer.Disconnect();
            }
        }
        private void ReadAllAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                foreach(PrinterController.PrinterSetting ps in Settings)
                {
                    if (ps.ParameterName.StartsWith("file"))
                        continue;

                    Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
                    string res = Printer.Recieve(1000, "\"");

                    ps.ReadValue = res.Trim('\"', '\0');
                }

                Printer.Disconnect();
            }
        }

        private async void WriteAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if(await DialogCoordinator.Instance.ShowMessageAsync(this, "Overwrite Parameter?", "Are you sure you want to overwrite the parameter?", MessageDialogStyle.AffirmativeAndNegative) != MessageDialogResult.Affirmative)
                return;


            if (Printer.Connect(ip, Port))
            {
                PrinterController.PrinterSetting ps = (PrinterController.PrinterSetting)parameter;

                Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\"\r\n");

                ReadAction(parameter);
            }
        }
        private void WriteAllAction(object parameter)
        {
            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                foreach (PrinterController.PrinterSetting ps in Settings)
                {
                    if (string.IsNullOrEmpty(ps.WriteValue))
                        continue;

                    Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\"\r\n");

                    Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
                    string res = Printer.Recieve(1000);

                    ps.ReadValue = res.Trim('\"', '\0');
                }

                Printer.Disconnect();
            }
        }


        private void AddSettingAction(object parameter)
        {

        }
        private void GetAllSettingsAction(object parameter)
        {
            Settings.Clear();

            string ip = GetIPAddress();
            if (ip == null)
            {
                Status = "Invalid Host Name or IP";
                return;
            }

            var settings = Printer.GetAllSettings(ip, Port);

            foreach(var set in settings)
            {
                Settings.Add(set);
            }
        }
    }
}
