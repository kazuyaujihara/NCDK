



// .NET Framework port by Kazuya Ujihara
// Copyright (C) 2016-2017  Kazuya Ujihara <ujihara.kazuya@gmail.com>

/* Copyright (C) 2003-2007  Egon Willighagen <egonw@users.sf.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either version 2.1
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using System.Collections.Generic;

namespace NCDK.Default
{
    /// <summary>
    /// A Mapping is an relation between two ChemObjects in a non-chemical
    /// entity. It is not a Bond, nor a Association, merely a relation.
    /// An example of such a mapping, is the mapping between corresponding atoms
    /// in a Reaction.
    /// </summary>
    // @cdk.keyword reaction, atom mapping 
    // @author  Egon Willighagen
    // @cdk.created 2003-08-16 
    public class Mapping
        : ChemObject, ICloneable, IMapping
    {
        private IChemObject[] relation = new IChemObject[2];

        /// <summary>
        /// Constructs an unconnected lone pair.
        /// </summary>
        /// <param name="objectOne">The first IChemObject of the mapping</param>
        /// <param name="objectTwo">The second IChemObject of the mapping</param>
        public Mapping(IChemObject objectOne, IChemObject objectTwo)
        {
            relation[0] = objectOne;
            relation[1] = objectTwo;
        }

        /// <summary>
        /// Retrieves the first or second of the related IChemObjects.
        /// </summary>
        /// <param name="pos">The position of the IChemObject.</param>
        /// <returns>The IChemObject to retrieve.</returns>
        public IChemObject this[int pos] => relation[pos];

        /// <summary>
        /// Enumerable to the two IChemObjects.
        /// </summary>
        /// <returns>An enumerable to two IChemObjects that define the mapping</returns>
        public IEnumerable<IChemObject> GetRelatedChemObjects()
        {
            return relation;
        }

        public override ICDKObject Clone(CDKObjectMap map)
        {
            var clone = (Mapping)base.Clone(map);
            clone.relation = new IAtom[relation.Length];
            for (var i = 0; i < relation.Length; i++)
                clone.relation[i] = (IAtom)relation[i].Clone(map);
            return clone;
        }

        public new IMapping Clone() => (IMapping)Clone(new CDKObjectMap());
        object ICloneable.Clone() => Clone();
    }
}
namespace NCDK.Silent
{
    /// <summary>
    /// A Mapping is an relation between two ChemObjects in a non-chemical
    /// entity. It is not a Bond, nor a Association, merely a relation.
    /// An example of such a mapping, is the mapping between corresponding atoms
    /// in a Reaction.
    /// </summary>
    // @cdk.keyword reaction, atom mapping 
    // @author  Egon Willighagen
    // @cdk.created 2003-08-16 
    public class Mapping
        : ChemObject, ICloneable, IMapping
    {
        private IChemObject[] relation = new IChemObject[2];

        /// <summary>
        /// Constructs an unconnected lone pair.
        /// </summary>
        /// <param name="objectOne">The first IChemObject of the mapping</param>
        /// <param name="objectTwo">The second IChemObject of the mapping</param>
        public Mapping(IChemObject objectOne, IChemObject objectTwo)
        {
            relation[0] = objectOne;
            relation[1] = objectTwo;
        }

        /// <summary>
        /// Retrieves the first or second of the related IChemObjects.
        /// </summary>
        /// <param name="pos">The position of the IChemObject.</param>
        /// <returns>The IChemObject to retrieve.</returns>
        public IChemObject this[int pos] => relation[pos];

        /// <summary>
        /// Enumerable to the two IChemObjects.
        /// </summary>
        /// <returns>An enumerable to two IChemObjects that define the mapping</returns>
        public IEnumerable<IChemObject> GetRelatedChemObjects()
        {
            return relation;
        }

        public override ICDKObject Clone(CDKObjectMap map)
        {
            var clone = (Mapping)base.Clone(map);
            clone.relation = new IAtom[relation.Length];
            for (var i = 0; i < relation.Length; i++)
                clone.relation[i] = (IAtom)relation[i].Clone(map);
            return clone;
        }

        public new IMapping Clone() => (IMapping)Clone(new CDKObjectMap());
        object ICloneable.Clone() => Clone();
    }
}
