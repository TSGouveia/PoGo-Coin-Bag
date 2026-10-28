// ==============================================================================
// ESP32 PoGo Coin Bag (Emulador Nintendo Switch para Pokémon GO)
// Suporta:
//   - Modo 0x00: Emparelhamento da Nintendo Switch
//   - Modo 0x01: Troca / Envio de Postal (Coin Bag / Gimmighoul) com RSA-2048
// ==============================================================================

#include <Arduino.h>
#include <BLEDevice.h>
#include <BLEUtils.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>
#include <BLEClient.h>

#include "proto_types.h"
#include "mbedtls/bignum.h"
#include "mbedtls/sha256.h"
#include "rsa_keys.h"

#define TRIGGER_BUTTON_PIN 0  // Botão BOOT (GPIO 0)
#define STATUS_LED_PIN     2  // LED D2 integrado (GPIO 2)

// UUIDs Oficiais da comunicação Nintendo Switch <-> Pokémon GO
static const char* ADV_SERVICE_UUID      = "b6c3089a-7e4f-487d-b9f8-73a281dd718f";
static const char* GATT_SERVICE_UUID     = "abfaf4c7-5a44-445a-a007-ad19fcb29e99";

static const char* CHAR_READ_PROTOCOL    = "291e0889-cb21-4add-a587-3bb494ca2acf";
static const char* CHAR_READ_NAME        = "95c33c9a-9b76-4194-8e41-3b66aa8bb021";
static const char* CHAR_READ_MODE        = "8f860fd8-51e4-497d-afed-94253dbe2e0a";
static const char* CHAR_WRITE_APP_ID     = "28fdc6ec-b073-4e4a-bf7e-91af16c3e890";
static const char* CHAR_WRITE_INIT_NONCE = "2f8b06c2-0e80-45e5-a0dd-ad34c9f7ceb0";
static const char* CHAR_WRITE_COMPLETE   = "c5b318da-89e6-481f-98e4-b4e0a4203487";
static const char* CHAR_WRITE_FIN_NONCE  = "6324802e-5266-4f4a-95be-3a8f811874fc";
static const char* CHAR_INDICATE_DATA    = "000dd3aa-d916-4bc7-8a08-ae1a8a008e8a";
static const char* CHAR_INDICATE_STAGE   = "e35da264-384e-4db3-ae27-a1ba2b1f533d";

static const char* SWITCH_APP_ID_STR = "bd7a2c59-d7f7-542a-a316-9c94d3990307+titan";
static const uint8_t SHA256_DIGEST_INFO_PREFIX[] = {
    0x30, 0x31, 0x30, 0x0d, 0x06, 0x09, 0x60, 0x86, 0x48, 0x01, 0x65, 0x03, 0x04, 0x02, 0x01, 0x05, 0x00, 0x04, 0x20
};

BLEScan* pBLEScan = nullptr;
BLEAdvertisedDevice* targetDevice = nullptr;
volatile bool deviceFound = false;

// Buffer para receção das indicações BLE
static uint8_t indicationDataBuffer[1024];
static volatile size_t indicationDataLen = 0;
static volatile bool indicationDataReceived = false;

static volatile bool indicationStageReceived = false;

// Task para LED
volatile int currentBlinkIntervalMs = 0; // 0 = desligado, -1 = ligado fixo
TaskHandle_t ledTaskHandle = nullptr;

void ledControlTask(void* parameter) {
    bool ledState = false;
    while (true) {
        if (currentBlinkIntervalMs > 0) {
            ledState = !ledState;
            digitalWrite(STATUS_LED_PIN, ledState ? HIGH : LOW);
            vTaskDelay(pdMS_TO_TICKS(currentBlinkIntervalMs));
        } else if (currentBlinkIntervalMs == -1) {
            digitalWrite(STATUS_LED_PIN, HIGH);
            vTaskDelay(pdMS_TO_TICKS(100));
        } else {
            digitalWrite(STATUS_LED_PIN, LOW);
            vTaskDelay(pdMS_TO_TICKS(100));
        }
    }
}

void setLedStage(int intervalMs) {
    currentBlinkIntervalMs = intervalMs;
}

void indicateError() {
    currentBlinkIntervalMs = 0;
    for (int i = 0; i < 3; i++) {
        digitalWrite(STATUS_LED_PIN, HIGH);
        delay(100);
        digitalWrite(STATUS_LED_PIN, LOW);
        delay(100);
    }
}

