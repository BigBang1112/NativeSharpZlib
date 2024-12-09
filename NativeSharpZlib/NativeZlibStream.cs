using System.IO.Compression;
using System.Runtime.InteropServices;

using static NativeSharpZlib.ZlibNative;

namespace NativeSharpZlib;

public class NativeZlibStream : Stream
{
    private readonly Stream stream;
    private readonly CompressionMode mode;
    private readonly ZlibNative zlibNative;
    private readonly bool leaveOpen;
    private readonly int compressedBlockSize;
    private readonly int uncompressedBlockSize;
    private readonly byte[] compressedBuffer;
    private readonly byte[] uncompressedBuffer;
    private readonly nint compressedBufferPtr;
    private readonly nint uncompressedBufferPtr;

    private int position;
    private int uncompressedIndex;
    private int uncompressedSize;
    private bool disposed;

    public override bool CanRead => mode == CompressionMode.Decompress;
    public override bool CanSeek => false;
    public override bool CanWrite => mode == CompressionMode.Compress;
    public override long Length => throw new NotSupportedException();
    public override long Position
    {
        get => position;
        set => throw new NotSupportedException();
    }

    public NativeZlibStream(Stream stream, CompressionMode mode, NativeZlibOptions options)
    {
        this.stream = stream;
        this.mode = mode;

        leaveOpen = options.LeaveOpen;
        compressedBlockSize = options.CompressedBlockSize;
        uncompressedBlockSize = options.UncompressedBlockSize;

        zlibNative = new ZlibNative();

        compressedBuffer = new byte[compressedBlockSize];
        uncompressedBuffer = new byte[uncompressedBlockSize];
        compressedBufferPtr = Marshal.AllocHGlobal(compressedBlockSize);
        uncompressedBufferPtr = Marshal.AllocHGlobal(uncompressedBlockSize);

        if (mode == CompressionMode.Compress)
        {
            zlibNative.DeflateInit(options.CompressionLevel);
        }
        else
        {
            zlibNative.InflateInit();
        }
    }

    public NativeZlibStream(Stream stream, CompressionMode mode, bool leaveOpen = false)
        : this(stream, mode, new NativeZlibOptions { LeaveOpen = leaveOpen }) { }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (!CanRead)
        {
            throw new NotSupportedException();
        }

        var bytesRemaining = count;

        while (bytesRemaining > 0)
        {
            if (uncompressedIndex == uncompressedSize) // Refill the buffer if empty
            {
                uncompressedIndex = 0;
                uncompressedSize = RefillUncompressedBuffer();

                if (uncompressedSize == 0)
                {
                    // End of data
                    break;
                }
            }

            var chunkSize = Math.Min(uncompressedSize - uncompressedIndex, bytesRemaining);
            Array.Copy(uncompressedBuffer, uncompressedIndex, buffer, offset, chunkSize);

            uncompressedIndex += chunkSize;
            position += chunkSize;
            offset += chunkSize;
            bytesRemaining -= chunkSize;
        }

        return count - bytesRemaining;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (!CanWrite)
        {
            throw new NotSupportedException();
        }

        var bytesRemaining = count;

        while (bytesRemaining > 0)
        {
            var chunkSize = Math.Min(uncompressedBlockSize - uncompressedIndex, bytesRemaining);
            Array.Copy(buffer, offset, uncompressedBuffer, uncompressedIndex, chunkSize);

            uncompressedIndex += chunkSize;
            position += chunkSize;
            offset += chunkSize;
            bytesRemaining -= chunkSize;

            if (uncompressedIndex == uncompressedBlockSize) // Compress if buffer is full
            {
                CompressBuffer();
            }
        }
    }

    public override void Flush()
    {
        
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        throw new NotSupportedException();
    }

    public override void SetLength(long value)
    {
        throw new NotSupportedException();
    }

    private int RefillUncompressedBuffer()
    {
        int uncompressedTotal = 0;

        while (uncompressedTotal < uncompressedBlockSize)
        {
            if (zlibNative.AvailIn == 0) // Refill compressed buffer if necessary
            {
                var bytesRead = stream.Read(compressedBuffer, 0, compressedBlockSize);

                if (bytesRead == 0)
                {
                    // End of stream
                    break; 
                }

                SetInflateInput(compressedBuffer, 0, bytesRead);
            }

            uncompressedTotal += InflateData(uncompressedBuffer, uncompressedTotal, uncompressedBlockSize - uncompressedTotal, out var streamEnd);

            if (streamEnd)
            {
                break;
            }
        }

        return uncompressedTotal;
    }

    private void CompressBuffer()
    {
        SetDeflateInput(uncompressedBuffer, 0, uncompressedIndex);
        FlushDeflateOutput(flushFinalBlock: false);
        uncompressedIndex = 0;
    }

    private void SetInflateInput(byte[] buffer, int offset, int count)
    {
        zlibNative.AvailIn = count;
        zlibNative.NextIn = compressedBufferPtr;
        Marshal.Copy(buffer, offset, compressedBufferPtr, count);
    }

    private void SetDeflateInput(byte[] buffer, int offset, int count)
    {
        zlibNative.AvailIn = count;
        zlibNative.NextIn = uncompressedBufferPtr;
        Marshal.Copy(buffer, offset, uncompressedBufferPtr, count);
    }

    private int InflateData(byte[] buffer, int offset, int count, out bool streamEnd)
    {
        zlibNative.AvailOut = count;
        zlibNative.NextOut = uncompressedBufferPtr;

        var status = zlibNative.Inflate(FlushType.Z_SYNC_FLUSH);
        streamEnd = status == Status.Z_STREAM_END;

        var inflatedBytes = count - zlibNative.AvailOut;
        Marshal.Copy(uncompressedBufferPtr, buffer, offset, inflatedBytes);

        return inflatedBytes;
    }

    private int DeflateData(byte[] buffer, int offset, int length, bool finish, out bool finished)
    {
        zlibNative.AvailOut = length;
        zlibNative.NextOut = compressedBufferPtr;

        var status = finish
            ? zlibNative.Deflate(FlushType.Z_FINISH)
            : zlibNative.Deflate(FlushType.Z_SYNC_FLUSH);

        finished = status == Status.Z_STREAM_END;

        var deflatedBytes = length - zlibNative.AvailOut;
        Marshal.Copy(compressedBufferPtr, buffer, offset, deflatedBytes);

        return deflatedBytes;
    }

    private void FlushDeflateOutput(bool flushFinalBlock)
    {
        var finished = false;
        while (zlibNative.AvailIn > 0 || (flushFinalBlock && !finished))
        {
            var deflatedBytes = DeflateData(compressedBuffer, 0, compressedBlockSize, flushFinalBlock, out finished);
            stream.Write(compressedBuffer, 0, deflatedBytes);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposed) return;

        try
        {
            if (disposing)
            {
                if (mode == CompressionMode.Compress)
                {
                    if (uncompressedIndex > 0)
                    {
                        CompressBuffer();
                    }

                    FlushDeflateOutput(flushFinalBlock: true);
                }

                if (!leaveOpen)
                {
                    stream.Dispose();
                }
            }

            if (mode == CompressionMode.Compress)
            {
                zlibNative?.DeflateEnd();
            }
            else
            {
                zlibNative?.InflateEnd();
            }
        }
        finally
        {
            Marshal.FreeHGlobal(compressedBufferPtr);
            Marshal.FreeHGlobal(uncompressedBufferPtr);
            disposed = true;

            base.Dispose(disposing);
        }
    }
}
