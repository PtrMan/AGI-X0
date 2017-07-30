using MetaNix.nars.config;
using MetaNix.nars.inference;
using MetaNix.nars.memory;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaNix.nars.entity {
    // see https://github.com/opennars/opennars/blob/master/nars_core/nars/entity/Stamp.java
    public class Stamp {
        public bool alreadyAnticipatedNegConfirmation = false; /* True when its a neg confirmation task that was already checked:*/


        /*default for atemporal events means "always" in Judgment/Question, but "current" in Goal/Quest*/
        public const long ETERNAL = long.MinValue;

        public bool isEternal {
            get {
                bool eternalOccurrence = occurrenceTime == ETERNAL;

                /*
                if (Parameters.DEBUG) {
                    if (eternalOccurrence && tense != Tense.Eternal) {
                        throw new RuntimeException("Stamp has inconsistent tense and eternal ocurrenceTime: tense=" + tense);
                    }
                }
                */

                return eternalOccurrence;
            }

            set {
                occurrenceTime = ETERNAL;
            }
        }

        public bool before(Stamp s, uint duration) {
            if (isEternal || s.isEternal) {
                return false;
            }
            return TemporalRules.order(s.occurrenceTime, occurrenceTime, duration) == TemporalRules.EnumOrder.BACKWARD;
        }

        public bool after(Stamp s, uint duration) {
            if (isEternal || s.isEternal) {
                return false;
            }
            return TemporalRules.order(s.occurrenceTime, occurrenceTime, duration) == TemporalRules.EnumOrder.FORWARD;
        }

        // force construction with class methods
        private Stamp() {}

        // force construction with make static methods
        /** used for when the ocrrence time will be set later; so should not be called from externally but through another Stamp constructor */
        private Stamp(EnumTense tense, long serial) {
            dualStamp = new DualStamp(Parameters.STAMP_NUMBEROFELEMENTS, Parameters.STAMP_BLOOMFILTERNUMBEROFBITS);
            dualStamp.insertAtFront(new List<uint> {
                // HACK< stamps must store long !!! >
                (uint)serial
            });
            
            this.tense = tense;
        }

        public static Stamp makeByTense(Memory memory, EnumTense tense) {
            return makeByTimeTenseSerialAndDuration(memory.time, tense, memory.newStampSerial(), memory.param.duration.value);
        }

        public static Stamp makeByTimeTenseSerialAndDuration(
            long time,
            EnumTense tense,
            long serial,
            uint duration
        ) {

            Stamp created = new Stamp(tense, serial);
            created.setCreationTime(time, duration);
            return created;
        }

        /** creates a stamp with default Present tense */
        public static Stamp makeWithPresentTense(Memory memory) {
            return makeByTense(memory, EnumTense.PRESENT);
        }

        /** sets the creation time; used to set input tasks with the actual time they enter Memory */
        public void setCreationTime(long time, uint duration) {
            creationTime = time;

            if (tense == null) {
                occurrenceTime = ETERNAL;
            }
            else if (tense == EnumTense.PAST) {
                occurrenceTime = time - duration;
            }
            else if (tense == EnumTense.FUTURE) {
                occurrenceTime = time + duration;
            }
            else if (tense == EnumTense.PRESENT) {
                occurrenceTime = time;
            }
            else {
                occurrenceTime = time;
            }
        }



        public enum EnumCompareCreationTime {
            YES,
            NO,
        }

        public enum EnumCompareOccurrenceTime {
            YES,
            NO,
        }

        // TODO LOW OPTIMIZATION< use bit based hashing scheme to accelerate this and avoid allocating nonsense >
        // see https://github.com/opennars/opennars/blob/4515f1d8e191a1f097859decc65153287d5979c5/nars_core/nars/entity/Stamp.java#L183
        public static bool checkBaseOverlap(Stamp a, Stamp b) {
            HashSet<uint> task_base = new HashSet<uint>();

            for (int i = 0; i < a.dualStamp.used; i++) {
                if (task_base.Contains(a.dualStamp.accessTermIdHistory(i))) { // can have an overlap in itself already
                    return true;
                }
                task_base.Add(a.dualStamp.accessTermIdHistory(i));
            }
            // too restrictive, its checked for non-deductive inference rules in derivedTask
            for (int i = 0; i < b.dualStamp.used; i++) {
                if (task_base.Contains(b.dualStamp.accessTermIdHistory(i))) { // can have an overlap in itself already
                    return true;
                }
                task_base.Add(b.dualStamp.accessTermIdHistory(i));
            }
            return false;
        }

        public enum EnumCompareEvidentialBaseTime {
            YES,
            NO,
        }
        
        /**
         * Check if two stamps contains the same types of content
         *
         * \param a The Stamp to be compared
         * \param b The Stamp to be compared
         * \return Whether the two have contain the same evidential base
         */
        public static bool checkEquals(
            Stamp a,
            Stamp b,
            EnumCompareCreationTime compareCreatingTime,
            EnumCompareOccurrenceTime compareOccurrenceTime,
            EnumCompareEvidentialBaseTime compareEvidentialBase
        ) {
            if (a == b) {
                return true;
            }

            if (compareCreatingTime == EnumCompareCreationTime.YES) {
                if (a.creationTime != b.creationTime) return false;
            }
            
            if (compareOccurrenceTime == EnumCompareOccurrenceTime.YES) {
                if (a.occurrenceTime != b.occurrenceTime) return false;
            }

            if (compareEvidentialBase == EnumCompareEvidentialBaseTime.YES) {
                // TODO if (a.evidentialHash != b.evidentialHash) return false;

                if (!DualStamp.checkEqual(a.dualStamp, b.dualStamp)) return false;
            }

            return true;
        }

        public long occurrenceTime {
            get {
                return privateOccurrenceTime;
            }

            set {
                privateOccurrenceTime = value;
            }
        }

        public Stamp clone() {
            Stamp cloned = new Stamp();
            cloned.tense = tense;
            cloned.dualStamp = dualStamp;
            cloned.creationTime = creationTime;
            cloned.privateOccurrenceTime = privateOccurrenceTime;
            return cloned;
        }

        // this is different from the OpenNARS implementation because we keep all the evidential base
        public Stamp cloneWithNewOccurrenceTime(long newOccurrenceTime) {
            Stamp result = clone();
            if (newOccurrenceTime == ETERNAL) {
                result.tense = EnumTense.ETERNAL;
            }
            result.occurrenceTime = newOccurrenceTime;
            return result;

            throw new NotImplementedException();
        }

        // checks overlap _inside_ a stamp
        // see https://github.com/opennars/opennars/blob/e844d1ee61a2d9af26e6df91b0cc63be7e7dccfc/nars_core/nars/control/DerivationContext.java#L159
        public static bool checkOverlap(Stamp stamp) {
            for( int i = 0; i < stamp.dualStamp.used; i++ ) {
                uint baseI = stamp.dualStamp.accessTermIdHistory(i);
                for( int j = 0; j < stamp.dualStamp.used; j++ ) {
                    if( i == j ) {
                        continue;
                    }

                    if( baseI == stamp.dualStamp.accessTermIdHistory(j) ) {
                        return true;
                    }
                }
            }

            return false;
        }

        EnumTense? tense;
        long creationTime = -1; /* creation time of the stamp */
        long privateOccurrenceTime; /* estimated occurrence time of the event */
        DualStamp dualStamp;

        /**
         * Generate a new stamp for derived sentence by merging the two from parents
         * the first one is no shorter than the second
         *
         * \param first The first Stamp
         * \param second The second Stamp
         */
        public static Stamp zipWithTime(
            Stamp first,
            Stamp second,
            long time
        ) {

            Debug.Assert(first.dualStamp.used >= second.dualStamp.used);

            Stamp result = new Stamp();
            result.dualStamp = DualStamp.zip(first.dualStamp, second.dualStamp);
            result.creationTime = time;
            result.occurrenceTime = first.occurrenceTime;
            return result;
        }
    }
}