// Callbacks de Scan BLE
class MyScanCallbacks: public BLEAdvertisedDeviceCallbacks {
    void onResult(BLEAdvertisedDevice advertisedDevice) {
        if (advertisedDevice.haveServiceUUID() && 
            advertisedDevice.isAdvertisingService(BLEUUID(ADV_SERVICE_UUID))) {
            Serial.printf("[BLE] Encontrado Pokémon GO: %s (RSSI: %d)\n", 
                          advertisedDevice.getAddress().toString().c_str(), advertisedDevice.getRSSI());
            targetDevice = new BLEAdvertisedDevice(advertisedDevice);
            deviceFound = true;
            pBLEScan->stop();
        }
    }
};

// Callbacks para as indicações (Indicate)
void dataIndicationCallback(BLERemoteCharacteristic* pBLERemoteCharacteristic, 
                            uint8_t* pData, size_t length, bool isNotify) {
    if (length > 0 && length <= sizeof(indicationDataBuffer)) {
        memcpy(indicationDataBuffer, pData, length);
        indicationDataLen = length;
        indicationDataReceived = true;
        Serial.printf("[BLE Indication] DATA recebida (%d bytes)\n", length);
    }
}

void stageIndicationCallback(BLERemoteCharacteristic* pBLERemoteCharacteristic, 
                             uint8_t* pData, size_t length, bool isNotify) {
    indicationStageReceived = true;
    Serial.printf("[BLE Indication] STAGE recebida (%d bytes)\n", length);
}

// ------------------------------------------------------------------------------
// PROTOCOLO & CRIPTOGRAFIA
// ------------------------------------------------------------------------------

// Gera Nonce periférico idêntico ao jogo e Web App: FNV-1a 64-bit de tick aleatório
String generateRandomPeripheralNonce() {
    uint64_t tick = ((uint64_t)esp_random() << 32) | esp_random();
    uint64_t value = 0xcbf29ce484222645ULL;
    for (int i = 0; i < 8; i++) {
        uint64_t octet = (tick >> (8 * i)) & 0xffULL;
        value ^= octet;
        value *= 0x100000001b3ULL;
    }
    char buf[20];
    snprintf(buf, sizeof(buf), "%llx", value);
    return String(buf);
}

// Protobuf helpers
size_t encodeVarint(uint64_t value, uint8_t* out) {
    size_t count = 0;
    while (value > 0x7f) {
        out[count++] = (uint8_t)((value & 0x7f) | 0x80);
        value >>= 7;
    }
    out[count++] = (uint8_t)(value & 0x7f);
    return count;
}

bool decodeVarint(const uint8_t* data, size_t len, size_t* offset, uint64_t* value) {
    *value = 0;
    int shift = 0;
    while (*offset < len && shift < 70) {
        uint8_t octet = data[(*offset)++];
        *value |= ((uint64_t)(octet & 0x7f)) << shift;
        if ((octet & 0x80) == 0) return true;
        shift += 7;
    }
    return false;
}


bool parseProtobuf(const uint8_t* data, size_t len, FieldRef* fields, size_t maxFields, size_t* numFields) {
    size_t offset = 0;
    *numFields = 0;
    while (offset < len) {
        uint64_t tag = 0;
        if (!decodeVarint(data, len, &offset, &tag)) return false;
        int fieldNumber = (int)(tag >> 3);
        int wireType = (int)(tag & 7);
        if (fieldNumber <= 0 || fieldNumber >= (int)maxFields) return false;

        fields[fieldNumber].wireType = wireType;
        if (wireType == 0) { // Varint
            if (!decodeVarint(data, len, &offset, &fields[fieldNumber].varintVal)) return false;
        } else if (wireType == 2) { // Length-delimited
            uint64_t fieldLen = 0;
            if (!decodeVarint(data, len, &offset, &fieldLen)) return false;
            if (offset + fieldLen > len) return false;
            fields[fieldNumber].bytesPtr = &data[offset];
            fields[fieldNumber].bytesLen = (size_t)fieldLen;
            offset += (size_t)fieldLen;
        } else {
            return false;
        }
    }
    return true;
}

