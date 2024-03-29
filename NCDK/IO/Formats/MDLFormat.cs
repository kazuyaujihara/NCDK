/* Copyright (C) 2004-2007  The Chemistry Development Kit (CDK) project
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 * but WITHOUT Any WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using NCDK.Common.Primitives;
using NCDK.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NCDK.IO.Formats
{
    /// <summary>
    /// See <see href="http://www.mdl.com/downloads/public/ctfile/ctfile.jsp">here</see>.
    /// </summary>
    // @cdk.module ioformats
    public class MDLFormat : SimpleChemFormatMatcher, IChemFormatMatcher
    {
        private static IResourceFormat myself = null;

        public MDLFormat() { }

        public static IResourceFormat Instance
        {
            get
            {
                if (myself == null) myself = new MDLFormat();
                return myself;
            }
        }

        /// <inheritdoc/>
        public override string FormatName => "MDL Molfile";

        /// <inheritdoc/>
        public override string MIMEType => "chemical/x-mdl-molfile";

        /// <inheritdoc/>
        public override string PreferredNameExtension => NameExtensions[0];

        /// <inheritdoc/>
        public override IReadOnlyList<string> NameExtensions { get; } = new string[] { "mol" };

        /// <inheritdoc/>
        public override string ReaderClassName => typeof(MDLReader).ToString();

        /// <inheritdoc/>
        public override string WriterClassName => null;

        /// <inheritdoc/>
        public override bool Matches(int lineNumber, string line)
        {
            if (lineNumber == 4 
             && line.Length > 7 
             && !line.ContainsOrdinal("2000") // MDL Mol V2000 format
             && !line.ContainsOrdinal("3000")) // MDL Mol V3000 format
            {
                // possibly a MDL mol file
                try
                {
                    string atomCountString = Strings.Substring(line, 0, 3).Trim();
                    string bondCountString = Strings.Substring(line, 3, 3).Trim();
                    int.Parse(atomCountString, NumberFormatInfo.InvariantInfo);
                    int.Parse(bondCountString, NumberFormatInfo.InvariantInfo);
                    var remainder = line.Substring(6).Trim();
                    for (int i = 0; i < remainder.Length; ++i)
                    {
                        char c = remainder[i];
                        if (!(char.IsDigit(c) || char.IsWhiteSpace(c)))
                        {
                            return false;
                        }
                    }
                }
                catch (FormatException)
                {
                    // Integers not found on fourth line; therefore not a MDL file
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool IsXmlBased => false;

        /// <inheritdoc/>
        public override DataFeatures SupportedDataFeatures
                => RequiredDataFeatures | DataFeatures.Has2DCoordinates | DataFeatures.Has3DCoordinates
                    | DataFeatures.HasGraphRepresentation;

        /// <inheritdoc/>
        public override DataFeatures RequiredDataFeatures
            => DataFeatures.HasAtomElementSymbol;
    }
}
