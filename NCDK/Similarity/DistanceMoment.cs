using NCDK.Common.Mathematics;
using System;
using NCDK.Numerics;

namespace NCDK.Similarity
{
    /// <summary>
    /// Fast similarity measure for 3D structures.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class implements a fast descriptor based 3D similarity measure described by Ballester et al
    /// (<token>cdk-cite-BALL2007</token>). The approach calculates the distances of each atom to four specific points: the
    /// centroid of the molecule, the atom that is closest to the centroid, the atom that is farthest from the
    /// centroid and the atom that is farthest from the previous atom. Thus we get 4 sets of distance distributions.
    /// The final descriptor set is generated by evaluating the first three moments of each distance distribution.
    /// </para>
    /// <para>
    /// The similarity between two molecules is then evaluated using the inverse of a normalized
    /// Manhattan type metric.
    /// </para>
    /// <para>
    /// This class allows you to evaluate the 3D similarity between two specified molecules as well as
    /// generate the 12 descriptors used to characterize the 3D structure which can then be used for a
    /// variety of purposes such as storing in a database.
    /// </para>
    /// <note type="note">
    /// The methods of this class do not perform hydrogen removal. If you want to
    /// do the calculations excluding hydrogens, you'll need to do it yourself. Also, if the molecule has
    /// disconnected components, you should consider one (usually the largest), otherwise all components
    /// are considered in the calculation.
    /// </note>
    /// </remarks>
    // @author Rajarshi Guha
    // @cdk.created 2007-03-11
    // @cdk.keyword similarity, 3D, manhattan
    // @cdk.githash
    // @cdk.module fingerprint
    public class DistanceMoment
    {
        private static Vector3 GetGeometricCenter(IAtomContainer atomContainer)
        {
            double x = 0;
            double y = 0;
            double z = 0;

            foreach (var atom in atomContainer.Atoms)
            {
                Vector3? p = atom.Point3D;
                if (p == null) throw new CDKException("Molecule must have 3D coordinates");
                x += p.Value.X;
                y += p.Value.Y;
                z += p.Value.Z;
            }
            x /= atomContainer.Atoms.Count;
            y /= atomContainer.Atoms.Count;
            z /= atomContainer.Atoms.Count;
            return new Vector3(x, y, z);
        }

        private static double Mu1(double[] x)
        {
            double sum = 0;
            foreach (var aX in x)
            {
                sum += aX;
            }
            return sum / x.Length;
        }

        private static double Mu2(double[] x, double mean)
        {
            double sum = 0;
            foreach (var aX in x)
            {
                sum += (aX - mean) * (aX - mean);
            }
            return sum / (x.Length - 1);
        }

        private static double Mu3(double[] x, double mean, double sigma)
        {
            double sum = 0;
            foreach (var aX in x)
            {
                sum += ((aX - mean) / sigma) * ((aX - mean) / sigma) * ((aX - mean) / sigma);
            }
            return sum / x.Length;
        }

        /// <summary>
        /// Evaluate the 12 descriptors used to characterize the 3D shape of a molecule.
        /// </summary>
        /// <param name="atomContainer">The molecule to consider, should have 3D coordinates</param>
        /// <returns>A 12 element array containing the descriptors.</returns>
        /// <exception cref="CDKException">if there are no 3D coordinates</exception>
        public static double[] GenerateMoments(IAtomContainer atomContainer)
        {
            // lets check if we have 3D coordinates
            int natom = atomContainer.Atoms.Count;

            Vector3 ctd = GetGeometricCenter(atomContainer);
            Vector3 cst = new Vector3();
            Vector3 fct = new Vector3();
            Vector3 ftf = new Vector3();

            double[] distCtd = new double[natom];
            double[] distCst = new double[natom];
            double[] distFct = new double[natom];
            double[] distFtf = new double[natom];

            int counter = 0;
            double min = double.MaxValue; ;
            double max = double.MinValue;

            // eval dist to centroid
            foreach (var atom in atomContainer.Atoms)
            {
                Vector3 p = atom.Point3D.Value;
                double d = Vector3.Distance(p, ctd);
                distCtd[counter++] = d;

                if (d < min)
                {
                    cst.X = p.X;
                    cst.Y = p.Y;
                    cst.Z = p.Z;
                    min = d;
                }
                if (d > max)
                {
                    fct.X = p.X;
                    fct.Y = p.Y;
                    fct.Z = p.Z;
                    max = d;
                }
            }

            // eval dist to cst
            counter = 0;
            foreach (var atom in atomContainer.Atoms)
            {
                Vector3 p = atom.Point3D.Value;
                double d = Vector3.Distance(p, cst);
                distCst[counter++] = d;
            }

            // eval dist to fct
            counter = 0;
            max = double.MinValue;
            foreach (var atom in atomContainer.Atoms)
            {
                Vector3 p = atom.Point3D.Value;
                double d = Vector3.Distance(p, fct);
                distFct[counter++] = d;

                if (d > max)
                {
                    ftf.X = p.X;
                    ftf.Y = p.Y;
                    ftf.Z = p.Z;
                    max = d;
                }
            }

            // eval dist to ftf
            counter = 0;
            foreach (var atom in atomContainer.Atoms)
            {
                Vector3 p = atom.Point3D.Value;
                double d = Vector3.Distance(p, ftf);
                distFtf[counter++] = d;
            }

            var moments = new double[12];

            double mean = Mu1(distCtd);
            double sigma2 = Mu2(distCtd, mean);
            double skewness = Mu3(distCtd, mean, Math.Sqrt(sigma2));
            moments[0] = mean;
            moments[1] = sigma2;
            moments[2] = skewness;

            mean = Mu1(distCst);
            sigma2 = Mu2(distCst, mean);
            skewness = Mu3(distCst, mean, Math.Sqrt(sigma2));
            moments[3] = mean;
            moments[4] = sigma2;
            moments[5] = skewness;

            mean = Mu1(distFct);
            sigma2 = Mu2(distFct, mean);
            skewness = Mu3(distFct, mean, Math.Sqrt(sigma2));
            moments[6] = mean;
            moments[7] = sigma2;
            moments[8] = skewness;

            mean = Mu1(distFtf);
            sigma2 = Mu2(distFtf, mean);
            skewness = Mu3(distFtf, mean, Math.Sqrt(sigma2));
            moments[9] = mean;
            moments[10] = sigma2;
            moments[11] = skewness;

            return moments;
        }

        /// <summary>
        /// Evaluate the 3D similarity between two molecules.
        /// </summary>
        /// <remarks>
        /// The method does not remove hydrogens. If this is required, remove them from the
        /// molecules before passing them here.
        /// </remarks>
        /// <param name="query">The query molecule</param>
        /// <param name="target">The target molecule</param>
        /// <returns>The similarity between the two molecules (ranging from 0 to 1)</returns>
        /// <exception cref="CDKException">if either molecule does not have 3D coordinates</exception>
        public static double Calculate(IAtomContainer query, IAtomContainer target)
        {
            var mom1 = GenerateMoments(query);
            var mom2 = GenerateMoments(target);
            double sum = 0;
            for (int i = 0; i < mom1.Length; i++)
            {
                sum += Math.Abs(mom1[i] - mom2[i]);
            }
            return 1.0 / (1.0 + sum / 12.0);
        }
    }
}
