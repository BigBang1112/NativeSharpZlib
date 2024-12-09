using System.Runtime.InteropServices;

namespace NativeSharpZlib;

internal sealed partial class ZlibNative(ZlibNative.ZStream stream)
{
    private const string Library = "zlib";
    private const string Version = "1.2.5";

    private readonly ZStream stream = stream;

    public int AvailIn
    {
        get => stream.avail_in;
        set => stream.avail_in = value;
    }

    public nint NextIn
    {
        get => stream.next_in;
        set => stream.next_in = value;
    }

    public int AvailOut
    {
        get => stream.avail_out;
        set => stream.avail_out = value;
    }

    public nint NextOut
    {
        get => stream.next_out;
        set => stream.next_out = value;
    }

    public ZlibNative() : this(new ZStream()) { }

    internal Status DeflateInit(int level = 8)
    {
        return ThrowWhenNotOk(deflateInit_(stream, level, Version, Marshal.SizeOf(stream)));
    }

    internal Status Deflate(FlushType flush)
    {
        return ThrowWhenNotOk(deflate(stream, flush));
    }

    internal Status DeflateEnd()
    {
        return ThrowWhenNotOk(deflateEnd(stream));
    }

    internal Status InflateInit()
    {
        return ThrowWhenNotOk(inflateInit_(stream, Version, Marshal.SizeOf(stream)));
    }

    internal Status Inflate(FlushType flush)
    {
        return ThrowWhenNotOk(inflate(stream, flush));
    }

    internal Status InflateEnd()
    {
        return ThrowWhenNotOk(inflateEnd(stream));
    }

    [DllImport(Library)]
    private static extern Status deflateInit_(ZStream stream, int level, string version, int stream_size);

    [DllImport(Library)]
    private static extern Status deflate(ZStream stream, FlushType flush);

    [DllImport(Library)]
    private static extern Status deflateEnd(ZStream stream);

    [DllImport(Library)]
    private static extern Status inflateInit_(ZStream stream, string version, int stream_size);

    [DllImport(Library)]
    private static extern Status inflate(ZStream stream, FlushType flush);

    [DllImport(Library)]
    private static extern Status inflateEnd(ZStream stream);

    [StructLayout(LayoutKind.Sequential)]
    internal class ZStream
    {
        public IntPtr next_in;
        public int avail_in;
        public int total_in;
        public IntPtr next_out;
        public int avail_out;
        public int total_out;
        public IntPtr msg;
        public IntPtr state;
        public IntPtr zalloc;
        public IntPtr zfree;
        public IntPtr opaque;
        public int data_type;
        public uint adler;
        public uint reserved;
    }

    internal enum FlushType
    {
        Z_NO_FLUSH = 0,
        Z_PARTIAL_FLUSH = 1,
        Z_SYNC_FLUSH = 2,
        Z_FULL_FLUSH = 3,
        Z_FINISH = 4,
        Z_BLOCK = 5,
        Z_TREES = 6
    }

    internal enum Status
    {
        Z_OK = 0,
        Z_STREAM_END = 1,
        Z_NEED_DICT = 2,
        Z_ERRNO = -1,
        Z_STREAM_ERROR = -2,
        Z_DATA_ERROR = -3,
        Z_MEM_ERROR = -4,
        Z_BUF_ERROR = -5,
        Z_VERSION_ERROR = -6
    }

    private Status ThrowWhenNotOk(Status status)
    {
        if (status < Status.Z_OK)
        {
            throw new ZlibException($"{status}: {stream.msg}");
        }

        return status;
    }
}
