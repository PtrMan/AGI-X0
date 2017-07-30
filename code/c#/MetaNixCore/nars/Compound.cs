using System;
using System.Diagnostics;
using System.Linq;

// uncommented because C# doesn't support these aliases
//using CompoundIdType = ulong;

namespace MetaNix.nars {
    // translation of https://github.com/PtrMan/fastMetaNARS/blob/master/source/d/fastMetaNars/Term.d
    
    public class Compound {
        public const uint COMPLEXITYINDEPENDENTVARIABLE = 1;
        public const uint COMPLEXITYDEPENDENTVARIABLE = 1;

        public FlagsOfCopula flagsOfCopula;

        public ulong compoundId; // unique id of the compound, is not GC'ed, used for hash calc


        public TermOrCompoundTermOrVariableReferer thisTermReferer; // term referer describing this compound
                                                                    // contains the id which is compound-gc'ed

        // uncommented because outdated by thisTermReferer
        //size_t compoundIndex; // compound-gc'ed index

        
        public TermTupleIndex termTupleIndex;

        public /*HashWithCompoundIdType*/uint cachedHashWithCompoundId;
        //version(DEBUG) bool cachedHashWithCompoundIdValid;

        public /*HashWithoutCompoundIdType*/ uint cachedHashWithoutCompoundId;
        //version(DEBUG) bool cachedHashWithoutCompoundIdValid;


        public void updateHashes() {
            updateHash(false);
            updateHash(true);
        }
    

        public void updateHash(bool withCompoundId) {
            void rotate(ref uint hashLocal, uint bits)
            {
                uint oldHash = hashLocal;
                hashLocal = (oldHash >> (int)bits) | (oldHash << (int)(32 - bits));
            }

            uint hash;
            hash = 0;
            hash ^= (uint)termTupleIndex.index;
            hash ^= (uint)(termTupleIndex.index >> 32);
            rotate(ref hash, 13);
            if (withCompoundId) {
                hash ^= ((uint)compoundId&0xffffffff);
                hash ^= ((uint)(compoundId >> 32) );
                rotate(ref hash, 13);
            }
            hash ^= flagsOfCopula.asNumberEncoding; rotate(ref hash, 13);

            if (withCompoundId) {
                cachedHashWithCompoundId = hash;
                // uncommented because from D, C# does this differently, TODO< rewrite >
                //version(DEBUG) {
                //    cachedHashWithCompoundIdValid = true;
                //}
            }
            else {
                cachedHashWithoutCompoundId = hash;
                // uncommented because from D, C# does this differently, TODO< rewrite >
                //version(DEBUG) {
                //    cachedHashWithoutCompoundIdValid = true;
                //}
            }
        }

        // TODO< overhaul so it returns RefererOrInterval >
        public TermOrCompoundTermOrVariableReferer left(CompoundAndTermContext compoundAndTermContext) {
            TermTuple dereferencedCompoundTuple = compoundAndTermContext.accessTermTupleByIndex(termTupleIndex);

            Debug.Assert(dereferencedCompoundTuple.refererOrIntervals.Length == 2, "only valid for binary compounds");
            Debug.Assert(dereferencedCompoundTuple.refererOrIntervals[0].isReferer);
            return dereferencedCompoundTuple.refererOrIntervals[0].referer;
        }
    
        // TODO< overhaul so it returns RefererOrInterval >
        public TermOrCompoundTermOrVariableReferer right(CompoundAndTermContext compoundAndTermContext) {
            TermTuple dereferencedCompoundTuple = compoundAndTermContext.accessTermTupleByIndex(termTupleIndex);

            Debug.Assert(dereferencedCompoundTuple.refererOrIntervals.Length == 2, "only valid for binary compounds");
            Debug.Assert(dereferencedCompoundTuple.refererOrIntervals[1].isReferer);
            return dereferencedCompoundTuple.refererOrIntervals[1].referer;
        }
    
