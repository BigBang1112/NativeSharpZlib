using System.Runtime.InteropServices;

namespace NativeSharpZlib;

internal sealed partial class ZlibNative(ZlibNative.ZStream stream)
{
    private const string Library = "zlib";
    private const string Version = "1.2.5";

    private ZStream stream = stream;

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
        return ThrowWhenNotOk(deflateInit_(ref stream, level, Version, Marshal.SizeOf<ZStream>()));
    }

    internal Status Deflate(FlushType flush)
    {
        return ThrowWhenNotOk(deflate(ref stream, flush));
    }

    internal Status DeflateEnd()
    {
        return ThrowWhenNotOk(deflateEnd(ref stream));
    }

    internal Status InflateInit()
    {
        return ThrowWhenNotOk(inflateInit_(ref stream, Version, Marshal.SizeOf<ZStream>()));
    }

    internal Status Inflate(FlushType flush)
    {
        return ThrowWhenNotOk(inflate(ref stream, flush));
    }

    internal Status InflateEnd()
    {
        return ThrowWhenNotOk(inflateEnd(ref stream));
    }

#if NET8_0_OR_GREATER
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial Status deflateInit_(ref ZStream stream, int level, string version, int stream_size);
#else
    [DllImport(Library, CharSet = CharSet.Unicode)]
    private static extern Status deflateInit_(ref ZStream stream, int level, string version, int stream_size);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status deflate(ref ZStream stream, FlushType flush);
#else
    [DllImport(Library)]
    private static extern Status deflate(ref ZStream stream, FlushType flush);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status deflateEnd(ref ZStream stream);
#else
    [DllImport(Library)]
    private static extern Status deflateEnd(ref ZStream stream);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial Status inflateInit_(ref ZStream stream, string version, int stream_size);
#else
    [DllImport(Library, CharSet = CharSet.Unicode)]
    private static extern Status inflateInit_(ref ZStream stream, string version, int stream_size);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status inflate(ref ZStream stream, FlushType flush);
#else
    [DllImport(Library)]
    private static extern Status inflate(ref ZStream stream, FlushType flush);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status inflateEnd(ref ZStream stream);
#else
    [DllImport(Library)]
    private static extern Status inflateEnd(ref ZStream stream);
#endif

    [StructLayout(LayoutKind.Sequential)]
    internal struct ZStream
    {
        public IntPtr next_in;
        public int avail_in;
#if WINDOWS
        public uint total_in;
#else
        public ulong total_in;
#endif
        public IntPtr next_out;
        public int avail_out;
#if WINDOWS
        public uint total_out;
#else
        public ulong total_out;
#endif
        public IntPtr msg;
        public IntPtr state;
        public IntPtr zalloc;
        public IntPtr zfree;
        public IntPtr opaque;
        public int data_type;
#if WINDOWS
        public uint adler;
        public uint reserved;
#else
        public ulong adler;
        public ulong reserved;
#endif
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
        if (status >= Status.Z_OK)
        {
            return status;
        }

        if (stream.msg != IntPtr.Zero)
        {
            throw new ZlibException($"{status}: {Marshal.PtrToStringAuto(stream.msg)}");
        }

        if (status == Status.Z_VERSION_ERROR)
        {
            throw new ZlibException($"{status}, sizeof stream: {Marshal.SizeOf<ZStream>()}");
        }

        throw new ZlibException(status.ToString());
    }
}
