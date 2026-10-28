# ESP32 Pokémon GO Coin Bag & Switch Emulator

Firmware para ESP32 compatível com a framework Arduino / PlatformIO que replica o comportamento da Nintendo Switch para obter o **Coin Bag (Gimmighoul)** e trocar postais com o Pokémon GO.

## Características
1. **Modo Automático contínuo:** Liga-se e funciona logo ao ser alimentado via USB/bateria (sem necessidade de pressionar botões). Faz scan constante e conecta-se automaticamente ao Pokémon GO quando solicitado no jogo.
2. **Identificação do Dispositivo:** Faz a leitura das características GATT `readName` e `readMode` para identificar o nome do treinador do Pokémon GO e o estado atual antes de concluir o handshake.
3. **Hardware Recomendado:**
   - ESP32 WROOM-32, ESP32-WROVER ou ESP32-S3 (com BLE nativo).
   - *Nota:* Arduinos clássicos de 8-bits (Uno, Mega, Nano clássico) não têm memória RAM nem suporte a criptografia RSA-2048 suficiente.

## Como Gravar no ESP32
1. Abre o [Arduino IDE](https://www.arduino.cc/en/software).
2. Adiciona o suporte para ESP32 em **Tools > Board > Boards Manager** (procura por `esp32` da Espressif).
3. Seleciona a tua placa (ex: `ESP32 Dev Module`).
4. Abre o ficheiro `esp32_pogo_coin_bag.ino` e clica em **Upload**.
5. Abre o Serial Monitor a **115200 baud** para ver os logs e o nome do treinador detetado.
