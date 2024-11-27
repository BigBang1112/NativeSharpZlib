using System.Runtime.InteropServices;

namespace NativeSharpZlib;

internal static partial class ZlibNative
{
    private const string Library = "zlib";

#if NET8_0_OR_GREATER
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial Status deflateInit_(Stream stream, int level, string version, int stream_size);
#else
    [DllImport(Library)]
    private static extern Status deflateInit_(Stream stream, int level, string version, int stream_size);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status deflate(Stream stream, FlushType flush);
#else
    [DllImport(Library)]
    private static extern Status deflate(Stream stream, FlushType flush);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status deflateEnd(Stream stream);
#else
    [DllImport(Library)]
    private static extern Status deflateEnd(Stream stream);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library, StringMarshalling = StringMarshalling.Utf8)]
    private static partial Status inflateInit_(Stream stream, string version, int stream_size);
#else
    [DllImport(Library)]
    private static extern Status inflateInit_(Stream stream, string version, int stream_size);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status inflate(Stream stream, FlushType flush);
#else
    [DllImport(Library)]
    private static extern Status inflate(Stream stream, FlushType flush);
#endif

#if NET8_0_OR_GREATER
    [LibraryImport(Library)]
    private static partial Status inflateEnd(Stream stream);
#else
    [DllImport(Library)]
    private static extern Status inflateEnd(Stream stream);
#endif

    [StructLayout(LayoutKind.Sequential)]
    private struct Stream
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

    private enum FlushType
    {
        Z_NO_FLUSH = 0,
        Z_PARTIAL_FLUSH = 1,
        Z_SYNC_FLUSH = 2,
        Z_FULL_FLUSH = 3,
        Z_FINISH = 4,
        Z_BLOCK = 5,
        Z_TREES = 6
    }

    private enum Status
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
}
