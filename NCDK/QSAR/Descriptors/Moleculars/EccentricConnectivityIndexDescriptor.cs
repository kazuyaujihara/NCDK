/*
 *  Copyright (C) 2004-2007  Rajarshi Guha <rajarshi@users.sourceforge.net>
 *
 *  Contact: cdk-devel@lists.sourceforge.net
 *
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public License
 *  as published by the Free Software Foundation; either version 2.1
 *  of the License, or (at your option) any later version.
 *
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA 02110-1301 USA.
 */
using NCDK.Graphs;
using NCDK.Graphs.Matrix;
using NCDK.QSAR.Result;
using NCDK.Tools.Manipulator;
using System.Linq;

namespace NCDK.QSAR.Descriptors.Moleculars
{
    /// <summary>
    /// A topological descriptor combining distance and adjacency information.
    /// This descriptor is described by Sharma et al. <token>cdk-cite-SHA97</token> and has been shown
    /// to correlate well with a number of physical properties. The descriptor is also reported to
    /// have good discriminatory ability.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The eccentric connectivity index for a hydrogen supressed molecular graph is given by the
    /// expression
    /// <pre>
    /// \xi^{c} = \sum_{i = 1}{n} E(i) V(i)
    /// </pre>
    /// where E(i) is the eccentricity of the i<sup>th</sup> atom (path length from the
    /// i<sup>th</sup> atom to the atom farthest from it) and V(i) is the vertex degree of the
    /// i<sup>th</sup> atom.
    /// </para>
    /// <para>This descriptor uses these parameters:
    /// <list type="table">
    ///   <item>
    ///     <term>Name</term>
    ///     <term>Default</term>
    ///     <term>Description</term>
    ///   </item>
    ///   <item>
    ///     <term></term>
    ///     <term></term>
    ///     <term>no parameters</term>
    ///   </item>
    /// </list> 
    /// </para>
    /// </remarks>
    /// Returns a single value with name <i>ECCEN</i>
    // @author      Rajarshi Guha
    // @cdk.created     2005-03-19
    // @cdk.module  qsarmolecular
    // @cdk.githash
    // @cdk.dictref qsar-descriptors:eccentricConnectivityIndex
    public class EccentricConnectivityIndexDescriptor : AbstractMolecularDescriptor, IMolecularDescriptor
    {
        private static readonly string[] NAMES = { "ECCEN" };

        public EccentricConnectivityIndexDescriptor() { }

        public override IImplementationSpecification Specification => _Specification;
        private static DescriptorSpecification _Specification { get; } =
         new DescriptorSpecification(
                "http://www.blueobelisk.org/ontologies/chemoinformatics-algorithms/#eccentricConnectivityIndex",
                typeof(EccentricConnectivityIndexDescriptor).FullName,
                "The Chemistry Development Kit");

        /// <summary>
        /// the parameterNames attribute of the EccentricConnectivityIndexDescriptor object
        /// </summary>
        public override string[] ParameterNames => null;

        /// <summary>
        ///  The parameterType attribute of the EccentricConnectivityIndexDescriptor object
        /// </summary>
        /// <param name="name">Description of the Parameter</param>
        /// <returns>The parameterType value</returns>
        public override object GetParameterType(string name) => null;

        public override string[] DescriptorNames => NAMES;

        /// <summary>
        /// the parameters attribute of the EccentricConnectivityIndexDescriptor object
        /// </summary>
        public override object[] Parameters { get { return null; } set { } }

        /// <summary>
        ///  Calculates the eccentric connectivity
        /// </summary>
        /// <param name="container">Parameter is the atom container.</param>
        /// <returns>An IntegerResult value representing the eccentric connectivity index</returns>
        public override DescriptorValue Calculate(IAtomContainer container)
        {
            IAtomContainer local = AtomContainerManipulator.RemoveHydrogens(container);

            int natom = local.Atoms.Count;
            int[][] admat = AdjacencyMatrix.GetMatrix(local);
            int[][] distmat = PathTools.ComputeFloydAPSP(admat);

            int eccenindex = 0;
            for (int i = 0; i < natom; i++)
            {
                int max = -1;
                for (int j = 0; j < natom; j++)
                {
                    if (distmat[i][j] > max) max = distmat[i][j];
                }
                int degree = local.GetConnectedBonds(local.Atoms[i]).Count();
                eccenindex += max * degree;
            }
            IntegerResult retval = new IntegerResult(eccenindex);
            return new DescriptorValue(_Specification, ParameterNames, Parameters, retval,
                    DescriptorNames, null);
        }

        /// <inheritdoc/>
        public override IDescriptorResult DescriptorResultType { get; } = new IntegerResult(1);
    }
}
