namespace NativeSharpZlib;

public sealed class NativeZlibOptions
{
    public int CompressedBlockSize { get; set; } = 256;
    public int UncompressedBlockSize { get; set; } = 1024;
    public bool LeaveOpen { get; set; }
}
