using System;
using System.IO;
using SabreTools.Data.Models.CDROM;
using SabreTools.IO;
using SabreTools.IO.Extensions;

namespace SabreTools.Data.Extensions
{
    public static class CDROM
    {
        protected class ISO9660Stream : Stream
        {
            private readonly Stream _baseStream;
            private const long _baseSectorSize = 2352;
            private const long _isoSectorSize = 2048;
            private long _position = 0;
            private SectorMode _currentMode = SectorMode.UNKNOWN;
            private long _userDataStart = 16;
            private long _userDataEnd = 2064;


            public ISO9660Stream(Stream inputStream)
            {
                _baseStream = inputStream;
            }

            public override bool CanRead => _baseStream.CanRead;
            public override bool CanSeek => _baseStream.CanSeek;
            public override bool CanWrite => false;

            public override void Flush()
            {
                _baseStream.Flush();
            }

            public override long Length
            {
                get
                {
                    return (_baseStream.Length / _baseSectorSize) * _isoSectorSize;
                }
            }

            public override void SetLength(long value)
            {
                throw new NotSupportedException("Setting the length of this stream is not supported.");
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException("Writing to this stream is not supported.");
            }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    _baseStream.Dispose();
                }
                base.Dispose(disposing);
            }

            public override long Position
            {
                // Get the position of the underlying ISO9660 stream
                get
                {
                    // Get the number of ISO sectors before current position
                    long isoPosition = (_position / _baseSectorSize) * _isoSectorSize;

                    // Get the user data location based on the current sector mode
                    SetUserDataLocation();

                    // Add the within-sector position
                    long remainder = _position % _baseSectorSize;
                    if (remainder > _userDataEnd)
                        isoPosition += _isoSectorSize;
                    else if (remainder > _userDataStart)
                        isoPosition += remainder - _userDataStart;

                    return isoPosition;
                }
                set
                {
                    // Seek to the underlying ISO9660 position
                    Seek(value, SeekOrigin.Begin);
                }
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                int totalRead = 0;
                int remaining = count;

                while (remaining > 0 && _position < Length)
                {
                    // Determine location of current sector
                    long baseStreamOffset = _position / _baseSectorSize;

                    // Set the current sector's mode and user data location
                    SetSectorMode(baseStreamOffset);

                    // Add the within-sector position
                    long remainder = _position % _baseSectorSize;
                    if (remainder < _userDataStart)
                        baseStreamOffset += _userDataStart;
                    else if (remainder >= _userDataEnd)
                        baseStreamOffset += _baseSectorSize;
                    else
                        baseStreamOffset += remainder;

                    // Sanity check on read location before seeking
                    if (baseStreamOffset < 0 || baseStreamOffset > _baseStream.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(offset), "Attempted to seek outside the stream boundaries.");
                    }

                    // Seek to target position in base CDROM stream
                    _baseStream.Seek(baseStreamOffset, SeekOrigin.Begin);

                    // Read the remaining bytes, up to max of one ISO sector (2048 bytes)
                    long withinSectorLocation = baseStreamOffset % _baseSectorSize;
                    int bytesToRead = (int)Math.Min(remaining, _isoSectorSize - (withinSectorLocation - _userDataStart));

                    // Don't overshoot end of stream
                    bytesToRead = (int)Math.Min(bytesToRead, Length - _position);

                    // Finish reading if no more bytes to be read
                    if (bytesToRead <= 0)
                        break;

                    // Read from base CDROM stream
                    int bytesRead = _baseStream.Read(buffer, offset + totalRead, bytesToRead);

                    // Update state
                    _position += bytesRead;
                    totalRead += bytesRead;
                    remaining -= bytesRead;

                    if (bytesRead == 0)
                        break;
                }

                return totalRead;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                // Get the intended position for the ISO9660 stream
                long targetPosition;
                switch (origin)
                {
                    case SeekOrigin.Begin:
                        targetPosition = offset;
                        break;
                    case SeekOrigin.Current:
                        targetPosition = _position + offset;
                        break;
                    case SeekOrigin.End:
                        targetPosition = Length + offset;
                        break;
                    default:
                        throw new ArgumentException("Invalid SeekOrigin.", nameof(origin));
                }

                // Get the number of ISO sectors before current position
                long newPosition = (targetPosition / _isoSectorSize) * _baseSectorSize;

                // Set the current sector's mode and user data location
                SetSectorMode(newPosition);

                // Add the within-sector position
                newPosition += _userDataStart + (targetPosition % _isoSectorSize);

                if (newPosition < 0 || newPosition > _baseStream.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset), "Attempted to seek outside the stream boundaries.");
                }

                _position = _baseStream.SeekIfPossible(newPosition, SeekOrigin.Begin);
                return Position;
            }

            private void SetSectorMode(long sectorLocation)
            {
                long modePosition = sectorLocation + 15;
                _baseStream.SeekIfPossible(modePosition, SeekOrigin.Begin);
                byte modeByte = _baseStream.ReadByteValue();
                if (modeByte == 0)
                    _currentMode = SectorMode.MODE0;
                else if (modeByte == 1)
                    _currentMode = SectorMode.MODE1;
                else if (modeByte == 2)
                {
                    _baseStream.SeekIfPossible(modePosition + 3, SeekOrigin.Begin);
                    byte submode = _baseStream.ReadByteValue();
                    if ((submode & 0x20) == 0x20)
                        _currentMode = SectorMode.MODE2_FORM2;
                    else
                        _currentMode = SectorMode.MODE2_FORM1;
                }
                else
                    _currentMode = SectorMode.UNKNOWN;

                SetUserDataLocation();
                return;
            }

            private void SetUserDataLocation()
            {
                switch (_currentMode)
                {
                    case SectorMode.MODE1:
                        _userDataStart = 16;
                        _userDataEnd = 2064;
                        return;

                    case SectorMode.MODE2_FORM1:
                        _userDataStart = 24;
                        _userDataEnd = 2072;
                        return;

                    case SectorMode.MODE2_FORM2:
                        _userDataStart = 24;
                        _userDataEnd = 2348;
                        return;

                    case SectorMode.MODE0:
                    case SectorMode.MODE2:
                        _userDataStart = 16;
                        _userDataEnd = 2352;
                        return;

                    case SectorMode.UNKNOWN:
                        _userDataStart = 16;
                        _userDataEnd = 2064;
                        return;
                }
            }
        }
    }
}

