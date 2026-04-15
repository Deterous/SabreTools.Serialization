using System.IO;
using SabreTools.IO.Extensions;

namespace SabreTools.Serialization.Writers
{
    /// <summary>
    /// Base class for all binary serializers
    /// </summary>
    /// <typeparam name="TModel">Type of the model to serialize</typeparam>
    /// <remarks>
    /// This class allows all inheriting types to only implement <see cref="IStreamWriter<>"/>
    /// and still implicitly implement <see cref="IByteWriter<>"/>  and <see cref="IFileWriter<>"/>
    /// </remarks>
    public abstract class BaseBinaryWriter<TModel> :
        IByteWriter<TModel>,
        IFileWriter<TModel>,
        IStreamWriter<TModel>
    {
        /// <inheritdoc/>
        public bool Debug { get; set; } = false;

        #region IByteWriter

        /// <inheritdoc/>
        public virtual byte[]? SerializeArray(TModel? obj)
        {
            using var stream = SerializeStream(obj);
            if (stream is null)
                return null;

            byte[] bytes = new byte[stream.Length];
            int read = stream.Read(bytes, 0, bytes.Length);
            return bytes;
        }

        #endregion

        #region IFileWriter

        /// <inheritdoc/>
        public virtual bool SerializeFile(TModel? obj, string? path)
        {
            System.Console.WriteLine("1");
            if (string.IsNullOrEmpty(path))
                return false;

            System.Console.WriteLine("2");
            using var stream = SerializeStream(obj);
            System.Console.WriteLine("3");
            if (stream is null)
                return false;
            System.Console.WriteLine("4");

            using var fs = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None);
            System.Console.WriteLine("5");
            stream.BlockCopy(fs);
            System.Console.WriteLine("6");
            fs.Flush();
            System.Console.WriteLine("7");

            return true;
        }

        #endregion

        #region IStreamWriter

        /// <inheritdoc/>
        public abstract Stream? SerializeStream(TModel? obj);

        #endregion
    }
}
