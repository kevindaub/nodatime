﻿#region Copyright and license information
// Copyright 2001-2009 Stephen Colebourne
// Copyright 2009 Jon Skeet
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
using System.Text;

namespace NodaTime.ZoneInfoCompiler
{
    /// <summary>
    /// Proveides a base clase for logging instances.
    /// </summary>
    internal abstract class LogBase
        : ILog
    {
        /// <summary>
        /// Called to actually log the message to where ever the logger sends its output. The
        /// destination can be different based on the message type and different loggers may not
        /// send all messages to the destination.
        /// </summary>
        /// <param name="type">The type of log message.</param>
        /// <param name="message">The message to log.</param>
        protected abstract void LogMessage(LogType type, string message);

        #region ILog Members

        /// <summary>
        /// Gets or sets the name of the file where the logging ocurred. If null then the log message
        /// is outside of file processing.
        /// </summary>
        /// <value>The name of the file.</value>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the line number currently being processed.
        /// </summary>
        /// <value>The line number.</value>
        public int LineNumber { get; set; }

        /// <summary>
        /// Writes an information message to the log. The string is formatted using string.Format().
        /// </summary>
        /// <param name="format">The format string to log.</param>
        /// <param name="arguments">The arguments for the string format if any.</param>
        public virtual void Info(string format, params object[] arguments)
        {
            LogMessage(LogType.Informational, Format(format, arguments));
        }

        /// <summary>
        /// Writes a warning message to the log. The string is formatted using string.Format().
        /// </summary>
        /// <param name="format">The format string to log.</param>
        /// <param name="arguments">The arguments for the string format if any.</param>
        public virtual void Warn(string format, params object[] arguments)
        {
            LogMessage(LogType.Warning, Format(format, arguments));
        }

        /// <summary>
        /// Writes an error message to the log. The string is formatted using string.Format().
        /// </summary>
        /// <param name="format">The format string to log.</param>
        /// <param name="arguments">The arguments for the string format if any.</param>
        public virtual void Error(string format, params object[] arguments)
        {
            LogMessage(LogType.Error, Format(format, arguments));
        }

        #endregion

        /// <summary>
        /// Helper method to format the specified message string with optional file location information.
        /// </summary>
        /// <param name="format">The format string.</param>
        /// <param name="arguments">The optional arguments.</param>
        /// <returns>The formatted message string.</returns>
        private string Format(string format, object[] arguments)
        {
            StringBuilder builder = new StringBuilder();
            string message = string.Format(CultureInfo.InvariantCulture, format, arguments);
            builder.Append(message);
            if (LineNumber > 0) {
                builder.Append(" at line ").Append(LineNumber);
                if (FileName != null) {
                    builder.Append(" of ").Append(FileName);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Defines the types of logging message that can be produced.
        /// </summary>
        protected enum LogType
        {
            /// <summary>
            /// This message is purely information and can be ignored.
            /// </summary>
            Informational,

            /// <summary>
            /// This indicates a possible problem but not a definite error.
            /// </summary>
            Warning,

            /// <summary>
            /// This defines an error. Processing will terminate without completing.
            /// </summary>
            Error
        }
    }
}
