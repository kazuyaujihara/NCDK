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
using NCDK.Tools;

namespace NCDK.IO.Formats
{
    /// <summary>
    /// See <see href="http://www.mdl.com/downloads/public/ctfile/ctfile.jsp">here</see>.
    /// </summary>
    // @cdk.module ioformats
    // @cdk.githash
    public class MDLRXNFormat : SimpleChemFormatMatcher, IChemFormatMatcher
    {
        private static IResourceFormat myself = null;

        public MDLRXNFormat() { }

        public static IResourceFormat Instance
        {
            get
            {
                if (myself == null) myself = new MDLRXNFormat();
                return myself;
            }
        }

        /// <inheritdoc/>
        public override string FormatName => "MDL Reaction format";

        /// <inheritdoc/>
        public override string MIMEType => "chemical/x-mdl-rxnfile";

        /// <inheritdoc/>
        public override string PreferredNameExtension => NameExtensions[0];

        /// <inheritdoc/>
        public override string[] NameExtensions => new string[] { "rxn" };

        /// <inheritdoc/>
        public override string ReaderClassName => "NCDK.IO.MDLRXNReader";

        /// <inheritdoc/>
        public override string WriterClassName => null;

        /// <inheritdoc/>
        public override bool Matches(int lineNumber, string line)
        {
            if (line.StartsWith("$RXN"))
            {
                return true;
            }
            return false;
        }

        /// <inheritdoc/>
        public override bool IsXmlBased => false;

        /// <inheritdoc/>
        public override int SupportedDataFeatures => DataFeatures.None;

        /// <inheritdoc/>
        public override int RequiredDataFeatures => DataFeatures.None;
    }
}
