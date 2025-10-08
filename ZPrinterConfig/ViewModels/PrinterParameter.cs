using CommunityToolkit.Mvvm.ComponentModel;

namespace ZPrinterConfig.ViewModels
{
    /// <summary>Represents a printer parameter with metadata, persisted write value, and runtime read/recommended values.</summary>
    public partial class PrinterParameter : ObservableObject
    {
        #region Fields

        /// <summary>Backing field for the generated 'ReadValue' property representing the latest value read from the device.</summary>
        [ObservableProperty]
        private string _ReadValue;
        /// <seealso cref="ReadValue"/>

        /// <summary>Backing field for the generated 'Recommended' property indicating the recommended value for this parameter.</summary>
        [ObservableProperty]
        private string _Recommended;
        /// <seealso cref="Recommended"/>

        #endregion

        #region Properties

        /// <summary>Gets the display name of the parameter.</summary>
        public string Name { get; internal set; }

        /// <summary>Gets the underlying parameter key used to read/write persisted values.</summary>
        public string ParameterName { get; internal set; }

        /// <summary>Gets or sets the persisted value for this parameter from <see cref="ParameterName"/>, falling back to <see cref="Default"/> when unavailable.</summary>
        public string WriteValue
        {
            get => App.Settings.GetValue(ParameterName, Default);
            set
            {
                App.Settings.SetValue(ParameterName, value);
                OnPropertyChanged(nameof(WriteValue));
            }
        }

        /// <summary>Gets the default value used when no persisted value is available.</summary>
        public string Default { get; internal set; }

        /// <summary>Gets the allowed option values, if any, typically as a delimited list.</summary>
        public string Options { get; internal set; }

        /// <summary>Gets a human-readable description of this parameter.</summary>
        public string Description { get; internal set; }

        #endregion
    }
}