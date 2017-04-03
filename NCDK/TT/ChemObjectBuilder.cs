﻿

// .NET Framework port by Kazuya Ujihara
// Copyright (C) 2016-2017  Kazuya Ujihara

using NCDK.Formula;
using NCDK.Stereo;
using System.Collections.Generic;
using NCDK.Numerics;

namespace NCDK.Default
{
    public sealed class ChemObjectBuilder
        : IChemObjectBuilder
    {
        public static readonly IChemObjectBuilder Instance = new ChemObjectBuilder();

        public T Create<T>() where T : IAtomContainer, new() => new T();

        public IAminoAcid CreateAminoAcid() => new AminoAcid();
        public IAtom CreateAtom() => new Atom();
        public IAtom CreateAtom(IElement element) => new Atom(element);
        public IAtom CreateAtom(string elementSymbol) => new Atom(elementSymbol);
        public IAtom CreateAtom(string elementSymbol, Vector2 point2d) => new Atom(elementSymbol, point2d);
        public IAtom CreateAtom(string elementSymbol, Vector3 point3d) => new Atom(elementSymbol, point3d);
        public IAtomContainer CreateAtomContainer() => new AtomContainer();
        public IAtomContainer CreateAtomContainer(IAtomContainer container) => new AtomContainer(container);
        public IAtomContainer CreateAtomContainer(IEnumerable<IAtom> atoms, IEnumerable<IBond> bonds) => new AtomContainer(atoms, bonds);
        public IAtomContainerSet<T> CreateAtomContainerSet<T>() where T : IAtomContainer => new AtomContainerSet<T>();
        public IAtomContainerSet<IAtomContainer> CreateAtomContainerSet() => new AtomContainerSet<IAtomContainer>();
        public IAtomType CreateAtomType(IElement element) => new AtomType(element);
        public IAtomType CreateAtomType(string elementSymbol) => new AtomType(elementSymbol);
        public IAtomType CreateAtomType(string identifier, string elementSymbol) => new AtomType(identifier, elementSymbol);
        public IBioPolymer CreateBioPolymer() => new BioPolymer();
        public IBond CreateBond() => new Bond();
        public IBond CreateBond(IAtom atom1, IAtom atom2) => new Bond(atom1, atom2);
        public IBond CreateBond(IAtom atom1, IAtom atom2, BondOrder order) => new Bond(atom1, atom2, order);
        public IBond CreateBond(IAtom atom1, IAtom atom2, BondOrder order, BondStereo stereo) => new Bond(atom1, atom2, order, stereo);
        public IBond CreateBond(IEnumerable<IAtom> atoms) => new Bond(atoms);
        public IBond CreateBond(IEnumerable<IAtom> atoms, BondOrder order) => new Bond(atoms, order);
        public IBond CreateBond(IEnumerable<IAtom> atoms, BondOrder order, BondStereo stereo) => new Bond(atoms, order, stereo);
        public IChemFile CreateChemFile() => new ChemFile();
        public IChemModel CreateChemModel() => new ChemModel();
        public IChemObject CreateChemObject() => new ChemObject();
        public IChemObject CreateChemObject(IChemObject chemObject) => new ChemObject(chemObject);
        public IChemSequence CreateChemSequence() => new ChemSequence();
        public ICrystal CreateCrystal() => new Crystal();
        public ICrystal CreateCrystal(IAtomContainer container) => new Crystal(container);
        public ILonePair CreateLonePair() => new LonePair();
        public ILonePair CreateLonePair(IAtom atom) => new LonePair(atom);
        public IElectronContainer CreateElectronContainer() => new ElectronContainer();
        public IElement CreateElement() => new Element();
        public IElement CreateElement(IElement element) => new Element(element);
        public IElement CreateElement(string symbol) => new Element(symbol);
        public IElement CreateElement(string symbol, int? atomicNumber) => new Element(symbol, atomicNumber);
        public IFragmentAtom CreateFragmentAtom() => new FragmentAtom();
        public IIsotope CreateIsotope(string elementSymbol) => new Isotope(elementSymbol);
        public IIsotope CreateIsotope(int atomicNumber, string elementSymbol, int massNumber, double exactMass, double abundance) => new Isotope(atomicNumber, elementSymbol, massNumber, exactMass, abundance);
        public IIsotope CreateIsotope(int atomicNumber, string elementSymbol, double exactMass, double abundance) => new Isotope(atomicNumber, elementSymbol, exactMass, abundance);
        public IIsotope CreateIsotope(string elementSymbol, int massNumber) => new Isotope(elementSymbol, massNumber);
        public IIsotope CreateIsotope(IElement element) => new Isotope(element);
        public IMapping CreateMapping(IChemObject objectOne, IChemObject objectTwo) => new Mapping(objectOne, objectTwo);
        public IMonomer CreateMonomer() => new Monomer();
        public IPDBAtom CreatePDBAtom(IElement element) => new PDBAtom(element);
        public IPDBAtom CreatePDBAtom(string symbol) => new PDBAtom(symbol);
        public IPDBAtom CreatePDBAtom(string symbol, Vector3 coordinate) => new PDBAtom(symbol, coordinate);
        public IPDBMonomer CreatePDBMonomer() => new PDBMonomer();
        public IPDBPolymer CreatePDBPolymer() => new PDBPolymer();
        public IPDBStructure CreatePDBStructure() => new PDBStructure();
        public IPolymer CreatePolymer() => new Polymer();
        public IPseudoAtom CreatePseudoAtom() => new PseudoAtom();
        public IPseudoAtom CreatePseudoAtom(string label) => new PseudoAtom(label);
        public IPseudoAtom CreatePseudoAtom(IElement element) => new PseudoAtom(element);
        public IPseudoAtom CreatePseudoAtom(string label, Vector2 point2d) => new PseudoAtom(label, point2d);
        public IPseudoAtom CreatePseudoAtom(string label, Vector3 point3d) => new PseudoAtom(label, point3d);
        public IReaction CreateReaction() => new Reaction();
        public IReactionSet CreateReactionSet() => new ReactionSet();
        public IReactionScheme CreateReactionScheme() => new ReactionScheme();
        public IRing CreateRing() => new Ring();
        public IRing CreateRing(int ringSize, string elementSymbol) => new Ring(ringSize, elementSymbol);
        public IRing CreateRing(IAtomContainer atomContainer) => new Ring(atomContainer);
        public IRing CreateRing(IEnumerable<IAtom> atoms, IEnumerable<IBond> bonds) => new Ring(atoms, bonds); 
        public IRingSet CreateRingSet() => new RingSet();
        public ISubstance CreateSubstance() => new Substance();
        public ISingleElectron CreateSingleElectron() => new SingleElectron();
        public ISingleElectron CreateSingleElectron(IAtom atom) => new SingleElectron(atom);
        public IStrand CreateStrand() => new Strand();
        public ITetrahedralChirality CreateTetrahedralChirality(IAtom chiralAtom, IEnumerable<IAtom> ligandAtoms, TetrahedralStereo chirality)
        {
            var o = new TetrahedralChirality(chiralAtom, ligandAtoms, chirality);
            o.Builder = this;
            return o;
        }

        public IAdductFormula CreateAdductFormula() => new AdductFormula();
        public IAdductFormula CreateAdductFormula(IMolecularFormula formula) => new AdductFormula(formula);
        public IMolecularFormula CreateMolecularFormula() => new MolecularFormula();
        public IMolecularFormulaSet CreateMolecularFormulaSet() => new MolecularFormulaSet();
        public IMolecularFormulaSet CreateMolecularFormulaSet(IMolecularFormula formula) => new MolecularFormulaSet(formula);
    }
}
namespace NCDK.Silent
{
    public sealed class ChemObjectBuilder
        : IChemObjectBuilder
    {
        public static readonly IChemObjectBuilder Instance = new ChemObjectBuilder();

        public T Create<T>() where T : IAtomContainer, new() => new T();

        public IAminoAcid CreateAminoAcid() => new AminoAcid();
        public IAtom CreateAtom() => new Atom();
        public IAtom CreateAtom(IElement element) => new Atom(element);
        public IAtom CreateAtom(string elementSymbol) => new Atom(elementSymbol);
        public IAtom CreateAtom(string elementSymbol, Vector2 point2d) => new Atom(elementSymbol, point2d);
        public IAtom CreateAtom(string elementSymbol, Vector3 point3d) => new Atom(elementSymbol, point3d);
        public IAtomContainer CreateAtomContainer() => new AtomContainer();
        public IAtomContainer CreateAtomContainer(IAtomContainer container) => new AtomContainer(container);
        public IAtomContainer CreateAtomContainer(IEnumerable<IAtom> atoms, IEnumerable<IBond> bonds) => new AtomContainer(atoms, bonds);
        public IAtomContainerSet<T> CreateAtomContainerSet<T>() where T : IAtomContainer => new AtomContainerSet<T>();
        public IAtomContainerSet<IAtomContainer> CreateAtomContainerSet() => new AtomContainerSet<IAtomContainer>();
        public IAtomType CreateAtomType(IElement element) => new AtomType(element);
        public IAtomType CreateAtomType(string elementSymbol) => new AtomType(elementSymbol);
        public IAtomType CreateAtomType(string identifier, string elementSymbol) => new AtomType(identifier, elementSymbol);
        public IBioPolymer CreateBioPolymer() => new BioPolymer();
        public IBond CreateBond() => new Bond();
        public IBond CreateBond(IAtom atom1, IAtom atom2) => new Bond(atom1, atom2);
        public IBond CreateBond(IAtom atom1, IAtom atom2, BondOrder order) => new Bond(atom1, atom2, order);
        public IBond CreateBond(IAtom atom1, IAtom atom2, BondOrder order, BondStereo stereo) => new Bond(atom1, atom2, order, stereo);
        public IBond CreateBond(IEnumerable<IAtom> atoms) => new Bond(atoms);
        public IBond CreateBond(IEnumerable<IAtom> atoms, BondOrder order) => new Bond(atoms, order);
        public IBond CreateBond(IEnumerable<IAtom> atoms, BondOrder order, BondStereo stereo) => new Bond(atoms, order, stereo);
        public IChemFile CreateChemFile() => new ChemFile();
        public IChemModel CreateChemModel() => new ChemModel();
        public IChemObject CreateChemObject() => new ChemObject();
        public IChemObject CreateChemObject(IChemObject chemObject) => new ChemObject(chemObject);
        public IChemSequence CreateChemSequence() => new ChemSequence();
        public ICrystal CreateCrystal() => new Crystal();
        public ICrystal CreateCrystal(IAtomContainer container) => new Crystal(container);
        public ILonePair CreateLonePair() => new LonePair();
        public ILonePair CreateLonePair(IAtom atom) => new LonePair(atom);
        public IElectronContainer CreateElectronContainer() => new ElectronContainer();
        public IElement CreateElement() => new Element();
        public IElement CreateElement(IElement element) => new Element(element);
        public IElement CreateElement(string symbol) => new Element(symbol);
        public IElement CreateElement(string symbol, int? atomicNumber) => new Element(symbol, atomicNumber);
        public IFragmentAtom CreateFragmentAtom() => new FragmentAtom();
        public IIsotope CreateIsotope(string elementSymbol) => new Isotope(elementSymbol);
        public IIsotope CreateIsotope(int atomicNumber, string elementSymbol, int massNumber, double exactMass, double abundance) => new Isotope(atomicNumber, elementSymbol, massNumber, exactMass, abundance);
        public IIsotope CreateIsotope(int atomicNumber, string elementSymbol, double exactMass, double abundance) => new Isotope(atomicNumber, elementSymbol, exactMass, abundance);
        public IIsotope CreateIsotope(string elementSymbol, int massNumber) => new Isotope(elementSymbol, massNumber);
        public IIsotope CreateIsotope(IElement element) => new Isotope(element);
        public IMapping CreateMapping(IChemObject objectOne, IChemObject objectTwo) => new Mapping(objectOne, objectTwo);
        public IMonomer CreateMonomer() => new Monomer();
        public IPDBAtom CreatePDBAtom(IElement element) => new PDBAtom(element);
        public IPDBAtom CreatePDBAtom(string symbol) => new PDBAtom(symbol);
        public IPDBAtom CreatePDBAtom(string symbol, Vector3 coordinate) => new PDBAtom(symbol, coordinate);
        public IPDBMonomer CreatePDBMonomer() => new PDBMonomer();
        public IPDBPolymer CreatePDBPolymer() => new PDBPolymer();
        public IPDBStructure CreatePDBStructure() => new PDBStructure();
        public IPolymer CreatePolymer() => new Polymer();
        public IPseudoAtom CreatePseudoAtom() => new PseudoAtom();
        public IPseudoAtom CreatePseudoAtom(string label) => new PseudoAtom(label);
        public IPseudoAtom CreatePseudoAtom(IElement element) => new PseudoAtom(element);
        public IPseudoAtom CreatePseudoAtom(string label, Vector2 point2d) => new PseudoAtom(label, point2d);
        public IPseudoAtom CreatePseudoAtom(string label, Vector3 point3d) => new PseudoAtom(label, point3d);
        public IReaction CreateReaction() => new Reaction();
        public IReactionSet CreateReactionSet() => new ReactionSet();
        public IReactionScheme CreateReactionScheme() => new ReactionScheme();
        public IRing CreateRing() => new Ring();
        public IRing CreateRing(int ringSize, string elementSymbol) => new Ring(ringSize, elementSymbol);
        public IRing CreateRing(IAtomContainer atomContainer) => new Ring(atomContainer);
        public IRing CreateRing(IEnumerable<IAtom> atoms, IEnumerable<IBond> bonds) => new Ring(atoms, bonds); 
        public IRingSet CreateRingSet() => new RingSet();
        public ISubstance CreateSubstance() => new Substance();
        public ISingleElectron CreateSingleElectron() => new SingleElectron();
        public ISingleElectron CreateSingleElectron(IAtom atom) => new SingleElectron(atom);
        public IStrand CreateStrand() => new Strand();
        public ITetrahedralChirality CreateTetrahedralChirality(IAtom chiralAtom, IEnumerable<IAtom> ligandAtoms, TetrahedralStereo chirality)
        {
            var o = new TetrahedralChirality(chiralAtom, ligandAtoms, chirality);
            o.Builder = this;
            return o;
        }

        public IAdductFormula CreateAdductFormula() => new AdductFormula();
        public IAdductFormula CreateAdductFormula(IMolecularFormula formula) => new AdductFormula(formula);
        public IMolecularFormula CreateMolecularFormula() => new MolecularFormula();
        public IMolecularFormulaSet CreateMolecularFormulaSet() => new MolecularFormulaSet();
        public IMolecularFormulaSet CreateMolecularFormulaSet(IMolecularFormula formula) => new MolecularFormulaSet(formula);
    }
}
