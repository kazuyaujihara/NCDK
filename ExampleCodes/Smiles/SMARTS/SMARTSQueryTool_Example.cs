﻿using NCDK.Aromaticities;
using NCDK.Graphs;
using NCDK.Silent;
using NCDK.Tools.Manipulator;
using System.Collections.Generic;

namespace NCDK.Smiles.SMARTS
{
    static class SMARTSQueryTool_Example
    {
        static void Main()
        {
            {
                #region
                SmilesParser sp = new SmilesParser();
                IAtomContainer atomContainer = sp.ParseSmiles("CC(=O)OC(=O)C");
                SMARTSQueryTool querytool = new SMARTSQueryTool("O=CO", ChemObjectBuilder.Instance);
                bool status = querytool.Matches(atomContainer);
                if (status)
                {
                    int nmatch = querytool.MatchesCount;
                    var mappings = querytool.GetMatchingAtoms();
                    foreach (var atomIndices in mappings)
                    {
                        // do something
                    }
                }
                #endregion
            }
            {
                string someSmartsPattern = null;
                IChemObjectSet<IAtomContainer> molecules = null;
                #region SetAromaticity
                SMARTSQueryTool sqt = new SMARTSQueryTool(someSmartsPattern, ChemObjectBuilder.Instance);
                sqt.SetAromaticity(new Aromaticity(ElectronDonation.CDKModel, Cycles.CDKAromaticSetFinder));
                foreach (var molecule in molecules)
                {
                    // CDK Aromatic model needs atom types
                    AtomContainerManipulator.PercieveAtomTypesAndConfigureAtoms(molecule);
                    sqt.Matches(molecule);
                }
                #endregion
            }
        }
    }
}
