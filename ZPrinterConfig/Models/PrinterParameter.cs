using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZPrinterConfig.Models
{
    public class PrinterParameter : Core.BaseViewModel
    {
        public string Name { get; internal set; }

        public string ParameterName { get; internal set; }

        public string WriteValue
        {
            get => App.Settings.GetValue(ParameterName, Default);
            set { App.Settings.SetValue(ParameterName, value); OnPropertyChanged("WriteValue"); }
        }
        public string ReadValue { get => _ReadValue; set => SetProperty(ref _ReadValue, value); }
        private string _ReadValue;
        public string Default { get; internal set; }

        public string Recommended { get => _Recommended; set => SetProperty(ref _Recommended, value); }
        private string _Recommended;

        public string Options { get; internal set; }

        public string Description { get; internal set; }
    }
}
