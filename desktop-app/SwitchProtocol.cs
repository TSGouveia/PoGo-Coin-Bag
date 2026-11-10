using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PoGoCoinBagWindows
{
    public class ProtoField
    {
        public long? VarintVal { get; set; }
        public byte[]? BytesVal { get; set; }
    }

    public record ServerResponse(long Data, long TransactionId, string ApplicationId, string Nonce);
    public record ProtocolBleSend(ServerResponse ServerResponse, byte[] ServerSignature);
    public record ProtocolBleFinalize(string Nonce, string ApplicationId, byte[] ServerSignature);

    public static class SwitchProtocol
    {
        private const string CONSTANTS_BASE64 = @"
eyJVVUlEUyI6eyJhZHZlcnRpc2VtZW50U2VydmljZSI6ImI2YzMwODlhLTdlNGYtNDg3ZC1iOWY4LTczYTI4MWRkNzE4ZiIsImdh
dHRTZXJ2aWNlIjoiYWJmYWY0YzctNWE0NC00NDVhLWEwMDctYWQxOWZjYjI5ZTk5IiwicmVhZFByb3RvY29sIjoiMjkxZTA4ODkt
Y2IyMS00YWRkLWE1ODctM2JiNDk0Y2EyYWNmIiwicmVhZE5hbWUiOiI5NWMzM2M5YS05Yjc2LTQxOTQtOGU0MS0zYjY2YWE4YmIw
MjEiLCJyZWFkTW9kZSI6IjhmODYwZmQ4LTUxZTQtNDk3ZC1hZmVkLTk0MjUzZGJlMmUwYSIsIndyaXRlQXBwbGljYXRpb25JZCI6
IjI4ZmRjNmVjLWIwNzMtNGU0YS1iZjdlLTkxYWYxNmMzZTg5MCIsIndyaXRlSW5pdGlhbE5vbmNlIjoiMmY4YjA2YzItMGU4MC00
NWU1LWEwZGQtYWQzNGM5ZjdjZWIwIiwid3JpdGVDb21wbGV0ZSI6ImM1YjMxOGRhLTg5ZTYtNDgxZi05OGU0LWI0ZTBhNDIwMzQ4
NyIsIndyaXRlRmluYWxOb25jZSI6IjYzMjQ4MDJlLTUyNjYtNGY0YS05NWJlLTNhOGY4MTE4NzRmYyIsImluZGljYXRlRGF0YSI6
IjAwMGRkM2FhLWQ5MTYtNGJjNy04YTA4LWFlMWE4YTAwOGU4YSIsImluZGljYXRlU3RhZ2UiOiJlMzVkYTI2NC0zODRlLTRkYjMt
YWUyNy1hMWJhMmIxZjUzM2QifSwiQVBQTElDQVRJT05fSURfU1VGRklYIjoiK3RpdGFuIiwiQVBQTElDQVRJT05fSURfR0FUVF9X
SURUSCI6NjksIlRFWFRfRU5DT0RJTkciOiJ1dGYtOCIsIlRFWFRfREVDT0RFUl9GQVRBTCI6dHJ1ZSwiUlNBX01PRFVMVVMiOiIw
eDAwY2Y5ZWRiMmFiMjE0YjYzNmYzYTRlYTYzZjc2ZTgyZjIwNjdiNzQ2NTYzYjM3YjY4ZDAxZmNiNTI3MmMxMzA3YTIyMmFiN2Q5
YWM1ZDhlOTA5YmMwMjA2MWIwNzEwNTk1Mjk0MzE1NzRjY2E3NWFkM2Y2Mzk5NzI5NGFjY2YxYzM1ZWQ5ZDc5YjE4ZjdkYTkwZWVm
ZjI0NmUzYWM4MDZmNTdmNDBlNGNiZTQyMWZkZWQ1ODY2OTU5MWJlNmE4OTQ4ZTYzMDQ3NDYyMjY3NmM4MjJhMDQ4MmJlY2YyYzkz
ZmZiZTE1M2FlMWExMzI5MmFjMTcxMzkzNDk1OWM3YTY2OTVkNTNkN2NmYWY4YjdmMDAxYzIyMmY3NTU0YTFhOTMzMDliY2EwODVm
ZjM4NDRmZTM3ZDE4ODhlNzZmNmQzNmI3YTU5ZjBiZDNjZTFiZTc5NTgzYmUxYzNhNjBjN2RiMzk2OTQxZDhkN2E4YTQwNmQzMjAy
ODEwZDkzZTRmYzU5ZWYyYmVkMDJlMWUyN2I2ODIyOGY1MTVmOWMyYzJjMmE1Mzg2NDE5NDEyNzkxMjgzMzc0MDRiODVkNmUzNTY0
NDdhNjZiYmNmNTM2NTNkMjFmMjY3NjE5MzJmMDExYzAwZTE2YzU3MGZjODI5NzdmZjIwOWFkOTZmNjVlY2YxZDciLCJSU0FfUFJJ
VkFURV9FWFBPTkVOVCI6IjB4NDk1ZWUyMjRkYzc5ZTU0YWQ2MWY1OTk1YWE0MzJiOTM0YzI3MjRhMzBmZjBjNTkzZWNiOGNmYTk3
YjU0M2E0NTZlZmQ5OWFiMWFmNjk1MDMxNTg3NzdlYTBkZTg2MzA4YWI2NDQxOThkZDE5ZTc0NzMwYTQ3OTdlZTYyODM2ZTdjYzA1
M2ZlNDU3OTY3ZjZlZTg4NmQxZTEwMjc0ZGRjMGI0MGZjMmNiMmFmYmI4MzhjMDFjYzA4ZTk4YjQxZDdmZGQ5OTAyMTcwZWUxNmRi
ZjMyMGExNjBhNDgzYzgwYjBjODAzYzhhMzIzY2MzMzc5MGQ0ZWE1YTYyZTRjMTdjYTUxNjNmYTAxZDYzZDY1MzQyOWMzMDljNjQ1
MmMwMGYwZTE5ZmQ4YWZhNzZkMzE2Zjg5OTc5MGE1M2Q5ZTI5ODkxYWM1ZTk5MjI0OTQ3NTQ0MzUyNjI3MWExMWVhNWE2OGY4MDMx
OTBhODk2ZDRlZmM1ZmEzM2RjYWQzZTFmNjE1NWUyNTMxNjU0NzU3ZjczN2UxNTgxNGQxNmYzOTZlYjI1ODMzYzUxOGZiZWI1M2Zj
YzM5Mzk0NTEwYTc5YjA3YTM4NGZjMjZhMGVmNGYzNTI0YmE2MDYyYWViYzA2ZmEyNmJiZTZmY2FhMTQzNTMwNDkwNDQ5OGVkNWM5
YTQyYzEiLCJTSEEyNTZfRElHRVNUX0lORk9fUFJFRklYIjoiMzAzMTMwMGQwNjA5NjA4NjQ4MDE2NTAzMDQwMjAxMDUwMDA0MjAi
fQ==
";

        public static readonly Guid AdvertisementServiceUuid;
        public static readonly Guid GattServiceUuid;
        public static readonly Guid ReadProtocolUuid;
        public static readonly Guid ReadNameUuid;
        public static readonly Guid ReadModeUuid;
        public static readonly Guid WriteApplicationIdUuid;
        public static readonly Guid WriteInitialNonceUuid;
        public static readonly Guid WriteCompleteUuid;
        public static readonly Guid WriteFinalNonceUuid;
        public static readonly Guid IndicateDataUuid;
        public static readonly Guid IndicateStageUuid;

        public static readonly string ApplicationIdSuffix;
        public static readonly int ApplicationIdGattWidth;
        private static readonly BigInteger RsaModulus;
        private static readonly BigInteger RsaPrivateExponent;
        private static readonly byte[] Sha256DigestInfoPrefix;

        static SwitchProtocol()
        {
            var raw = Convert.FromBase64String(CONSTANTS_BASE64.Replace("\r", "").Replace("\n", "").Trim());
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            var uuids = root.GetProperty("UUIDS");

            AdvertisementServiceUuid = Guid.Parse(uuids.GetProperty("advertisementService").GetString()!);
            GattServiceUuid = Guid.Parse(uuids.GetProperty("gattService").GetString()!);
            ReadProtocolUuid = Guid.Parse(uuids.GetProperty("readProtocol").GetString()!);
            ReadNameUuid = Guid.Parse(uuids.GetProperty("readName").GetString()!);
            ReadModeUuid = Guid.Parse(uuids.GetProperty("readMode").GetString()!);
            WriteApplicationIdUuid = Guid.Parse(uuids.GetProperty("writeApplicationId").GetString()!);
            WriteInitialNonceUuid = Guid.Parse(uuids.GetProperty("writeInitialNonce").GetString()!);
            WriteCompleteUuid = Guid.Parse(uuids.GetProperty("writeComplete").GetString()!);
            WriteFinalNonceUuid = Guid.Parse(uuids.GetProperty("writeFinalNonce").GetString()!);
            IndicateDataUuid = Guid.Parse(uuids.GetProperty("indicateData").GetString()!);
            IndicateStageUuid = Guid.Parse(uuids.GetProperty("indicateStage").GetString()!);

            ApplicationIdSuffix = root.GetProperty("APPLICATION_ID_SUFFIX").GetString()!;
            ApplicationIdGattWidth = root.GetProperty("APPLICATION_ID_GATT_WIDTH").GetInt32();

            var modHex = root.GetProperty("RSA_MODULUS").GetString()!.Replace("0x", "");
            RsaModulus = BigInteger.Parse("0" + modHex, System.Globalization.NumberStyles.HexNumber);

            var expHex = root.GetProperty("RSA_PRIVATE_EXPONENT").GetString()!.Replace("0x", "");
            RsaPrivateExponent = BigInteger.Parse("0" + expHex, System.Globalization.NumberStyles.HexNumber);

            var digestHex = root.GetProperty("SHA256_DIGEST_INFO_PREFIX").GetString()!;
            Sha256DigestInfoPrefix = HexToBytes(digestHex);
        }

        public static byte[] ConcatBytes(params byte[][] parts)
        {
            int total = 0;
            foreach (var p in parts) total += p.Length;
            var res = new byte[total];
            int off = 0;
            foreach (var p in parts)
            {
                Buffer.BlockCopy(p, 0, res, off, p.Length);
                off += p.Length;
            }
            return res;
        }

        public static byte[] HexToBytes(string hex)
        {
            if (hex.Length % 2 != 0) throw new ArgumentException("Hex length must be even");
            var res = new byte[hex.Length / 2];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return res;
        }

        public static string BytesToHex(byte[] bytes)
        {
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (var b in bytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }

        public static BigInteger BytesToBigInt(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return BigInteger.Zero;
            var hex = BytesToHex(bytes);
            return BigInteger.Parse("0" + hex, System.Globalization.NumberStyles.HexNumber);
        }

        public static byte[] BigIntToBytes(BigInteger value, int width)
        {
            var hex = value.ToString("x");
            if (hex.Length % 2 != 0) hex = "0" + hex;
            var raw = HexToBytes(hex);
            if (raw.Length > width)
            {
                // Strip possible leading zero
                if (raw.Length == width + 1 && raw[0] == 0)
                {
                    var stripped = new byte[width];
                    Buffer.BlockCopy(raw, 1, stripped, 0, width);
                    return stripped;
                }
                throw new ArgumentException($"Integer does not fit in {width} bytes");
            }
            var res = new byte[width];
            Buffer.BlockCopy(raw, 0, res, width - raw.Length, raw.Length);
            return res;
        }

        public static byte[] EncodeVarint(long input)
        {
            var value = (ulong)input;
            var list = new List<byte>();
            while (true)
            {
                var septet = (byte)(value & 0x7f);
                value >>= 7;
                if (value != 0)
                {
                    list.Add((byte)(septet | 0x80));
                }
                else
                {
                    list.Add(septet);
                    break;
                }
            }
            return list.ToArray();
        }

        public static (long value, int offset) DecodeVarint(byte[] data, int initialOffset = 0)
        {
            ulong value = 0;
            int shift = 0;
            int offset = initialOffset;
            while (offset < data.Length && shift < 70)
            {
                byte octet = data[offset++];
                value |= ((ulong)(octet & 0x7f)) << shift;
                if ((octet & 0x80) == 0) return ((long)value, offset);
                shift += 7;
            }
            throw new ArgumentException("Invalid or truncated protobuf varint");
        }

        public static byte[] BytesField(int number, byte[] value) =>
            ConcatBytes(EncodeVarint((number << 3) | 2), EncodeVarint(value.Length), value);

        public static byte[] StringField(int number, string value) =>
            BytesField(number, Encoding.UTF8.GetBytes(value));

        public static byte[] VarintField(int number, long value) =>
            ConcatBytes(EncodeVarint(number << 3), EncodeVarint(value));

        public static Dictionary<int, ProtoField> ParseFields(byte[] data)
        {
            var fields = new Dictionary<int, ProtoField>();
            int offset = 0;
            while (offset < data.Length)
            {
                var (tag, newOffset) = DecodeVarint(data, offset);
                offset = newOffset;
                int number = (int)(tag >> 3);
                int wireType = (int)(tag & 7);
                if (number == 0) throw new ArgumentException("Protobuf field number zero");

                if (wireType == 0)
                {
                    var (val, nextOffset) = DecodeVarint(data, offset);
                    fields[number] = new ProtoField { VarintVal = val };
                    offset = nextOffset;
                }
                else if (wireType == 2)
                {
                    var (length, nextOffset) = DecodeVarint(data, offset);
                    offset = nextOffset;
                    var slice = new byte[length];
                    Buffer.BlockCopy(data, offset, slice, 0, (int)length);
                    fields[number] = new ProtoField { BytesVal = slice };
                    offset += (int)length;
                }
                else
                {
                    throw new NotSupportedException($"Wire type {wireType} unsupported");
                }
            }
            return fields;
        }

        public static byte[] RequireBytes(Dictionary<int, ProtoField> fields, int number)
        {
            if (!fields.TryGetValue(number, out var field) || field.BytesVal == null)
                throw new ArgumentException($"Missing or invalid bytes field {number}");
            return field.BytesVal;
        }

        public static long RequireVarint(Dictionary<int, ProtoField> fields, int number)
        {
            if (!fields.TryGetValue(number, out var field) || !field.VarintVal.HasValue)
                throw new ArgumentException($"Missing or invalid varint field {number}");
            return field.VarintVal.Value;
        }

        public static ProtocolBleSend DecodeProtocolBleSend(byte[] data)
        {
            var outer = ParseFields(data);
            var prep = ParseFields(RequireBytes(outer, 1));
            return new ProtocolBleSend(
                ServerResponse: new ServerResponse(
                    Data: RequireVarint(prep, 1),
                    TransactionId: RequireVarint(prep, 2),
                    ApplicationId: Encoding.UTF8.GetString(RequireBytes(prep, 3)),
                    Nonce: Encoding.UTF8.GetString(RequireBytes(prep, 4))
                ),
                ServerSignature: RequireBytes(outer, 2)
            );
        }

        public static ProtocolBleFinalize DecodeProtocolBleFinalize(byte[] data)
        {
            var outer = ParseFields(data);
            var complete = ParseFields(RequireBytes(outer, 1));
            return new ProtocolBleFinalize(
                Nonce: Encoding.UTF8.GetString(RequireBytes(complete, 1)),
                ApplicationId: Encoding.UTF8.GetString(RequireBytes(complete, 2)),
                ServerSignature: RequireBytes(outer, 2)
            );
        }

        public static byte[] EncodeProtocolShare(long transactionId, string nonce) =>
            ConcatBytes(VarintField(1, transactionId), StringField(2, nonce));

        public static byte[] PeripheralRsaSignature(byte[] message)
        {
            using var sha256 = SHA256.Create();
            var digest = sha256.ComputeHash(message);
            int paddingLength = 256 - 3 - Sha256DigestInfoPrefix.Length - digest.Length;
            if (paddingLength < 8) throw new ArgumentException("Digest does not fit RSA modulus");

            var padding = new byte[paddingLength];
            for (int i = 0; i < paddingLength; i++) padding[i] = 0xff;

            var encoded = ConcatBytes(
                new byte[] { 0x00, 0x01 },
                padding,
                new byte[] { 0x00 },
                Sha256DigestInfoPrefix,
                digest
            );

            var signature = BigInteger.ModPow(BytesToBigInt(encoded), RsaPrivateExponent, RsaModulus);
            return BigIntToBytes(signature, 256);
        }

        public static byte[] EncodeProtocolComplete(long transactionId, string nonce)
        {
            var share = EncodeProtocolShare(transactionId, nonce);
            var appSig = PeripheralRsaSignature(share);
            var firmwareSig = new byte[16];
            Buffer.BlockCopy(appSig, 0, firmwareSig, 0, 16);

            return ConcatBytes(
                BytesField(1, share),
                BytesField(2, appSig),
                BytesField(3, firmwareSig)
            );
        }

        private static readonly byte[] UuidV5Namespace = HexToBytes("6ba7b8109dad11d180b400c04fd430c8");

        public static string GenerateApplicationId()
        {
            var name = new byte[16];
            RandomNumberGenerator.Fill(name);
            using var sha1 = SHA1.Create();
            var input = ConcatBytes(UuidV5Namespace, name);
            var hash = sha1.ComputeHash(input);
            hash[6] = (byte)((hash[6] & 0x0f) | 0x50);
            hash[8] = (byte)((hash[8] & 0x3f) | 0x80);

            var hex = BytesToHex(hash);
            string formatted = $"{hex[..8]}-{hex.Substring(8, 4)}-{hex.Substring(12, 4)}-{hex.Substring(16, 4)}-{hex.Substring(20, 12)}";
            return $"{formatted}{ApplicationIdSuffix}";
        }

        public static byte[] ApplicationIdGattValue(string applicationId)
        {
            var bytes = Encoding.UTF8.GetBytes(applicationId);
            var res = new byte[ApplicationIdGattWidth];
            Buffer.BlockCopy(bytes, 0, res, 0, bytes.Length);
            return res;
        }

        public static string DecodePlayerName(byte[] value) => Encoding.UTF8.GetString(value);

        public static byte[] InitialNonceGattValue(string nonce) =>
            ConcatBytes(Encoding.UTF8.GetBytes(nonce), new byte[] { 0x00 });

        public static byte[] FinalNonceGattValue(string nonce) =>
            Encoding.UTF8.GetBytes(nonce);

        public static string RandomPeripheralNonce()
        {
            ulong tick = (ulong)RandomNumberGenerator.GetInt32(int.MaxValue);
            ulong value = 0xcbf29ce484222645UL;
            for (int i = 0; i < 8; i++)
            {
                ulong octet = (tick >> (8 * i)) & 0xffUL;
                value ^= octet;
                value *= 0x100000001b3UL;
            }
            return value.ToString("x");
        }
    }
}
