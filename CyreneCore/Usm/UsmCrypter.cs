using System.Runtime.InteropServices;

namespace CyreneCore.Usm;

public class UsmCrypter
{
    private readonly byte[] VideoMask1 = new byte[0x20];
    private readonly byte[] VideoMask2 = new byte[0x20];
    private readonly byte[] AudioMask = new byte[0x20];

    public UsmCrypter(ulong key)
    {
        InitKey(key);
    }

    private void InitKey(ulong key)
    {
        if (key <= 0) throw new Exception("Cannot init from non-positive raw key.");

        Span<byte> key1 = stackalloc byte[0x4];
        var tmpKey1 = (uint)(key & 0xFFFFFFFF);
        MemoryMarshal.Write(key1, in tmpKey1); // LE

        Span<byte> key2 = stackalloc byte[0x4];
        var tmpKey2 = (uint)(key >> 0x20);
        MemoryMarshal.Write(key2, in tmpKey2); // LE

        Span<byte> table2 = [0x55, 0x52, 0x55, 0x43]; // URUC

        VideoMask1[0x00] = key1[0];
        VideoMask1[0x01] = key1[1];
        VideoMask1[0x02] = key1[2];
        VideoMask1[0x03] = (byte)(key1[3] - 0x34);
        VideoMask1[0x04] = (byte)(key2[0] + 0xF9);
        VideoMask1[0x05] = (byte)(key2[1] ^ 0x13);
        VideoMask1[0x06] = (byte)(key2[2] + 0x61);
        VideoMask1[0x07] = (byte)(VideoMask1[0x00] ^ 0xFF);
        VideoMask1[0x08] = (byte)(VideoMask1[0x02] + VideoMask1[0x01]);
        VideoMask1[0x09] = (byte)(VideoMask1[0x01] - VideoMask1[0x07]);
        VideoMask1[0x0A] = (byte)(VideoMask1[0x02] ^ 0xFF);
        VideoMask1[0x0B] = (byte)(VideoMask1[0x01] ^ 0xFF);
        VideoMask1[0x0C] = (byte)(VideoMask1[0x0B] + VideoMask1[0x09]);
        VideoMask1[0x0D] = (byte)(VideoMask1[0x08] - VideoMask1[0x03]);
        VideoMask1[0x0E] = (byte)(VideoMask1[0x0D] ^ 0xFF);
        VideoMask1[0x0F] = (byte)(VideoMask1[0x0A] - VideoMask1[0x0B]);
        VideoMask1[0x10] = (byte)(VideoMask1[0x08] - VideoMask1[0x0F]);
        VideoMask1[0x11] = (byte)(VideoMask1[0x10] ^ VideoMask1[0x07]);
        VideoMask1[0x12] = (byte)(VideoMask1[0x0F] ^ 0xFF);
        VideoMask1[0x13] = (byte)(VideoMask1[0x03] ^ 0x10);
        VideoMask1[0x14] = (byte)(VideoMask1[0x04] - 0x32);
        VideoMask1[0x15] = (byte)(VideoMask1[0x05] + 0xED);
        VideoMask1[0x16] = (byte)(VideoMask1[0x06] ^ 0xF3);
        VideoMask1[0x17] = (byte)(VideoMask1[0x13] - VideoMask1[0x0F]);
        VideoMask1[0x18] = (byte)(VideoMask1[0x15] + VideoMask1[0x07]);
        VideoMask1[0x19] = (byte)(0x21 - VideoMask1[0x13]);
        VideoMask1[0x1A] = (byte)(VideoMask1[0x14] ^ VideoMask1[0x17]);
        VideoMask1[0x1B] = (byte)(VideoMask1[0x16] + VideoMask1[0x16]);
        VideoMask1[0x1C] = (byte)(VideoMask1[0x17] + 0x44);
        VideoMask1[0x1D] = (byte)(VideoMask1[0x03] + VideoMask1[0x04]);
        VideoMask1[0x1E] = (byte)(VideoMask1[0x05] - VideoMask1[0x16]);
        VideoMask1[0x1F] = (byte)(VideoMask1[0x1D] ^ VideoMask1[0x13]);

        for (int i = 0; i < 0x20; i++)
        {
            VideoMask2[i] = (byte)(VideoMask1[i] ^ 0xFF);
            AudioMask[i] = (i & 1) == 1 ? table2[(i >> 1) & 3] : (byte)(VideoMask1[i] ^ 0xFF);
        }
    }

    public void DecryptVideo(Span<byte> data)
    {
        if (data.Length < 0x240) return;

        ReadOnlySpan<byte> vMask1 = VideoMask1;
        ReadOnlySpan<byte> vMask2 = VideoMask2;
        Span<byte> mask = stackalloc byte[0x20];

        vMask2.CopyTo(mask);
        var payload = data[0x40..];
        for (var i = 0x100; i < payload.Length; i++)
        {
            var maskIdx = i & 0x1F;
            payload[i] ^= mask[maskIdx]; // Decrypt byte
            mask[maskIdx] = (byte)(payload[i] ^ vMask2[maskIdx]); // Update mask
        }

        vMask1.CopyTo(mask);
        for (var i = 0; i < 0x100; i++)
        {
            var maskIdx = i & 0x1F;
            mask[maskIdx] ^= payload[0x100 + i]; // Update mask
            payload[i] ^= mask[maskIdx]; // Decrypt byte
        }
    }

    public void DecryptAudio(Span<byte> data)
    {
        if (data.Length <= 0x140) return;

        ReadOnlySpan<byte> aMask = AudioMask;

        for (var i = 0x140; i < data.Length; i++)
            data[i] ^= aMask[i & 0x1F];
    }
}
