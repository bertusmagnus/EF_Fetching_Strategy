#region Imported Libraries

using System;
using System.Text;
using System.Diagnostics;
using System.IO;

#endregion

#region Namespace Declaration

namespace Org.TechAvalanche.Orm.Debug
{

    #if DEBUG

    #region Class Declaration

    /// <summary>
    /// Debugging Class used to print
    /// SQL Statements to the Debug Window.
    /// </summary>
    [Serializable]
    internal class DebuggerTextWriter : TextWriter
    {
        /// <summary>
        /// Writes a message to the Standard IO
        /// and Debugger attached.
        /// </summary>
        /// <param name="value">The message (SQL)</param>
        public override void Write(string value)
        {
            Debugger.Log(0, "", value);
            Console.Write(value);
        }

        /// <summary>
        /// Writes terminated line message to the Standard IO
        /// and Debugger attached.
        /// </summary>
        /// <param name="value">The message (SQL)</param>
        public override void WriteLine(string value)
        {
            Debugger.Log(0, "", value + Environment.NewLine);
            Console.WriteLine(value);
        }

        /// <summary>
        /// Returns the Encoding for the message
        /// sent to the debugger window.
        /// </summary>
        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }
    }

    #endregion

    #endif

}

#endregion