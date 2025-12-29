namespace CyreneCore.Usm;

public struct UsmData
{
    public Memory<byte> Data { get; set; }
    public UsmMetadata Meta { get; set; }
    public uint Framerate { get; set; }
    public bool IsVideo { get; set; }
    public byte ChannelNumber { get; set; }
    public int Index { get; set; }
}

public struct UsmMetadata
{
    public uint Frametime { get; set; }
    public uint Framerate { get; set; }
}