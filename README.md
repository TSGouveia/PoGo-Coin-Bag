<div align="center">

# 🪙 PoGo Coin Bag

### Nintendo Switch Emulator for Pokémon GO

Activate the **Coin Bag**, catch **Roaming Form Gimmighoul**, collect **Gimmighoul Coins**, and earn **Golden Lure Modules** daily in Pokémon GO without needing a physical console.

</div>

---

## 🎁 In-Game Rewards

By sending a daily postcard, you automatically receive in Pokémon GO:

* 🎒 **Coin Bag:** Activates a special 30-minute incense that attracts Roaming Form Gimmighoul.
* 🪙 **Gimmighoul Coins:** Earn coins by catching Gimmighoul and spinning Golden PokéStops (999 coins are needed to evolve into **Gholdengo**).
* 🟨 **Golden Lure Module:** Receive 1 Golden Lure Module every 5 postcards sent.

---

## 📋 Step-by-Step Guide

> [!WARNING]
> **Autocatcher Warning:** If you have an active or connected autocatcher device, the connection to the Nintendo Switch emulator may fail. It is strongly recommended to temporarily disconnect any autocatcher before pairing or sending postcards.

To receive rewards, simply pick **one** of the available platforms below.

---

### Option 1: In the Browser (Web)
*Requirements: PC or Mac with Bluetooth turned on and Google Chrome, Microsoft Edge, or Opera.*

#### Step A: Pairing (First time only)
1. On your phone, open **Pokémon GO**, tap the Poké Ball > **Settings** (top right).
2. Scroll down to **Connected Devices and Services** > select **Nintendo Switch**.
3. Tap **Connect to Nintendo Switch** (the game begins searching).
4. On your computer, open: **[https://na-ji.github.io/pokemon-go-coin-bag/](https://na-ji.github.io/pokemon-go-coin-bag/)**.
5. Click the **Connect** button and select your phone from the browser list. The game confirms successful pairing!

#### Step B: Send Daily Postcard
1. On your phone, open **Pokémon GO**, go to your Item Bag, and open the **Postcard Book**.
2. Open any saved postcard.
3. Tap the options menu (bottom right of the postcard) and choose **Send to Nintendo Switch**.
4. On the computer website, click **Connect** and select your phone.
5. The postcard will be sent and the **Coin Bag is immediately activated** in your account!

---

### Option 2: Windows Desktop App (.exe)
*Requirements: Windows 10/11 PC with Bluetooth.*

> 📥 **Download:** [`PoGoCoinBag.exe`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/windows-v1.0.0/PoGoCoinBag.exe)

A modern desktop assistant for Windows that checks your Bluetooth driver/adapter, tells you whether Bluetooth is turned on, lets you turn it on or open Windows Bluetooth settings with one click, and launches the web console directly.

#### Step A: Setup & Diagnostics
1. Download and run **`PoGoCoinBag.exe`**.
2. The app automatically detects your Bluetooth hardware and status:
   - If Bluetooth is **OFF**, click **⚡ Turn on Bluetooth** or open Windows settings directly from the app.
   - If Bluetooth is **ON**, you're ready to proceed!

#### Step B: Pairing & Sending Postcard
1. On your phone, open **Pokémon GO** and follow the Switch pairing or postcard send screen.
2. Click **🚀 Open Web Console** inside the desktop app.
3. Select your phone and the Coin Bag will be activated!

---

### Option 3: Using ESP32 (Plug & Play)
*Requirements: An ESP32 development board plugged into any USB port or charger.*

> 📥 **Download:** [`esp32_pogo_coin_bag.zip`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/esp32-v1.0.0/esp32_pogo_coin_bag.zip)

#### Step A: Setup
1. Download the zip file from the release and flash the code to your ESP32 board using Arduino IDE.
2. Plug the ESP32 into a power outlet or USB port whenever you want to use it.

#### Step B: Pairing (First time only)
1. On your phone, open **Pokémon GO** and go to **Settings** > **Connected Devices and Services** > **Nintendo Switch**.
2. Tap **Connect to Nintendo Switch** (the game begins searching).
3. Power on or reset the ESP32 (or hold it nearby). The ESP32 will detect the phone and complete pairing automatically within seconds.

#### Step C: Send Daily Postcard
1. On your phone, open **Pokémon GO**, open the **Postcard Book**, and pick a postcard.
2. Tap the options menu and choose **Send to Nintendo Switch**.
3. The ESP32 will automatically detect the transfer. The LED turns solid to confirm success, and the Coin Bag is activated in the game!

---

### Option 4: Android App (On a secondary phone)
*Requirements: A secondary Android phone or tablet with Bluetooth.*

> 📥 **Download:** [`pogo-coin-bag-android.apk`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/android-v1.0.0/pogo-coin-bag-android.apk)

#### Step A: Installation & Setup
1. Install and open the `.apk` on your secondary phone (grant Bluetooth and Nearby Devices permissions).
2. Keep the app open on screen.

#### Step B: Pairing (First time only)
1. On your primary phone, open **Pokémon GO** and go to **Settings** > **Connected Devices and Services** > **Nintendo Switch**.
2. Tap **Connect to Nintendo Switch** (the game begins searching).
3. In the app on the secondary phone, tap the **Connect** button (starts scan). The connection completes automatically!

#### Step C: Send Daily Postcard
1. On your primary phone, open **Pokémon GO**, open the **Postcard Book**, and choose a postcard.
2. Tap the options menu and choose **Send to Nintendo Switch**.
3. In the app on the secondary phone, tap the **Connect** button.
4. The postcard transfers, the app confirms success, and the Coin Bag is activated in the game!

---

### Option 5: Smartwatch (Wear OS)
*Requirements: Wear OS smartwatch (Galaxy Watch 4/5/6, Pixel Watch, etc.).*

> 📥 **Download:** [`pogo-coin-bag-wearos.apk`](https://github.com/TSGouveia/PoGo-Coin-Bag/releases/download/wear-os-v1.0.0/pogo-coin-bag-wearos.apk)

#### Step A: Installation & Setup
1. Sideload the app onto your smartwatch (using wireless ADB or tools like Wear Installer).
2. Open the app on your watch and keep the screen active.

#### Step B: Pairing (First time only)
1. On your phone, open **Pokémon GO** and go to **Settings** > **Connected Devices and Services** > **Nintendo Switch**.
2. Tap **Connect to Nintendo Switch** (the game begins searching).
3. On your watch, tap the **Connect** button. Initial pairing is completed immediately!

#### Step C: Send Daily Postcard
1. On your phone, go to the **Postcard Book** in Pokémon GO and choose a postcard.
2. Tap the options menu and choose **Send to Nintendo Switch**.
3. On your watch, tap the **Connect** button.
4. The watch handles the transfer and the Coin Bag is activated in the game!

---

## 👏 Credits

This project is based on the original implementation developed by **[@na-ji](https://github.com/na-ji)** in the repository [pokemon-go-coin-bag](https://github.com/na-ji/pokemon-go-coin-bag). All credit for protocol reverse engineering belongs to the original author.
