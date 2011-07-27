﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.454
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace NodaTime.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("NodaTime.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Format string is missing a close quote: {0}.
        /// </summary>
        internal static string Format_BadQuote {
            get {
                return ResourceManager.GetString("Format_BadQuote", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Format error..
        /// </summary>
        internal static string FormatDefaultExceptionMessage {
            get {
                return ResourceManager.GetString("FormatDefaultExceptionMessage", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Argument cannot be null..
        /// </summary>
        internal static string Noda_ArgumentNull {
            get {
                return ResourceManager.GetString("Noda_ArgumentNull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Cannot change a read only object..
        /// </summary>
        internal static string Noda_CannotChangeReadOnly {
            get {
                return ResourceManager.GetString("Noda_CannotChangeReadOnly", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The string cannot be empty..
        /// </summary>
        internal static string Noda_StringEmpty {
            get {
                return ResourceManager.GetString("Noda_StringEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The string cannot be empty or only contain white space..
        /// </summary>
        internal static string Noda_StringEmptyOrWhitespace {
            get {
                return ResourceManager.GetString("Noda_StringEmptyOrWhitespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to +H:mm:ss.fff.
        /// </summary>
        internal static string OffsetPatternFull {
            get {
                return ResourceManager.GetString("OffsetPatternFull", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to +H:mm:ss.
        /// </summary>
        internal static string OffsetPatternLong {
            get {
                return ResourceManager.GetString("OffsetPatternLong", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to +H:mm.
        /// </summary>
        internal static string OffsetPatternMedium {
            get {
                return ResourceManager.GetString("OffsetPatternMedium", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to +H.
        /// </summary>
        internal static string OffsetPatternShort {
            get {
                return ResourceManager.GetString("OffsetPatternShort", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value &quot;{0}&quot; cannot be parsed into an instance of {1} using pattern &quot;{2}&quot;.
        /// </summary>
        internal static string Parse_CannotParseValue {
            get {
                return ResourceManager.GetString("Parse_CannotParseValue", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The pattern flag &apos;{0}&apos; cannot appear twice and parse different values..
        /// </summary>
        internal static string Parse_DoubleAssignment {
            get {
                return ResourceManager.GetString("Parse_DoubleAssignment", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The list of formats cannot be empty..
        /// </summary>
        internal static string Parse_EmptyFormatsArray {
            get {
                return ResourceManager.GetString("Parse_EmptyFormatsArray", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format string has an escape character (backslash &apos;\&apos;) at the end of the string..
        /// </summary>
        internal static string Parse_EscapeAtEndOfString {
            get {
                return ResourceManager.GetString("Parse_EscapeAtEndOfString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string does not match an escaped character in the format string: &quot;\{0}&quot;.
        /// </summary>
        internal static string Parse_EscapedCharacterMismatch {
            get {
                return ResourceManager.GetString("Parse_EscapedCharacterMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format matches a prefix of the value string but not the entire string. Part not matching: &quot;{0}&quot;..
        /// </summary>
        internal static string Parse_ExtraValueCharacters {
            get {
                return ResourceManager.GetString("Parse_ExtraValueCharacters", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The items of the format string array cannot be null or empty..
        /// </summary>
        internal static string Parse_FormatElementInvalid {
            get {
                return ResourceManager.GetString("Parse_FormatElementInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format string is invalid: &quot;{0}&quot;.
        /// </summary>
        internal static string Parse_FormatInvalid {
            get {
                return ResourceManager.GetString("Parse_FormatInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format string is empty..
        /// </summary>
        internal static string Parse_FormatStringEmpty {
            get {
                return ResourceManager.GetString("Parse_FormatStringEmpty", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The &apos;h&apos; pattern flag (12 hour format) is not supported by the {0} type..
        /// </summary>
        internal static string Parse_Hour12PatternNotSupported {
            get {
                return ResourceManager.GetString("Parse_Hour12PatternNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string does not match a simple character in the format string &quot;{0}&quot;..
        /// </summary>
        internal static string Parse_MismatchedCharacter {
            get {
                return ResourceManager.GetString("Parse_MismatchedCharacter", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string does not match the required number from the format string &quot;{0}&quot;..
        /// </summary>
        internal static string Parse_MismatchedNumber {
            get {
                return ResourceManager.GetString("Parse_MismatchedNumber", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string does not match a space in the format string..
        /// </summary>
        internal static string Parse_MismatchedSpace {
            get {
                return ResourceManager.GetString("Parse_MismatchedSpace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format string contains a decimal separator that does not match the value and the decimal separator is not followed by an &quot;F&quot; pattern character..
        /// </summary>
        internal static string Parse_MissingDecimalSeparator {
            get {
                return ResourceManager.GetString("Parse_MissingDecimalSeparator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The format string is missing the end quote character &quot;{0}&quot;..
        /// </summary>
        internal static string Parse_MissingEndQuote {
            get {
                return ResourceManager.GetString("Parse_MissingEndQuote", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The required value sign is missing..
        /// </summary>
        internal static string Parse_MissingSign {
            get {
                return ResourceManager.GetString("Parse_MissingSign", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to None of the specified formats matches the given value string..
        /// </summary>
        internal static string Parse_NoMatchingFormat {
            get {
                return ResourceManager.GetString("Parse_NoMatchingFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A percent sign (%) appears at the end of the format string..
        /// </summary>
        internal static string Parse_PercentAtEndOfString {
            get {
                return ResourceManager.GetString("Parse_PercentAtEndOfString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A percent sign (%) is followed by another percent sign in the format string..
        /// </summary>
        internal static string Parse_PercentDoubled {
            get {
                return ResourceManager.GetString("Parse_PercentDoubled", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to A positive value sign is not valid at this point..
        /// </summary>
        internal static string Parse_PositiveSignInvalid {
            get {
                return ResourceManager.GetString("Parse_PositiveSignInvalid", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The standard format &quot;{0}&quot; for type {1} does not support a precision..
        /// </summary>
        internal static string Parse_PrecisionNotSupported {
            get {
                return ResourceManager.GetString("Parse_PrecisionNotSupported", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string does not match a quoted string in the pattern..
        /// </summary>
        internal static string Parse_QuotedStringMismatch {
            get {
                return ResourceManager.GetString("Parse_QuotedStringMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to There were more consecutive copies of the pattern character &quot;{0}&quot; than the maximum allowed ({1}) in the format string..
        /// </summary>
        internal static string Parse_RepeatCountExceeded {
            get {
                return ResourceManager.GetString("Parse_RepeatCountExceeded", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The standard format &quot;{0}&quot; for type {1} cannot contain white space..
        /// </summary>
        internal static string Parse_StandardFormatWhitespace {
            get {
                return ResourceManager.GetString("Parse_StandardFormatWhitespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string does not match a time separator in the format string..
        /// </summary>
        internal static string Parse_TimeSeparatorMismatch {
            get {
                return ResourceManager.GetString("Parse_TimeSeparatorMismatch", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to More characters were expected in the parsable string [{0}]..
        /// </summary>
        internal static string Parse_UnexpectedEndOfString {
            get {
                return ResourceManager.GetString("Parse_UnexpectedEndOfString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Internal Error: The specified ParseFailureKind is unknown [{0}]..
        /// </summary>
        internal static string Parse_UnknownFailure {
            get {
                return ResourceManager.GetString("Parse_UnknownFailure", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The standard format &quot;{0}&quot; is not valid for the {1} type..
        /// </summary>
        internal static string Parse_UnknownStandardFormat {
            get {
                return ResourceManager.GetString("Parse_UnknownStandardFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value {0} is out of the legal range for the {1} type..
        /// </summary>
        internal static string Parse_ValueOutOfRange {
            get {
                return ResourceManager.GetString("Parse_ValueOutOfRange", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to The value string is empty..
        /// </summary>
        internal static string Parse_ValueStringEmpty {
            get {
                return ResourceManager.GetString("Parse_ValueStringEmpty", resourceCulture);
            }
        }
    }
}
