#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009-2010 Jon Skeet
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using System.Globalization;
using System.IO;
using System.Text;

namespace NodaTime.ZoneInfoCompiler
{
    /// <summary>
    ///   Proveides a base clase for logging instances.
    /// </summary>
    public abstract class LogBase : ILog
    {
        #region LogType enum
        /// <summary>
        ///   Defines the types of logging message that can be produced.
        /// </summary>
        public enum LogType
        {
            /// <summary>
            ///   This message is purely information and can be ignored.
            /// </summary>
            Informational,

            /// <summary>
            ///   This indicates a possible problem but not a definite error.
            /// </summary>
            Warning,

            /// <summary>
            ///   This defines an error. Processing will terminate without completing.
            /// </summary>
            Error
        }
        #endregion

        #region ILog Members
        /// <summary>
        ///   Writes an error message to the log. The string is formatted using string.Format().
        /// </summary>
        /// <param name = "format">The format string to log.</param>
        /// <param name = "arguments">The arguments for the string format if any.</param>
        public virtual void Error(string format, params object[] arguments)
        {
            LogMessage(LogType.Error, Format(format, arguments));
        }

        /// <summary>
        ///   Gets the <see cref = "TextWriter" /> that sends its output to <see cref = "Error" />.
        /// </summary>
        /// <value>The <see cref = "TextWriter" />.</value>
        public TextWriter ErrorWriter
        {
            get { return new LogTextWriter(Error); }
        }

        /// <summary>
        ///   Gets or sets the name of the file where the logging ocurred. If null then the log message
        ///   is outside of file processing.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        ///   Writes an information message to the log. The string is formatted using string.Format().
        /// </summary>
        /// <param name = "format">The format string to log.</param>
        /// <param name = "arguments">The arguments for the string format if any.</param>
        public virtual void Info(string format, params object[] arguments)
        {
            LogMessage(LogType.Informational, Format(format, arguments));
        }

        /// <summary>
        ///   Gets the <see cref = "TextWriter" /> that sends its output to <see cref = "Info" />.
        /// </summary>
        /// <value>The <see cref = "TextWriter" />.</value>
        public TextWriter InfoWriter
        {
            get { return new LogTextWriter(Info); }
        }

        /// <summary>
        ///   Gets or sets the line number currently being processed.
        /// </summary>
        /// <value>The line number.</value>
        public int LineNumber { get; set; }

        /// <summary>
        ///   Writes a warning message to the log. The string is formatted using string.Format().
        /// </summary>
        /// <param name = "format">The format string to log.</param>
        /// <param name = "arguments">The arguments for the string format if any.</param>
        public virtual void Warn(string format, params object[] arguments)
        {
            LogMessage(LogType.Warning, Format(format, arguments));
        }

        /// <summary>
        ///   Gets the <see cref = "TextWriter" /> that sends its output to <see cref = "Warn" />.
        /// </summary>
        /// <value>The <see cref = "TextWriter" />.</value>
        public TextWriter WarnWriter
        {
            get { return new LogTextWriter(Warn); }
        }
        #endregion

        /// <summary>
        ///   Helper method to format the specified message string with optional file location information.
        /// </summary>
        /// <param name = "format">The format string.</param>
        /// <param name = "arguments">The optional arguments.</param>
        /// <returns>The formatted message string.</returns>
        private string Format(string format, object[] arguments)
        {
            var builder = new StringBuilder();
            var message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            builder.Append(message);
            if (LineNumber > 0)
            {
                builder.Append(" at line ").Append(LineNumber);
                if (FileName != null)
                {
                    builder.Append(" of ").Append(FileName);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        ///   Called to actually log the message to where ever the logger sends its output. The
        ///   destination can be different based on the message type and different loggers may not
        ///   send all messages to the destination.
        /// </summary>
        /// <param name = "type">The type of log message.</param>
        /// <param name = "message">The message to log.</param>
        protected abstract void LogMessage(LogType type, string message);

        #region Nested type: LogOutputMethod
        private delegate void LogOutputMethod(string format, params object[] arguments);
        #endregion

        #region Nested type: LogTextWriter
        /// <summary>
        ///   Private class to implement a <see cref = "TextWriter" /> that sends its output
        ///   to the given output method.
        /// </summary>
        private class LogTextWriter : TextWriter
        {
            private readonly StringBuilder builder = new StringBuilder();
            private readonly LogOutputMethod output;

            /// <summary>
            ///   Initializes a new instance of the <see cref = "LogTextWriter" /> class.
            /// </summary>
            /// <param name = "output">The log.</param>
            public LogTextWriter(LogOutputMethod output) : base(CultureInfo.InvariantCulture)
            {
                this.output = output;
            }

            /// <summary>
            ///   When overridden in a derived class, returns the <see cref = "T:System.Text.Encoding" /> in which the output is written.
            /// </summary>
            /// <returns>The Encoding in which the output is written.</returns>
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }

            /// <summary>
            ///   Closes the current writer and releases any system resources associated with the writer.
            /// </summary>
            /// <filterpriority>1</filterpriority>
            public override void Close()
            {
                var message = builder.ToString();
                output("{0}", message);
                base.Close();
            }

            /// <summary>
            ///   Writes a character to the text stream.
            /// </summary>
            /// <param name = "value">The character to write to the text stream.</param>
            /// <exception cref = "T:System.ObjectDisposedException">
            ///   The <see cref = "T:System.IO.TextWriter" /> is closed.
            /// </exception>
            /// <exception cref = "T:System.IO.IOException">
            ///   An I/O error occurs.
            /// </exception>
            public override void Write(char value)
            {
                switch (value)
                {
                    case '\r':
                        break;
                    case '\n':
                        var message = builder.ToString();
                        output("{0}", message);
                        builder.Remove(0, message.Length);
                        break;
                    default:
                        builder.Append(value);
                        break;
                }
            }
        }
        #endregion
    }
}
