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
        private readonly BleSwitchClient _client = new();

        public MainWindow()
        {
            InitializeComponent();

            _client.StateChanged += OnClientStateChanged;
            _client.LogMessage += OnClientLog;

            Loaded += async (_, _) => await CheckBluetoothHardwareAsync();
        }

        private async Task CheckBluetoothHardwareAsync()
        {
            BtnRefreshStatus.IsEnabled = false;
            TxtRadioStatus.Text = "A verificar...";
            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));

            try
            {
                BluetoothAdapter? adapter = null;
                try
                {
                    adapter = await BluetoothAdapter.GetDefaultAsync();
                }
                catch { }

                if (adapter == null)
                {
                    TxtAdapterName.Text = "Nenhum adaptador encontrado";
                    TxtRadioStatus.Text = "Sem Hardware Bluetooth";
                    TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                    BtnTurnOnBluetooth.Visibility = Visibility.Collapsed;
                    BtnConnect.IsEnabled = false;
                    AppendLog("[Hardware] Nenhum adaptador Bluetooth foi detetado nesta máquina.");
                    return;
                }

                try
                {
                    _bluetoothRadio = await adapter.GetRadioAsync();
                }
                catch
                {
                    _bluetoothRadio = null;
                }

                if (_bluetoothRadio == null)
                {
                    var radios = await Radio.GetRadiosAsync();
                    _bluetoothRadio = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
                }

                if (_bluetoothRadio != null)
                {
                    TxtAdapterName.Text = string.IsNullOrWhiteSpace(_bluetoothRadio.Name)
                        ? "Adaptador Bluetooth Windows"
                        : _bluetoothRadio.Name;

                    switch (_bluetoothRadio.State)
                    {
                        case RadioState.On:
                            TxtRadioStatus.Text = "LIGADO (Ativo)";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                            BtnTurnOnBluetooth.Visibility = Visibility.Collapsed;
                            BtnConnect.IsEnabled = true;
                            AppendLog($"[Hardware] Bluetooth ativo e funcional: {TxtAdapterName.Text}");
                            break;

                        case RadioState.Off:
                            TxtRadioStatus.Text = "DESLIGADO";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                            BtnTurnOnBluetooth.Visibility = Visibility.Visible;
                            BtnConnect.IsEnabled = false;
                            AppendLog("[Hardware] O Bluetooth está desligado. Carrega em 'Ligar Bluetooth'.");
                            break;

                        case RadioState.Disabled:
                            TxtRadioStatus.Text = "Desativado pelo Sistema";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
                            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B));
                            BtnTurnOnBluetooth.Visibility = Visibility.Collapsed;
                            BtnConnect.IsEnabled = false;
                            AppendLog("[Hardware] O rádio Bluetooth foi desativado nas definições de rede.");
                            break;

                        default:
                            TxtRadioStatus.Text = "Desconhecido";
                            TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
                            StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x94, 0xA3, 0xB8));
                            BtnTurnOnBluetooth.Visibility = Visibility.Visible;
                            BtnConnect.IsEnabled = true;
                            break;
                    }
                }
                else
                {
                    TxtAdapterName.Text = "Controlador Bluetooth Detetado";
                    TxtRadioStatus.Text = "Operacional";
                    TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                    StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                    BtnTurnOnBluetooth.Visibility = Visibility.Collapsed;
                    BtnConnect.IsEnabled = true;
                }
            }
            catch (Exception ex)
            {
                TxtAdapterName.Text = "Erro de Deteção";
                TxtRadioStatus.Text = ex.Message;
                TxtRadioStatus.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                StatusDot.Fill = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            }
            finally
            {
                BtnRefreshStatus.IsEnabled = true;
            }
        }

        private async void BtnTurnOnBluetooth_Click(object sender, RoutedEventArgs e)
        {
            if (_bluetoothRadio != null)
            {
                BtnTurnOnBluetooth.IsEnabled = false;
                try
                {
                    var result = await _bluetoothRadio.SetStateAsync(RadioState.On);
                    if (result == RadioAccessStatus.Allowed)
                    {
                        await Task.Delay(500);
                        await CheckBluetoothHardwareAsync();
                    }
                    else
                    {
                        OpenBluetoothSettings();
                    }
                }
                catch
                {
                    OpenBluetoothSettings();
                }
                finally
                {
                    BtnTurnOnBluetooth.IsEnabled = true;
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

        private async void BtnRefreshStatus_Click(object sender, RoutedEventArgs e)
        {
            await CheckBluetoothHardwareAsync();
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            BtnConnect.IsEnabled = false;
            BtnCancel.Visibility = Visibility.Visible;
            await _client.StartAsync();
            BtnConnect.IsEnabled = true;
            BtnCancel.Visibility = Visibility.Collapsed;
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            _client.Cancel();
            BtnConnect.IsEnabled = true;
            BtnCancel.Visibility = Visibility.Collapsed;
        }

        private void OnClientStateChanged(SessionState state, string message)
        {
            Dispatcher.Invoke(() =>
            {
                switch (state)
                {
                    case SessionState.Idle:
                        TxtStateHeadline.Text = "Pronto a Conectar";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8));
                        TxtStateDetail.Text = message;
                        BtnConnect.Content = "📡 Conectar e Receber";
                        break;

                    case SessionState.Scanning:
                        TxtStateHeadline.Text = "À Procura de Dispositivo...";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0xFA, 0xCC, 0x15));
                        TxtStateDetail.Text = "Mantém o Pokémon GO aberto no ecrã de conexão ou envio do postal.";
                        BtnConnect.Content = "🔍 A procurar...";
                        break;

                    case SessionState.Connecting:
                        TxtStateHeadline.Text = "A Ligar ao Telemóvel...";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0x38, 0xBD, 0xF8));
                        TxtStateDetail.Text = message;
                        BtnConnect.Content = "🔗 A conectar...";
                        break;

                    case SessionState.ReadingDevice:
                        TxtStateHeadline.Text = "A Ler Informações...";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0x81, 0x8C, 0xF8));
                        TxtStateDetail.Text = message;
                        break;

                    case SessionState.Pairing:
                        TxtStateHeadline.Text = "A Emparelhar Nintendo Switch...";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0xA8, 0x55, 0xF7));
                        TxtStateDetail.Text = message;
                        break;

                    case SessionState.Exchanging:
                        TxtStateHeadline.Text = "A Receber Postal...";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                        TxtStateDetail.Text = message;
                        break;

                    case SessionState.SuccessPair:
                        TxtStateHeadline.Text = "🎉 Emparelhado com Sucesso!";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                        TxtStateDetail.Text = message;
                        BtnConnect.Content = "📡 Conectar e Receber";
                        break;

                    case SessionState.SuccessExchange:
                        TxtStateHeadline.Text = "🎁 Postal Recebido! Coin Bag Ativo!";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0x10, 0xB9, 0x81));
                        TxtStateDetail.Text = message;
                        BtnConnect.Content = "📡 Conectar e Receber Novamente";
                        break;

                    case SessionState.Failed:
                        TxtStateHeadline.Text = "⚠️ Falha na Operação";
                        TxtStateHeadline.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
                        TxtStateDetail.Text = message;
                        BtnConnect.Content = "📡 Tentar Novamente";
                        break;
                }
            });
        }

        private void OnClientLog(string message)
        {
            Dispatcher.Invoke(() =>
            {
                AppendLog(message);
            });
        }

        private void AppendLog(string message)
        {
            string time = DateTime.Now.ToString("HH:mm:ss");
            TxtLog.AppendText($"[{time}] {message}\n");
            LogScrollViewer.ScrollToEnd();
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            TxtLog.Text = string.Empty;
        }
    }
}
