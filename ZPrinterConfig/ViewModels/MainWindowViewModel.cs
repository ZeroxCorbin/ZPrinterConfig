using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ZPrinterConfig.Controllers;

namespace ZPrinterConfig.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public string Version => App.Version;

        private PrinterController Printer { get; } = new PrinterController();

        [ObservableProperty] private string host = App.Settings.GetValue(nameof(Host), "");
        partial void OnHostChanged(string value) => App.Settings.SetValue(nameof(Host), value);

        [ObservableProperty] private string port = App.Settings.GetValue(nameof(Port), "6101");
        partial void OnPortChanged(string value) => App.Settings.SetValue(nameof(Port), value);

        [ObservableProperty] private string _Status;
        [ObservableProperty] private string _SocketStatus;

        public ObservableCollection<PrinterParameter> BVSettings { get; } =
        [
            new() { Name= "Reprint Mode", ParameterName = "ezpl.reprint_mode", Default = "off", Options = "on, off", Description = "This setting turns on/off the reprint mode." },
            new() { Name= "Reprint Void", ParameterName = "ezpl.reprint_void", Default = "off", Options = "on, off, custom", Description = "This setting allows the user to enable the backup and void functionality (if reprint_mode is on)." },
            new() { Name= "Reprint Void Length", ParameterName = "ezpl.reprint_void_length", Default = "203", Options = "1 - 32000", Description = "This setting allows the user to set a custom backup and void distance in dots.\r\nThis is only applied when ezpl.reprint_void is set to \"custom\"." },
            new() { Name= "Reprint Void Pattern", ParameterName = "ezpl.reprint_void_pattern", Default = "1", Options = "1, 2, 3, 4" , Recommended = "", Description = "This setting allows the user to set which void pattern to use.\r\nThere are four different patterns provided depending on customer preference.\r\nDifferent patterns have different levels of black applied across the\r\ncourse of the label, which can affect physical behavior such as labels sticking\r\nto ribbon, ribbon wrinkle, or ribbon tears.\r\nPattern two is specifically chosen to prevent any barcodes from being scannable after the void is applied."},
            new() { Name= "Start Print Signal", ParameterName = "device.applicator.start_print_mode", Default = "level",  Options = "level, pulse", Description = "This setting selects the applicator port START PRINT mode of operation." },
            new() { Name= "Tear Off", ParameterName = "ezpl.tear_off", Default = "0",  Options = "-120 - 1200" },
            new() { Name= "Print Mode", ParameterName = "media.printmode", Default = "tear off",  Options = "tear off" },
            new() { Name= "Applicator", ParameterName = "device.applicator.end_print", Default = "1",  Options = "off, 1" },
        ];
        public ObservableCollection<string> BVOperationTypes { get; } = new ObservableCollection<string>()
        {
            "Backup/Void Transfer",
            "Backup/Void Direct",
            "Normal",
        };

        [ObservableProperty] private string _BVSelectedOperationType = App.Settings.GetValue(nameof(BVSelectedOperationType), "Backup/Void Ribbon");

        public MainWindowViewModel()
        {
            Printer.SocketStateEvent += Printer_SocketStateEvent;

            OnBVSelectedOperationTypeChanged(BVSelectedOperationType);
        }

        [RelayCommand]
        private void Read(object parameter)
        {
            Task.Run(ResetStatus);

            string ip = GetIPAddress();
            if (ip == null)
            {
                SocketStatus = "Invalid Host Name or IP";
                return;
            }

            _ = Task.Run(() =>
            {
                Status = "Writing parameter.";

                if (Printer.Connect(ip, Port))
                {
                    PrinterParameter ps = (PrinterParameter)parameter;

                    Printer.Send($"! U1 getvar \"{ps.ParameterName}\" \r\n");
                    string res = Printer.Recieve(1000);

                    ps.ReadValue = res.Trim('\"', '\0');

                    Printer.Disconnect();
                }

                Task.Run(ResetStatus);
            });
        }
        [RelayCommand]
        private void ReadAll()
        {
            Task.Run(ResetStatus);

            string ip = GetIPAddress();
            if (ip == null)
            {
                SocketStatus = "Invalid Host Name or IP";
                return;
            }

            _ = Task.Run(() =>
            {
                Status = "Writing parameter.";

                if (Printer.Connect(ip, Port))
                {
                    foreach (PrinterParameter ps in BVSettings)
                    {
                        Printer.Send($"! U1 getvar \"{ps.ParameterName}\" \r\n");
                        string res = Printer.Recieve(1000);

                        ps.ReadValue = res.Trim('\"', '\0');
                    }

                    Printer.Disconnect();
                }

                Task.Run(ResetStatus);
            });
        }
        [RelayCommand]
        private void CopyRecommended(PrinterParameter parameter)
        {
            if (string.IsNullOrEmpty(parameter.Recommended))
                return;

            parameter.WriteValue = parameter.Recommended;
        }
        [RelayCommand]
        private async Task Write(PrinterParameter parameter)
        {
            _ = Task.Run(ResetStatus);

            string ip = GetIPAddress();
            if (ip == null)
            {
                SocketStatus = "Invalid Host Name or IP";
                return;
            }

            if (await DialogCoordinator.Instance.ShowMessageAsync(this, "Overwrite Parameter?", "Are you sure you want to overwrite the parameter?", MessageDialogStyle.AffirmativeAndNegative) != MessageDialogResult.Affirmative)
                return;

            _ = Task.Run(() =>
            {
                Status = "Writing parameter.";

                if (Printer.Connect(ip, Port))
                {
                    //Printer.Send($"! U1 getvar \"\" \"\"\r\n");
                    Printer.Send($"! U1 setvar \"{parameter.ParameterName}\" \"{parameter.WriteValue}\"\r\n ");
                    Read(parameter);
                }

                Task.Run(ResetStatus);
            });

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
        private void Printer_SocketStateEvent(SocketStates state, string message) => SocketStatus = message;

        private void ResetStatus()
        {
            Status = "";
            SocketStatus = "";
        }

        partial void OnBVSelectedOperationTypeChanged(string value)
        {
            App.Settings.SetValue("BVSelectedOperationType", value);

            if (value == "Backup/Void Transfer")
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
    }
}
