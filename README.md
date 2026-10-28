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
1. No telemóvel, abre o **Pokémon GO** e toca na Pokébola > **Definições** (canto superior direito).
2. Desce até **Dispositivos Conectados e Serviços** > seleciona **Nintendo Switch**.
3. Toca em **Conectar à Nintendo Switch** (o jogo fica à procura).
4. No computador, abre o site: **[https://na-ji.github.io/pokemon-go-coin-bag/](https://na-ji.github.io/pokemon-go-coin-bag/)**.
5. Clica no botão **Connect** e seleciona o teu telemóvel na lista do navegador. O jogo confirma o emparelhamento com sucesso!

#### Passo B: Enviar o Postal Diário
1. No telemóvel, abre o **Pokémon GO**, vai à Bolsa de Itens e entra no **Livro de Postais**.
2. Abre qualquer postal guardado.
3. Toca no botão de opções (canto inferior direito do postal) e escolhe **Enviar para a Nintendo Switch**.
4. No site no computador, clica em **Connect** e seleciona o teu telemóvel.
5. O postal é enviado e o **Coin Bag é ativado de imediato** na tua conta!

---

### Opção 2: Com ESP32 (Plug & Play)
*Requisitos: Uma placa ESP32 ligada a qualquer porta USB ou carregador.*

> 📥 **Download:** [`pogo-coin-bag-esp32-firmware.zip`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/esp32-v1.0.0/pogo-coin-bag-esp32-firmware.zip)

#### Passo A: Preparação
1. Descarrega o ficheiro zip da release e grava o código na tua placa ESP32 através do Arduino IDE.
2. Liga o ESP32 à tomada ou porta USB quando quiseres usar.

#### Passo B: Emparelhar (Apenas na 1.ª vez)
1. No telemóvel, abre o **Pokémon GO** e vai a **Definições** > **Dispositivos Conectados e Serviços** > **Nintendo Switch**.
2. Toca em **Conectar à Nintendo Switch** (o jogo fica à procura).
3. Liga ou reinicia o ESP32 (ou mantém-no perto). O ESP32 deteta o telemóvel e conclui o emparelhamento sozinho em poucos segundos.

#### Passo C: Enviar o Postal Diário
1. No telemóvel, abre o **Pokémon GO**, vai ao **Livro de Postais** e escolhe um postal.
2. Toca no botão de opções e escolhe **Enviar para a Nintendo Switch**.
3. O ESP32 deteta a transferência automaticamente. O LED acende fixo a confirmar o sucesso e o Coin Bag é ativado no jogo!

---

### Opção 3: App Android (Em telemóvel secundário)
*Requisitos: Um segundo telemóvel ou tablet Android com Bluetooth.*

> 📥 **Download:** [`pogo-coin-bag-android.apk`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/android-v1.0.0/pogo-coin-bag-android.apk)

#### Passo A: Instalação e Preparação
1. Instala e abre o ficheiro `.apk` no teu segundo telemóvel (aceita as permissões de Bluetooth e Dispositivos Próximos).
2. Mantém a app aberta no ecrã.

#### Passo B: Emparelhar (Apenas na 1.ª vez)
1. No teu telemóvel principal, abre o **Pokémon GO** e vai a **Definições** > **Dispositivos Conectados e Serviços** > **Nintendo Switch**.
2. Toca em **Conectar à Nintendo Switch** (o jogo fica à procura).
3. Na app do segundo telemóvel, clica no botão **Conectar** (iniciar procura). A ligação é concluída automaticamente!

#### Passo C: Enviar o Postal Diário
1. No telemóvel principal, abre o **Pokémon GO**, vai ao **Livro de Postais** e escolhe um postal.
2. Toca no botão de opções e escolhe **Enviar para a Nintendo Switch**.
3. Na app do segundo telemóvel, clica no botão **Conectar**.
4. O postal é transferido, a app confirma o sucesso e o Coin Bag é ativado no jogo!

---

### Opção 4: Smartwatch (Wear OS)
*Requisitos: Smartwatch com Wear OS (Galaxy Watch 4/5/6, Pixel Watch, etc.).*

> 📥 **Download:** [`pogo-coin-bag-wearos.apk`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/wear-os-v1.0.0/pogo-coin-bag-wearos.apk)

#### Passo A: Instalação e Preparação
1. Instala a app no teu smartwatch (usando ADB wireless ou apps como Wear Installer).
2. Abre a app no relógio e mantém o ecrã ativo.

#### Passo B: Emparelhar (Apenas na 1.ª vez)
1. No telemóvel, abre o **Pokémon GO** e vai a **Definições** > **Dispositivos Conectados e Serviços** > **Nintendo Switch**.
2. Toca em **Conectar à Nintendo Switch** (o jogo fica à procura).
3. No relógio, toca no botão **Conectar**. O emparelhamento inicial fica concluído de imediato!

#### Passo C: Enviar o Postal Diário
1. No telemóvel, vai ao **Livro de Postais** no Pokémon GO e escolhe um postal.
2. Toca no botão de opções e escolhe **Enviar para a Nintendo Switch**.
3. No relógio, toca no botão **Conectar**.
4. O relógio processa a transferência e o Coin Bag é ativado no jogo!

---

## 👏 Créditos

Este projeto baseia-se na implementação original desenvolvida por **[@na-ji](https://github.com/na-ji)** no repositório [pokemon-go-coin-bag](https://github.com/na-ji/pokemon-go-coin-bag). Todo o reconhecimento pelo trabalho de engenharia do protocolo pertence ao autor original.
