using System;
using System.Text;

namespace SabreTools.Serialization.Wrappers
{
    public partial class CDROM : IPrintable
    {
        /// <inheritdoc/>
        public new void PrintInformation(StringBuilder builder)
        {
            builder.AppendLine("CD-ROM Data Track Information:");
            builder.AppendLine("-------------------------");
            builder.AppendLine();

            if (iso != null)
                iso.PrintInformation(builder);
        }
    }
}
