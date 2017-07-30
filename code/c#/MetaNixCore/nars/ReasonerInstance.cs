using MetaNix.nars.memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MetaNix.nars {
    // input for prototyping purposes, can only build certain (compound)term structures
    public class PrototypingInput {
        public PrototypingInput(CompoundAndTermContext compoundAndTermContext) {
            this.compoundAndTermContext = compoundAndTermContext;
        }

        public CompoundIndex makeInheritance(string a, string b) {
            int termOrCompoundTermOrVariableRefererIdOfA, termOrCompoundTermOrVariableRefererIdOfB;

            TermTupleIndex termTupleIndexOfA = compoundAndTermContext.returnIndexOfTermTupleForHumanReadableTermNameOrCreate(a, out termOrCompoundTermOrVariableRefererIdOfA);
            TermTupleIndex termTupleIndexOfB = compoundAndTermContext.returnIndexOfTermTupleForHumanReadableTermNameOrCreate(b, out termOrCompoundTermOrVariableRefererIdOfB);

            var compoundReferers = new TermOrCompoundTermOrVariableReferer[] {
                compoundAndTermContext.accessTermTupleByIndex(termTupleIndexOfA).refererOrIntervals[0].referer,
                compoundAndTermContext.accessTermTupleByIndex(termTupleIndexOfB).refererOrIntervals[0].referer,
            };

            TermTupleIndex termTupleIndex;

            if ( compoundAndTermContext.existTermTuple(compoundReferers) ) {
                termTupleIndex = compoundAndTermContext.getTermTupleIndexByReferers(compoundReferers);
            }
            else {
                termTupleIndex = compoundAndTermContext.addTermTupleByReferers(compoundReferers);
            }
            
            // create compound
            Compound.MakeParameters makeParameters = new Compound.MakeParameters();
            makeParameters.flagsOfCopula = FlagsOfCopula.makeInheritance();
            makeParameters.termComplexity = 3;
            makeParameters.termTupleIndex = termTupleIndex;
            makeParameters.thisTermReferer = TermOrCompoundTermOrVariableReferer.makeNonatomic(compoundAndTermContext.TermOrCompoundTermOrVariableRefererCounter);
            compoundAndTermContext.TermOrCompoundTermOrVariableRefererCounter++;


            Compound compound = compoundAndTermContext.createCompound(makeParameters);
            CompoundIndex compoundIndex = compoundAndTermContext.addCompound(compound);


            return compoundIndex;
        }

        CompoundAndTermContext compoundAndTermContext;
    }

    public class CompoundAndTermContext {
        internal bool existHumanReadableTermName(string termName, out TermTupleIndex index) {
            if(termNamesByhumanReadableName.ContainsKey(termName) ) {
                index = termNamesByhumanReadableName[termName];
                return true;
            }

            index = TermTupleIndex.makeInvalid();
            return false;
        }

        // returns the index where the next compound will be created
        internal ulong getCompoundCreateIndex() {
            return (ulong)compounds.Count;
        }

        // all fields of parameters have to be initialized except compoundId
        internal Compound createCompound(Compound.MakeParameters parameters) {
            parameters.compoundId = compoundIdCounter;
            Compound resultCompound = Compound.make(parameters);

            compoundIdCounter++;



            return resultCompound;
        }

        internal Compound translateToCompoundChecked(TermOrCompoundTermOrVariableReferer taskTerm) {
            Ensure.ensure(!taskTerm.isAtomic);

            Compound compound;
            try {
                compound = termRefererIdToCompound[taskTerm.getTerm];
            }
            catch(KeyNotFoundException) {
                // should never happen

                throw new Exception("Key not found");
            }

            return compound;
        }

        internal TermTupleIndex returnIndexOfTermTupleForHumanReadableTermNameOrCreate(string name, out int termOrCompoundTermOrVariableRefererId) {
            bool nameIsPresentInHumanReadableNames = termNamesByhumanReadableName.ContainsKey(name);
            if (nameIsPresentInHumanReadableNames) {
                termOrCompoundTermOrVariableRefererId = termTuples[(int)termNamesByhumanReadableName[name].index].refererOrIntervals[0].referer.getAtomic;
                return termNamesByhumanReadableName[name];
            }
            // else here

            termOrCompoundTermOrVariableRefererId = TermOrCompoundTermOrVariableRefererCounter;
            TermOrCompoundTermOrVariableRefererCounter++;

            TermTuple termTuple = TermTuple.makeByReferers(new TermOrCompoundTermOrVariableReferer[] { TermOrCompoundTermOrVariableReferer.makeAtomic(termOrCompoundTermOrVariableRefererId) });

            TermTupleIndex index = TermTupleIndex.make((ulong)termTuples.Count);
            termTuples.Add(termTuple);

            termNamesByhumanReadableName[name] = index;
            termOrCompoundTermOrVariableRefererIdToHumanReadableName[termOrCompoundTermOrVariableRefererId] = name;

            return index;
        }



        static ulong calcHashOfTermOrCompoundTermOrVariableReferers(IEnumerable<TermOrCompoundTermOrVariableReferer> compounds) {
            void rotate(ref ulong hash2, uint bits)
            {
                ulong oldHash = hash2;
                hash2 = (oldHash >> (int)bits) | (oldHash << (int)(64 - bits));
            }

            ulong calcHash()
            {
                ulong hash2 = 0;

                foreach (var iterationCompound in compounds) {
                    rotate(ref hash2, 13);
                    hash2 ^= (ulong)iterationCompound.rawEncoding;
                }

                return hash2;
            }

            ulong hash = calcHash();
            return hash;
        }


        internal bool existTermTuple(IList<TermOrCompoundTermOrVariableReferer> referers) {
            ulong hash = calcHashOfTermOrCompoundTermOrVariableReferers(referers);
            if (!termTupleIndicesByTermTupleHash.ContainsKey(hash)) {
                return false;
            }

            // compare
            IList<TermTupleIndex> termTupleIndices = termTupleIndicesByTermTupleHash[hash];
            foreach (TermTupleIndex iterationTermTupleIndex in termTupleIndices) {
                Debug.Assert(termTuples[(int)iterationTermTupleIndex.index].refererOrIntervals.All(a => a.isReferer));
                if (termTuples[(int)iterationTermTupleIndex.index].refererOrIntervals.Select(a => a.referer) == referers) { // TODO< maybe we need an helper which compares element by element >
                    return true;
                }
            }

            return false;
        }

        internal TermTupleIndex getTermTupleIndexByReferers(TermOrCompoundTermOrVariableReferer[] referers) {
            ulong hash = calcHashOfTermOrCompoundTermOrVariableReferers(referers);
            if (!termTupleIndicesByTermTupleHash.ContainsKey(hash)) {
                throw new Exception("Compound(Term) was not found by hash!");// indicates an internal error
            }

            // compare
            IList<TermTupleIndex> termTupleIndices = termTupleIndicesByTermTupleHash[hash];
            foreach (TermTupleIndex iterationTermTupleIndex in termTupleIndices) {
                Debug.Assert(termTuples[(int)iterationTermTupleIndex.index].refererOrIntervals.All(a => a.isReferer));
                if (termTuples[(int)iterationTermTupleIndex.index].refererOrIntervals.Select(a => a.referer) == referers) { // TODO< maybe we need an helper which compares element by element >
                    return iterationTermTupleIndex;
                }
            }

            throw new Exception("compound wasn't indexed"); // indicates an internal error, existTermTuple() should return true before doing this query
        }


        internal TermTupleIndex addTermTupleByReferers(TermOrCompoundTermOrVariableReferer[] referers) {
            ulong hash = calcHashOfTermOrCompoundTermOrVariableReferers(referers);

            TermTupleIndex insertionIndex = TermTupleIndex.make((ulong)termTuples.Count);
            termTuples.Add(TermTuple.makeByReferers(referers));
            if (!termTupleIndicesByTermTupleHash.ContainsKey(hash)) {
                termTupleIndicesByTermTupleHash[hash] = new List<TermTupleIndex>() { insertionIndex };
                return insertionIndex;
            }

            termTupleIndicesByTermTupleHash[hash].Add(insertionIndex);

            return insertionIndex;
        }


        internal CompoundIndex translateCompoundIdToCompoundIndex(/*Compound.CompoundIdType*/ulong compoundId) {
            Debug.Assert(compoundIdToCompoundIndex.ContainsKey(compoundId));
            return compoundIdToCompoundIndex[compoundId];
        }


        internal CompoundIndex addCompound(Compound compound) {
            CompoundIndex compoundIndex = CompoundIndex.make((uint)compounds.Count);
            compounds.Add(compound);

            compoundIdToCompoundIndex[compound.compoundId] = compoundIndex;

            termRefererIdToCompound[compound.thisTermReferer.getMaskedOutId] = compound;



            // add it to the hashtables
            compound.updateHashes();
            compoundHashtableByWithId.insert(compoundIndex);
            compoundHashtableByWithoutId.insert(compoundIndex);

            return compoundIndex;
        }

        public CompoundAndTermContext() {
            compoundHashtableByWithId = new CompoundHashtable(this, true);
            compoundHashtableByWithoutId = new CompoundHashtable(this, false);
        }

        internal TermTuple accessTermTupleByIndex(TermTupleIndex value) {
            return termTuples[(int)value.index];
        }

        public Compound accessCompoundByIndex(CompoundIndex value) {
            return compounds[(int)value.index];
        }

        static uint getTermComplexityOfCopula(FlagsOfCopula flagsOfCopula) {
            return 1;
        }

        internal uint getTermComplexityOfAndByTermReferer(TermOrCompoundTermOrVariableReferer termReferer) {
            if (termReferer.isVariable) {
                return 1;
            }
            else if (termReferer.isAtomic) {
                return 1;
            }
            else if (!termReferer.isSpecial) {
                // if the referer is not a compound 
                return accessCompoundByIndex(translateCompoundIdToCompoundIndex((ulong)termReferer.getAtomicOrTerm)).termComplexity;
            }
            else {
                throw new Exception("Term referer is not a variable or not special, not handled, is an internal error");
            }
        }

        internal string getDebugStringByTermReferer(TermOrCompoundTermOrVariableReferer termReferer) {
            if (termReferer.isVariable) {
                return "var"; // TODO< a real debug string >
            }
            else if (termReferer.isAtomic) {
                return termOrCompoundTermOrVariableRefererIdToHumanReadableName[termReferer.getAtomicOrTerm];
            }
            else if (!termReferer.isSpecial) {
                // if the referer is not a compound 
                return accessCompoundByIndex(translateCompoundIdToCompoundIndex((ulong)termReferer.getAtomicOrTerm)).getDebugStringRecursive(this);
            }
            else {
                throw new Exception("Term referer is not a variable or not special, not handled, is an internal error");
            }
        }



        // returns the index of the term
        internal TermTupleIndex translateTermOfCompoundToIndex(TermOrCompoundTermOrVariableReferer term) {
            return translateTermOfCompoundToCompound(term).termTupleIndex;
        }

        internal Compound translateTermOfCompoundToCompound(TermOrCompoundTermOrVariableReferer term) {
            Debug.Assert(termRefererIdToCompound[term.getMaskedOutId].thisTermReferer.getTerm == term.getTerm); // must be equal else we have an internal consistency problem
            return termRefererIdToCompound[term.getMaskedOutId];
        }

        
        
        
        private ulong[] getPotentialIndicesOfCompoundsByHashWithoutCompoundId(/*Compound.HashWithoutCompoundIdType*/ulong hash) {
            return compoundHashtableByWithoutId.getPotentialIndicesOfCompoundsByHash(hash);
        }

        // checks if the compound exists and returns the index of the compound if it is the case
        // the compoundId is not used for hash lookup/comparision
        internal bool existsCompoundWithoutCompoundId(Compound compoundToCompareWithoutCompoundId, out CompoundIndex foundCompoundIndex) {
            foundCompoundIndex = CompoundIndex.makeInvalid();

            if (!compoundHashtableByWithoutId.existHash(compoundToCompareWithoutCompoundId.cachedHashWithoutCompoundId)) {
                return false;
            }

            ulong[] potentialCompoundIndices = getPotentialIndicesOfCompoundsByHashWithoutCompoundId(compoundToCompareWithoutCompoundId.cachedHashWithoutCompoundId);

            // compare and search the compound which matches the queried one
            foreach (var iCompoundIndex in potentialCompoundIndices) {
                Compound iCompound = compounds[(int)iCompoundIndex];
                if (Compound.isEqualWithoutCompoundIdAndTermReferer(compoundToCompareWithoutCompoundId, iCompound)) {
                    foundCompoundIndex = CompoundIndex.make((/*quick and dirty conversation*/(uint)iCompoundIndex));
                    return true;
                }
            }

            return false;
        }


        // points at the indexes of the termTuple
        private IDictionary<string, TermTupleIndex> termNamesByhumanReadableName = new Dictionary<string, TermTupleIndex>();
        private IDictionary<int, string> termOrCompoundTermOrVariableRefererIdToHumanReadableName = new Dictionary<int, string>();

        // compounds describe concepts connected with copula and the references/id's of the children compounds are stored in termTuples
        private IList<Compound> compounds = new List<Compound>();

        private IList<TermTuple> termTuples = new List<TermTuple>();

        private IDictionary<int, Compound> termRefererIdToCompound = new Dictionary<int, Compound>();

        private IDictionary</*Compound.CompoundIdType*/ulong, CompoundIndex> compoundIdToCompoundIndex = new Dictionary</*Compound.CompoundIdType*/ulong, CompoundIndex>();


        private IDictionary<ulong, IList<TermTupleIndex>> termTupleIndicesByTermTupleHash = new Dictionary<ulong, IList<TermTupleIndex>>();

        private uint compoundIdCounter = 200000;

        internal int TermOrCompoundTermOrVariableRefererCounter = 300000;

        private CompoundHashtable compoundHashtableByWithId;
        private CompoundHashtable compoundHashtableByWithoutId;
    }
    
}
