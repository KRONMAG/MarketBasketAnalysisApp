
//namespace MarketBasketAnalysis.Server.API.Services;

//public sealed class ReadOnlyListStream : Stream
//{
//    private readonly IReadOnlyList<byte> _buffer;
//    private long _position;

//    public override bool CanRead => true;

//    public override bool CanSeek => true;

//    public override bool CanWrite => false;

//    public override long Length => _buffer.Count;

//    public override long Position
//    {
//        get => _position;
//        set
//        {
//            ArgumentOutOfRangeException.ThrowIfNegative(value);
//            ArgumentOutOfRangeException.ThrowIfGreaterThan(value, Length);

//            _position = value;
//        }
//    }

//    public ReadOnlyListStream(IReadOnlyList<byte> buffer)
//    {
//        ArgumentNullException.ThrowIfNull(buffer);

//        _buffer = buffer;
//    }

//    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
//        throw new NotSupportedException();

//    public override int ReadByte()
//    {
//        return base.ReadByte();
//    }

//    public override int Read(byte[] buffer, int offset, int count)
//    {
//        ArgumentNullException.ThrowIfNull(buffer);
//        ArgumentOutOfRangeException.ThrowIfNegative(offset);
//        ArgumentOutOfRangeException.ThrowIfNegative(count);
//        ArgumentOutOfRangeException.ThrowIfGreaterThan(offset + count, buffer.Length);

//        var remaining = Length - Position;

//        if (remaining <= 0)
//        {
//            return 0;
//        }

//        var toRead = (int)Math.Min(count, remaining);

//        for (var i = 0; i < toRead; i++)
//        {
//            buffer[offset + i] = _buffer[(int)(_position + i)];
//        }

//        _position += toRead;

//        return toRead;
//    }

//    public override long Seek(long offset, SeekOrigin origin)
//    {
//        var newPosition = origin switch
//        {
//            SeekOrigin.Begin => offset,
//            SeekOrigin.Current => _position + offset,
//            SeekOrigin.End => Length + offset,
//            _ => throw new ArgumentOutOfRangeException(nameof(origin))
//        };

//        if (newPosition < 0 || newPosition > Length)
//            throw new IOException("Attempted to seek outside the stream bounds.");

//        _position = newPosition;
//        return _position;
//    }

//    public override void Flush()
//    {
//        // no-op (read-only)
//    }

//    public override void SetLength(long value) =>
//        throw new NotSupportedException();

//    public override void Write(byte[] buffer, int offset, int count) =>
//        throw new NotSupportedException();
//}
