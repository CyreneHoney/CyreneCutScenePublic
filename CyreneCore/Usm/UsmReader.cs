using System.Buffers.Binary;
using CyreneCore.Utils;

namespace CyreneCore.Usm;

public class UsmReader(string path) : IDisposable
{
    private readonly byte[] HeaderBuffer = new byte[24];
    private readonly FileStream Stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan);

    public IEnumerable<UsmData> IterChunks()
    {
        var chunkIndex = 0;
        var fileSize = Stream.Length;

        while (Stream.Position < fileSize)
        {
            var read = Stream.Read(HeaderBuffer, 0, 0x18);
            if (read < 0x18) break;

            // Parse header
            var signature = BitConverter.ToUInt32(HeaderBuffer, 0); // 0-3 Signature
            var chunkSize = BinaryPrimitives.ReadUInt32BigEndian(HeaderBuffer.AsSpan(0x4)); // 4-7 Chunk Size (BE)
            var dataOffset = HeaderBuffer[0x9]; // 9 Data Offset
            var paddingSize = BinaryPrimitives.ReadUInt16BigEndian(HeaderBuffer.AsSpan(0xA)); // 10-11 Padding Size (BE)
            var chno = HeaderBuffer[0xC]; // 12 Channel No
            var dataType = (byte)(HeaderBuffer[0xF] & 0x3); // 15 Data Type
            var frameTime = BinaryPrimitives.ReadUInt32BigEndian(HeaderBuffer.AsSpan(0x10)); // 16-19 Frame Time (BE)
            var frameRate = BinaryPrimitives.ReadUInt32BigEndian(HeaderBuffer.AsSpan(0x14)); // 20-23 Frame Rate (BE)

            var skipBytes = dataOffset + 0x8 - 0x18;
            var payloadSize = (int)(chunkSize - dataOffset - paddingSize);

            // Verify sig
            var isVideo = dataType == 0 && signature == CoreConst.UsmVideoSig;
            var isAudio = dataType == 0 && signature == CoreConst.UsmAudioSig;
            if (!isVideo && !isAudio)
                Stream.Seek(chunkSize - 0x10, SeekOrigin.Current); // Skip this chunk entirely
            else
            {
                if (skipBytes < 0) throw new InvalidDataException("Negative skip bytes.");

                // Skip head
                if (skipBytes > 0) Stream.Seek(skipBytes, SeekOrigin.Current);

                // Read payload
                byte[] data = new byte[payloadSize];
                int bytesRead = Stream.Read(data, 0, payloadSize);
                if (bytesRead != payloadSize) throw new EndOfStreamException();

                yield return new UsmData
                {
                    Data = data,
                    Meta = new UsmMetadata
                    {
                        Frametime = frameTime,
                        Framerate = frameRate
                    },
                    IsVideo = isVideo,
                    ChannelNumber = chno,
                    Index = chunkIndex++
                };

                // Skip padding
                if (paddingSize > 0) Stream.Seek(paddingSize, SeekOrigin.Current);
            }
        }
    }

    public void Dispose()
    {
        Stream.Dispose();
        GC.SuppressFinalize(this);
    }
}
