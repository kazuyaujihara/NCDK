/* Copyright (C) 2007  Federico
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

using NCDK.Config;
using NCDK.Graphs.Matrix;
using NCDK.QSAR.Results;
using NCDK.Tools.Manipulator;
using System;
using System.Collections.Generic;
using System.IO;

namespace NCDK.QSAR.Descriptors.Moleculars
{
    /// <summary>
    /// This class calculates ATS autocorrelation descriptor, where the weight equal
    /// to the scaled atomic mass <token>cdk-cite-Moreau1980</token>.
    /// </summary>
    // @author      Federico
    // @cdk.created 2007-02-08
    // @cdk.module  qsarmolecular
    // @cdk.githash
    public class AutocorrelationDescriptorMass : AbstractMolecularDescriptor, IMolecularDescriptor
    {
        private readonly static string[] Names = { "ATSm1", "ATSm2", "ATSm3", "ATSm4", "ATSm5" };
        private const double CarbonMass = 12.010735896788;

        private static double ScaledAtomicMasses(IElement element)
        {
            IsotopeFactory isofac = BODRIsotopeFactory.Instance;
            double realmasses = isofac.GetNaturalMass(element);
            return (realmasses / CarbonMass);
        }

        private static double[] ListConvertion(IAtomContainer container)
        {
            var natom = container.Atoms.Count;
            var scalated = new double[natom];

            for (int i = 0; i < natom; i++)
            {
                scalated[i] = ScaledAtomicMasses(container.Atoms[i]);
            }
            return scalated;
        }

        /// <summary>
        /// This method calculate the ATS Autocorrelation descriptor.
        /// </summary>
        public DescriptorValue<ArrayResult<double>> Calculate(IAtomContainer atomContainer)
        {
            IAtomContainer container;
            container = (IAtomContainer)atomContainer.Clone();
            container = AtomContainerManipulator.RemoveHydrogens(container);

            try
            {
                var w = ListConvertion(container);
                var natom = container.Atoms.Count;
                var distancematrix = TopologicalMatrix.GetMatrix(container);
                var masSum = new double[5];

                for (int k = 0; k < 5; k++)
                {
                    for (int i = 0; i < natom; i++)
                    {
                        for (int j = 0; j < natom; j++)
                        {

                            if (distancematrix[i][j] == k)
                            {
                                masSum[k] += w[i] * w[j];
                            }
                            else
                                masSum[k] += 0.0;
                        }
                    }
                    if (k > 0)
                        masSum[k] = masSum[k] / 2;
                }
                var result = new ArrayResult<double>(5);
                foreach (var aMasSum in masSum)
                {
                    result.Add(aMasSum);
                }

                return new DescriptorValue<ArrayResult<double>>(specification, ParameterNames, Parameters, result, DescriptorNames);
            }
            catch (IOException ex)
            {
                var result = new ArrayResult<double>(5);
                for (int i = 0; i < 5; i++)
                    result.Add(double.NaN);
                return new DescriptorValue<ArrayResult<double>>(specification, ParameterNames, Parameters, result,
                        DescriptorNames, new CDKException("Error while calculating the ATS_mass descriptor: "
                                + ex.Message, ex));
            }
        }

        public override IReadOnlyList<string> ParameterNames { get; } = Array.Empty<string>();
        public override object GetParameterType(string name) => null;

        public override IReadOnlyList<object> Parameters
        {
            get { return null; }
            set { }
        }

        public override IReadOnlyList<string> DescriptorNames => Names;

        public override IImplementationSpecification Specification => specification;
        private static readonly DescriptorSpecification specification =
            new DescriptorSpecification(
                "http://www.blueobelisk.org/ontologies/chemoinformatics-algorithms/#autoCorrelationMass",
                typeof(AutocorrelationDescriptorMass).FullName,
                "The Chemistry Development Kit");

        public override IDescriptorResult DescriptorResultType { get; } = new ArrayResult<double>(5);

        IDescriptorValue IMolecularDescriptor.Calculate(IAtomContainer container) => Calculate(container);
    }
}
