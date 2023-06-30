﻿using MahApps.Metro.Controls.Dialogs;
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
using ZPrinterConfig.Models;

namespace ZPrinterConfig.WindowViewModele
{
    public class MainWindowViewModel : Core.BaseViewModel
    {

        public string Version => App.Version;

        private PrinterController Printer { get; } = new PrinterController();

        public string Host { get => App.Settings.GetValue("PrinterHost", ""); set => App.Settings.SetValue("PrinterHost", value); }
        public string Port { get => App.Settings.GetValue("PrinterPort", "6101"); set { App.Settings.SetValue("PrinterPort", value); OnPropertyChanged("Port"); } }

        public string Status
        {
            get { return _Status; }
            set { SetProperty(ref _Status, value); }
        }
        private string _Status;


        public string SocketStatus
        {
            get { return _SocketStatus; }
            set { SetProperty(ref _SocketStatus, value); }
        }
        private string _SocketStatus;

        public ObservableCollection<PrinterParameter> BVSettings { get; } = new ObservableCollection<PrinterParameter>()
        {
            new PrinterParameter() { Name= "Reprint Mode", ParameterName = "ezpl.reprint_mode", Default = "off", Options = "on, off", Description = "This setting turns on/off the reprint mode." },
            new PrinterParameter() { Name= "Reprint Void", ParameterName = "ezpl.reprint_void", Default = "off", Options = "on, off, custom", Description = "This setting allows the user to enable the backup and void functionality (if reprint_mode is on)." },
            new PrinterParameter() { Name= "Reprint Void Length", ParameterName = "ezpl.reprint_void_length", Default = "203", Options = "1 - 32000", Description = "This setting allows the user to set a custom backup and void distance in dots.\r\nThis is only applied when ezpl.reprint_void is set to \"custom\"." },
            new PrinterParameter() { Name= "Reprint Void Pattern", ParameterName = "ezpl.reprint_void_pattern", Default = "1", Options = "1, 2, 3, 4" , Recommended = "", Description = "This setting allows the user to set which void pattern to use.\r\nThere are four different patterns provided depending on customer preference.\r\nDifferent patterns have different levels of black applied across the\r\ncourse of the label, which can affect physical behavior such as labels sticking\r\nto ribbon, ribbon wrinkle, or ribbon tears.\r\nPattern two is specifically chosen to prevent any barcodes from being scannable after the void is applied."},
            new PrinterParameter() { Name= "Start Print Signal", ParameterName = "device.applicator.start_print_mode", Default = "level",  Options = "level, pulse", Description = "This setting selects the applicator port START PRINT mode of operation." },
            new PrinterParameter() { Name= "Tear Off", ParameterName = "ezpl.tear_off", Default = "0",  Options = "-120 - 1200" },
            new PrinterParameter() { Name= "Print Mode", ParameterName = "media.printmode", Default = "tear off",  Options = "tear off" },
            new PrinterParameter() { Name= "Applicator", ParameterName = "device.applicator.end_print", Default = "1",  Options = "off, 1" },
        };
        public ObservableCollection<string> BVOperationTypes { get; } = new ObservableCollection<string>()
        {
            "Backup/Void Transfer",
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
        public ICommand BVCopyRecommended { get; }
        public ICommand BVWrite { get; }

        public ObservableCollection<PrinterParameter> Settings { get; } = new ObservableCollection<PrinterParameter>();

        public ICommand GetAllSettings { get; }

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
            BVCopyRecommended = new Core.RelayCommand(BVCopyRecommendedAction, c => true);

            GetAllSettings = new Core.RelayCommand(GetAllSettingsAction, c => true);

            Read = new Core.RelayCommand(ReadAction, c => true);
            ReadAll = new Core.RelayCommand(ReadAllAction, c => true);

            Write = new Core.RelayCommand(WriteAction, c => true);
            //WriteAll = new Core.RelayCommand(WriteAllAction, c => true);
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
            SocketStatus = message;
        }

        private void ResetStatus()
        {
            Status = "";
            SocketStatus = "";
        }

        private void BVSelectedOperationType_Changed(string value)
        {
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

        private void BVReadAction(object parameter)
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
        private void BVReadAllAction(object parameter)
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
        private void BVCopyRecommendedAction(object parameter)
        {
            PrinterParameter ps = (PrinterParameter)parameter;

            if (string.IsNullOrEmpty(ps.Recommended))
                return;

            ps.WriteValue = ps.Recommended;
        }
        private async void BVWriteAction(object parameter)
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
                    PrinterParameter ps = (PrinterParameter)parameter;

                    //Printer.Send($"! U1 getvar \"\" \"\"\r\n");
                    Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\"\r\n ");

