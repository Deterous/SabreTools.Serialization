using SabreTools.Data.Models.ZArchive;

namespace SabreTools.Wrappers
{
    public partial class ZArchive : IPrintable
    {
#if NETCOREAPP
        /// <inheritdoc/>
        public string ExportJSON() => System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        /// <inheritdoc/>
        public void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("ZArchive Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            // Print(builder, Model.Archive);
        }
    }
}
