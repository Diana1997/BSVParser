// __________________
// 
// [2016] - 2025 ICVR LLC
// All Rights Reserved.
// 
// NOTICE:  All information contained herein is, and remains
// the property of ICVR LLC and its suppliers,
// if any.  The intellectual and technical concepts contained
// herein are proprietary to ICVR LLC
// and its suppliers and may be covered by U.S. and Foreign Patents,
// patents in process, and are protected by trade secret or copyright law.
// Dissemination of this information or reproduction of this material
// is strictly forbidden unless prior written permission is obtained
// from ICVR LLC.

using System.Security.Cryptography;
using BSVParser.Models;

namespace BSVParser.Services;

public static class TransactionParserService
{
    public static Transaction Parse(byte[] rawTransaction)
    {
        using var stream = new MemoryStream(rawTransaction);
        using var reader = new BinaryReader(stream);

        var version = reader.ReadUInt32();
        var inputCount = ReadVarInt(reader);

        var inputs = new List<TransactionInput>();
        for (ulong i = 0; i < inputCount; i++)
        {
            inputs.Add(ReadInput(reader));
        }

        var outputCount = ReadVarInt(reader);

        var outputs = new List<TransactionOutput>();
        for (ulong i = 0; i < outputCount; i++)
        {
            outputs.Add(ReadOutput(reader));
        }

        var lockTime = reader.ReadUInt32();

        stream.Position = 0;
        var txBytes = reader.ReadBytes((int)stream.Length);

        var txId = ComputeTransactionId(txBytes);

        return new Transaction
        {
            TransactionId = txId,
            Version = version,
            Inputs = inputs,
            Outputs = outputs,
            LockTime = lockTime
        };
    }

    private static TransactionInput ReadInput(BinaryReader reader)
    {
        var prevTxId = reader.ReadBytes(32).Reverse().ToArray();
        var vout = reader.ReadUInt32();
        var scriptLength = ReadVarInt(reader);
        var scriptSig = reader.ReadBytes((int)scriptLength);
        var sequence = reader.ReadUInt32();

        return new TransactionInput
        {
            PreviousTransactionId = BytesToHex(prevTxId),
            Vout = vout,
            ScriptSig = BytesToHex(scriptSig),
            Sequence = sequence
        };
    }

    private static TransactionOutput ReadOutput(BinaryReader reader)
    {
        var value = reader.ReadUInt64();
        var scriptLength = ReadVarInt(reader);
        var scriptPubKey = reader.ReadBytes((int)scriptLength);
        var address = TryExtractAddress(scriptPubKey);

        return new TransactionOutput
        {
            Value = value,
            ScriptPubKey = BytesToHex(scriptPubKey),
            Address = address
        };
    }

    private static ulong ReadVarInt(BinaryReader reader)
    {
        byte prefix = reader.ReadByte();
        return prefix switch
        {
            < 0xfd => prefix,
            0xfd => reader.ReadUInt16(),
            0xfe => reader.ReadUInt32(),
            _ => reader.ReadUInt64()
        };
    }

    private static string TryExtractAddress(byte[] scriptPubKey)
    {
        if (scriptPubKey.Length == 25 &&
            scriptPubKey[0] == 0x76 &&
            scriptPubKey[1] == 0xa9 &&
            scriptPubKey[2] == 0x14 &&
            scriptPubKey[^2] == 0x88 &&
            scriptPubKey[^1] == 0xac)
        {
            var pubKeyHash = scriptPubKey.Skip(3).Take(20).ToArray();
            return Base58EncodeWithChecksum(new byte[] { 0x00 }.Concat(pubKeyHash).ToArray());
        }

        return "Unknown";
    }

    private static string ComputeTransactionId(byte[] txBytes)
    {
        using var sha256 = SHA256.Create();
        var first = sha256.ComputeHash(txBytes);
        var second = sha256.ComputeHash(first);
        return BytesToHex(second.Reverse().ToArray());
    }

    private static string BytesToHex(byte[] bytes) =>
        BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();

    private static string Base58EncodeWithChecksum(byte[] data)
    {
        using var sha256 = SHA256.Create();
        var checksum = sha256.ComputeHash(sha256.ComputeHash(data)).Take(4).ToArray();
        var dataWithChecksum = data.Concat(checksum).ToArray();
        return Base58Encode(dataWithChecksum);
    }

    private static string Base58Encode(byte[] array)
    {
        const string alphabet = "123456789ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz";
        var intData = new System.Numerics.BigInteger(array.Reverse().Concat(new byte[] { 0 }).ToArray());
        var result = "";

        while (intData > 0)
        {
            int remainder = (int)(intData % 58);
            intData /= 58;
            result = alphabet[remainder] + result;
        }

        foreach (var b in array)
        {
            if (b == 0)
                result = alphabet[0] + result;
            else
                break;
        }

        return result;
    }
}