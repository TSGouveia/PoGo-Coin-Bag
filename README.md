# Pokémon GO Coin Bag & Switch Emulator Suite

Coleção de ferramentas e emuladores do protocolo Nintendo Switch (Pokémon Scarlet & Violet) para Pokémon GO, permitindo obter o **Coin Bag (Gimmighoul)** e postais.

## Módulos do Projeto

1. **`esp32-arduino/`**: Firmware ESP32 (C++/Arduino) em modo autónomo (liga e conecta automaticamente via BLE sem necessidade de botões).
2. **`android-app/`**: Aplicação móvel Android nativa (Kotlin + Jetpack Compose) com suporte multi-contas e histórico.
3. **`wear-os/`**: Aplicação para smartwatches Wear OS (Kotlin + Compose for Wear OS).
4. **`web/`**: Aplicação Web standalone (HTML/CSS/JS) com Web Bluetooth API para uso direto no Chrome/Edge.

## Como Usar
- Para usar no mesmo telemóvel onde joga Pokémon GO: use o **ESP32** (o Android não suporta loopback BLE local no mesmo rádio).
- Para usar entre dois aparelhos ou com smartwatch: use a app Android, Wear OS ou Web num segundo dispositivo.
