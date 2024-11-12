using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using SabreTools.IO.Extensions;
using SabreTools.Serialization.Interfaces;

namespace SabreTools.Serialization.Wrappers
{
    public abstract class WrapperBase<T> : WrapperBase, IWrapper<T>
    {
        #region Properties

        /// <inheritdoc/>
        public T GetModel() => Model;

        /// <summary>
        /// Internal model
        /// </summary>
        public T Model { get; private set; }

        #endregion

        #region Instance Variables

        /// <summary>
        /// Source of the original data
        /// </summary>
        protected DataSource _dataSource = DataSource.UNKNOWN;

        /// <summary>
        /// Lock object for reading from the source
        /// </summary>
        private readonly object _streamDataLock = new();

        /// <summary>
        /// Source byte array data
        /// </summary>
        /// <remarks>This is only populated if <see cref="_dataSource"/> is <see cref="DataSource.ByteArray"/></remarks>
        protected byte[]? _byteArrayData = null;

        /// <summary>
        /// Source byte array data offset
        /// </summary>
        /// <remarks>This is only populated if <see cref="_dataSource"/> is <see cref="DataSource.ByteArray"/></remarks>
        protected int _byteArrayOffset = -1;

        /// <summary>
        /// Source Stream data
        /// </summary>
        /// <remarks>This is only populated if <see cref="_dataSource"/> is <see cref="DataSource.Stream"/></remarks>
        protected Stream? _streamData = null;

#if !NETFRAMEWORK
        /// <summary>
        /// JSON serializer options for output printing
        /// </summary>
        protected System.Text.Json.JsonSerializerOptions _jsonSerializerOptions
        {
            get
            {
#if NETCOREAPP3_1
                var serializer = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
#else
                var serializer = new System.Text.Json.JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
#endif
                serializer.Converters.Add(new ConcreteAbstractSerializer());
                serializer.Converters.Add(new ConcreteInterfaceSerializer());
                serializer.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                return serializer;
            }
        }
#endif

        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new instance of the wrapper from a byte array
        /// </summary>
        protected WrapperBase(T? model, byte[]? data, int offset)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (offset < 0 || offset >= data.Length)
                throw new ArgumentOutOfRangeException(nameof(offset));

            Model = model;
            _dataSource = DataSource.ByteArray;
            _byteArrayData = data;
            _byteArrayOffset = offset;
        }

