using ControlzEx.Theming;
using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {

        }

        private void DataGrid_CellEditEnding_1(object sender, DataGridCellEditEndingEventArgs e)
        {
            ((Controllers.PrinterController.PrinterSetting)e.EditingElement.DataContext).WriteValue = ((TextBox)e.EditingElement).Text;
        }

        private void btnLightTheme_Click(object sender, RoutedEventArgs e) => ThemeManager.Current.ChangeTheme(App.Current, "Light.Steel");

        private void btnDarkTheme_Click(object sender, RoutedEventArgs e) => ThemeManager.Current.ChangeTheme(App.Current, "Dark.Steel");

        private void btnResetPortNumber_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindowViewModel)DataContext).Port = "9100";
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
            ((Controllers.PrinterController.PrinterSetting)((TextBox)e.Source).DataContext).WriteValue = ((TextBox)e.Source).Text;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dgBVParameters.UnselectAll();
            dgBVParameters.UnselectAllCells();
        }
    }
}
