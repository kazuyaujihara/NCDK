/* Generated By:JavaCC: Do not edit this line. TokenMgrError.java Version 5.0 */
/* JavaCCOptions: */
/* Copyright (C) 2004-2007  The Chemistry Development Kit (CDK) project
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 * (or see http://www.gnu.org/copyleft/lesser.html)
 */

using System;
using System.Text;

namespace NCDK.Smiles.SMARTS.Parser
{
    /// <summary>Token Manager Error.</summary>
    [Obsolete]
    public class TokenManagerException : Exception
    {
        /// <summary>
        /// Ordinals for various reasons why an Error of this type can be thrown.
        /// </summary>
        internal enum ErrorCodes
        {
            /// <summary>
            /// Lexical error occurred.
            /// </summary>
            LexicalError = 0,

            /// <summary>
            /// An attempt was made to create a second instance of a static token manager.
            /// </summary>
            StaticLexicalError = 1,

            /// <summary>
            /// Tried to change to an invalid lexical state.
            /// </summary>
            InvalidLexicalState = 2,

            /// <summary>
            /// Detected (and bailed out of) an infinite loop in the token manager.
            /// </summary>
            LoopDetected = 3,
        }

        /// <summary>
        /// Indicates the reason why the exception is thrown. It will have
        /// one of the above 4 values.
        /// </summary>
        internal ErrorCodes errorCode;

        /// <summary>
        /// Replaces unprintable characters by their escaped (or unicode escaped)
        /// equivalents in the given string
        /// </summary>
        protected static string AddEscapes(string str)
        {
            var retval = new StringBuilder();
            char ch;
            for (int i = 0; i < str.Length; i++)
            {
                switch (str[i])
                {
                    case '\0':
                        continue;
                    case '\b':
                        retval.Append("\\b");
                        continue;
                    case '\t':
                        retval.Append("\\t");
                        continue;
                    case '\n':
                        retval.Append("\\n");
                        continue;
                    case '\f':
                        retval.Append("\\f");
                        continue;
                    case '\r':
                        retval.Append("\\r");
                        continue;
                    case '\"':
                        retval.Append("\\\"");
                        continue;
                    case '\'':
                        retval.Append("\\\'");
                        continue;
                    case '\\':
                        retval.Append("\\\\");
                        continue;
                    default:
                        if ((ch = str[i]) < 0x20 || ch > 0x7e)
                        {
                            string s = "0000" + Convert.ToString(ch, 16);
                            retval.Append("\\u" + s.Substring(s.Length - 4));
                        }
                        else
                        {
                            retval.Append(ch);
                        }
                        continue;
                }
            }
            return retval.ToString();
        }

        /// <summary>
        /// Returns a detailed message for the Error when it is thrown by the
        /// token manager to indicate a lexical error.
        /// </summary>
        /// <remarks>
        /// Parameters :
        /// <list type="bullet">
        /// <item><term></term><description></description></item>
        /// <item><term></term><description></description></item>
        /// <item><term>EOFSeen</term><description>indicates if EOF caused the lexical error</description></item>
        /// <item><term>curLexState</term><description>lexical state in which this error occurred</description></item>
        /// <item><term>errorLine</term><description>line number when the error occurred</description></item>
        /// <item><term>errorColumn</term><description>column number when the error occurred</description></item>
        /// <item><term>errorAfter</term><description>prefix that was seen before this error occurred</description></item>
        /// <item><term>curchar</term><description>the offending character</description></item>
        /// </list>
        /// <note type="note">
        /// You can customize the lexical error message by modifying this method.
        /// </note>
        /// </remarks>
        protected static string LexicalError(bool EOFSeen, int lexState, int errorLine, int errorColumn, string errorAfter, char curChar)
        {
            return ("Lexical error at line " +
                  errorLine + ", column " +
                  errorColumn + ".  Encountered: " +
                  (EOFSeen ? "<EOF> " : ("\"" + AddEscapes(new string(new[] { curChar })) + "\"") + " (" + (int)curChar + "), ") +
                  "after : \"" + AddEscapes(errorAfter) + "\"");
        }

        /// <summary>
        /// You can also modify the body of this method to customize your error messages.
        /// </summary>
        /// <remarks>
        /// For example, cases like <see cref="ErrorCodes.LoopDetected"/> and <see cref="ErrorCodes.InvalidLexicalState"/> are not
        /// of end-users concern, so you can return something like :
        /// <para>
        /// <pre>
        ///     "Internal Error : Please file a bug report .... "
        /// </pre>     
        /// </para>
        /// from this method for such cases in the release version of your parser.
        /// </remarks>
        public override string Message => base.Message;

        // Constructors of various flavors follow.

        /// <summary>No arg constructor.</summary>
        public TokenManagerException()
        {
        }

        /// <summary>Constructor with message and reason.</summary>
        internal TokenManagerException(string message, ErrorCodes reason)
            : base(message)
        {
            errorCode = reason;
        }

        /// <summary>Full Constructor.</summary>
        internal TokenManagerException(bool EOFSeen, int lexState, int errorLine, int errorColumn, string errorAfter, char curChar, ErrorCodes reason)
            : this(LexicalError(EOFSeen, lexState, errorLine, errorColumn, errorAfter, curChar), reason)
        { }

        public TokenManagerException(string message) : base(message)
        {
        }

        public TokenManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
