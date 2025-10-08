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
    /// <summary>View model for the main window that manages printer connectivity, parameter read/write operations, and backup/void presets.</summary>
    public partial class MainWindowViewModel : ObservableObject
    {
        #region Version and Dependencies

        /// <summary>Gets the application version.</summary>
        public string Version => App.Version;

        /// <summary>Gets the controller responsible for managing TCP socket communication with the printer.</summary>
        private PrinterController Printer { get; } = new PrinterController();

        #endregion

        #region Observable State (MVVM Toolkit)

        /// <summary>Stores the printer host name or IP address.</summary>
        [ObservableProperty] private string host = App.Settings.GetValue(nameof(Host), "");
        /// <seealso cref="Host"/>

        /// <summary>Persists changes to the <see cref="Host"/> setting.</summary>
        /// <param name="value">The updated host value.</param>
        partial void OnHostChanged(string value) => App.Settings.SetValue(nameof(Host), value);

        /// <summary>Stores the TCP port used for printer communication.</summary>
        [ObservableProperty] private string port = App.Settings.GetValue(nameof(Port), "6101");
        /// <seealso cref="Port"/>

        /// <summary>Persists changes to the <see cref="Port"/> setting.</summary>
        /// <param name="value">The updated port value.</param>
        partial void OnPortChanged(string value) => App.Settings.SetValue(nameof(Port), value);

        /// <summary>Holds a short status message for UI feedback.</summary>
        [ObservableProperty] private string _Status;
        /// <seealso cref="Status"/>

        /// <summary>Holds the latest socket connection status message.</summary>
        [ObservableProperty] private string _SocketStatus;
        /// <seealso cref="SocketStatus"/>

        /// <summary>Stores the selected backup/void operation type.</summary>
        [ObservableProperty] private string _BVSelectedOperationType = App.Settings.GetValue(nameof(BVSelectedOperationType), "Backup/Void Ribbon");
        /// <seealso cref="BVSelectedOperationType"/>

        /// <summary>Persists the selected operation type and updates recommended values in <see cref="BVSettings"/>.</summary>
        /// <param name="value">The newly selected operation type.</param>
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

        #endregion

        #region Collections

        /// <summary>Gets the collection of printer parameters displayed and managed by the view; used by <see cref="ReadAll"/> and operation type presets.</summary>
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

        /// <summary>Gets the supported operation types used by <see cref="BVSelectedOperationType"/>.</summary>
        public ObservableCollection<string> BVOperationTypes { get; } = new ObservableCollection<string>()
        {
            "Backup/Void Transfer",
            "Backup/Void Direct",
            "Normal",
        };

        #endregion

        #region Constructor

        /// <summary>Initializes the view model, subscribes to socket state notifications, and applies initial presets based on <see cref="BVSelectedOperationType"/>.</summary>
        public MainWindowViewModel()
        {
            Printer.SocketStateEvent += Printer_SocketStateEvent;
            OnBVSelectedOperationTypeChanged(BVSelectedOperationType);
        }

        #endregion

        #region Commands

        /// <summary>Reads the current value of the specified printer parameter from the device and updates its <see cref="PrinterParameter.ReadValue"/>.</summary>
        /// <seealso cref="ReadCommand"/>
        /// <param name="parameter">The associated <see cref="PrinterParameter"/> to read.</param>
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

        /// <summary>Reads the current value for all parameters in <see cref="BVSettings"/> from the device.</summary>
        /// <seealso cref="ReadAllCommand"/>
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

        /// <summary>Copies the parameter's <see cref="PrinterParameter.Recommended"/> value into <see cref="PrinterParameter.WriteValue"/>.</summary>
        /// <seealso cref="CopyRecommendedCommand"/>
        /// <param name="parameter">The parameter to update.</param>
        [RelayCommand]
        private void CopyRecommended(PrinterParameter parameter)
        {
            if (string.IsNullOrEmpty(parameter.Recommended))
                return;

            parameter.WriteValue = parameter.Recommended;
        }

        /// <summary>Writes the parameter's <see cref="PrinterParameter.WriteValue"/> to the device, then refreshes by invoking <see cref="Read(object)"/>.</summary>
        /// <seealso cref="WriteCommand"/>
        /// <param name="parameter">The parameter to write.</param>
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
                    Printer.Send($"! U1 setvar \"{parameter.ParameterName}\" \"{parameter.WriteValue}\"\r\n ");
                    Read(parameter);
                }

                Task.Run(ResetStatus);
            });
        }

        #endregion

        #region Networking and Helpers

        /// <summary>Resolves the <see cref="Host"/> value to an IP address string or returns null if resolution fails.</summary>
        /// <returns>The resolved IP address string, or null if invalid or not found.</returns>
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

        /// <summary>Clears transient UI messages by resetting <see cref="Status"/> and <see cref="SocketStatus"/>.</summary>
        private void ResetStatus()
        {
            Status = "";
            SocketStatus = "";
        }

        #endregion

        #region Event Handlers

        /// <summary>Updates <see cref="SocketStatus"/> when the printer socket state changes.</summary>
        /// <param name="state">The new socket state.</param>
        /// <param name="message">A human-readable status message.</param>
        private void Printer_SocketStateEvent(SocketStates state, string message) => SocketStatus = message;

        #endregion
    }
}