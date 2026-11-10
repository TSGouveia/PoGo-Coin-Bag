using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Windows.Devices.Bluetooth;
using Windows.Devices.Radios;

namespace PoGoCoinBagWindows
{
    public partial class MainWindow : Window
    {
        private Radio? _bluetoothRadio;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += async (_, _) => await CheckBluetoothStatusAsync();
        }

        private async Task CheckBluetoothStatusAsync()
        {
            BtnRefresh.IsEnabled = false;
            TxtRadioStatus.Text = "A verificar estado...";
            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
            StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0xEA, 0xB3, 0x08)); // Yellow

            try
            {
                // 1. Check default adapter availability
                BluetoothAdapter? adapter = null;
                try
                {
                    adapter = await BluetoothAdapter.GetDefaultAsync();
                }
                catch
                {
                    // Adapter query exception
                }

                if (adapter == null)
                {
                    TxtAdapterName.Text = "Nenhum adaptador detetado";
                    TxtRadioStatus.Text = "Sem Suporte Bluetooth";
                    TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // Red
                    StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                    BtnTurnOn.Visibility = Visibility.Collapsed;
                    BtnRefresh.IsEnabled = true;
                    return;
                }

                // 2. Obtain Radio object
                try
                {
                    _bluetoothRadio = await adapter.GetRadioAsync();
                }
                catch
                {
                    _bluetoothRadio = null;
                }

                // Fallback: check all radios if adapter.GetRadioAsync() returned null
                if (_bluetoothRadio == null)
                {
                    var radios = await Radio.GetRadiosAsync();
                    _bluetoothRadio = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                }

                if (_bluetoothRadio != null)
                {
                    TxtAdapterName.Text = string.IsNullOrWhiteSpace(_bluetoothRadio.Name) 
                        ? "Adaptador Bluetooth" 
                        : _bluetoothRadio.Name;

                    switch (_bluetoothRadio.State)
                    {
                        case RadioState.On:
                            TxtRadioStatus.Text = "Ligado e Pronto (ON)";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81)); // Green
                            StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                            BtnTurnOn.Visibility = Visibility.Collapsed;
                            break;

                        case RadioState.Off:
                            TxtRadioStatus.Text = "Desligado (OFF)";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)); // Red
                            StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                            BtnTurnOn.Visibility = Visibility.Visible;
                            break;

                        case RadioState.Disabled:
                            TxtRadioStatus.Text = "Desativado pelo Sistema";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)); // Orange
                            StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
                            BtnTurnOn.Visibility = Visibility.Collapsed;
                            break;

                        default:
                            TxtRadioStatus.Text = "Estado Desconhecido";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
                            StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
                            BtnTurnOn.Visibility = Visibility.Visible;
                            break;
                    }
                }
                else
                {
                    // Adapter exists but Radio object wasn't accessible
                    TxtAdapterName.Text = "Controlador Bluetooth Detetado";
                    TxtRadioStatus.Text = "Ativo (verifique nas definições)";
                    TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                    StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                    BtnTurnOn.Visibility = Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                TxtAdapterName.Text = "Erro de Leitura";
                TxtRadioStatus.Text = ex.Message;
                TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                StatusIndicatorDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }
            finally
            {
                BtnRefresh.IsEnabled = true;
            }
        }

        private async void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {
            await CheckBluetoothStatusAsync();
        }

        private async void BtnTurnOn_Click(object sender, RoutedEventArgs e)
        {
            if (_bluetoothRadio != null)
            {
                BtnTurnOn.IsEnabled = false;
                try
                {
                    var result = await _bluetoothRadio.SetStateAsync(RadioState.On);
                    if (result == RadioAccessStatus.Allowed)
                    {
                        await Task.Delay(500);
                        await CheckBluetoothStatusAsync();
                    }
                    else
                    {
                        // If system blocked programmatic toggle, open Windows Settings
                        OpenBluetoothSettings();
                    }
                }
                catch
                {
                    OpenBluetoothSettings();
                }
                finally
                {
                    BtnTurnOn.IsEnabled = true;
                }
            }
            else
            {
                OpenBluetoothSettings();
            }
        }

        private void BtnOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            OpenBluetoothSettings();
        }

        private void OpenBluetoothSettings()
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "ms-settings:bluetooth",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Não foi possível abrir as definições: {ex.Message}", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void BtnOpenWebApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://na-ji.github.io/pokemon-go-coin-bag/",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro ao abrir o navegador: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