        public RefererOrInterval getComponentByIndex(CompoundAndTermContext compoundAndTermContext, uint index) {
            TermTuple dereferencedCompoundTuple = compoundAndTermContext.accessTermTupleByIndex(termTupleIndex);
            
            return dereferencedCompoundTuple.refererOrIntervals[index];
        }

        public uint getComponentLength(CompoundAndTermContext compoundAndTermContext) {
            TermTuple dereferencedCompoundTuple = compoundAndTermContext.accessTermTupleByIndex(termTupleIndex);
            return (uint)dereferencedCompoundTuple.refererOrIntervals.Length;
        }

        
        public string getDebugStringRecursive(CompoundAndTermContext compoundAndTermContext) {
            if (thisTermReferer.isIndependentVariable) {
                return String.Format("${0}", thisTermReferer.getIndependentVariable);
            }
            else if (thisTermReferer.isDependentVariable) {
                return String.Format("#{0}", thisTermReferer.getDependentVariable);
            }
            else if (!thisTermReferer.isSpecial) {
                if (thisTermReferer.isAtomic) {
                    // TODO< return name if there is a human readable name >

                    return String.Format("c:{0}", thisTermReferer.getAtomic);

                    //return String.Format("c:{0}:{1}", thisTermReferer.getAtomic, compoundAndTermContext.termNamesByhumanReadableName[thisTermReferer.getAtomic]);
                }
                else {
                    string
                        debugStringForLeft = compoundAndTermContext.getDebugStringByTermReferer(left(compoundAndTermContext)),
                        debugStringForRight = compoundAndTermContext.getDebugStringByTermReferer(right(compoundAndTermContext));

                    // TODO< check if the term is prefix or nonprefix >
                    // TODO< implement for nonbinary >
                    return String.Format("<{0} {1} {2}>", debugStringForLeft, flagsOfCopula.convToHumanString(), debugStringForRight);
                }
            }
            else {
                throw new Exception("Term referer is not a variable or not special, not handled, is an internal error");
            }
        }

        public uint termComplexity { get {
            return protectedTermComplexity;
        }}

        protected uint protectedTermComplexity;

        public class MakeParameters {
            public uint termComplexity;
            public FlagsOfCopula flagsOfCopula;
            public ulong compoundId;
            public TermOrCompoundTermOrVariableReferer thisTermReferer;
            public TermTupleIndex termTupleIndex;
        }
    
        public static Compound make(MakeParameters parameters) {
            Compound result = new Compound();
            result.protectedTermComplexity = parameters.termComplexity;
            result.flagsOfCopula = parameters.flagsOfCopula;
            result.compoundId = parameters.compoundId;
            result.thisTermReferer = parameters.thisTermReferer;
            result.termTupleIndex = parameters.termTupleIndex;
            result.updateHashes();
            return result;
        }
    
        public static bool isEqualWithoutCompoundIdAndTermReferer(Compound a, Compound b) {
            return a.flagsOfCopula == b.flagsOfCopula && a.termTupleIndex == b.termTupleIndex && a.termComplexity == b.termComplexity;
        }
    }

    public class TermTuple {
        public RefererOrInterval[] refererOrIntervals;

        public static TermTuple makeByReferers(TermOrCompoundTermOrVariableReferer[] referers) {
            TermTuple result = new TermTuple();
            result.refererOrIntervals = referers.Select(v => RefererOrInterval.makeReferer(v)).ToArray();
            return result;
        }
    }

    // we need this indirection because a sequence can contain referers(Term or compoundterm or variable) or a interval
    public struct RefererOrInterval {
        public bool isInterval;

        public bool isReferer { get {
                return !isInterval;
        } }

        public TermOrCompoundTermOrVariableReferer referer;
        public Interval interval;

        public static RefererOrInterval makeInterval(Interval interval) {
            RefererOrInterval result = new RefererOrInterval();
            result.isInterval = true;
            result.interval = interval;
            return result;
        }

        public static RefererOrInterval makeReferer(TermOrCompoundTermOrVariableReferer referer) {
            RefererOrInterval result = new RefererOrInterval();
            result.isInterval = false;
            result.referer = referer;
            return result;
        }
    }

}
