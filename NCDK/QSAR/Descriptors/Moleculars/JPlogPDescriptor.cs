﻿/* Copyright (C) 2018  Jeffrey Plante (Lhasa Limited)  <Jeffrey.Plante@lhasalimited.org>
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

using NCDK.Aromaticities;
using NCDK.Tools.Manipulator;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NCDK.QSAR.Descriptors.Moleculars
{
    /// <summary>
    /// A logP model donated by Lhasa Limited. It is based on an atom contribution
    /// model. See <token>cdk-cite-Plante2018</token>.
    /// </summary>
    // @author Jeffrey Plante
    // @cdk.created 2018-12-15
    // @cdk.keyword JPLogP
    // @cdk.keyword descriptor
    // @cdk.keyword lipophilicity
    [DescriptorSpecification(DescriptorTargets.AtomContainer, "JPlogP developed at Lhasa Limited www.lhasalimited.org", Vendor = "Jeffrey Plante - Lhasa Limited")]
    public class JPlogPDescriptor : AbstractDescriptor, IMolecularDescriptor
    {
        /// <summary>
        /// Initialises the required coefficients for the trained model from the paper.
        /// </summary>
        private static IReadOnlyDictionary<int, double> GetDefaultCoeffs() => WeightsCurr;

        private readonly IReadOnlyDictionary<int, double> coeffs;

        /// <summary>
        /// Default constructor which will setup the required coefficients to enable
        /// a prediction
        /// </summary>
        public JPlogPDescriptor(IReadOnlyDictionary<int, double> coeffs = null)
        {
            this.coeffs = coeffs ?? GetDefaultCoeffs();
        }

        [DescriptorResult]
        public class Result : AbstractDescriptorResult
        {
            public Result(double value)
            {
                this.JLogP = value;
            }

            public Result(Exception e) : base(e) { }

            [DescriptorResultProperty]
            public double JLogP { get; private set; }

            public double Value => JLogP;
        }

        /// <summary>
        /// Given a structure in the correct configuration (explicit H and aromatised) it will return the logP as a Double
        /// or if it is out of domain (encounters an unknown atomtype) it will return <see cref="Double.NaN"/>.
        /// </summary>
        /// <param name="container">the structure to calculate which have explicit H and be aromatised.</param>
        /// <returns>The calculated logP</returns>
        public Result Calculate(IAtomContainer container)
        {
            container = (IAtomContainer)container.Clone();
            AtomContainerManipulator.PercieveAtomTypesAndConfigureAtoms(container);
            var hAdder = CDK.HydrogenAdder;
            hAdder.AddImplicitHydrogens(container);
            AtomContainerManipulator.ConvertImplicitToExplicitHydrogens(container);
            Aromaticity.CDKLegacy.Apply(container);
            return new JPlogPCalculator(container, coeffs).Calculate();
        }

        /// <summary>
        /// The class that calculated the logP according to the JPlogP method described in:
        /// Journal of Cheminformatics 2018 10:61 https://doi.org/10.1186/s13321-018-0316-5
        /// This is lower level access and should normally be obtained through the descriptor above.
        /// </summary>
        // @author Jeffrey
        internal class JPlogPCalculator
        {
            class UndefinedAtomTypeException : Exception
            {
                public int AtomTypeCode { get; private set; }

                public UndefinedAtomTypeException()
                    : this(0)
                {
                }

                public UndefinedAtomTypeException(int typeCode)
                {
                    AtomTypeCode = typeCode;
                }
            }

            /// <summary>
            /// Initialises the required coefficients for the trained model from the paper.
            /// </summary>
            private readonly IReadOnlyDictionary<int, double> coeffs;

            private readonly IAtomContainer container;

            public JPlogPCalculator(IAtomContainer container, IReadOnlyDictionary<int, double> coeffs)
            {
                this.coeffs = coeffs;
                this.container = container;
            }

            public Result Calculate()
            {
                var result = CalcLogP();
                return new Result(result);
            }

            public double CalcLogP()
            {
                double logP = 0.0;

                try
                {
                    foreach (var atom in container.Atoms)
                    {
                        var atomtype = GetAtomTypeCode(atom);
                        if (coeffs.TryGetValue(atomtype, out double increment))
                        {
                            logP += increment;
                        }
                        else
                        {
                            throw new UndefinedAtomTypeException(atomtype);
                        }
                    }
                }
                catch (UndefinedAtomTypeException e)
                {
                    Trace.TraceInformation((e.AtomTypeCode == 0 ? "Atom type" : e.AtomTypeCode.ToString()) + " is not found");
                    return double.NaN;
                }

                return logP;
            }

            /// <summary>
            /// Used in Training the model
            /// </summary>
            /// <param name="container"></param>
            /// <returns><see cref="IDictionary{Int32, Int32}"/> representing the Hologram of the given structure</returns>
            public static Dictionary<int, int> GetMappedHologram(IAtomContainer container)
            {
                var holo = new Dictionary<int, int>();
                foreach (var atom in container.Atoms)
                {
                    var type = GetAtomTypeCode(atom);
                    if (holo.ContainsKey(type))
                    {
                        int count = holo[type];
                        count++;
                        holo[type] = count;
                    }
                    else
                    {
                        holo[type] = 1;
                    }
                }
                return holo;
            }

            /// <summary>
            /// Returns the AtomCode for the logP atomtype as previously developed at
            /// Lhasa see citation at top of class for more information
            /// </summary>
            /// <param name="atom">the atom to type</param>
            /// <returns>
            /// Integer of the type CAANSS c = charge+1 AA = Atomic Number N =
            /// NonH Neighbour count SS = elemental subselection via bonding and
            /// neighbour identification
            /// </returns>
            private static int GetAtomTypeCode(IAtom atom)
            {
                int returnMe = 0;
                var atomicNumber = atom.AtomicNumber;

                // Setup initial parameters for assigning atomtype
                int nonHNeighbours = NonHNeighbours(atom);
                int charge = atom.FormalCharge.Value;
                int aNum = atom.AtomicNumber;

                // Initialise the type integer with what we know so far
                returnMe += 100000 * (charge + 1);
                returnMe += aNum * 1000;
                returnMe += nonHNeighbours * 100;

                int toadd;
                switch (atomicNumber)
                {
                    case AtomicNumbers.C:
                        toadd = GetCarbonSpecial(atom);
                        break;

                    case AtomicNumbers.N:
                        toadd = GetNitrogenSpecial(atom);
                        break;

                    case AtomicNumbers.O:
                        toadd = GetOxygenSpecial(atom);
                        break;

                    case AtomicNumbers.H:
                        toadd = GetHydrogenSpecial(atom);
                        break;

                    case AtomicNumbers.F:
                        toadd = GetFluorineSpecial(atom);
                        break;

                    default:
                        toadd = GetDefaultSpecial(atom);
                        break;
                }
                returnMe += toadd;
                // check for any errors and if so return a null value
                if (toadd != 99)
                {
                    return returnMe;
                }
                else
                    throw new UndefinedAtomTypeException();
            }

            /// <summary>
            /// Determines and returns the SS (subsection) portion of the atomtype integer for a Hydrogen Atom
            /// </summary>
            /// <param name="atom"></param>
            /// <returns>the final 2 digits for the given atom</returns>
            internal static int GetHydrogenSpecial(IAtom atom)
            {
                int toadd = 0;
                int bondCount = atom.Bonds.Count;
                if (bondCount > 0)
                {
                    var neighbour = atom.Bonds[0].GetOther(atom);
                    int numNeighbours = neighbour.Bonds.Count;
                    if (neighbour.AtomicNumber == AtomicNumbers.C)
                    {
                        if (CarbonylConjugated(neighbour))
                            toadd = 51;
                        else
                        {
                            double formalOxState = GetNumMoreElectronegativethanCarbon(neighbour);
                            switch (numNeighbours)
                            {
                                case 4:
                                    if (formalOxState == 0.0)
                                        toadd = 46;
                                    else if (formalOxState == 1.0)
                                        toadd = 47;
                                    else if (formalOxState == 2.0)
                                        toadd = 48;
                                    else if (formalOxState == 3.0)
                                        toadd = 49;
                                    break;
                                case 3:
                                    if (formalOxState == 0.0)
                                        toadd = 47;
                                    else if (formalOxState == 1.0)
                                        toadd = 48;
                                    else if (formalOxState >= 2.0)
                                        toadd = 49;
                                    break;
                                case 2:
                                    if (formalOxState == 0.0)
                                        toadd = 48;
                                    else if (formalOxState >= 1.0)
                                        toadd = 49;
                                    break;
                                case 1:
                                    toadd = 121;
                                    break;
                            }
                        }
                    }
                    else
                        toadd = 50;
                }
                return toadd;
            }

            /// <summary>
            /// Determines and returns the SS (subsection) portion of the atomtype integer for a "Default" ie not C,N,O,H,F Atom
            /// </summary>
            /// <param name="atom"></param>
            /// <returns>the final 2 digits for the given atom</returns>
            internal static int GetDefaultSpecial(IAtom atom)
            {
                int toadd;
                int[] polarbondCounts = GetPolarBondArray(atom);
                int singleBondPolar = polarbondCounts[0];
                int doubleBondPolar = polarbondCounts[2];
                int tripleBondPolar = polarbondCounts[3];
                int aromaticBondPolar = polarbondCounts[1];

                if (atom.IsAromatic)
                    toadd = 10;
                else
                    toadd = singleBondPolar + doubleBondPolar + tripleBondPolar + aromaticBondPolar;
                return toadd;
            }

            /// <summary>
            /// Determines and returns the SS (subsection) portion of the atomtype integer for a Fluorine Atom
            /// </summary>
            /// <param name="atom"></param>
            /// <returns>the final 2 digits for the given atom</returns>
            internal static int GetFluorineSpecial(IAtom atom)
            {
                int toadd;
                int numconn = atom.Bonds.Count;
                if (numconn == 1)
                {
                    var bond = atom.Bonds[0];
                    var next = bond.GetOther(atom);
                    var neighbourconn = next.Bonds.Count;
                    var ox = GetNumMoreElectronegativethanCarbon(next);
                    switch (next.AtomicNumber)
                    {
                        case AtomicNumbers.S:
                            toadd = 8; // F-S
                            break;
                        case AtomicNumbers.B:
                            toadd = 9; // F-B
                            break;
                        default:
                            toadd = 1; // F-hetero
                            break;
                        case AtomicNumbers.C:
                            switch (neighbourconn)
                            {
                                case 2:
                                    toadd = 2;
                                    break;
                                case 3:
                                    toadd = 3;
                                    break;
                                case 4:
                                    if (ox <= 2)
                                        toadd = 5;
                                    else if (ox > 2)
                                        toadd = 7;
                                    else
                                        toadd = 99;
                                    break;
                                default:
                                    toadd = 99;
                                    break;
                            }
                            break;
                    }
                }
                else
                {
                    toadd = 99;
                }
                return toadd;
            }

            /// <summary>
            /// Determines and returns the SS (subsection) portion of the atomtype integer for an Oxygen Atom
            /// </summary>
            /// <param name="atom"></param>
            /// <returns>the final 2 digits for the given atom</returns>
            internal static int GetOxygenSpecial(IAtom atom)
            {
                int toadd;
                int numConnections = atom.Bonds.Count;
                switch (numConnections)
                {
                    case 2:
                        if (BoundTo(atom, AtomicNumbers.N))
                            toadd = 1; // A-O-N
                        else if (BoundTo(atom, AtomicNumbers.S))
                            toadd = 2; // A-O-S
                        else if (atom.IsAromatic)
                            toadd = 8; // A-=O-=A
                        else
                            toadd = 3; // A-O-A
                        break;
                    case 1:
                        if (BoundTo(atom, AtomicNumbers.N))
                            toadd = 4; // O=N
                        else if (BoundTo(atom, AtomicNumbers.S))
                            toadd = 5; // O=S
                        else if (CheckAlphaCarbonyl(atom, AtomicNumbers.O))
                            toadd = 6; // O=A-O
                        else if (CheckAlphaCarbonyl(atom, AtomicNumbers.N))
                            toadd = 9; // O=A-N
                        else if (CheckAlphaCarbonyl(atom, AtomicNumbers.S))
                            toadd = 10; // O=A-S
                        else
                            toadd = 7; // O=A
                        break;
                    default:
                        toadd = 0;
                        break;
                }
                return toadd;
            }

            /// <summary>
            /// Determines and returns the SS (subsection) portion of the atomtype integer for a Nitrogen Atom 
            /// </summary>
            /// <param name="atom"></param>
            /// <returns>the final 2 digits for the given atom</returns>
            internal static int GetNitrogenSpecial(IAtom atom)
            {
                int toadd;
                int numConnections = atom.Bonds.Count;
                int[] polarbondCounts = GetPolarBondArray(atom);
                int singleBondPolar = polarbondCounts[0];
                switch (numConnections)
                {
                    case 4:
                        toadd = 9; // A-N(-A)(-A)-A probably also positively charged
                        break;
                    case 3:
                        if (NextToAromatic(atom))
                            toadd = 1; // A-N(-A)-Aromatic Atom
                        else if (CarbonylConjugated(atom))
                            toadd = 2; // N-A=Polar
                        else if (DoubleBondHetero(atom))
                            toadd = 10; // A-(A-)N={ONS} probably also charged
                        else if (singleBondPolar > 0)
                            toadd = 3; // A-N(-A)-Polar
                        else
                            toadd = 4; // A-N(-A)-A
                        break;
                    case 2:
                        if (atom.IsAromatic)
                            toadd = 5; // A-=N-=A
                        else if (DoubleBondHetero(atom))
                            toadd = 6; // A-N=Polar
                        else
                            toadd = 7; // A-N=A
                        break;
                    case 1:
                        toadd = 8; // N%A
                        break;
                    default:
                        toadd = 0; // N????
                        break;
                }
                return toadd;
            }

            /// <summary>
            /// Determines and returns the SS (subsection) portion of the atomtype integer for a Carbon Atom
            /// </summary>
            /// <param name="atom"></param>
            /// <returns>the final 2 digits for the given atom</returns>
            internal static int GetCarbonSpecial(IAtom atom)
            {
                int toadd;
                int numConnections = atom.Bonds.Count;
                int[] polarbondCounts = GetPolarBondArray(atom);
                int singleBondPolar = polarbondCounts[0];
                int doubleBondPolar = polarbondCounts[2];
                int tripleBondPolar = polarbondCounts[3];
                int aromaticBondPolar = polarbondCounts[1];
                switch (numConnections)
                {
                    case 4: // 4 connections
                        toadd = 2; // sp3 C
                        if (singleBondPolar > 0)
                            toadd = 3; // sp3 carbon next to S,N,P,O
                        break;
                    case 3: // 3 connections
                        if (atom.IsAromatic)
                        {
                            if (aromaticBondPolar >= 1 && singleBondPolar == 0)
                                toadd = 11; // A-=C(-A)-=Polar
                            else if (aromaticBondPolar == 0 && singleBondPolar == 1)
                                toadd = 5; // A-=C(-Polar)-=A
                            else if (aromaticBondPolar >= 1 && singleBondPolar == 1)
                                toadd = 13; // A-=C-(Polar)-=Polar or P-=C(-P)-=P
                            else
                                toadd = 4; // A-=C(-A)-=A
                        }
                        else
                        {
                            if (doubleBondPolar == 1 && singleBondPolar == 0)
                                toadd = 7; // A-C(=Polar)-A
                            else if (singleBondPolar >= 1 && doubleBondPolar == 0)
                                toadd = 8; // A-C(=A)-Polar
                            else if (doubleBondPolar == 1 && singleBondPolar >= 1)
                                toadd = 14; // A-C(=P)-P
                            else
                                toadd = 6; // A-C(=A)-A
                        }
                        break;
                    case 2: // 2 connections
                        if (tripleBondPolar == 1 && singleBondPolar == 0)
                            toadd = 12; // A-C%Polar
                        else if (tripleBondPolar == 0 && singleBondPolar == 1)
                            toadd = 10; // Polar-C%A
                        else if (tripleBondPolar == 1 && singleBondPolar == 1)
                            toadd = 15; // p-C%P
                        else
                            toadd = 9; // A-C%A
                        break;
                    default:
                        toadd = 0; // C???
                        if (singleBondPolar > 0 || doubleBondPolar > 0 || aromaticBondPolar > 0 || tripleBondPolar > 0)
                            toadd = 1; // C??Polar
                        break;
                }
                return toadd;
            }

            /// <summary>
            /// Should be called from the carbonyl oxygen
            /// </summary>
            /// <returns><see langword="true"/> if there is an atom of sybmol alpha to the carbonyl</returns>
            internal static bool CheckAlphaCarbonyl(IAtom atom, int atomicNumber)
            {
                foreach (var bond in atom.Bonds)
                {
                    var next = bond.GetOther(atom);
                    foreach (var bond2 in next.Bonds)
                    {
                        var next2 = bond2.GetOther(next);
                        if (next2.AtomicNumber == atomicNumber && bond2.Order.Numeric() == 1)
                            return true;
                    }
                }
                return false;
            }

            /// <returns><see langword="true"/> if the atom has a bond to an atom of the given symbol</returns>
            internal static bool BoundTo(IAtom atom, int atomicNumber)
            {
                foreach (var bond in atom.Bonds)
                {
                    IAtom next = bond.GetOther(atom);
                    if (next.AtomicNumber == atomicNumber)
                        return true;
                }
                return false;
            }

            /// <returns>bond order for electron withdrawing atoms from the given atom ie =O = 2</returns>
            internal static double GetNumMoreElectronegativethanCarbon(IAtom atom)
            {
                double returnme = 0;
                foreach (var bond in atom.Bonds)
                {
                    IAtom compare = bond.GetOther(atom);
                    double bondOrder = bond.Order.Numeric();
                    if (ElectronWithdrawing(compare))
                        returnme += bondOrder;
                }
                return returnme;
            }

            /// <returns><see langword="true"/> if the atom is considered electron withdrawing relative to carbon (N,O,S,F,Cl,Br,I)</returns>
            internal static bool ElectronWithdrawing(IAtom atom)
            {
                switch (atom.AtomicNumber)
                {
                    case AtomicNumbers.N:
                    case AtomicNumbers.O:
                    case AtomicNumbers.S:
                    case AtomicNumbers.F:
                    case AtomicNumbers.Cl:
                    case AtomicNumbers.Br:
                    case AtomicNumbers.I:
                        return true;
                    default:
                        return false;
                }
            }

            /// <returns>number of heavy atoms bound to the atom</returns>
            internal static int NonHNeighbours(IAtom atom)
            {
                int returnMe = 0;
                foreach (var bond in atom.Bonds)
                {
                    var neighbor = bond.GetOther(atom);
                    if (neighbor.AtomicNumber != 1)
                        returnMe++;
                }
                return returnMe;
            }

            /// <returns>in array of bond information to polar atoms array form is [single, aromatic, double, triple]</returns>
            internal static int[] GetPolarBondArray(IAtom atom)
            {
                int[] array = new int[4];
                foreach (var bond in atom.Bonds)
                {
                    var neighbor = bond.GetOther(atom);
                    if (IsPolar(neighbor))
                    {
                        if (bond.IsAromatic)
                            array[1]++;
                        else if (bond.Order.Numeric() == 1)
                            array[0]++;
                        else if (bond.Order.Numeric() == 2)
                            array[2]++;
                        else if (bond.Order.Numeric() == 3)
                            array[3]++;
                    }
                }
                return array;
            }

            /// <returns><see langword="true"/> if atom is a "polar atom" (O,N,S,P)</returns>
            internal static bool IsPolar(IAtom atom)
            {
                switch (atom.AtomicNumber)
                {
                    case AtomicNumbers.O:
                    case AtomicNumbers.S:
                    case AtomicNumbers.N:
                    case AtomicNumbers.P:
                        return true;
                    default:
                        return false;
                }
            }

            /// <returns><see langword="true"/> if atom is doublebonded to a heteroatom (polar atom)</returns>
            internal static bool DoubleBondHetero(IAtom atom)
            {
                foreach (var bond in atom.Bonds)
                {
                    var neighbour = bond.GetOther(atom);
                    if (!(bond.IsAromatic) && IsPolar(neighbour) && bond.Order.Numeric() == 2)
                        return true;
                }
                return false;
            }

            /// <returns><see langword="true"/> if atom is singly bonded to a carbonyl</returns>
            internal static bool CarbonylConjugated(IAtom atom)
            {
                foreach (var bond in atom.Bonds)
                {
                    var next = bond.GetOther(atom);
                    if (!(bond.IsAromatic) && bond.Order.Numeric() == 1 && DoubleBondHetero(next))
                        return true;
                }
                return false;
            }

            /// <returns><see langword="true"/> if single bonded to an aromatic atom</returns>
            internal static bool NextToAromatic(IAtom atom)
            {
                if (!atom.IsAromatic)
                {
                    foreach (var bond in atom.Bonds)
                    {
                        var next = bond.GetOther(atom);
                        if (next.IsAromatic
                                && bond.Order.Numeric() == 1)
                            return true;
                    }
                }
                return false;
            }
        }

        public IReadOnlyDictionary<int, double> Coefficients
        {
            get => coeffs;
        }

        /// <summary>
        /// initialise the model with the required values. Could instead read from a
        /// serialised file, but this is simpler and the number of coefficients isn't
        /// too large. Quite simple to update as well when able to output the model
        /// to the screen with minor text manupilation with regex strings. 
        /// </summary>
        public static readonly IReadOnlyDictionary<int, double> WeightsCurr = new Dictionary<int, double>
        {
            { 115200, 0.3428999504964441 },
            { 134400, -0.6009339899021935 },
            { 207110, -0.2537868203838485 },
            { 134404, -1.175240011610751 },
            { 153100, 0.9269788584798107 },
            { 153101, 1.0143514773529836 },
            { 133402, -0.25834811742258956 },
            { 114200, 0.02852160272741147 },
            { 133403, -0.8194572348477968 },
            { 5401, 0.10394247591686294 },
            { 101147, 1.1304660454056645 },
            { 5402, 0.05199289233087018 },
            { 101146, 1.2682622213479229 },
            { 133401, -0.3667171872036366 },
            { 5403, 0.6787470799044643 },
            { 101149, 1.1949147655182892 },
            { 101148, 1.140089012601143 },
            { 101151, 1.124474664082366 },
            { 133404, -0.6914424692323852 },
            { 101150, -0.243037761960754 },
            { 107301, 0.17827258865107412 },
            { 107303, -0.018054804206490728 },
            { 107302, 0.174747113462705 },
            { 7207, -0.1900259417225229 },
            { 107304, 0.15377727288498777 },
            { 109101, 0.4139612626967727 },
            { 115501, -0.14127468400014018 },
            { 115500, 0.17611532140449737 },
            { 115503, 0.01466761555846904 },
            { 109103, 0.26645165703555906 },
            { 115502, -0.12059099254331078 },
            { 115505, -0.3426049607280402 },
            { 109105, 0.43024980310815464 },
            { 115504, -0.10220447776639764 },
            { 207409, -0.2390615198632319 },
            { 109107, 0.46498369443558685 },
            { 109109, 0.4802991193785544 },
            { 109108, 0.2432096581199898 },
            { 134202, -0.6346778955294479 },
            { 134200, -0.049120673015459124 },
            { 134201, 0.011636535054497877 },
            { 106303, -1.3926887806607753 },
            { 106302, -1.1712347846892444 },
            { 106305, 0.11166429492968204 },
            { 134210, 0.8723651043132042 },
            { 106304, 0.2093224791516782 },
            { 106307, -0.2713000174540411 },
            { 106306, 0.01721171808054029 },
            { 108101, -0.09302944637095042 },
            { 106308, 0.02622454278720872 },
            { 108103, 0.016350079235673567 },
            { 106311, 0.14287513828273285 },
            { 108102, 0.09827832065783548 },
            { 108105, -0.47007800462613053 },
            { 106313, 0.29015591756689035 },
            { 108104, 0.015437937018121088 },
            { 108107, 0.03827614806405689 },
            { 108106, 0.19321034882780294 },
            { 106314, -0.3943404186929503 },
            { 108109, -0.10401802166752877 },
            { 116301, -0.05382098343764403 },
            { 116300, 0.7387521460456399 },
            { 116303, -0.16756182500979416 },
            { 116302, -0.13139795079560512 },
            { 108110, 0.09217290650820302 },
            { 5201, 0.23346448860951585 },
            { 5202, 0.2039139119266068 },
            { 133200, -0.30769228187947917 },
            { 208208, 0.7096167841949101 },
            { 133201, -0.4991951722354514 },
            { 105301, -0.16207989873206854 },
            { 105300, -0.17440788934127419 },
            { 105303, -0.14262227829859978 },
            { 105302, -0.1772536453086892 },
            { 107101, 0.06472635260371458 },
            { 107103, -0.25579034271921075 },
            { 107102, 0.07815605927333318 },
            { 107104, -0.3248358665028741 },
            { 107107, 0.40442424583948916 },
            { 107106, 0.396893949325775 },
            { 207207, -0.07223876787467944 },
            { 115301, -0.02360338462248146 },
            { 107108, 0.11602222985765208 },
            { 207206, -0.24327541812800021 },
            { 115300, 0.08742831050292114 },
            { 115303, -0.23502791004771306 },
            { 115302, -0.10635975733575764 },
            { 117101, 0.7375898512161616 },
            { 117100, 0.711562233142568 },
            { 153200, 0.7953592172870871 },
            { 106103, -3.373528417699127 },
            { 106102, -3.222960398502975 },
            { 116600, 1.2414649166226785 },
            { 106107, -2.1610769299516286 },
            { 106106, -1.794277022889798 },
            { 114301, -0.13076494426939203 },
            { 106109, -0.7983974980016113 },
            { 114300, -0.2992443066268472 },
            { 114302, -0.3024419065452111 },
            { 106112, -1.1706517781448094 },
            { 116101, 1.25794766342382 },
            { 116100, 0.7033847677524618 },
            { 216210, 0.7111335744036255 },
            { 134302, -0.3781625467763615 },
            { 134303, -0.363065393849358 },
            { 134300, -0.46600673735969483 },
            { 134301, -0.38176583952591553 },
            { 106403, -0.6031848047124038 },
            { 106402, -0.19127514916328328 },
            { 8104, -0.24231144996862355 },
            { 108201, 0.03078259547644759 },
            { 8105, -0.42080392882157724 },
            { 108203, 0.16517108509661693 },
            { 8106, -0.29674843353741903 },
            { 108202, 0.12117897946674977 },
            { 116401, 0.7441487272627919 },
            { 108208, 0.06651412646122902 },
            { 116403, 0.3031855131038271 },
            { 116402, 0.543649606560247 },
            { 133302, -0.7027480278305203 },
            { 114100, 0.3786112153751248 },
            { 133303, -0.431414306011833 },
            { 116404, 0.02729394485921511 },
            { 133300, -0.20426419244061747 },
            { 133301, 0.2550357745554604 },
            { 135100, 0.8603178696688789 },
            { 135101, 0.7236559576494113 },
            { 107201, 0.260897789525647 },
            { 107203, 0.2070230861251806 },
            { 107202, 0.16622067949480449 },
            { 107205, -0.2544031618211347 },
            { 7108, 0.2615348564811823 },
            { 207303, -0.4723141310610628 },
            { 107204, 0.13376742355228766 },
            { 207302, 0.24697598201746412 },
            { 107207, 0.316655927726688 },
            { 207301, 0.36095025588716984 },
            { 107206, 0.3112912468366892 },
            { 115401, -0.33411394854894955 },
            { 115400, -0.10319130468863787 },
            { 115403, -0.7380151037685063 },
            { 207304, -0.07460535077700184 },
            { 115402, -0.38147795848833443 },
            { 115404, -0.8125502294660335 },
            { 207310, 0.3572618921544123 },
            { 153302, 0.6010094256860743 },
            { 134100, -0.13465231260837543 },
            { 134101, 0.11087519417725553 },
            { 153301, 0.5747227306225426 },
            { 106203, -2.393849677660491 },
            { 106202, -2.0873117350423795 },
            { 106204, -0.8226943130543642 },
            { 106207, -1.4051640234159233 },
            { 106206, -0.8797695620379107 },
            { 106209, 0.1943653458623092 },
            { 114401, -0.16364741554376241 },
            { 106208, -1.0638733077308757 },
            { 114400, -0.11449728057513861 },
            { 106211, -1.0460240915898267 },
            { 114403, -0.15482665868271833 },
            { 114402, -0.10981861418848725 },
            { 106210, 0.16195495590632014 },
            { 106212, -0.25091180447462924 },
            { 114404, -0.13028646729956206 },
            { 106215, -0.14549848501097237 },
            { 106214, -1.4797542026181651 },
            { 116201, 0.6388354010094074 },
            { 116200, 0.7924621516585404 },
            { 116202, 0.533270577934211 },
            { 216301, 0.16747913472407247 },
            { 216300, 0.8099240433489436 },
            { 105201, 0.07571701124699833 },
            { 105202, -0.06906898812339575 },
            { 116210, 0.5793769304831321 },
            { 216310, 0.16964757212192544 }
        };

        public static IReadOnlyDictionary<int, double> WeightsPlante2018 { get; } = new Dictionary<int, double>
        {
            { 115201, 0.09994758075256505 },
            { 115200, 0.3379378715836258 },
            { 134400, -0.601185704550091 },
            { 115202, 0.30788026393512663 },
            { 207110, -0.26496784659823264 },
            { 134404, -1.1724223083800398 },
            { 115210, -0.08346526510422402 },
            { 153100, 0.9270429695306335 },
            { 153101, 1.0145354986151272 },
            { 116503, 0.4425591506257104 },
            { 133402, -0.2557512835716269 },
            { 114200, 0.01526633068977459 },
            { 133403, -0.8169297847709985 },
            { 5401, 0.10441747048024147 },
            { 101147, 1.2616122128062803 },
            { 5402, 0.05162677089603265 },
            { 101146, 1.3994445700193028 },
            { 133401, -0.3639701318790265 },
            { 5403, 0.6788714142147848 },
            { 101149, 1.3258747052968567 },
            { 101148, 1.2711079053599976 },
            { 101151, 1.2556350911435799 },
            { 133404, -0.6891007636859257 },
            { 101150, -0.2363827296335956 },
            { 107301, 0.1787473725640726 },
            { 107303, -0.016959741231455404 },
            { 107302, 0.17510323694483412 },
            { 7207, -0.1902718970453204 },
            { 107304, 0.1542614658530209 },
            { 109101, 0.41374364339440817 },
            { 115501, -0.14068629542864905 },
            { 115500, 0.17750686328369028 },
            { 115503, 0.013887172778612027 },
            { 109103, 0.26651823980406203 },
            { 115502, -0.11992335751384754 },
            { 115505, -0.34311166776744884 },
            { 109105, 0.43019405241170144 },
            { 115504, -0.1025811855926768 },
            { 207409, -0.23852255872700964 },
            { 109107, 0.46487210540519147 },
            { 109109, 0.4801727828186138 },
            { 109108, 0.2430918227919903 },
            { 134202, -0.631693669606887 },
            { 134200, -0.04910253697266963 },
            { 134201, 0.011171177597208612 },
            { 106303, -1.5239332211866237 },
            { 106302, -1.3023723757750838 },
            { 106305, 0.11154050989797104 },
            { 134210, 0.8725452294362313 },
            { 106304, 0.20930194677993302 },
            { 106307, -0.2690238196488019 },
            { 106306, 0.01713355115342431 },
            { 108101, -0.09994092418236927 },
            { 106308, 0.026068238656834854 },
            { 108103, 0.009836423634389751 },
            { 106311, 0.1427758223243856 },
            { 108102, 0.09143879448168145 },
            { 108105, -0.4749180976677123 },
            { 106313, 0.2897701159093737 },
            { 108104, 0.015474674863268114 },
            { 108107, 0.03602987937616161 },
            { 108106, 0.19048034389371205 },
            { 106314, -0.39178934954486583 },
            { 108109, -0.10666832936758427 },
            { 116301, -0.04914876490157527 },
            { 116300, 0.7367961788572792 },
            { 116303, -0.16169601215900375 },
            { 116302, -0.12643665926860584 },
            { 108110, 0.08928780909185945 },
            { 5201, 0.2201279736215536 },
            { 5202, 0.19045980382858146 },
            { 133200, -0.3076946592117401 },
            { 208208, 0.7099234399396344 },
            { 133201, -0.49680932374826 },
            { 105301, -0.1621399782797916 },
            { 105300, -0.174370011345452 },
            { 105303, -0.1432571497021001 },
            { 105302, -0.17755682989875274 },
            { 107101, 0.051702151541644426 },
            { 107103, -0.2691841786263712 },
            { 107102, 0.06496457627779738 },
            { 107104, -0.33802382979998147 },
            { 107107, 0.394978253857827 },
            { 107106, 0.3859974866654674 },
            { 207207, -0.07239795705976523 },
            { 115301, -0.023836916386805847 },
            { 107108, 0.11642255395641928 },
            { 207206, -0.24558051744952064 },
            { 115300, 0.08797456644925557 },
            { 115303, -0.23605983536956895 },
            { 115302, -0.10814292962539623 },
            { 117101, 0.7369857420763359 },
            { 117100, 0.7116079866622599 },
            { 153200, 0.7888787630537003 },
            { 106103, -3.767115237033892 },
            { 106102, -3.616478490407742 },
            { 116600, 1.2424471654297324 },
            { 106107, -2.416958126564593 },
            { 106106, -2.0565095206356196 },
            { 114301, -0.13761744158191205 },
            { 106109, -0.929959267287108 },
            { 114300, -0.3058393642193975 },
            { 114302, -0.3095457315739295 },
            { 106112, -1.3012751020335893 },
            { 116101, 1.2551746199963494 },
            { 116100, 0.7001698255404422 },
            { 105100, 0.36881886842575007 },
            { 216210, 0.7113783097640652 },
            { 134302, -0.37554769340770927 },
            { 134303, -0.36036185997589393 },
            { 115100, 0.6096777283013224 },
            { 134300, -0.4657894122488925 },
            { 134301, -0.3795150596315356 },
            { 106403, -0.6035455702256183 },
            { 106402, -0.19123067665076543 },
            { 8104, -0.24195926611016633 },
            { 108201, 0.030702954811754002 },
            { 8105, -0.4215369017701643 },
            { 108203, 0.16547595574733062 },
            { 8106, -0.2964579842443157 },
            { 108202, 0.12058552604519218 },
            { 116401, 0.7460081218102875 },
            { 116400, 0.9078902724309305 },
            { 108208, 0.06665724849079398 },
            { 116403, 0.3132146936243478 },
            { 116402, 0.5536499705080733 },
            { 133302, -0.7030388263360682 },
            { 114100, 0.3587262776601395 },
            { 133303, -0.4317778955330324 },
            { 116404, 0.0372737885433136 },
            { 133300, -0.2042709645502799 },
            { 133301, 0.25473886515880656 },
            { 135100, 0.8603892414982898 },
            { 135101, 0.7235643505309818 },
            { 107201, 0.25454958160787483 },
            { 107203, 0.20036994532453556 },
            { 107202, 0.15973823557158393 },
            { 107205, -0.2543417785988061 },
            { 7108, 0.2634019114789942 },
            { 207303, -0.47290811359153156 },
            { 107204, 0.1275311923081761 },
            { 207302, 0.24535814336356004 },
            { 107207, 0.3145047278208787 },
            { 207301, 0.36070459894156437 },
            { 107206, 0.3113299058370724 },
            { 115401, -0.33332080419904164 },
            { 115400, -0.10861521692106911 },
            { 115403, -0.7356011823089021 },
            { 207304, -0.07464529856367844 },
            { 115402, -0.37912343134016635 },
            { 115404, -0.8103937160471404 },
            { 207310, 0.35707776316923207 },
            { 153302, 0.6015475892374368 },
            { 134100, -0.1363315394071071 },
            { 134101, 0.1092440266426836 },
            { 153301, 0.5748646504612611 },
            { 106203, -2.6563042881537227 },
            { 106202, -2.349643420766774 },
            { 106204, -0.953850490807928 },
            { 106207, -1.5339493033443787 },
            { 106206, -1.0109064714897187 },
            { 106209, 0.19490613802180773 },
            { 114401, -0.16389842380630032 },
            { 106208, -1.195325147036 },
            { 114400, -0.11455385699777243 },
            { 106211, -1.1770219185049515 },
            { 114403, -0.1553108949228919 },
            { 114402, -0.1103226433176957 },
            { 106210, 0.16163164657523407 },
            { 106212, -0.2512600932013654 },
            { 114404, -0.13111265719465162 },
            { 106215, -0.14610561930731214 },
            { 106214, -1.6084019086078098 },
            { 116201, 0.6390448917281739 },
            { 116200, 0.7928741182178681 },
            { 116202, 0.5335850658686033 },
            { 216301, 0.16768140357444056 },
            { 216300, 0.8104414861979382 },
            { 105201, 0.06904337130830694 },
            { 105202, -0.0745914491562598 },
            { 116210, 0.5791544003852439 },
            { 216310, 0.16982252294512776},
        };

        IDescriptorResult IMolecularDescriptor.Calculate(IAtomContainer mol)
            => Calculate(mol);
    }
}