// Assinatura RSA-2048 PKCS#1 v1.5 com SHA-256 usando mbedtls
bool calculateRsaSignature(const uint8_t* message, size_t messageLen, uint8_t* outSignature256) {
    // 1. SHA-256
    uint8_t hash[32];
    mbedtls_sha256(message, messageLen, hash, 0);

    // 2. PKCS#1 v1.5 padding (256 bytes)
    // 00 01 FF..FF 00 [DigestInfo 19 bytes] [Hash 32 bytes]
    uint8_t padded[256];
    padded[0] = 0x00;
    padded[1] = 0x01;
    size_t prefixLen = sizeof(SHA256_DIGEST_INFO_PREFIX);
    size_t padLen = 256 - 3 - prefixLen - 32;
    memset(&padded[2], 0xFF, padLen);
    padded[2 + padLen] = 0x00;
    memcpy(&padded[3 + padLen], SHA256_DIGEST_INFO_PREFIX, prefixLen);
    memcpy(&padded[3 + padLen + prefixLen], hash, 32);

    // 3. Modular Exponentiation: signature = padded^D mod N
    mbedtls_mpi M, D, N, S;
    mbedtls_mpi_init(&M);
    mbedtls_mpi_init(&D);
    mbedtls_mpi_init(&N);
    mbedtls_mpi_init(&S);

    int ret = 0;
    ret |= mbedtls_mpi_read_binary(&M, padded, 256);
    ret |= mbedtls_mpi_read_binary(&N, RSA_N, 256);
    ret |= mbedtls_mpi_read_binary(&D, RSA_D, 256);

    if (ret == 0) {
        ret = mbedtls_mpi_exp_mod(&S, &M, &D, &N, NULL);
    }

    if (ret == 0) {
        mbedtls_mpi_write_binary(&S, outSignature256, 256);
    }

    mbedtls_mpi_free(&M);
    mbedtls_mpi_free(&D);
    mbedtls_mpi_free(&N);
    mbedtls_mpi_free(&S);

    return (ret == 0);
}

// Descodifica o pacote de indicação recebido do Pokémon GO
bool decodeProtocolBleSend(const uint8_t* data, size_t len, uint64_t* outTxId, String* outNonce, String* outAppId) {
    FieldRef outer[5] = {0};
    size_t nOuter = 0;
    if (!parseProtobuf(data, len, outer, 5, &nOuter)) return false;
    if (outer[1].wireType != 2 || outer[1].bytesLen == 0) return false;

    FieldRef prep[6] = {0};
    size_t nPrep = 0;
    if (!parseProtobuf(outer[1].bytesPtr, outer[1].bytesLen, prep, 6, &nPrep)) return false;

    if (prep[2].wireType != 0) return false; // TransactionId (varint)
    *outTxId = prep[2].varintVal;

    if (prep[3].wireType != 2) return false; // ApplicationId
    *outAppId = String((const char*)prep[3].bytesPtr).substring(0, prep[3].bytesLen);

    if (prep[4].wireType != 2) return false; // Server Nonce
    *outNonce = String((const char*)prep[4].bytesPtr).substring(0, prep[4].bytesLen);

    return true;
}

// Constrói o pacote de conclusão assinado para enviar ao Pokémon GO
size_t buildCompletePayload(uint64_t transactionId, const String& serverNonce, uint8_t* outBuf) {
    // 1. Constrói o Share: field 1 = txId (varint), field 2 = serverNonce (string)
    uint8_t shareBuf[128];
    size_t shareLen = 0;
    shareBuf[shareLen++] = (1 << 3) | 0; // field 1, varint
    shareLen += encodeVarint(transactionId, &shareBuf[shareLen]);
    shareBuf[shareLen++] = (2 << 3) | 2; // field 2, length-delimited
    shareLen += encodeVarint(serverNonce.length(), &shareBuf[shareLen]);
    memcpy(&shareBuf[shareLen], serverNonce.c_str(), serverNonce.length());
    shareLen += serverNonce.length();

    // 2. Calcula assinatura RSA (256 bytes)
    uint8_t rsaSig[256];
    if (!calculateRsaSignature(shareBuf, shareLen, rsaSig)) {
        Serial.println("[ERRO] Falha ao calcular assinatura RSA!");
        return 0;
    }

    // 3. Monta Complete Protobuf:
    // field 1 (bytes): share
    // field 2 (bytes): rsaSig (256 bytes)
    // field 3 (bytes): firmwareSig (primeiros 16 bytes de rsaSig)
    size_t completeLen = 0;

    // Field 1: Share
    outBuf[completeLen++] = (1 << 3) | 2;
    completeLen += encodeVarint(shareLen, &outBuf[completeLen]);
    memcpy(&outBuf[completeLen], shareBuf, shareLen);
    completeLen += shareLen;

    // Field 2: RSA Signature (256 bytes)
    outBuf[completeLen++] = (2 << 3) | 2;
    completeLen += encodeVarint(256, &outBuf[completeLen]);
    memcpy(&outBuf[completeLen], rsaSig, 256);
    completeLen += 256;

    // Field 3: Firmware Signature (16 bytes)
    outBuf[completeLen++] = (3 << 3) | 2;
    completeLen += encodeVarint(16, &outBuf[completeLen]);
    memcpy(&outBuf[completeLen], rsaSig, 16);
    completeLen += 16;

    return completeLen;
}

