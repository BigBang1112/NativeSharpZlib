namespace NativeSharpZlib;

public sealed class NativeZlibOptions
{
    /// <summary>
    /// The size of the compressed block. Default is 256.
    /// </summary>
    public int CompressedBlockSize { get; set; } = 256;

    /// <summary>
    /// The size of the uncompressed block. Default is 1024.
    /// </summary>
    public int UncompressedBlockSize { get; set; } = 1024;

    /// <summary>
    /// The compression level. Default is 8. Valid values are 0-9.
    /// </summary>
    public int CompressionLevel { get; set; } = 8;

    public bool LeaveOpen { get; set; }
}
