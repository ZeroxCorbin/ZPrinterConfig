using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZPrinterConfig.Models;
using ZPrinterConfig.WindowViewModele;

namespace ZPrinterConfig.WindowViews
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : MetroWindow
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private void btnLightTheme_Click(object sender, RoutedEventArgs e) => ThemeManager.Current.ChangeTheme(App.Current, "Light.Steel");
        private void btnDarkTheme_Click(object sender, RoutedEventArgs e) => ThemeManager.Current.ChangeTheme(App.Current, "Dark.Steel");

        private void btnResetPortNumber_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).Port = "6101";
        }

        private void MetroWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers) == (ModifierKeys.Control | ModifierKeys.Alt))
            {
                if (Keyboard.IsKeyDown(Key.A))
                {
                    if (bdrAllParams.Visibility == Visibility.Collapsed)
                        bdrAllParams.Visibility = Visibility.Visible;
                    else
                        bdrAllParams.Visibility = Visibility.Collapsed;

                }
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ((PrinterParameter)((TextBox)e.Source).DataContext).WriteValue = ((TextBox)e.Source).Text;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgBVParameters.UnselectAll();
            dgBVParameters.UnselectAllCells();
        }
    }
}