        /// <summary>
        /// Construct a new instance of the wrapper from a Stream
        /// </summary>
        protected WrapperBase(T? model, Stream? data)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));
            if (data == null)
                throw new ArgumentNullException(nameof(data));
            if (data.Length == 0 || !data.CanSeek || !data.CanRead)
                throw new ArgumentOutOfRangeException(nameof(data));

            Model = model;
            _dataSource = DataSource.Stream;
            _streamData = data;
        }

        #endregion

        #region Data

        /// <summary>
        /// Validate the backing data source
        /// </summary>
        /// <returns>True if the data source is valid, false otherwise</returns>
        public bool DataSourceIsValid()
        {
            switch (_dataSource)
            {
                // Byte array data requires both a valid array and offset
                case DataSource.ByteArray:
                    return _byteArrayData != null && _byteArrayOffset >= 0;

                // Stream data requires both a valid stream
                case DataSource.Stream:
                    return _streamData != null && _streamData.CanRead && _streamData.CanSeek;

                // Everything else is invalid
                case DataSource.UNKNOWN:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Check if a data segment is valid in the data source 
        /// </summary>
        /// <param name="position">Position in the source</param>
        /// <param name="length">Length of the data to check</param>
        /// <returns>True if the positional data is valid, false otherwise</returns>
        public bool SegmentValid(int position, int length)
        {
            // Validate the data souece
            if (!DataSourceIsValid())
                return false;

            // If we have an invalid position
            if (position < 0 || position >= GetEndOfFile())
                return false;

            switch (_dataSource)
            {
                case DataSource.ByteArray:
                    return _byteArrayOffset + position + length <= _byteArrayData!.Length;

                case DataSource.Stream:
                    return position + length <= _streamData!.Length;

                // Everything else is invalid
                case DataSource.UNKNOWN:
                default:
                    return false;
            }
        }

        /// <summary>
        /// Read data from the source
        /// </summary>
        /// <param name="position">Position in the source to read from</param>
        /// <param name="length">Length of the requested data</param>
        /// <returns>Byte array containing the requested data, null on error</returns>
        public byte[]? ReadFromDataSource(int position, int length)
        {
            // Validate the data source
            if (!DataSourceIsValid())
                return null;

            // Validate the requested segment
            if (!SegmentValid(position, length))
                return null;

            // Read and return the data
            byte[]? sectionData = null;
            switch (_dataSource)
            {
                case DataSource.ByteArray:
                    sectionData = new byte[length];
                    Array.Copy(_byteArrayData!, _byteArrayOffset + position, sectionData, 0, length);
                    break;

                case DataSource.Stream:
                    lock (_streamDataLock)
                    {
                        long currentLocation = _streamData!.Position;
                        _streamData.Seek(position, SeekOrigin.Begin);
                        sectionData = _streamData.ReadBytes(length);
                        _streamData.Seek(currentLocation, SeekOrigin.Begin);
                        break;
                    }
            }

            return sectionData;
        }

        /// <summary>
        /// Read string data from the source
        /// </summary>
        /// <param name="position">Position in the source to read from</param>
        /// <param name="length">Length of the requested data</param>
        /// <param name="charLimit">Number of characters needed to be a valid string</param>
        /// <returns>String list containing the requested data, null on error</returns>
        public List<string>? ReadStringsFromDataSource(int position, int length, int charLimit = 5)
        {
            // Read the data as a byte array first
            byte[]? sourceData = ReadFromDataSource(position, length);
            if (sourceData == null)
                return null;

            // Check for ASCII strings
            var asciiStrings = ReadStringsWithEncoding(sourceData, charLimit, Encoding.ASCII);

            // Check for UTF-8 strings
            // We are limiting the check for Unicode characters with a second byte of 0x00 for now
            var utf8Strings = ReadStringsWithEncoding(sourceData, charLimit, Encoding.UTF8);

            // Check for Unicode strings
            // We are limiting the check for Unicode characters with a second byte of 0x00 for now
            var unicodeStrings = ReadStringsWithEncoding(sourceData, charLimit, Encoding.Unicode);

            // Deduplicate the string list for storage
            List<string> sourceStrings = [.. asciiStrings, .. utf8Strings, .. unicodeStrings];
            sourceStrings = [.. sourceStrings.Distinct().OrderBy(s => s)];

            return sourceStrings;
        }

        /// <summary>
        /// Get the ending offset of the source
        /// </summary>
        /// <returns>Value greater than 0 for a valid end of file, -1 on error</returns>
        public int GetEndOfFile()
        {
            // Validate the data souece
            if (!DataSourceIsValid())
                return -1;

            // Return the effective endpoint
            switch (_dataSource)
            {
                case DataSource.ByteArray:
                    return _byteArrayData!.Length - _byteArrayOffset;

                case DataSource.Stream:
                    return (int)_streamData!.Length;

                case DataSource.UNKNOWN:
                default:
                    return -1;
            }
        }

        /// <summary>
        /// Read string data from the source with an encoding
        /// </summary>
        /// <param name="sourceData">Byte array representing the source data</param>
        /// <param name="charLimit">Number of characters needed to be a valid string</param>
        /// <param name="encoding">Character encoding to use for checking</param>
        /// <returns>String list containing the requested data, empty on error</returns>
        /// <remarks>TODO: Move to IO?</remarks>
        private List<string> ReadStringsWithEncoding(byte[] sourceData, int charLimit, Encoding encoding)
        {
            // If we have an invalid character limit, default to 5
            if (charLimit <= 0)
                charLimit = 5;

            // Create the string list to return
            var sourceStrings = new List<string>();

            // Setup cached data
            int sourceDataIndex = 0;
            string cachedString = string.Empty;

            // Check for strings
            while (sourceDataIndex < sourceData.Length)
            {
                // Read the next character
                char ch = encoding.GetChars(sourceData, sourceDataIndex, 1)[0];

                // If we have a control character or an invalid byte
                bool isValid = !char.IsControl(ch) && (ch & 0xFF00) == 0;
                if (!isValid)
                {
                    // If we have no cached string
                    if (cachedString.Length == 0)
                        continue;

                    // If we have a cached string greater than the limit
                    if (cachedString.Length >= charLimit)
                        sourceStrings.Add(cachedString);

                    cachedString = string.Empty;
                    continue;
                }

                // If a long repeating string is found, discard it
                if (cachedString.Length >= 64 && cachedString.All(c => c == cachedString[0]))
                {
                    cachedString = string.Empty;
                    continue;
                }

                // Append the character to the cached string
                cachedString += ch;
                sourceDataIndex++;
            }

            // If we have a cached string greater than the limit
            if (cachedString.Length >= charLimit)
                sourceStrings.Add(cachedString);

            return sourceStrings;
        }

        #endregion

        #region JSON Export

#if !NETFRAMEWORK
        /// <summary>
        /// Export the item information as JSON
        /// </summary>
        public override string ExportJSON() =>  System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        #endregion
    }
}