// ------------------------------------------------------------------------------
// CICLO BLE PRINCIPAL
// ------------------------------------------------------------------------------
void setup() {
    Serial.begin(115200);
    pinMode(TRIGGER_BUTTON_PIN, INPUT_PULLUP);
    pinMode(STATUS_LED_PIN, OUTPUT);
    digitalWrite(STATUS_LED_PIN, LOW);

    xTaskCreatePinnedToCore(
        ledControlTask,
        "LedTask",
        2048,
        nullptr,
        1,
        &ledTaskHandle,
        0
    );
    
    Serial.println("\n==================================================");
    Serial.println("  ESP32 Nintendo Switch Emulator - PoGo Coin Bag  ");
    Serial.println("==================================================");
    Serial.println("Modo Automático: A procurar Pokémon GO continuamente...");

    BLEDevice::init("");
    pBLEScan = BLEDevice::getScan();
    pBLEScan->setAdvertisedDeviceCallbacks(new MyScanCallbacks());
    pBLEScan->setActiveScan(true);
    pBLEScan->setInterval(100);
    pBLEScan->setWindow(99);
}

void runBleCycle() {
    // =========================================================================
    // ETAPA 1: Procurar telemóvel (Scanning) -> LED LENTO (400ms)
    // =========================================================================
    Serial.println("\n[Etapa 1/4] A procurar dispositivo Pokémon GO... (LED Lento)");
    setLedStage(400);

    deviceFound = false;
    targetDevice = nullptr;
    pBLEScan->start(10, false);

    if (!deviceFound || targetDevice == nullptr) {
        Serial.println("[AVISO] Nenhum Pokémon GO encontrado no modo Switch.");
        indicateError();
        return;
    }

    // =========================================================================
    // ETAPA 2: A conectar ao telemóvel (Connecting) -> LED MÉDIO (200ms)
    // =========================================================================
    Serial.printf("[Etapa 2/4] A conectar a %s... (LED Médio)\n", targetDevice->getAddress().toString().c_str());
    setLedStage(200);

    BLEClient* pClient = BLEDevice::createClient();

    if (!pClient->connect(targetDevice)) {
        Serial.println("[ERRO] Falha ao conectar via BLE.");
        indicateError();
        delete targetDevice;
        targetDevice = nullptr;
        return;
    }

    // MTU essencial para o Pokémon GO enviar o pacote protobuf completo
    pClient->setMTU(517);
    delay(150);

    // =========================================================================
    // ETAPA 3: A ler serviços GATT e identificação -> LED RÁPIDO (90ms)
    // =========================================================================
    Serial.println("[Etapa 3/4] A descobrir serviços GATT...");
    setLedStage(90);

    BLERemoteService* pRemoteService = pClient->getService(BLEUUID(GATT_SERVICE_UUID));
    if (pRemoteService == nullptr) {
        Serial.println("[ERRO] Falha ao obter serviço GATT principal.");
        pClient->disconnect();
        indicateError();
        delete targetDevice;
        targetDevice = nullptr;
        return;
    }
    Serial.println("[BLE] Serviço GATT obtido!");

    BLERemoteCharacteristic* pCharProtocol  = pRemoteService->getCharacteristic(BLEUUID(CHAR_READ_PROTOCOL));
    BLERemoteCharacteristic* pCharName      = pRemoteService->getCharacteristic(BLEUUID(CHAR_READ_NAME));
    BLERemoteCharacteristic* pCharMode      = pRemoteService->getCharacteristic(BLEUUID(CHAR_READ_MODE));
    BLERemoteCharacteristic* pCharAppId     = pRemoteService->getCharacteristic(BLEUUID(CHAR_WRITE_APP_ID));
    BLERemoteCharacteristic* pCharInitNonce = pRemoteService->getCharacteristic(BLEUUID(CHAR_WRITE_INIT_NONCE));
    BLERemoteCharacteristic* pCharComplete  = pRemoteService->getCharacteristic(BLEUUID(CHAR_WRITE_COMPLETE));
    BLERemoteCharacteristic* pCharFinNonce  = pRemoteService->getCharacteristic(BLEUUID(CHAR_WRITE_FIN_NONCE));
    BLERemoteCharacteristic* pCharData      = pRemoteService->getCharacteristic(BLEUUID(CHAR_INDICATE_DATA));
    BLERemoteCharacteristic* pCharStage     = pRemoteService->getCharacteristic(BLEUUID(CHAR_INDICATE_STAGE));

    String playerName = "Desconhecido";
    uint8_t mode = 0;

    if (pCharName != nullptr && pCharName->canRead()) {
        playerName = pCharName->readValue();
        Serial.printf(">>> Treinador Identificado: %s <<<\n", playerName.c_str());
    }

    if (pCharMode != nullptr && pCharMode->canRead()) {
        String modeVal = pCharMode->readValue();
        if (modeVal.length() > 0) {
            mode = (uint8_t)modeVal[0];
            Serial.printf(">>> Modo Detetado: 0x%02X (%s) <<<\n", 
                          mode, (mode == 0 ? "Emparelhar Switch" : "Troca de Postal"));
        }
    }

    // =========================================================================
    // ETAPA 4: Operação (Emparelhamento ou Troca de Postal)
    // =========================================================================
    setLedStage(35); // LED ULTRA-RÁPIDO

    // Prepara Application ID de 69 bytes
    uint8_t appIdPayload[69] = {0};
    int appLen = strlen(SWITCH_APP_ID_STR);
    if (appLen > 69) appLen = 69;
    memcpy(appIdPayload, SWITCH_APP_ID_STR, appLen);

    if (mode == 0x00) {
        // --- MODO 0x00: EMPARELHAMENTO ---
        Serial.println("\n[Etapa 4/4] A emparelhar Nintendo Switch com Pokémon GO...");
        if (pCharAppId != nullptr) {
            delay(150);
            pCharAppId->writeValue(appIdPayload, 69, true); // Com resposta
            Serial.println(">>> Switch emparelhada com sucesso! <<<");
            Serial.println(">>> Agora vá ao Álbum de Postais no jogo e clique em 'Enviar para a Nintendo Switch'. <<<");
        } else {
            Serial.println("[ERRO] Característica AppId não encontrada.");
            indicateError();
            pClient->disconnect();
            delete targetDevice;
            targetDevice = nullptr;
            return;
        }
    } else {
        // --- MODO 0x01: TROCA DE POSTAL / COIN BAG ---
        Serial.println("\n[Etapa 4/4] A executar troca de postal (Coin Bag)...");

        // 1. Escreve Application ID (com resposta)
        if (pCharAppId != nullptr) {
            pCharAppId->writeValue(appIdPayload, 69, true);
            Serial.println("[1/5] Application ID enviado.");
        }

        // 2. Gera e escreve Initial Nonce (sem resposta, com terminação NUL)
        String peripheralNonce = generateRandomPeripheralNonce();
        Serial.printf("[BLE] Nonce periférico gerado: %s\n", peripheralNonce.c_str());

        if (pCharInitNonce != nullptr) {
            uint8_t initNonceBuf[32];
            size_t nLen = peripheralNonce.length();
            memcpy(initNonceBuf, peripheralNonce.c_str(), nLen);
            initNonceBuf[nLen] = 0x00; // byte NUL exigido pelo protocolo
            pCharInitNonce->writeValue(initNonceBuf, nLen + 1, false);
            Serial.println("[2/5] Initial Nonce enviado.");
        }

        // 3. Regista callback e ativa indicações em CHAR_INDICATE_DATA
        indicationDataReceived = false;
        indicationDataLen = 0;
        if (pCharData != nullptr && pCharData->canIndicate()) {
            pCharData->registerForNotify(dataIndicationCallback);
            // Escreve descritor CCCD (0x0002 para indicação)
            BLERemoteDescriptor* pDesc = pCharData->getDescriptor(BLEUUID((uint16_t)0x2902));
            if (pDesc != nullptr) {
                uint8_t val[2] = {0x02, 0x00};
                pDesc->writeValue(val, 2, true);
            }
            Serial.println("[3/5] Indicações DATA ativadas. A aguardar resposta do jogo...");
        } else {
            Serial.println("[ERRO] Falha ao registar indicações DATA.");
            indicateError();
            pClient->disconnect();
            delete targetDevice;
            targetDevice = nullptr;
            return;
        }

        // Aguarda indicação DATA com timeout de 15 segundos
        unsigned long startTime = millis();
        while (!indicationDataReceived && (millis() - startTime < 15000)) {
            delay(20);
        }

        if (!indicationDataReceived || indicationDataLen < 50) {
            Serial.printf("[ERRO] Timeout ou dados insuficientes na indicação (%d bytes recebidos).\n", indicationDataLen);
            indicateError();
            pClient->disconnect();
            delete targetDevice;
            targetDevice = nullptr;
            return;
        }

        // 4. Descodifica resposta e gera assinatura RSA
        uint64_t txId = 0;
        String serverNonce = "";
        String appIdResp = "";

        if (!decodeProtocolBleSend(indicationDataBuffer, indicationDataLen, &txId, &serverNonce, &appIdResp)) {
            Serial.println("[ERRO] Falha ao descodificar Protobuf recebido do jogo!");
            indicateError();
            pClient->disconnect();
            delete targetDevice;
            targetDevice = nullptr;
            return;
        }

        Serial.printf("[BLE] Transaction ID: %llu\n", txId);
        Serial.printf("[BLE] Server Nonce: %s\n", serverNonce.c_str());

        // Ativa STAGE indications antes de enviar o Complete
        indicationStageReceived = false;
        if (pCharStage != nullptr && pCharStage->canIndicate()) {
            pCharStage->registerForNotify(stageIndicationCallback);
            BLERemoteDescriptor* pDesc = pCharStage->getDescriptor(BLEUUID((uint16_t)0x2902));
            if (pDesc != nullptr) {
                uint8_t val[2] = {0x02, 0x00};
                pDesc->writeValue(val, 2, true);
            }
        }

        // Constrói e envia CHAR_WRITE_COMPLETE com RSA-2048
        uint8_t completePayload[512];
        size_t compLen = buildCompletePayload(txId, serverNonce, completePayload);
        if (compLen == 0 || pCharComplete == nullptr) {
            Serial.println("[ERRO] Falha ao construir pacote Complete.");
            indicateError();
            pClient->disconnect();
            delete targetDevice;
            targetDevice = nullptr;
            return;
        }

        pCharComplete->writeValue(completePayload, compLen, false); // Sem resposta
        Serial.printf("[4/5] Pacote RSA Complete enviado (%d bytes)!\n", compLen);

        // Aguarda brevemente a finalização
        delay(600);

        // 5. Envia Final Nonce (com resposta, sem terminação NUL)
        if (pCharFinNonce != nullptr) {
            pCharFinNonce->writeValue((uint8_t*)peripheralNonce.c_str(), peripheralNonce.length(), true);
            Serial.println("[5/5] Final Nonce enviado com sucesso!");
        }
    }

    // =========================================================================
    // SUCESSO: Concluído -> LED D2 FIXO (HOLD 5s)
    // =========================================================================
    Serial.println("\n=======================================================");
    Serial.println(">>> [SUCESSO TOTAL] Postal Processado / Coin Bag Ativo! <<<");
    Serial.println("=======================================================");
    Serial.println(">>> LED D2 FIXO durante 5 segundos <<<");
    setLedStage(-1);
    delay(5000);

    pClient->disconnect();
    delete targetDevice;
    targetDevice = nullptr;
    setLedStage(0);

    Serial.println("\n[Ciclo Concluído] A aguardar 5 segundos antes de reiniciar procura...");
    setLedStage(0);
    delay(5000);
}

void loop() {
    // Modo automático: corre o ciclo continuamente ao ligar (sem necessidade de pressionar botão)
    runBleCycle();
    delay(200);
}
