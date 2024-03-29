



// .NET Framework port by Kazuya Ujihara
// Copyright (C) 2016-2017  Kazuya Ujihara <ujihara.kazuya@gmail.com>

/* Copyright (C) 1997-2007  Christoph Steinbeck <steinbeck@users.sf.net>
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
using System.Linq;

namespace NCDK.Default
{
    /// <summary>
    /// Class representing a ring structure in a molecule.
    /// A ring is a linear sequence of
    /// N atoms interconnected to each other by covalent bonds,
    /// such that atom i (1 &lt; i &lt; N) is bonded to
    /// atom i-1 and atom i + 1 and atom 1 is bonded to atom N and atom 2.
    /// </summary>
    // @cdk.module  data
    // @cdk.keyword ring 
    public class Ring
        : AtomContainer, IRing
    {
        /// <summary>
        /// Constructs an empty ring.
        /// </summary>
        public Ring()
            : base()
        { }

        /// <summary>
        /// Constructs a ring from the atoms in an <see cref="IAtomContainer"/> object.
        /// </summary>
        /// <param name="atomContainer">The <see cref="IAtomContainer"/> object containing the atoms to form the ring</param>
        public Ring(IAtomContainer atomContainer)
            : base(atomContainer)
        { }

        /// <summary>
        /// Constructs a ring from the atoms and bonds.
        /// </summary>
        /// <param name="atoms">atoms</param>
        /// <param name="bonds">bonds</param>
        public Ring(
           IEnumerable<IAtom> atoms,
           IEnumerable<IBond> bonds)
           : base(atoms, bonds)
        { }

        /// <summary>
        /// Constructs a ring that will have a certain number of atoms of the given elements.
        /// </summary>
        /// <param name="ringSize">The number of atoms and bonds the ring will have</param>
        /// <param name="elementSymbol">The element of the atoms the ring will have</param>
        public Ring(int ringSize, string elementSymbol)
            : base()
        {
            var prevAtom = new Atom(elementSymbol);
            atoms.Add(prevAtom);
            for (int i = 1; i < ringSize; i++)
            {
                var atom = new Atom(elementSymbol);
                atoms.Add(atom);
                bonds.Add(new Bond(prevAtom, atom, BondOrder.Single));
                prevAtom = atom;
            }
            bonds.Add(new Bond(prevAtom, atoms[0], BondOrder.Single));
        }

        /// <summary>
        /// the number of atoms\edges in this ring.
        /// </summary>
        public int RingSize => atoms.Count;

        /// <summary>
        /// Returns the next bond in order, relative to a given bond and atom.
        /// Example: Let the ring be composed of 0-1, 1-2, 2-3 and 3-0. A request getNextBond(1-2, 2)
        /// will return Bond 2-3.
        /// </summary>
        /// <param name="bond"> A bond for which an atom from a consecutive bond is sought</param>
        /// <param name="atom">A atom from the bond above to assign a search direction</param>
        /// <returns>The next bond in the order given by the above assignment</returns>
        public IBond GetNextBond(IBond bond, IAtom atom)
            => bonds.Where(n => n.Contains(atom) && !n.Equals(bond)).FirstOrDefault();

        /// <summary>
        /// The sum of all bond orders in the ring.
        /// </summary>
        public int GetBondOrderSum()
            => bonds.Where(n => !n.Order.IsUnset()).Select(n => n.Order.Numeric()).Sum();

        public override ICDKObject Clone(CDKObjectMap map)
        {
            return (Ring)base.Clone(map);
        }

        public new IRing Clone() => (IRing)Clone(new CDKObjectMap());
        object ICloneable.Clone() => Clone();
    }
}
namespace NCDK.Silent
{
    /// <summary>
    /// Class representing a ring structure in a molecule.
    /// A ring is a linear sequence of
    /// N atoms interconnected to each other by covalent bonds,
    /// such that atom i (1 &lt; i &lt; N) is bonded to
    /// atom i-1 and atom i + 1 and atom 1 is bonded to atom N and atom 2.
    /// </summary>
    // @cdk.module  data
    // @cdk.keyword ring 
    public class Ring
        : AtomContainer, IRing
    {
        /// <summary>
        /// Constructs an empty ring.
        /// </summary>
        public Ring()
            : base()
        { }

        /// <summary>
        /// Constructs a ring from the atoms in an <see cref="IAtomContainer"/> object.
        /// </summary>
        /// <param name="atomContainer">The <see cref="IAtomContainer"/> object containing the atoms to form the ring</param>
        public Ring(IAtomContainer atomContainer)
            : base(atomContainer)
        { }

        /// <summary>
        /// Constructs a ring from the atoms and bonds.
        /// </summary>
        /// <param name="atoms">atoms</param>
        /// <param name="bonds">bonds</param>
        public Ring(
           IEnumerable<IAtom> atoms,
           IEnumerable<IBond> bonds)
           : base(atoms, bonds)
        { }

        /// <summary>
        /// Constructs a ring that will have a certain number of atoms of the given elements.
        /// </summary>
        /// <param name="ringSize">The number of atoms and bonds the ring will have</param>
        /// <param name="elementSymbol">The element of the atoms the ring will have</param>
        public Ring(int ringSize, string elementSymbol)
            : base()
        {
            var prevAtom = new Atom(elementSymbol);
            atoms.Add(prevAtom);
            for (int i = 1; i < ringSize; i++)
            {
                var atom = new Atom(elementSymbol);
                atoms.Add(atom);
                bonds.Add(new Bond(prevAtom, atom, BondOrder.Single));
                prevAtom = atom;
            }
            bonds.Add(new Bond(prevAtom, atoms[0], BondOrder.Single));
        }

        /// <summary>
        /// the number of atoms\edges in this ring.
        /// </summary>
        public int RingSize => atoms.Count;

        /// <summary>
        /// Returns the next bond in order, relative to a given bond and atom.
        /// Example: Let the ring be composed of 0-1, 1-2, 2-3 and 3-0. A request getNextBond(1-2, 2)
        /// will return Bond 2-3.
        /// </summary>
        /// <param name="bond"> A bond for which an atom from a consecutive bond is sought</param>
        /// <param name="atom">A atom from the bond above to assign a search direction</param>
        /// <returns>The next bond in the order given by the above assignment</returns>
        public IBond GetNextBond(IBond bond, IAtom atom)
            => bonds.Where(n => n.Contains(atom) && !n.Equals(bond)).FirstOrDefault();

        /// <summary>
        /// The sum of all bond orders in the ring.
        /// </summary>
        public int GetBondOrderSum()
            => bonds.Where(n => !n.Order.IsUnset()).Select(n => n.Order.Numeric()).Sum();

        public override ICDKObject Clone(CDKObjectMap map)
        {
            return (Ring)base.Clone(map);
        }

        public new IRing Clone() => (IRing)Clone(new CDKObjectMap());
        object ICloneable.Clone() => Clone();
    }
}
