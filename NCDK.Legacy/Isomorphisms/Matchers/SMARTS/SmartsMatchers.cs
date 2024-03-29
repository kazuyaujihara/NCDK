/*
 * Copyright (c) 2013 European Bioinformatics Institute (EMBL-EBI)
 *                    John May <jwmay@users.sf.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or modify it
 * under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation; either version 2.1 of the License, or (at
 * your option) any later version. All we ask is that proper credit is given
 * for our work, which includes - but is not limited to - adding the above
 * copyright notice to the beginning of your source code files, and to any
 * copyright notice that you may distribute with programs based on this work.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT
 * ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or
 * FITNESS FOR A PARTICULAR PURPOSE.  See the GNU Lesser General Public
 * License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 U
 */

using System;

namespace NCDK.Isomorphisms.Matchers.SMARTS
{
    /// <summary>
    /// Bridging class between the SMARTS matcher and the parser/query tool. The
    /// class are currently split across different packages. This classes temporary
    /// functionality is to expose package private functionality through a single
    /// location.
    /// </summary>
    // @author John May
    // @cdk.module smarts
    [Obsolete]
    public sealed class SmartsMatchers
    {
        /// <summary>
        /// Do not use - temporary method until the SMARTS packages are cleaned up.
        /// </summary>
        /// <remarks>
        /// Prepares a target molecule for matching with SMARTS.
        /// </remarks>
        /// <param name="container">the container to initialise</param>
        /// <param name="ringQuery">whether the smarts will check ring size queries</param>
        public static void Prepare(IAtomContainer container, bool ringQuery)
        {
            if (ringQuery)
            {
                SMARTSAtomInvariants.ConfigureDaylightWithRingInfo(container);
            }
            else
            {
                SMARTSAtomInvariants.ConfigureDaylightWithoutRingInfo(container);
            }
        }
    }
}
