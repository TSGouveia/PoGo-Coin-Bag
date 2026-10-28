<div align="center">

# 🪙 PoGo Coin Bag

### Emulador de Nintendo Switch para Pokémon GO

Ativa o **Coin Bag (Saco de Moedas)**, apanha o **Gimmighoul**, junta **Gimmighoul Coins** e ganha **Golden Lure Modules** diariamente no Pokémon GO sem precisares de uma consola física.

---

[![GitHub Release](https://img.shields.io/github/v/release/TSGouveia/PoGo-Coin-Bag?style=for-the-badge&logo=github&color=blue)](https://github.com/TSGouveia/PoGo-Coin-Bag/releases)
[![Web App](https://img.shields.io/badge/Web_App-na--ji.github.io-success?style=for-the-badge&logo=googlechrome)](https://na-ji.github.io/pokemon-go-coin-bag/)

</div>

---

## 🎁 Recompensas no Jogo

Ao enviares um postal diário, recebes automaticamente no Pokémon GO:

* 🎒 **Coin Bag (Saco de Moedas):** Ativa um incenso especial de 30 minutos que atrai a forma ambulante de Gimmighoul.
* 🪙 **Gimmighoul Coins:** Ganhas moedas ao apanhar Gimmighouls e ao girar PokéStops douradas (são necessárias 999 moedas para evoluir para **Gholdengo**).
* 🟨 **Golden Lure Module:** Recebes 1 Módulo Atraente Dourado a cada 5 postais enviados.

---

## 📋 Guia Passo a Passo

Para receberes as recompensas, só precisas de escolher **uma** das plataformas abaixo.

---

### Opção 1: No Browser (Web)
*Requisitos: PC ou Mac com Bluetooth ligado e navegador Google Chrome, Microsoft Edge ou Opera.*

#### Passo A: Emparelhar (Apenas na 1.ª vez)
1. No computador, abre o site: **[https://na-ji.github.io/pokemon-go-coin-bag/](https://na-ji.github.io/pokemon-go-coin-bag/)**.
2. No telemóvel, abre o **Pokémon GO** e toca na Pokébola > **Definições** (canto superior direito).
3. Desce até **Dispositivos Conectados e Serviços** > seleciona **Nintendo Switch**.
4. Toca em **Conectar à Nintendo Switch**.
5. No computador, clica no botão **Connect** do site e escolhe o teu telemóvel na janela que surgir. O jogo confirma a ligação com sucesso.

#### Passo B: Enviar o Postal Diário
1. No Pokémon GO, abre a tua Bolsa de Itens e entra no **Livro de Postais**.
2. Abre qualquer postal que tenhas guardado.
3. Toca no botão de opções (canto inferior direito do postal) e escolhe **Enviar para a Nintendo Switch**.
4. No site no computador, clica novamente em **Connect** e seleciona o telemóvel.
5. O postal é transferido e o **Coin Bag é ativado de imediato** na tua conta!

---

### Opção 2: Com ESP32 (Plug & Play)
*Requisitos: Uma placa ESP32 ligada a qualquer porta USB ou carregador.*

> 📥 **Download:** [`pogo-coin-bag-esp32-firmware.zip`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/esp32-v1.0.0/pogo-coin-bag-esp32-firmware.zip)

#### Passo A: Preparação
1. Descarrega o ficheiro zip da release e grava o código na tua placa ESP32 através do Arduino IDE.
2. Liga o ESP32 à tomada ou a uma porta USB. Ele inicia logo o modo de procura contínuo automaticamente.

#### Passo B: Emparelhar (Apenas na 1.ª vez)
1. No Pokémon GO, vai a **Definições** > **Dispositivos Conectados e Serviços** > **Nintendo Switch**.
2. Toca em **Conectar à Nintendo Switch**.
3. O ESP32 deteta o telemóvel e conclui o emparelhamento sozinho em poucos segundos.

#### Passo C: Enviar o Postal Diário
1. Vai ao teu **Livro de Postais** no Pokémon GO e escolhe um postal.
2. Toca nas opções do postal e seleciona **Enviar para a Nintendo Switch**.
3. O ESP32 recebe a transferência automaticamente. O LED da placa acende fixo a confirmar o sucesso e recebes o Coin Bag no jogo.

---

### Opção 3: App Android (Em telemóvel secundário)
*Requisitos: Um segundo telemóvel ou tablet Android com Bluetooth.*

> 📥 **Download:** [`pogo-coin-bag-android.apk`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/android-v1.0.0/pogo-coin-bag-android.apk)

#### Passo A: Instalação e Preparação
1. Instala e abre o ficheiro `.apk` no teu segundo telemóvel (aceita as permissões de Bluetooth / Dispositivos Próximos).
2. Deixa a app aberta no ecrã.

#### Passo B: Emparelhar (Apenas na 1.ª vez)
1. No teu telemóvel principal (onde jogas Pokémon GO), abre o jogo e vai a **Definições** > **Dispositivos Conectados e Serviços** > **Nintendo Switch**.
2. Toca em **Conectar à Nintendo Switch** para emparelhar com a app.

#### Passo C: Enviar o Postal Diário
1. No Pokémon GO, abre a tua Bolsa de Itens e entra no **Livro de Postais**.
2. Escolhe qualquer postal guardado, toca no botão de opções e seleciona **Enviar para a Nintendo Switch**.
3. A app recebe a transferência, regista no histórico e o Coin Bag é ativado no jogo!

---

### Opção 4: Smartwatch (Wear OS)
*Requisitos: Smartwatch com Wear OS (Galaxy Watch 4/5/6, Pixel Watch, etc.).*

> 📥 **Download:** [`pogo-coin-bag-wearos.apk`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/wear-os-v1.0.0/pogo-coin-bag-wearos.apk)

#### Passo A: Instalação e Preparação
1. Instala a app no teu smartwatch (usando ADB wireless ou apps como Wear Installer).
2. Abre a app no relógio e mantém-na no ecrã.

#### Passo B: Emparelhar (Apenas na 1.ª vez)
1. No Pokémon GO do teu telemóvel, vai a **Definições** > **Dispositivos Conectados e Serviços** > **Nintendo Switch**.
2. Toca em **Conectar à Nintendo Switch** para concluir a ligação inicial.

#### Passo C: Enviar o Postal Diário
1. No telemóvel, vai ao **Livro de Postais** no Pokémon GO e escolhe um postal.
2. Toca nas opções do postal e seleciona **Enviar para a Nintendo Switch**.
3. O relógio processa o envio e o Coin Bag é ativado no jogo!

---

## 👏 Créditos

Este projeto baseia-se na implementação original desenvolvida por **[@na-ji](https://github.com/na-ji)** no repositório [pokemon-go-coin-bag](https://github.com/na-ji/pokemon-go-coin-bag). Todo o reconhecimento pelo trabalho de engenharia do protocolo pertence ao autor original.