                    ReadAction(parameter);
                }

                Task.Run(ResetStatus);
            });

        }

        private void GetAllSettingsAction(object parameter)
        {
            Task.Run(ResetStatus);

            Settings.Clear();

            string ip = GetIPAddress();
            if (ip == null)
            {
                SocketStatus = "Invalid Host Name or IP";
                return;
            }

            Task.Run(() =>
            {
                Status = "Getting all available parameters. This can take awhile!";

                var settings = Printer.GetAllSettings(ip, Port);

                App.Current.Dispatcher.Invoke(new Action(() =>
                {
                    foreach (var set in settings)
                    {
                        Settings.Add(set);
                    }
                }));

                Task.Run(ResetStatus);
            });

        }
        private void ReadAction(object parameter)
        {
            Task.Run(ResetStatus);

            string ip = GetIPAddress();
            if (ip == null)
            {
                SocketStatus = "Invalid Host Name or IP";
                return;
            }

            if (Printer.Connect(ip, Port))
            {
                Status = "Reading parameter.";

                PrinterParameter ps = (PrinterParameter)parameter;

                Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
                string res = Printer.Recieve(1000, "\"");

                ps.ReadValue = res.Trim('\"', '\0');

                Printer.Disconnect();

                Task.Run(ResetStatus);
            }
        }
        private void ReadAllAction(object parameter)
        {
            Task.Run(ResetStatus);

            string ip = GetIPAddress();
            if (ip == null)
            {
                SocketStatus = "Invalid Host Name or IP";
                return;
            }

            Task.Run(() =>
            {
                Status = "Reading all parameters. This can take awhile!";

                if (Printer.Connect(ip, Port))
                {
                    foreach (PrinterParameter ps in Settings)
                    {
                        if (ps.ParameterName.StartsWith("file"))
                            continue;

                        Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
                        string res = Printer.Recieve(1000, "\"");

                        ps.ReadValue = res.Trim('\"', '\0');
                    }

                    Printer.Disconnect();
                }

                Task.Run(ResetStatus);
            });


        }
        private async void WriteAction(object parameter)
        {
            _ = Task.Run(ResetStatus);

            PrinterParameter ps = (PrinterParameter)parameter;

            if (string.IsNullOrEmpty(ps.WriteValue))
                return;

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
                Status = "Reading parameter.";

                if (Printer.Connect(ip, Port))
                {
                    Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\"\r\n");

                    ReadAction(parameter);
                }

                Task.Run(ResetStatus);
            });

        }


        //private void BVWriteAllAction(object parameter)
        //{
        //    Task.Run(ResetStatus);

        //    string ip = GetIPAddress();
        //    if (ip == null)
        //    {
        //        SocketStatus = "Invalid Host Name or IP";
        //        return;
        //    }

        //    if (Printer.Connect(ip, Port))
        //    {
        //        foreach (PrinterController.PrinterSetting ps in BVSettings)
        //        {
        //            Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\" \r\n");

        //            Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
        //            string res = Printer.Recieve(1000);

        //            ps.ReadValue = res.Trim('\"', '\0');
        //        }

        //        Printer.Disconnect();
        //    }
        //}

        //private void WriteAllAction(object parameter)
        //{
        //    Task.Run(ResetStatus);

        //    string ip = GetIPAddress();
        //    if (ip == null)
        //    {
        //        SocketStatus = "Invalid Host Name or IP";
        //        return;
        //    }

        //    if (Printer.Connect(ip, Port))
        //    {
        //        foreach (PrinterController.PrinterSetting ps in Settings)
        //        {
        //            if (string.IsNullOrEmpty(ps.WriteValue))
        //                continue;

        //            Printer.Send($"! U1 setvar \"{ps.ParameterName}\" \"{ps.WriteValue}\"\r\n");

        //            Printer.Send($"! U1 getvar \"{ps.ParameterName}\"\r\n");
        //            string res = Printer.Recieve(1000);

        //            ps.ReadValue = res.Trim('\"', '\0');
        //        }

        //        Printer.Disconnect();
        //    }
        //}
    }
}
