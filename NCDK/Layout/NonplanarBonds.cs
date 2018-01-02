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

using NCDK.Common.Collections;
using NCDK.Common.Mathematics;
using NCDK.Geometries;
using NCDK.Graphs;
using NCDK.Numerics;
using NCDK.RingSearches;
using NCDK.Stereo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace NCDK.Layout
{
    /// <summary>
    /// Assigns non-planar labels (wedge/hatch) to the tetrahedral and extended tetrahedral 
    /// stereocentres in a 2D depiction. Labels are assigned to atoms using the following priority. 
    /// <list type="bullet">
    /// <item>bond to non-stereo atoms</item> 
    /// <item>acyclic bonds</item> 
    /// <item>bonds to atoms with lower degree (i.e. terminal)</item>
    /// <item>lower atomic number</item>
    /// </list>
    /// Unspecified bonds are also marked.
    /// </summary>
    // @author John May
    // @cdk.module sdg
    internal sealed class NonplanarBonds
    {
        /// <summary>The structure we are assigning labels to.</summary>
        private readonly IAtomContainer container;

        /// <summary>Adjacency list graph representation of the structure.</summary>
        private readonly int[][] graph;

        /// <summary>Search for cyclic atoms.</summary>
        private readonly RingSearch ringSearch;

        /// <summary>Tetrahedral elements indexed by central atom.</summary>
        private readonly ITetrahedralChirality[] tetrahedralElements;

        /// <summary>Double-bond elements indexed by end atoms.</summary>
        private readonly IDoubleBondStereochemistry[] doubleBondElements;

        /// <summary>Lookup atom index (avoid IAtomContainer).</summary>
        private readonly IDictionary<IAtom, int> atomToIndex;

        /// <summary>Quick lookup of a bond give the atom index of it's atoms.</summary>
        private readonly GraphUtil.EdgeToBondMap edgeToBond;

        /// <summary>
        /// Assign non-planar, up and down labels to indicate tetrahedral configuration. Currently all
        /// existing directional labels are removed before assigning new labels.
        /// </summary>
        /// <param name="container">the structure to assign labels to</param>
        /// <returns>a container with assigned labels (currently the same as the input)</returns>
        /// <exception cref="ArgumentException">an atom had no 2D coordinates or labels could not be assigned to a tetrahedral centre</exception>
        public static IAtomContainer Assign(IAtomContainer container)
        {
            GraphUtil.EdgeToBondMap edgeToBond = GraphUtil.EdgeToBondMap.WithSpaceFor(container);
            new NonplanarBonds(container, GraphUtil.ToAdjList(container, edgeToBond), edgeToBond);
            return container;
        }

        /// <summary>
        /// Assign non-planar bonds to the tetrahedral stereocenters in the <paramref name="container"/>.
        /// </summary>
        /// <param name="container">structure</param>
        /// <param name="g">graph adjacency list representation</param>
        /// <exception cref="ArgumentException">an atom had no 2D coordinates or labels could not be assigned to a tetrahedral centre</exception>
        NonplanarBonds(IAtomContainer container, int[][] g, GraphUtil.EdgeToBondMap edgeToBond)
        {
            this.container = container;
            this.tetrahedralElements = new ITetrahedralChirality[container.Atoms.Count];
            this.doubleBondElements = new IDoubleBondStereochemistry[container.Atoms.Count];
            this.graph = g;
            this.atomToIndex = new Dictionary<IAtom, int>(container.Atoms.Count);
            this.edgeToBond = edgeToBond;
            this.ringSearch = new RingSearch(container, graph);

            // clear existing up/down labels to avoid collision, this isn't strictly
            // needed if the atom positions weren't adjusted but we can't guarantee
            // that so it's safe to clear them
            foreach (var bond in container.Bonds)
            {
                switch (bond.Stereo)
                {
                    case BondStereo.Up:
                    case BondStereo.UpInverted:
                    case BondStereo.Down:
                    case BondStereo.DownInverted:
                        bond.Stereo = BondStereo.None;
                        break;
                }
            }

            for (int i = 0; i < container.Atoms.Count; i++)
            {
                IAtom atom = container.Atoms[i];
                atomToIndex[atom] = i;
                if (atom.Point2D == null)
                    throw new ArgumentException("atom " + i + " had unset coordinates");
            }

            // index the tetrahedral elements by their focus
            int[] foci = new int[container.Atoms.Count];
            int n = 0;
            foreach (var element in container.StereoElements)
            {
                if (element is ITetrahedralChirality)
                {
                    ITetrahedralChirality tc = (ITetrahedralChirality)element;
                    int focus = atomToIndex[tc.ChiralAtom];
                    tetrahedralElements[focus] = tc;
                    foci[n++] = focus;
                }
                else if (element is IDoubleBondStereochemistry)
                {
                    IBond doubleBond = ((IDoubleBondStereochemistry)element).StereoBond;
                    doubleBondElements[atomToIndex[doubleBond.Begin]] =
                            doubleBondElements[atomToIndex[doubleBond.End]] = (IDoubleBondStereochemistry)element;
                }
            }

            // prioritise to highly-congested tetrahedral centres first

            Array.Sort(foci, 0, n, new NAdjacentCentresComparer(this));

            // Tetrahedral labels
            for (int i = 0; i < n; i++)
            {
                Label(tetrahedralElements[foci[i]]);
            }

            // Rarer types of stereo
            foreach (var se in container.StereoElements)
            {
                if (se is ExtendedTetrahedral)
                {
                    Label((ExtendedTetrahedral)se);
                }
                else if (se is Atropisomeric)
                {
                    Label((Atropisomeric)se);
                }
                else if (se is SquarePlanar)
                {
                    ModifyAndLabel((SquarePlanar)se);
                }
                else if (se is TrigonalBipyramidal)
                {
                    ModifyAndLabel((TrigonalBipyramidal)se);
                }
                else if (se is Octahedral)
                {
                    ModifyAndLabel((Octahedral)se);
                }
            }

            // Unspecified double bond, indicated with an up/down wavy bond
            foreach (var bond in FindUnspecifiedDoubleBonds(g))
            {
                LabelUnspecified(bond);
            }
        }

        class NAdjacentCentresComparer : IComparer<int>
        {
            NonplanarBonds parent;

            public NAdjacentCentresComparer(NonplanarBonds parent)
            {
                this.parent = parent;
            }

            public int Compare(int i, int j)
            {
                return -parent.NAdjacentCentres(i).CompareTo(parent.NAdjacentCentres(j));
            }
        }

        private void Rotate(Vector2 p, Vector2 pivot, double cos, double sin)
        {
            double x = p.X - pivot.X;
            double y = p.Y - pivot.Y;
            double nx = x * cos + y * sin;
            double ny = -x * sin + y * cos;
            p.X = nx + pivot.X;
            p.Y = ny + pivot.Y;
        }

        private Vector2 GetRotated(Vector2 org, Vector2 piviot, double theta)
        {
            Vector2 cpy = org;
            Rotate(cpy, piviot, Math.Cos(theta), Math.Sin(theta));
            return cpy;
        }

        // tP=target point
        private void SnapBondToPosition(IAtom beg, IBond bond, Vector2 tP)
        {
            IAtom end = bond.GetOther(beg);
            Vector2 bP = beg.Point2D.Value;
            Vector2 eP = end.Point2D.Value;
            Vector2 curr = new Vector2(eP.X - bP.X, eP.Y - bP.Y);
            Vector2 dest = new Vector2(tP.X - bP.X, tP.Y - bP.Y);
            double theta = Math.Atan2(curr.Y, curr.X) - Math.Atan2(dest.Y, dest.X);
            double sin = Math.Sin(theta);
            double cos = Math.Cos(theta);
            bond.IsVisited = true;
            var queue = new ArrayDeque<IAtom>();
            queue.Add(end);
            while (queue.Any())
            {
                IAtom atom = queue.Poll();
                if (!atom.IsVisited)
                {
                    Rotate(atom.Point2D.Value, bP, cos, sin);
                    atom.IsVisited = true;
                }
                foreach (IBond b in container.GetConnectedBonds(atom))
                    if (!b.IsVisited)
                    {
                        queue.Add(b.GetOther(atom));
                        b.IsVisited = true;
                    }
            }
        }

        private void ModifyAndLabel(SquarePlanar se)
        {
            var atoms = se.Normalize().Carriers;
            var bonds = new List<IBond>(4);
            double blen = 0;
            foreach (IAtom atom in atoms)
            {
                IBond bond = container.GetBond(se.Focus, atom);
                // can't handled these using this method!
                if (bond.IsInRing)
                    return;
                bonds.Add(bond);
                blen += GeometryUtil.GetLength2D(bond);
            }
            blen /= bonds.Count;
            IAtom focus = se.Focus;
            Vector2 fp = focus.Point2D.Value;

            foreach (IAtom atom in container.Atoms)
                atom.IsVisited = false;
            foreach (IBond bond in container.Bonds)
                bond.IsVisited = false;
            Vector2 ref_ = new Vector2(fp.X, fp.Y + blen);
            SnapBondToPosition(focus, bonds[0], GetRotated(ref_, fp, Vectors.DegreeToRadian(-60)));
            SnapBondToPosition(focus, bonds[1], GetRotated(ref_, fp, Vectors.DegreeToRadian(60)));
            SnapBondToPosition(focus, bonds[2], GetRotated(ref_, fp, Vectors.DegreeToRadian(120)));
            SnapBondToPosition(focus, bonds[3], GetRotated(ref_, fp, Vectors.DegreeToRadian(-120)));
            SetBondDisplay(bonds[0], focus, BondStereo.Down);
            SetBondDisplay(bonds[1], focus, BondStereo.Down);
            SetBondDisplay(bonds[2], focus, BondStereo.Up);
            SetBondDisplay(bonds[3], focus, BondStereo.Up);
        }

        private bool DoMirror(List<IAtom> atoms)
        {
            int p = 1;
            for (int i = 0; i < atoms.Count; i++)
            {
                IAtom a = atoms[i];
                for (int j = i + 1; j < atoms.Count; j++)
                {
                    IAtom b = atoms[j];
                    if (a.AtomicNumber > b.AtomicNumber)
                        p *= -1;
                }
            }
            return p < 0;
        }

        private void ModifyAndLabel(TrigonalBipyramidal se)
        {
            var atoms = se.Normalize().Carriers.ToList();
            var bonds = new List<IBond>(4);
            double blen = 0;
            foreach (IAtom atom in atoms)
            {
                IBond bond = container.GetBond(se.Focus, atom);
                // can't handled these using this method!
                if (bond.IsInRing)
                    return;
                bonds.Add(bond);
                blen += GeometryUtil.GetLength2D(bond);
            }
            blen /= bonds.Count;
            IAtom focus = se.Focus;
            Vector2 fp = focus.Point2D.Value;
            foreach (IAtom atom in container.Atoms)
                atom.IsVisited = false;
            foreach (IBond bond in container.Bonds)
                bond.IsVisited = false;
            Vector2 ref_ = new Vector2(fp.X, fp.Y + blen);

            // Optional but have a look at the equatorial ligands
            // and maybe invert the image based on the permutation
            // parity of their atomic numbers.
            bool mirror = DoMirror(atoms.GetRange(1, 3));

            if (mirror)
            {
                SnapBondToPosition(focus, bonds[0], GetRotated(ref_, fp, Vectors.DegreeToRadian(0)));
                SnapBondToPosition(focus, bonds[3], GetRotated(ref_, fp, Vectors.DegreeToRadian(-60)));
                SnapBondToPosition(focus, bonds[2], GetRotated(ref_, fp, Vectors.DegreeToRadian(90)));
                SnapBondToPosition(focus, bonds[1], GetRotated(ref_, fp, Vectors.DegreeToRadian(-120)));
                SnapBondToPosition(focus, bonds[4], GetRotated(ref_, fp, Vectors.DegreeToRadian(180)));
                SetBondDisplay(bonds[1], focus, BondStereo.Up);
                SetBondDisplay(bonds[3], focus, BondStereo.Down);
            }
            else
            {
                SnapBondToPosition(focus, bonds[0], GetRotated(ref_, fp, Vectors.DegreeToRadian(0)));
                SnapBondToPosition(focus, bonds[1], GetRotated(ref_, fp, Vectors.DegreeToRadian(60)));
                SnapBondToPosition(focus, bonds[2], GetRotated(ref_, fp, Vectors.DegreeToRadian(-90)));
                SnapBondToPosition(focus, bonds[3], GetRotated(ref_, fp, Vectors.DegreeToRadian(120)));
                SnapBondToPosition(focus, bonds[4], GetRotated(ref_, fp, Vectors.DegreeToRadian(180)));
                SetBondDisplay(bonds[1], focus, BondStereo.Down);
                SetBondDisplay(bonds[3], focus, BondStereo.Up);
            }
        }

        private void ModifyAndLabel(Octahedral oc)
        {
            var atoms = oc.Normalize().Carriers;
            var bonds = new List<IBond>(4);

            double blen = 0;
            foreach (IAtom atom in atoms)
            {
                IBond bond = container.GetBond(oc.Focus, atom);
                // can't handled these using this method!
                if (bond.IsInRing)
                    return;
                bonds.Add(bond);
                blen += GeometryUtil.GetLength2D(bond);
            }
            blen /= bonds.Count;
            IAtom focus = oc.Focus;
            Vector2 fp = focus.Point2D.Value;
            foreach (IAtom atom in container.Atoms)
                atom.IsVisited = false;
            foreach (IBond bond in container.Bonds)
                bond.IsVisited = false;
            Vector2 ref_ = new Vector2(fp.X, fp.Y + blen);

            SnapBondToPosition(focus, bonds[0], GetRotated(ref_, fp, Vectors.DegreeToRadian(0)));
            SnapBondToPosition(focus, bonds[1], GetRotated(ref_, fp, Vectors.DegreeToRadian(60)));
            SnapBondToPosition(focus, bonds[2], GetRotated(ref_, fp, Vectors.DegreeToRadian(-60)));
            SnapBondToPosition(focus, bonds[3], GetRotated(ref_, fp, Vectors.DegreeToRadian(-120)));
            SnapBondToPosition(focus, bonds[4], GetRotated(ref_, fp, Vectors.DegreeToRadian(120)));
            SnapBondToPosition(focus, bonds[5], GetRotated(ref_, fp, Vectors.DegreeToRadian(180)));
            SetBondDisplay(bonds[1], focus, BondStereo.Down);
            SetBondDisplay(bonds[2], focus, BondStereo.Down);
            SetBondDisplay(bonds[3], focus, BondStereo.Up);
            SetBondDisplay(bonds[4], focus, BondStereo.Up);
        }

        private BondStereo Flip(BondStereo disp)
        {
            switch (disp)
            {
                case BondStereo.Up: return BondStereo.UpInverted;
                case BondStereo.UpInverted: return BondStereo.Up;
                case BondStereo.Down: return BondStereo.DownInverted;
                case BondStereo.DownInverted: return BondStereo.Down;
                case BondStereo.UpOrDown: return BondStereo.UpOrDownInverted;
                case BondStereo.UpOrDownInverted: return BondStereo.UpOrDown;
                default: return disp;
            }
        }

        private void SetBondDisplay(IBond bond, IAtom focus, BondStereo display)
        {
            if (bond.Begin.Equals(focus))
                bond.Stereo = display;
            else
                bond.Stereo = Flip(display);
        }

        /// <summary>
        /// Assign non-planar labels (wedge/hatch) to the bonds of extended
        /// tetrahedral elements to correctly represent its stereochemistry.
        /// </summary>
        /// <param name="element">a extended tetrahedral element</param>
        private void Label(ExtendedTetrahedral element)
        {
            IAtom focus = element.Focus;
            IAtom[] atoms = element.Peripherals;
            IBond[] bonds = new IBond[4];

            int p = Parity(element.Winding);

            var focusBonds = container.GetConnectedBonds(focus);

            if (focusBonds.Count() != 2)
            {
                Trace.TraceWarning(
                    "Non-cumulated carbon presented as the focus of extended tetrahedral stereo configuration");
                return;
            }

            var terminals = element.FindTerminalAtoms(container);

            IAtom left = terminals[0];
            IAtom right = terminals[1];

            // some bonds may be null if, this happens when an implicit atom
            // is present and one or more 'atoms' is a terminal atom
            bonds[0] = container.GetBond(left, atoms[0]);
            bonds[1] = container.GetBond(left, atoms[1]);
            bonds[2] = container.GetBond(right, atoms[2]);
            bonds[3] = container.GetBond(right, atoms[3]);

            // find the clockwise ordering (in the plane of the page) by sorting by
            // polar corodinates
            int[] rank = new int[4];
            {
                for (int i = 0; i < 4; i++)
                    rank[i] = i;
            }
            p *= SortClockwise(rank, focus, atoms, 4);

            // assign all up/down labels to an auxiliary array
            BondStereo[] labels = new BondStereo[4];
            {
                for (int i = 0; i < 4; i++)
                {
                    int v = rank[i];
                    p *= -1;
                    labels[v] = p > 0 ? BondStereo.Up : BondStereo.Down;
                }
            }

            int[] priority = new int[] { 5, 5, 5, 5 };

            {
                // set the label for the highest priority and available bonds on one side
                // of the cumulated system, setting both sides doesn't make sense
                int i = 0;
                foreach (var v in Priority(atomToIndex[focus], atoms, 4))
                {
                    IBond bond = bonds[v];
                    if (bond == null) continue;
                    if (bond.Stereo == BondStereo.None && bond.Order == BondOrder.Single) priority[v] = i++;
                }
            }

            // we now check which side was more favourable and assign two labels
            // to that side only
            if (priority[0] + priority[1] < priority[2] + priority[3])
            {
                if (priority[0] < 5)
                {
                    bonds[0].SetAtoms(new IAtom[] { left, atoms[0] });
                    bonds[0].Stereo = labels[0];
                }
                if (priority[1] < 5)
                {
                    bonds[1].SetAtoms(new IAtom[] { left, atoms[1] });
                    bonds[1].Stereo = labels[1];
                }
            }
            else
            {
                if (priority[2] < 5)
                {
                    bonds[2].SetAtoms(new IAtom[] { right, atoms[2] });
                    bonds[2].Stereo = labels[2];
                }
                if (priority[3] < 5)
                {
                    bonds[3].SetAtoms(new IAtom[] { right, atoms[3] });
                    bonds[3].Stereo = labels[3];
                }
            }
        }

        /// <summary>
        /// Assign non-planar labels (wedge/hatch) to the bonds to
        /// atropisomers
        /// </summary>
        /// <param name="element">a extended tetrahedral element</param>
        private void Label(Atropisomeric element)
        {
            IBond focus = element.Focus;
            IAtom beg = focus.Begin;
            IAtom end = focus.End;
            IAtom[] atoms = element.Carriers.ToArray();
            IBond[] bonds = new IBond[4];

            int p = 0;
            switch (element.Configure)
            {
                case StereoElement.Configurations.Left:
                    p = +1;
                    break;
                case StereoElement.Configurations.Right:
                    p = -1;
                    break;
            }

            // some bonds may be null if, this happens when an implicit atom
            // is present and one or more 'atoms' is a terminal atom
            bonds[0] = container.GetBond(beg, atoms[0]);
            bonds[1] = container.GetBond(beg, atoms[1]);
            bonds[2] = container.GetBond(end, atoms[2]);
            bonds[3] = container.GetBond(end, atoms[3]);

            // may be back to front?
            if (bonds[0] == null || bonds[1] == null ||
                bonds[2] == null || bonds[3] == null)
                throw new InvalidOperationException("Unexpected configuration ordering, beg/end bonds should be in that order.");

            // find the clockwise ordering (in the plane of the page) by sorting by
            // polar corodinates
            int[] rank = new int[4];
            for (var i = 0; i < 4; i++)
                rank[i] = i;

            IAtom phantom = beg.Builder.NewAtom();
            phantom.Point2D = new Vector2((beg.Point2D.Value.X + end.Point2D.Value.X) / 2,
                                       (beg.Point2D.Value.Y + end.Point2D.Value.Y) / 2);
            p *= SortClockwise(rank, phantom, atoms, 4);

            // assign all up/down labels to an auxiliary array
            BondStereo[] labels = new BondStereo[4];
            for (var i = 0; i < 4; i++)
            {
                int v = rank[i];
                p *= -1;
                labels[v] = p > 0 ? BondStereo.Up : BondStereo.Down;
            }

            int[] priority = new int[] { 5, 5, 5, 5 };

            // set the label for the highest priority and available bonds on one side
            // of the cumulated system, setting both sides doesn't make sense
            {
                int i = 0;
                foreach (int v in new int[] { 0, 1, 2, 3 })
                {
                    IBond bond = bonds[v];
                    if (bond == null) continue;
                    if (bond.Stereo == BondStereo.None && bond.Order == BondOrder.Single) priority[v] = i++;
                }
            }

            // we now check which side was more favourable and assign two labels
            // to that side only
            if (priority[0] + priority[1] < priority[2] + priority[3])
            {
                if (priority[0] < 5)
                {
                    bonds[0].SetAtoms(new IAtom[] { beg, atoms[0] });
                    bonds[0].Stereo = labels[0];
                }
                if (priority[1] < 5)
                {
                    bonds[1].SetAtoms(new IAtom[] { beg, atoms[1] });
                    bonds[1].Stereo = labels[1];
                }
            }
            else
            {
                if (priority[2] < 5)
                {
                    bonds[2].SetAtoms(new IAtom[] { end, atoms[2] });
                    bonds[2].Stereo = labels[2];
                }
                if (priority[3] < 5)
                {
                    bonds[3].SetAtoms(new IAtom[] { end, atoms[3] });
                    bonds[3].Stereo = labels[3];
                }
            }
        }

        /// <summary>
        /// Assign labels to the bonds of tetrahedral element to correctly represent
        /// its stereo configuration.
        /// </summary>
        /// <param name="element">a tetrahedral element</param>
        /// <exception cref="ArgumentException">the labels could not be assigned</exception>
        private void Label(ITetrahedralChirality element)
        {
            IAtom focus = element.ChiralAtom;
            var atoms = element.Ligands;
            IBond[] bonds = new IBond[4];

            int p = Parity(element.Stereo);
            int n = 0;

            // unspecified centre, no need to assign labels
            if (p == 0) return;

            for (int i = 0; i < 4; i++)
            {
                if (atoms[i] == focus)
                {
                    p *= IndexParity(i); // implicit H, adjust parity
                }
                else
                {
                    bonds[n] = container.GetBond(focus, atoms[i]);
                    if (bonds[n] == null)
                        throw new ArgumentException("Inconsistent stereo,"
                                                           + " tetrahedral centre"
                                                           + " contained atom not"
                                                           + " stored in molecule");
                    atoms[n] = atoms[i];
                    n++;
                }
            }

            // sort coordinates and adjust parity (rank gives us the sorted order)
            int[] rank = new int[n];
            for (int i = 0; i < n; i++)
                rank[i] = i;
            p *= SortClockwise(rank, focus, atoms, n);

            // special case when there are three neighbors are acute and an implicit
            // hydrogen is opposite all three neighbors. The central label needs to
            // be inverted, atoms could be laid out like this automatically, consider
            // CC1C[C@H]2CC[C@@H]1C2
            int invert = -1;
            if (n == 3)
            {
                // find a triangle of non-sequential neighbors (sorted clockwise)
                // which has anti-clockwise winding
                for (int i = 0; i < n; i++)
                {
                    Vector2 a = atoms[rank[i]].Point2D.Value;
                    Vector2 b = focus.Point2D.Value;
                    Vector2 c = atoms[rank[(i + 2) % n]].Point2D.Value;
                    double det = (a.X - c.X) * (b.Y - c.Y) - (a.Y - c.Y) * (b.X - c.X);
                    if (det > 0)
                    {
                        invert = rank[(i + 1) % n];
                        break;
                    }
                }
            }

            // assign all up/down labels to an auxiliary array
            BondStereo[] labels = new BondStereo[n];
            for (int i = 0; i < n; i++)
            {
                int v = rank[i];

                // 4 neighbors (invert every other one)
                if (n == 4) p *= -1;

                labels[v] = invert == v ? p > 0 ? BondStereo.Down : BondStereo.Up : p > 0 ? BondStereo.Up : BondStereo.Down;
            }

            // set the label for the highest priority and available bond
            bool found = false;
            BondStereo firstlabel = BondStereo.None;
            bool assignTwoLabels = AssignTwoLabels(bonds, labels);
            foreach (int v in Priority(atomToIndex[focus], atoms, n))
            {
                IBond bond = bonds[v];
                if (bond.Stereo != BondStereo.None || bond.Order != BondOrder.Single)
                    continue;
                // first label
                if (!found)
                {
                    found = true;
                    bond.SetAtoms(new IAtom[] { focus, atoms[v] }); // avoids UP_INVERTED/DOWN_INVERTED
                    bond.Stereo = labels[v];
                    firstlabel = labels[v];
                    // don't assign a second label when there are only three ligands
                    if (!assignTwoLabels)
                        break;
                }
                // second label
                else if (labels[v] != firstlabel)
                {
                    bond.SetAtoms(new IAtom[] { focus, atoms[v] }); // avoids UP_INVERTED/DOWN_INVERTED
                    bond.Stereo = labels[v];
                    break;
                }
            }

            // it should be possible to always assign labels somewhere -> unchecked exception
            if (!found)
                throw new ArgumentException("could not assign non-planar (up/down) labels");
        }

        private bool AssignTwoLabels(IBond[] bonds, BondStereo[] labels)
        {
            return labels.Length == 4 && CountRingBonds(bonds) != 3;
        }

        private int CountRingBonds(IBond[] bonds)
        {
            int rbonds = 0;
            foreach (IBond bond in bonds)
            {
                if (bond != null && bond.IsInRing)
                    rbonds++;
            }
            return rbonds;
        }

        /// <summary>
        /// Obtain the parity of a value x. The parity is -1 if the value is odd or
        /// +1 if the value is even.
        /// </summary>
        /// <param name="x">a value</param>
        /// <returns>the parity</returns>
        private int IndexParity(int x)
        {
            return (x & 0x1) == 1 ? -1 : +1;
        }

        /// <summary>
        /// Obtain the parity (winding) of a tetrahedral element. The parity is -1
        /// for clockwise (odd), +1 for anticlockwise (even) and 0 for unspecified.
        /// </summary>
        /// <param name="stereo">configuration</param>
        /// <returns>the parity</returns>
        private int Parity(TetrahedralStereo stereo)
        {
            switch (stereo)
            {
                case TetrahedralStereo.Clockwise:
                    return -1;
                case TetrahedralStereo.AntiClockwise:
                    return +1;
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Obtain the number of centres adjacent to the atom at the index, <paramref name="i"/>.
        /// </summary>
        /// <param name="i">atom index</param>
        /// <returns>number of adjacent centres</returns>
        private int NAdjacentCentres(int i)
        {
            int n = 0;
            foreach (var atom in tetrahedralElements[i].Ligands)
                if (tetrahedralElements[atomToIndex[atom]] != null) n++;
            return n;
        }

        /// <summary>
        /// Obtain a prioritised array where the indices 0 to <paramref name="n"/> which correspond to
        /// the provided <paramref name="atoms"/>.
        /// </summary>
        /// <param name="focus">focus of the tetrahedral atom</param>
        /// <param name="atoms">the atom</param>
        /// <param name="n">number of atoms</param>
        /// <returns>prioritised indices</returns>
        private int[] Priority(int focus, IList<IAtom> atoms, int n)
        {
            int[] rank = new int[n];
            for (int i = 0; i < n; i++)
                rank[i] = i;
            for (int j = 1; j < n; j++)
            {
                int v = rank[j];
                int i = j - 1;
                while ((i >= 0) && HasPriority(focus, atomToIndex[atoms[v]], atomToIndex[atoms[rank[i]]]))
                {
                    rank[i + 1] = rank[i--];
                }
                rank[i + 1] = v;
            }
            return rank;
        }

        private bool IsSp3Carbon(IAtom atom, int deg)
        {
            int? elem = atom.AtomicNumber;
            int? hcnt = atom.ImplicitHydrogenCount;
            if (elem == null || hcnt == null) return false;
            return elem == 6 && hcnt <= 1 && deg + hcnt == 4;
        }

        /// <summary>
        /// Does the atom at index <paramref name="i"/> have priority over the atom at index
        /// <paramref name="j"/> for the tetrahedral atom <paramref name="focus"/>.
        /// </summary>
        /// <param name="focus">tetrahedral centre (or -1 if double bond)</param>
        /// <param name="i">adjacent atom index</param>
        /// <param name="j">adjacent atom index</param>
        /// <returns>whether atom i has priority</returns>
        bool HasPriority(int focus, int i, int j)
        {
            // prioritise bonds to non-centres
            if (tetrahedralElements[i] == null && tetrahedralElements[j] != null) return true;
            if (tetrahedralElements[i] != null && tetrahedralElements[j] == null) return false;
            if (doubleBondElements[i] == null && doubleBondElements[j] != null) return true;
            if (doubleBondElements[i] != null && doubleBondElements[j] == null) return false;

            IAtom iAtom = container.Atoms[i];
            IAtom jAtom = container.Atoms[j];

            bool iIsSp3 = IsSp3Carbon(iAtom, graph[i].Length);
            bool jIsSp3 = IsSp3Carbon(jAtom, graph[j].Length);
            if (iIsSp3 != jIsSp3)
                return !iIsSp3;

            // avoid possible Sp3 centers
            if (tetrahedralElements[i] == null && tetrahedralElements[j] != null) return true;
            if (tetrahedralElements[i] != null && tetrahedralElements[j] == null) return false;

            // prioritise acyclic bonds
            bool iCyclic = focus >= 0 ? ringSearch.Cyclic(focus, i) : ringSearch.Cyclic(i);
            bool jCyclic = focus >= 0 ? ringSearch.Cyclic(focus, j) : ringSearch.Cyclic(j);
            if (!iCyclic && jCyclic) return true;
            if (iCyclic && !jCyclic) return false;

            // avoid placing on pseudo atoms
            if (iAtom.AtomicNumber > 0 && jAtom.AtomicNumber == 0)
                return true;
            if (iAtom.AtomicNumber == 0 && jAtom.AtomicNumber > 0)
                return false;

            int iDegree = graph[i].Length;
            int iElem = iAtom.AtomicNumber.Value;
            int jDegree = graph[j].Length;
            int jElem = jAtom.AtomicNumber.Value;

            // rank carbon's last
            if (iElem == 6) iElem = 256;
            if (jElem == 6) jElem = 256;

            // terminal atoms are always best
            if (iDegree == 1 && jDegree > 1)
                return true;
            if (jDegree == 1 && iDegree > 1)
                return false;

            // prioritise by atomic number, H < N < O < ... < C
            if (iElem < jElem)
                return true;
            if (iElem > jElem)
                return false;

            // prioritise atoms with fewer neighbors
            if (iDegree < jDegree) return true;
            if (iDegree > jDegree) return false;

            return false;
        }

        /// <summary>
        /// Sort the <paramref name="indices"/>, which correspond to an index in the <paramref name="atoms"/> array in
        /// clockwise order.
        /// </summary>
        /// <param name="indices">indices, 0 to n</param>
        /// <param name="focus">the central atom</param>
        /// <param name="atoms">the neighbors of the focus</param>
        /// <param name="n">the number of neighbors</param>
        /// <returns>the permutation parity of the sort</returns>
        private int SortClockwise(int[] indices, IAtom focus, IList<IAtom> atoms, int n)
        {
            int x = 0;
            for (int j = 1; j < n; j++)
            {
                int v = indices[j];
                int i = j - 1;
                while ((i >= 0) && Less(v, indices[i], atoms, focus.Point2D.Value))
                {
                    indices[i + 1] = indices[i--];
                    x++;
                }
                indices[i + 1] = v;
            }
            return IndexParity(x);
        }

        /// <summary>
        /// Is index <paramref name="i"/>, to the left of index <paramref name="j"/> when sorting clockwise around the <paramref name="center"/>.
        /// </summary>
        /// <param name="i">an index in <paramref name="atoms"/></param>
        /// <param name="j">an index in <paramref name="atoms"/></param>
        /// <param name="atoms">atoms</param>
        /// <param name="center">central point</param>
        /// <returns>atom <paramref name="i"/> is before <paramref name="j"/></returns>
        /// <seealso href="http://stackoverflow.com/a/6989383">Sort points in clockwise order, ciamej</seealso>
        static bool Less(int i, int j, IList<IAtom> atoms, Vector2 center)
        {
            Vector2 a = atoms[i].Point2D.Value;
            Vector2 b = atoms[j].Point2D.Value;

            if (a.X - center.X >= 0 && b.X - center.X < 0) return true;
            if (a.X - center.X < 0 && b.X - center.X >= 0) return false;
            if (a.X - center.X == 0 && b.X - center.X == 0)
            {
                if (a.Y - center.Y >= 0 || b.Y - center.Y >= 0) return a.Y > b.Y;
                return b.Y > a.Y;
            }

            // compute the cross product of vectors (center -> a) x (center -> b)
            double det = (a.X - center.X) * (b.Y - center.Y) - (b.X - center.X) * (a.Y - center.Y);
            if (det < 0) return true;
            if (det > 0) return false;

            // points a and b are on the same line from the center
            // check which point is closer to the center
            double d1 = (a.X - center.X) * (a.X - center.X) + (a.Y - center.Y) * (a.Y - center.Y);
            double d2 = (b.X - center.X) * (b.X - center.X) + (b.Y - center.Y) * (b.Y - center.Y);
            return d1 > d2;
        }

        /// <summary>
        /// Labels a double bond as unspecified either by marking an adjacent bond as
        /// wavy (up/down) or if that's not possible (e.g. it's conjugated with other double bonds
        /// that have a conformation), setting the bond to a crossed double bond.
        /// </summary>
        /// <param name="doubleBond">the bond to mark as unspecified</param>
        private void LabelUnspecified(IBond doubleBond)
        {
            IAtom aBeg = doubleBond.Begin;
            IAtom aEnd = doubleBond.End;

            int beg = atomToIndex[aBeg];
            int end = atomToIndex[aEnd];

            int nAdj = 0;
            IAtom[] focus = new IAtom[4];
            IAtom[] adj = new IAtom[4];

            // build up adj list of all potential atoms
            foreach (var neighbor in graph[beg])
            {
                IBond bond = edgeToBond[beg, neighbor];
                if (bond.Order == BondOrder.Single)
                {
                    if (nAdj == 4) return; // more than 4? not a stereo-dbond
                    focus[nAdj] = aBeg;
                    adj[nAdj++] = container.Atoms[neighbor];
                }
                // conjugated and someone else has marked it as unspecified
                if (bond.Stereo == BondStereo.UpOrDown || bond.Stereo == BondStereo.UpOrDownInverted)
                {
                    return;
                }
            }
            foreach (var neighbor in graph[end])
            {
                IBond bond = edgeToBond[end, neighbor];
                if (bond.Order == BondOrder.Single)
                {
                    if (nAdj == 4) return; // more than 4? not a stereo-dbond
                    focus[nAdj] = aEnd;
                    adj[nAdj++] = container.Atoms[neighbor];
                }
                // conjugated and someone else has marked it as unspecified
                if (bond.Stereo == BondStereo.UpOrDown || bond.Stereo == BondStereo.UpOrDownInverted)
                {
                    return;
                }
            }

            int[] rank = Priority(-1, adj, nAdj);

            // set the bond to up/down wavy to mark unspecified stereochemistry taking care not
            // to accidentally mark another stereocentre as unspecified
            for (int i = 0; i < nAdj; i++)
            {
                if (doubleBondElements[atomToIndex[adj[rank[i]]]] == null &&
                        tetrahedralElements[atomToIndex[adj[rank[i]]]] == null)
                {
                    edgeToBond[atomToIndex[focus[rank[i]]],
                                   atomToIndex[adj[rank[i]]]].Stereo = BondStereo.UpOrDown;
                    return;
                }
            }

            // we got here an no bond was marked, fortunately we have a fallback and can use 
            // crossed bond
            doubleBond.Stereo = BondStereo.EOrZ;
        }

        /// <summary>
        /// Checks if the atom can be involved in a double-bond.
        /// </summary>
        /// <param name="idx">atom idx</param>
        /// <returns>the atom at index (idx) is valid for a double bond</returns>
        /// <seealso href="http://www.inchi-trust.org/download/104/InChI_TechMan.pdf">Double bond stereochemistry, InChI Technical Manual</seealso>
        private bool IsCisTransEndPoint(int idx)
        {
            IAtom atom = container.Atoms[idx];
            // error: uninit atom
            if (atom.AtomicNumber == null ||
                atom.FormalCharge == null ||
                atom.ImplicitHydrogenCount == null)
                return false;
            int chg = atom.FormalCharge.Value;
            int btypes = GetBondTypes(idx);
            switch (atom.AtomicNumber)
            {
                case 6:  // C
                case 14: // Si
                case 32: // Ge
                         // double, single, single
                    return chg == 0 && btypes == 0x0102;
                case 7:  // N
                    if (chg == 0) // double, single
                        return btypes == 0x0101;
                    if (chg == +1) // double, single, single
                        return btypes == 0x0102;
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Generate a bond type code for a given atom. The bond code
        /// can be quickly tested to count the number of single, double,
        /// or 'other' bonds.
        /// </summary>
        /// <param name="idx">the atom idx</param>
        /// <returns>bond code</returns>
        private int GetBondTypes(int idx)
        {
            int btypes = container.Atoms[idx].ImplicitHydrogenCount.Value;
            foreach (int end in graph[idx])
            {
                IBond bond = edgeToBond[idx, end];
                if (bond.Order == BondOrder.Single)
                    btypes += 0x000001;
                else if (bond.Order == BondOrder.Double)
                    btypes += 0x000100;
                else // other bond types
                    btypes += 0x010000;
            }
            return btypes;
        }

        /// <summary>
        /// Locates double bonds to mark as unspecified stereochemistry.
        /// </summary>
        /// <returns>set of double bonds</returns>
        private List<IBond> FindUnspecifiedDoubleBonds(int[][] adjList)
        {
            var unspecifiedDoubleBonds = new List<IBond>();
            foreach (var bond in container.Bonds)
            {
                // non-double bond, ignore it
                if (bond.Order != BondOrder.Double)
                    continue;

                IAtom aBeg = bond.Begin;
                IAtom aEnd = bond.End;

                int beg = atomToIndex[aBeg];
                int end = atomToIndex[aEnd];

                // cyclic bond, ignore it (FIXME may be a cis/trans bond in macro cycle |V| > 7)
                if (ringSearch.Cyclic(beg, end))
                    continue;

                // stereo bond, ignore it depiction is correct
                if ((doubleBondElements[beg] != null && doubleBondElements[beg].StereoBond == bond) ||
                        (doubleBondElements[end] != null && doubleBondElements[end].StereoBond == bond))
                    continue;

                // is actually a tetrahedral centre
                if (tetrahedralElements[beg] != null || tetrahedralElements[end] != null)
                    continue;

                if (!IsCisTransEndPoint(beg) || !IsCisTransEndPoint(end))
                    continue;

                if (!HasOnlyPlainBonds(beg, bond) || !HasOnlyPlainBonds(end, bond))
                    continue;

                if (HasLinearEqualPaths(adjList, beg, end) || HasLinearEqualPaths(adjList, end, beg))
                    continue;

                unspecifiedDoubleBonds.Add(bond);
            }
            return unspecifiedDoubleBonds;
        }

        private bool HasLinearEqualPaths(int[][] adjList, int start, int prev)
        {
            int a = -1;
            int b = -1;
            foreach (var w in adjList[start])
            {
                if (w == prev) continue;
                else if (a == -1) a = w;
                else if (b == -1) b = w;
                else return false; // ???
            }
            if (b < 0)
                return false;
            var visit = new HashSet<IAtom>();
            IAtom aAtom = container.Atoms[a];
            IAtom bAtom = container.Atoms[b];
            visit.Add(container.Atoms[start]);
            if (aAtom.IsInRing || bAtom.IsInRing)
                return false;
            IAtom aNext = aAtom;
            IAtom bNext = bAtom;
            while (aNext != null && bNext != null)
            {
                aAtom = aNext;
                bAtom = bNext;
                visit.Add(aAtom);
                visit.Add(bAtom);
                aNext = null;
                bNext = null;

                // different atoms
                if (NotEqual(aAtom.AtomicNumber, bAtom.AtomicNumber))
                    return false;
                if (NotEqual(aAtom.FormalCharge, bAtom.FormalCharge))
                    return false;
                if (NotEqual(aAtom.MassNumber, bAtom.MassNumber))
                    return false;

                var hCntA = aAtom.ImplicitHydrogenCount;
                var hCntB = bAtom.ImplicitHydrogenCount;
                int cntA = 0, cntB = 0;
                foreach (var w in adjList[atomToIndex[aAtom]])
                {
                    IAtom atom = container.Atoms[w];
                    if (visit.Contains(atom))
                        continue;
                    // hydrogen
                    if (atom.AtomicNumber == 1 && adjList[w].Length == 1)
                    {
                        hCntA++;
                        continue;
                    }
                    aNext = cntA == 0 ? atom : null;
                    cntA++;
                }
                foreach (var w in adjList[atomToIndex[bAtom]])
                {
                    IAtom atom = container.Atoms[w];
                    if (visit.Contains(atom))
                        continue;
                    // hydrogen
                    if (atom.AtomicNumber == 1 && adjList[w].Length == 1)
                    {
                        hCntB++;
                        continue;
                    }
                    bNext = cntB == 0 ? atom : null;
                    cntB++;
                }

                // hydrogen counts are different
                if (hCntA != hCntB)
                    return false;

                // differing in co
                if (cntA != cntB || (cntA > 1 && cntB > 1))
                    return false;
            }

            if (aNext != null || bNext != null)
                return false;

            // traversed the path till the end
            return true;
        }

        private bool NotEqual(int? a, int? b)
        {
            return a == null ? b != null : !a.Equals(b);
        }

        /// <summary>
        /// Check that an atom (<paramref name="v"/>:index) is only adjacent to plain single bonds (may be a bold or
        /// hashed wedged - e.g. at fat end) with the single exception being the allowed double bond
        /// passed as an argument. 
        /// </summary>
        /// <param name="v">atom index</param>
        /// <param name="allowedDoubleBond">a double bond that is allowed</param>
        /// <returns>the atom is adjacent to one or more plain single bonds</returns>
        private bool HasOnlyPlainBonds(int v, IBond allowedDoubleBond)
        {
            int count = 0;
            foreach (var neighbor in graph[v])
            {
                IBond adjBond = edgeToBond[v, neighbor];
                // non single bonds
                if (adjBond.Order.Numeric() > 1)
                {
                    if (allowedDoubleBond != adjBond)
                    {
                        return false;
                    }
                }
                // single bonds
                else
                {
                    if (adjBond.Stereo == BondStereo.UpOrDown || adjBond.Stereo == BondStereo.UpOrDownInverted)
                    {
                        return false;
                    }
                    count++;
                }
            }
            return count > 0;
        }
    }
}
