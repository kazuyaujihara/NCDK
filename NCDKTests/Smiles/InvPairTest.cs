/* Copyright (C) 2007  Egon Willighagen <egonw@users.sf.net>
 *
 * Contact: cdk-devel@lists.sourceforge.net
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public License
 * as published by the Free Software Foundation; either version 2.1
 * of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT Any WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace NCDK.Smiles
{
    // @cdk.module     test-standard
    [TestClass()]
    public class InvPairTest : CDKTestCase
    {
        private readonly IChemObjectBuilder builder = CDK.Builder;

        public InvPairTest()
                : base()
        {
        }

        [TestMethod()]
        public void TestInvPair()
        {
            InvPair pair = new InvPair();
            Assert.IsNotNull(pair);
        }

        [TestMethod()]
        public void TestInvPairLongIAtom()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.IsNotNull(pair);
            Assert.AreEqual(5L, pair.Curr);
            Assert.AreEqual(atom, pair.Atom);
        }

        [TestMethod()]
        public void TestEqualsObject()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.AreEqual(pair, pair);
            Assert.AreNotSame("NotSame", pair);
            Assert.AreNotSame(new InvPair(), pair);
        }

        [TestMethod()]
        public void TestToString()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.IsNotNull(pair.ToString());
            Assert.IsTrue(pair.ToString().Length > 0);
        }

        [TestMethod()]
        public void TestSetAtomIAtom()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair();
            Assert.AreNotSame(atom, pair.Atom);
            pair.Atom = atom;
            Assert.AreEqual(atom, pair.Atom);
        }

        [TestMethod()]
        public void TestGetAtom()
        {
            InvPair pair = new InvPair();
            Assert.IsNull(pair.Atom);
            pair.Atom = builder.NewAtom(ChemicalElement.C);
            Assert.IsNotNull(pair.Atom);
        }

        // @cdk.bug 2045574
        [TestMethod()]
        public void TestGetPrime()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5, atom);
            pair.SetPrime();
            int prime = pair.Prime;
            pair.SetPrime();
            Assert.AreEqual(prime, pair.Prime, "The prime should not change when curr is not changed");
            pair.Curr = 61;
            pair.SetPrime();
            Assert.AreNotSame(prime, pair.Prime);
        }

        [TestMethod()]
        public void TestSetPrime()
        {
            InvPair pair = new InvPair();
            try
            {
                pair.SetPrime();
                Assert.Fail("should have failed with an ArrayIndexOutOfBounds exception");
            }
            catch (Exception)
            {
                // OK, is apparently expected to happen
            }
        }

        [TestMethod()]
        public void TestCommit()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            pair.Commit();
            Assert.IsNotNull(atom.GetProperty<long>(InvPair.CanonicalLabelPropertyKey));
            Assert.AreEqual(5, ((long)atom.GetProperty<long>(InvPair.CanonicalLabelPropertyKey)));
        }

        [TestMethod()]
        public void TestSetCurrLong()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.AreEqual(5L, pair.Curr);
            pair.Curr = 4L;
            Assert.AreEqual(4L, pair.Curr);
        }

        [TestMethod()]
        public void TestGetCurr()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.AreEqual(5L, pair.Curr);
        }

        [TestMethod()]
        public void TestSetLastLong()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.AreEqual(0L, pair.Last);
            pair.Last = 4L;
            Assert.AreEqual(4L, pair.Last);
        }

        [TestMethod()]
        public void TestGetLast()
        {
            var atom = builder.NewAtom(ChemicalElement.C);
            InvPair pair = new InvPair(5L, atom);
            Assert.AreEqual(0L, pair.Last);
        }
    }
}
