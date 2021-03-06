/*
 * Copyright (c) 2013. John May <jwmay@users.sf.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either version 2.1
 * of the License, or (at your option) any later version.
 * All we ask is that proper credit is given for our work, which includes
 * - but is not limited to - adding the above copyright notice to the beginning
 * of your source code files, and to any copyright notice that you may distribute
 * with programs based on this work.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 U
 */
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NCDK.Common.Base;
using NCDK.Silent;
using NCDK.Hash.Stereo;

namespace NCDK.Hash
{
    // @author John May
    // @cdk.module test-hash
    [TestClass()]
    public class PerturbedAtomHashGeneratorTest
    {
        [TestMethod()]
        public void TestGenerate()
        {
            IAtomContainer m1 = Cyclopentylcyclopentane();
            IAtomContainer m2 = Decahydronaphthalene();

            SeedGenerator seeding = new SeedGenerator(BasicAtomEncoder.AtomicNumber);
            Pseudorandom pseudorandom = new Xorshift();

            IMoleculeHashGenerator basic = new BasicMoleculeHashGenerator(new BasicAtomHashGenerator(seeding, pseudorandom, 8));
            IMoleculeHashGenerator perturb = new BasicMoleculeHashGenerator(new PerturbedAtomHashGenerator(seeding,
                    new BasicAtomHashGenerator(seeding, pseudorandom, 8), pseudorandom, StereoEncoderFactory.Empty,
                    new MinimumEquivalentCyclicSet(), AtomSuppression.Unsuppressed));
            // basic encoding should say these are the same
            Assert.AreEqual(basic.Generate(m1), basic.Generate(m2));

            // perturbed encoding should differentiate them
            Assert.AreNotEqual(perturb.Generate(m1), perturb.Generate(m2));
        }

        [TestMethod()]
        public void TestCombine()
        {
            Xorshift prng = new Xorshift();
            PerturbedAtomHashGenerator generator = new PerturbedAtomHashGenerator(new SeedGenerator(
                    BasicAtomEncoder.AtomicNumber), new BasicAtomHashGenerator(new SeedGenerator(
                    BasicAtomEncoder.AtomicNumber), prng, 8), prng, StereoEncoderFactory.Empty,
                    new MinimumEquivalentCyclicSet(), AtomSuppression.Unsuppressed);
            long[][] perturbed = new long[][] { new long[] { 1, 2, 3, 4 }, new long[] { 1, 1, 1, 1 }, new long[] { 1, 2, 2, 4 }, new long[] { 2, 2, 2, 2 }, };

            long _0 = 1 ^ 2 ^ 3 ^ 4;
            long _1 = 1 ^ prng.Next(1) ^ prng.Next(prng.Next(1)) ^ prng.Next(prng.Next(prng.Next(1)));
            long _2 = 1 ^ 2 ^ prng.Next(2) ^ 4;
            long _3 = 2 ^ prng.Next(2) ^ prng.Next(prng.Next(2)) ^ prng.Next(prng.Next(prng.Next(2)));

            long[] values = generator.Combine(perturbed);
            Assert.IsTrue(Compares.AreDeepEqual(new long[] { _0, _1, _2, _3 }, values));
        }

        public IAtomContainer Cyclopentylcyclopentane()
        {
            IAtom[] atoms = new IAtom[]{new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"),
                new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"),};
            IBond[] bonds = new IBond[]{new Bond(atoms[0], atoms[1], BondOrder.Single), new Bond(atoms[0], atoms[4], BondOrder.Single),
                new Bond(atoms[1], atoms[2], BondOrder.Single), new Bond(atoms[2], atoms[3], BondOrder.Single),
                new Bond(atoms[3], atoms[4], BondOrder.Single), new Bond(atoms[5], atoms[6], BondOrder.Single),
                new Bond(atoms[5], atoms[9], BondOrder.Single), new Bond(atoms[6], atoms[7], BondOrder.Single),
                new Bond(atoms[7], atoms[8], BondOrder.Single), new Bond(atoms[8], atoms[9], BondOrder.Single),
                new Bond(atoms[8], atoms[0], BondOrder.Single),};
            var mol = new AtomContainer();
            mol.SetAtoms(atoms);
            mol.SetBonds(bonds);
            return mol;
        }

        // @cdk.inchi InChI=1S/C10H18/c1-2-6-10-8-4-3-7-9(10)5-1/h9-10H,1-8H2
        public IAtomContainer Decahydronaphthalene()
        {
            IAtom[] atoms = new IAtom[]{new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"),
                new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"), new Atom("C"),};
            IBond[] bonds = new IBond[]{new Bond(atoms[0], atoms[1], BondOrder.Single), new Bond(atoms[0], atoms[5], BondOrder.Single),
                new Bond(atoms[1], atoms[2], BondOrder.Single), new Bond(atoms[2], atoms[3], BondOrder.Single),
                new Bond(atoms[3], atoms[4], BondOrder.Single), new Bond(atoms[6], atoms[5], BondOrder.Single),
                new Bond(atoms[5], atoms[4], BondOrder.Single), new Bond(atoms[4], atoms[7], BondOrder.Single),
                new Bond(atoms[6], atoms[9], BondOrder.Single), new Bond(atoms[7], atoms[8], BondOrder.Single),
                new Bond(atoms[8], atoms[9], BondOrder.Single),};
            var mol = new AtomContainer();
            mol.SetAtoms(atoms);
            mol.SetBonds(bonds);
            return mol;
        }
    }
}
