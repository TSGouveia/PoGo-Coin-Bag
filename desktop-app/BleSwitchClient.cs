using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace PoGoCoinBagWindows
{
    public enum SessionState
    {
        Idle,
        Scanning,
        Connecting,
        ReadingDevice,
        Pairing,
        Exchanging,
        SuccessPair,
        SuccessExchange,
        Failed
    }

    public class BleSwitchClient
    {
        private const int IndicationTimeoutMs = 15000;
        private const int SettleMs = 80;

        private BluetoothLEDevice? _device;
        private CancellationTokenSource? _cts;

        public event Action<SessionState, string>? StateChanged;
        public event Action<string>? LogMessage;

        private void Log(string message) => LogMessage?.Invoke(message);

        private string GetOrGenerateApplicationId()
        {
            try
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                var dir = Path.Combine(appData, "PoGoCoinBag");
                Directory.CreateDirectory(dir);
                var path = Path.Combine(dir, "application_id.txt");
                if (File.Exists(path))
                {
                    var id = File.ReadAllText(path).Trim();
                    if (!string.IsNullOrEmpty(id) && id.EndsWith(SwitchProtocol.ApplicationIdSuffix))
                    {
                        return id;
                    }
                }
                var newId = SwitchProtocol.GenerateApplicationId();
                File.WriteAllText(path, newId);
                return newId;
            }
            catch
            {
                return SwitchProtocol.GenerateApplicationId();
            }
        }

        public void Cancel()
        {
            _cts?.Cancel();
            _device?.Dispose();
            _device = null;
            StateChanged?.Invoke(SessionState.Idle, "Cancelado.");
        }

        public async Task StartAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            var ct = _cts.Token;

            try
            {
                StateChanged?.Invoke(SessionState.Scanning, "À procura do telemóvel...");
                Log("A iniciar scan BLE procurando o serviço do Pokémon GO...");

                var bluetoothAddress = await ScanForDeviceAsync(ct);
                Log($"Dispositivo encontrado! Endereço: {bluetoothAddress:X}");

                StateChanged?.Invoke(SessionState.Connecting, "A ligar ao telemóvel...");
                _device = await BluetoothLEDevice.FromBluetoothAddressAsync(bluetoothAddress);
                if (_device == null) throw new Exception("Falha ao obter dispositivo BLE.");

                var gatt = await _device.GetGattServicesForUuidAsync(SwitchProtocol.GattServiceUuid);
                if (gatt.Status != GattCommunicationStatus.Success || gatt.Services.Count == 0)
                {
                    throw new Exception("Serviço GATT do Pokémon GO não encontrado.");
                }
                var service = gatt.Services[0];

                StateChanged?.Invoke(SessionState.ReadingDevice, "A ler características...");
                var charsResult = await service.GetCharacteristicsAsync();
                if (charsResult.Status != GattCommunicationStatus.Success)
                {
                    throw new Exception("Falha ao ler características do serviço GATT.");
                }

                var chars = charsResult.Characteristics.ToDictionary(c => c.Uuid);

                // Read protocol characteristic
                if (!chars.TryGetValue(SwitchProtocol.ReadProtocolUuid, out var readProtocolChar))
                    throw new Exception("Característica ReadProtocol não encontrada.");
                var protoRead = await readProtocolChar.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (protoRead.Status != GattCommunicationStatus.Success)
                    throw new Exception("Falha ao ler versão do protocolo.");

                var protoBytes = protoRead.Value.ToArray();
                if (protoBytes.Length != 1 || protoBytes[0] != 0x01)
                    throw new Exception("Versão de protocolo móvel não suportada.");

                // Read player name
                if (!chars.TryGetValue(SwitchProtocol.ReadNameUuid, out var readNameChar))
                    throw new Exception("Característica ReadName não encontrada.");
                var nameRead = await readNameChar.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (nameRead.Status != GattCommunicationStatus.Success)
                    throw new Exception("Falha ao ler nome do jogador.");

                string playerName = SwitchProtocol.DecodePlayerName(nameRead.Value.ToArray());
                Log($"Treinador detetado: {playerName}");

                // Read mode
                if (!chars.TryGetValue(SwitchProtocol.ReadModeUuid, out var readModeChar))
                    throw new Exception("Característica ReadMode não encontrada.");
                var modeRead = await readModeChar.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (modeRead.Status != GattCommunicationStatus.Success)
                    throw new Exception("Falha ao ler modo do telemóvel.");

                var modeBytes = modeRead.Value.ToArray();
                if (modeBytes.Length != 1) throw new Exception("Valor de modo inválido.");
                byte mode = modeBytes[0];

                string applicationId = GetOrGenerateApplicationId();
                string nonce = SwitchProtocol.RandomPeripheralNonce();

                if (mode == 0x00)
                {
                    // PAIRING MODE
                    StateChanged?.Invoke(SessionState.Pairing, $"A emparelhar com {playerName}...");
                    Log("Modo de Emparelhamento detetado. A enviar Application ID...");
                    await Task.Delay(250, ct);

                    if (!chars.TryGetValue(SwitchProtocol.WriteApplicationIdUuid, out var writeAppIdChar))
                        throw new Exception("Característica WriteApplicationId não encontrada.");

                    var appIdGatt = SwitchProtocol.ApplicationIdGattValue(applicationId);
                    var status = await writeAppIdChar.WriteValueWithResultAsync(appIdGatt.AsBuffer());
                    if (status.Status != GattCommunicationStatus.Success)
                        throw new Exception($"Erro ao registar Switch: {status.Status}");

                    Log($"Emparelhado com sucesso com o treinador {playerName}!");
                    StateChanged?.Invoke(SessionState.SuccessPair, $"Nintendo Switch emparelhada com sucesso com {playerName}!");
                }
                else if (mode == 0x01)
                {
                    // EXCHANGE POSTCARD MODE
                    StateChanged?.Invoke(SessionState.Exchanging, $"A receber postal de {playerName}...");
                    Log("Modo de Transferência detetado! A processar postal...");

                    await PerformExchangeAsync(chars, applicationId, nonce, ct);

                    Log($"Postal recebido de {playerName}! Coin Bag ativado com sucesso!");
                    StateChanged?.Invoke(SessionState.SuccessExchange, $"Postal recebido com sucesso de {playerName}! O Coin Bag já está ativo no jogo!");
                }
                else
                {
                    throw new Exception($"Modo desconhecido do telemóvel: 0x{mode:X2}");
                }
            }
            catch (OperationCanceledException)
            {
                StateChanged?.Invoke(SessionState.Idle, "Operação cancelada.");
            }
            catch (Exception ex)
            {
                Log($"Erro: {ex.Message}");
                StateChanged?.Invoke(SessionState.Failed, ex.Message);
            }
            finally
            {
                _device?.Dispose();
                _device = null;
            }
        }

        private async Task PerformExchangeAsync(
            Dictionary<Guid, GattCharacteristic> chars,
            string applicationId,
            string nonce,
            CancellationToken ct)
        {
            var writeAppIdChar = chars[SwitchProtocol.WriteApplicationIdUuid];
            var writeInitialNonceChar = chars[SwitchProtocol.WriteInitialNonceUuid];
            var indicateDataChar = chars[SwitchProtocol.IndicateDataUuid];
            var indicateStageChar = chars[SwitchProtocol.IndicateStageUuid];
            var writeCompleteChar = chars[SwitchProtocol.WriteCompleteUuid];
            var writeFinalNonceChar = chars[SwitchProtocol.WriteFinalNonceUuid];

            // 1. Write ApplicationId
            var appIdGatt = SwitchProtocol.ApplicationIdGattValue(applicationId);
            await writeAppIdChar.WriteValueWithResultAsync(appIdGatt.AsBuffer());

            // 2. Write Initial Nonce
            var initialNonce = SwitchProtocol.InitialNonceGattValue(nonce);
            await writeInitialNonceChar.WriteValueAsync(initialNonce.AsBuffer(), GattWriteOption.WriteWithoutResponse);

            // 3. Setup Indication for IndicateData
            var dataQueue = new BlockingCollection<byte[]>();
            TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> onDataChanged = (s, e) =>
            {
                dataQueue.Add(e.CharacteristicValue.ToArray());
            };

            indicateDataChar.ValueChanged += onDataChanged;
            try
            {
                var indStatus = await indicateDataChar.WriteClientCharacteristicConfigurationDescriptorAsync(
                    GattClientCharacteristicConfigurationDescriptorValue.Indicate);
                if (indStatus != GattCommunicationStatus.Success)
                    throw new Exception("Falha ao ativar indicações de dados.");

                // Wait for first indication payload
                byte[] firstRaw = await Task.Run(() =>
                {
                    if (dataQueue.TryTake(out var item, IndicationTimeoutMs, ct)) return item;
                    throw new TimeoutException("Tempo limite esgotado à espera dos dados do postal.");
                }, ct);

                if (firstRaw.Length < 303)
                    throw new Exception($"Payload incompleto ({firstRaw.Length} bytes).");

                var send = SwitchProtocol.DecodeProtocolBleSend(firstRaw);
                if (send.ServerResponse.ApplicationId != applicationId || send.ServerResponse.Nonce != nonce)
                    throw new Exception("Incoerência nos identificadores de sessão da resposta.");

                // 4. Encode complete response
                var complete = SwitchProtocol.EncodeProtocolComplete(
                    send.ServerResponse.TransactionId,
                    send.ServerResponse.Nonce);

                // 5. Setup stage indication & write complete
                var stageQueue = new BlockingCollection<byte[]>();
                TypedEventHandler<GattCharacteristic, GattValueChangedEventArgs> onStageChanged = (s, e) =>
                {
                    stageQueue.Add(e.CharacteristicValue.ToArray());
                };

                indicateStageChar.ValueChanged += onStageChanged;
                try
                {
                    await indicateStageChar.WriteClientCharacteristicConfigurationDescriptorAsync(
                        GattClientCharacteristicConfigurationDescriptorValue.Indicate);

                    await writeCompleteChar.WriteValueAsync(complete.AsBuffer(), GattWriteOption.WriteWithoutResponse);

                    // Wait for final indication from data or stage
                    byte[]? finalRaw = await Task.Run(() =>
                    {
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                        linkedCts.CancelAfter(IndicationTimeoutMs);

                        while (!linkedCts.IsCancellationRequested)
                        {
                            if (dataQueue.TryTake(out var d, 100)) return d;
                            if (stageQueue.TryTake(out var s, 100)) return s;
                        }
                        return null;
                    }, ct);

                    if (finalRaw != null)
                    {
                        var finalNonceVal = SwitchProtocol.FinalNonceGattValue(nonce);
                        await writeFinalNonceChar.WriteValueWithResultAsync(finalNonceVal.AsBuffer());

                        var finalize = SwitchProtocol.DecodeProtocolBleFinalize(finalRaw);
                        if (finalize.ApplicationId != applicationId || finalize.Nonce != nonce)
                            throw new Exception("Incoerência na finalização do postal.");
                    }
                }
                finally
                {
                    indicateStageChar.ValueChanged -= onStageChanged;
                }
            }
            finally
            {
                indicateDataChar.ValueChanged -= onDataChanged;
            }
        }

        private Task<ulong> ScanForDeviceAsync(CancellationToken ct)
        {
            var tcs = new TaskCompletionSource<ulong>();
            var watcher = new BluetoothLEAdvertisementWatcher
            {
                ScanningMode = BluetoothLEScanningMode.Active
            };

            watcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(SwitchProtocol.AdvertisementServiceUuid);

            watcher.Received += (s, e) =>
            {
                watcher.Stop();
                tcs.TrySetResult(e.BluetoothAddress);
            };

            ct.Register(() =>
            {
                watcher.Stop();
                tcs.TrySetCanceled();
            });

            watcher.Start();
            return tcs.Task;
        }
    }
}
