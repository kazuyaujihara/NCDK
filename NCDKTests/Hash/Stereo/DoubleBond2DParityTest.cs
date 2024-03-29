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
using NCDK.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NCDK.Hash.Stereo
{
    // @author John May
    // @cdk.module test-hash
    [TestClass()]
    public class DoubleBond2DParityTest
    {
        private const int UNSPECIFIED = 0;
        private const int Opposite = +1;
        private const int Together = -1;

        [TestMethod()]
        public void Unspecified()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.6518, 0.1473), new Vector2(-1.8268, 0.1473),
                    new Vector2(-3.0643, 0.8618), new Vector2(-1.4143, 0.1473));
            Assert.AreEqual(UNSPECIFIED, geometric.Parity);
        }

        [TestMethod()]
        public void OppositeTest()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.6518, 0.1473), new Vector2(-1.8268, 0.1473),
                    new Vector2(-3.0643, 0.8618), new Vector2(-1.4143, -0.5671));
            Assert.AreEqual(Opposite, geometric.Parity);
        }

        [TestMethod()]
        public void TogetherTest()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.6518, 0.1473), new Vector2(-1.8268, 0.1473),
                    new Vector2(-3.0643, 0.8618), new Vector2(-1.4143, 0.8618));
            Assert.AreEqual(Together, geometric.Parity);
        }

        [TestMethod()]
        public void Unspecified_both()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.6518, 0.1473), new Vector2(-1.8268, 0.1473),
                    new Vector2(-3.0643, 0.1473), new Vector2(-1.4143, 0.1473));
            Assert.AreEqual(UNSPECIFIED, geometric.Parity);
        }

        [TestMethod()]
        public void Opposite_inverted()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.6518, 0.1473), new Vector2(-1.8268, 0.1473),
                    new Vector2(-3.0643, -0.5671), new Vector2(-1.4143, 0.8618));
            Assert.AreEqual(Opposite, geometric.Parity);
        }

        [TestMethod()]
        public void Together_inverted()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.6518, 0.1473), new Vector2(-1.8268, 0.1473),
                    new Vector2(-3.0643, -0.5671), new Vector2(-1.4143, -0.5671));
            Assert.AreEqual(Together, geometric.Parity);
        }

        // double bond rotated pi/6 radians (30 degrees)
        [TestMethod()]
        public void Opposite30()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.4455, 0.5046), new Vector2(-2.0330, -0.2099),
                    new Vector2(-2.0330, 1.2191), new Vector2(-2.4455, -0.9244));
            Assert.AreEqual(Opposite, geometric.Parity);
        }

        // double bond rotated pi/6 radians (30 degrees)
        [TestMethod()]
        public void Together30()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.4455, 0.5046), new Vector2(-2.0330, -0.2099),
                    new Vector2(-3.2705, 0.5046), new Vector2(-2.4455, -0.9244));
            Assert.AreEqual(Together, geometric.Parity);
        }

        // double bond rotated pi/6 radians (30 degrees)
        [TestMethod()]
        public void Unspecified30()
        {
            GeometricParity geometric = new DoubleBond2DParity(new Vector2(-2.4455, 0.5046), new Vector2(-2.0330, -0.2099),
                    new Vector2(-2.8580, 1.2191), new Vector2(-2.4455, -0.9244));
            Assert.AreEqual(UNSPECIFIED, geometric.Parity);
        }
    }
}
