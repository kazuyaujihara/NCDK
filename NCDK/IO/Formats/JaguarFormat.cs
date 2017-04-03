/* Copyright (C) 2003-2007  The Chemistry Development Kit (CDK) project
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
    /// See <see href="http://www.schrodinger.com/">here</see>.
    /// </summary>
    // @cdk.module ioformats
    // @cdk.githash
    // @cdk.set    io-formats
    public class JaguarFormat : SimpleChemFormatMatcher, IChemFormatMatcher
    {
        private static IResourceFormat myself = null;

        public JaguarFormat() { }

        public static IResourceFormat Instance
        {
            get
            {
                if (myself == null) myself = new JaguarFormat();
                return myself;
            }
        }

        public override string FormatName => "Jaguar";
        public override string MIMEType => null;
        public override string PreferredNameExtension => NameExtensions[0];
        public override string[] NameExtensions { get; } = new string[] { "j", "jin", "jout" };
        public override string ReaderClassName => null;
        public override string WriterClassName => null;

        public override bool Matches(int lineNumber, string line)
        {
            if (line.IndexOf("Jaguar") >= 0 && line.IndexOf("Schrodinger") >= 0)
            {
                return true;
            }
            return false;
        }

        public override bool IsXmlBased => false;
        public override int SupportedDataFeatures => DataFeatures.None;
        public override int RequiredDataFeatures => DataFeatures.None;
    }
}
