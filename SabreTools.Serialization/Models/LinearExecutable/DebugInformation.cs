﻿namespace SabreTools.Data.Models.LinearExecutable
{
    /// <summary>
    /// The debug information is defined by the debugger and is not controlled by
    /// the linear EXE format or linker. The only data defined by the linear EXE
    /// format relative to the debug information is it's offset in the EXE file and
    /// length in bytes as defined in the linear EXE header.
    /// 
    /// To support multiple debuggers the first word of the debug information is a
    /// type field which determines the format of the debug information.
    /// </summary>
    /// <see href="https://faydoc.tripod.com/formats/exe-LE.htm"/>
    /// <see href="http://www.edm2.com/index.php/LX_-_Linear_eXecutable_Module_Format_Description"/>
    public sealed class DebugInformation
    {
        /// <summary>
        /// The signature consists of a string of three (3) ASCII characters: "NB0"
        /// </summary>
        public string? Signature { get; set; }

        /// <summary>
        /// This defines the type of debugger data that exists in the remainder of the
        /// debug information.
        /// </summary>
        public DebugFormatType FormatType { get; set; }

        /// <summary>
        /// The format of the debugger data is defined by the debugger that is being used.
        /// The values defined for the type field are not enforced by the system. It is
        /// the responsibility of the linker or debugging tools to follow the convention
        /// for the type field that is defined here.
        /// </summary>
        public byte[]? DebuggerData { get; set; }
    }
}
