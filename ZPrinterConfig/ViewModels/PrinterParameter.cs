using CommunityToolkit.Mvvm.ComponentModel;

namespace ZPrinterConfig.ViewModels
{
    public partial class PrinterParameter : ObservableObject
    {
        public string Name { get; internal set; }

        public string ParameterName { get; internal set; }

        public string WriteValue
        {
            get => App.Settings.GetValue(ParameterName, Default);
            set { App.Settings.SetValue(ParameterName, value); OnPropertyChanged(nameof(WriteValue)); }
        }
        [ObservableProperty] private string _ReadValue;

        public string Default { get; internal set; }

        [ObservableProperty] private string _Recommended;

        public string Options { get; internal set; }

        public string Description { get; internal set; }
    }
}
