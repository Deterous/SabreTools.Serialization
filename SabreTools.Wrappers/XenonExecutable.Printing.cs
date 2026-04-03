using System;
using System.Text;
using SabreTools.Data.Models.XenonExecutable;
using SabreTools.Text.Extensions;

namespace SabreTools.Wrappers
{
    public partial class XenonExecutable : IPrintable
    {
#if NETCOREAPP
        /// <inheritdoc/>
        public string ExportJSON() => System.Text.Json.JsonSerializer.Serialize(Model, _jsonSerializerOptions);
#endif

        /// <inheritdoc/>
        public void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("Xenon (Xbox 360) Executable Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            Print(builder, Model.Header);
            Print(builder, Model.Header.OptionalHeaders);
            Print(builder, Model.Certificate);
        }

        private static void Print(StringBuilder builder, Header? header)
        {
            builder.AppendLine("  Header Information:");
            builder.AppendLine("  -------------------------");
            if (header is null)
            {
                builder.AppendLine("  No header");
                builder.AppendLine();
                return;
            }

            builder.AppendLine(header.MagicNumber, "  Magic Number");
            builder.AppendLine(header.ModuleFlags, "  Module Flags");
            builder.AppendLine(header.PEDataOffset, "  PE Data Offset");
            builder.AppendLine(header.Reserved, "  Reserved");
            builder.AppendLine(header.CertificateOffset, "  Certificate Offset");
            builder.AppendLine(header.OptionalHeaderCount, "  Optional Header Count");

            builder.AppendLine();
        }

        private static void Print(StringBuilder builder, OptionalHeader[]? optionalHeaders)
        {
            builder.AppendLine("  Optional Headers:");
            builder.AppendLine("  -------------------------");
            if (optionalHeaders is null)
            {
                builder.AppendLine("  No optional headers");
                builder.AppendLine();
                return;
            }

            foreach (var optionalHeader in optionalHeaders)
            {
                builder.AppendLine(optionalHeader.HeaderID, "  Header ID");
                if (Constants.OptionalHeaderTypes.TryGetValue(optionalHeader.HeaderID, out string? headerType))
                    builder.AppendLine(headerType, "  Header Type (Parsed)");
                else
                    builder.AppendLine("[UNKNOWN]", "  Header Type (Parsed)");

                if (optionalHeader.HeaderDataBytes is not null)
                {
                    builder.AppendLine(optionalHeader.HeaderData, "  Header Offset");
                    builder.AppendLine(optionalHeader.HeaderDataBytes, "  Header Data");
                }
                else
                {
                    builder.AppendLine(optionalHeader.HeaderData, "  Header Data");
                }

                builder.AppendLine();
            }

            builder.AppendLine();
        }

        private static void Print(StringBuilder builder, Certificate? certificate)
        {
            builder.AppendLine("  Certificate Information:");
            builder.AppendLine("  -------------------------");
            if (certificate is null)
            {
                builder.AppendLine("  No certificate");
                builder.AppendLine();
                return;
            }

            builder.AppendLine(certificate.Length, "  Length");

            builder.AppendLine();
        }
    }
}
