using System;
using System.Collections.Generic;
using System.Linq;

using MetaNix.search.levin2;

namespace MetaNix.control.levinProgramSearch {
    // database for all programs we hve already found
    public class AdvancedAdaptiveLevinSearchProgramDatabase {
        public AdvancedAdaptiveLevinSearchProgramDatabaseQuery getQuery() {
            return new AdvancedAdaptiveLevinSearchProgramDatabaseQuery(entries, entriesSync);
        }

        public ulong createNewEntryId() {
            return entryIdCounter++;
        }

        ulong entryIdCounter;

        Object entriesSync = new Object();

        public IList<AdvancedAdaptiveLevinSearchProgramDatabaseEntry> entries = new List<AdvancedAdaptiveLevinSearchProgramDatabaseEntry>();
    }

    // used for formulating short descriptive queries
    public class AdvancedAdaptiveLevinSearchProgramDatabaseQuery {
        public AdvancedAdaptiveLevinSearchProgramDatabaseQuery(IEnumerable<AdvancedAdaptiveLevinSearchProgramDatabaseEntry> enumerable, Object sync) {
            this.privateEnumerable = enumerable;
            this.sync = sync;
        }

        public AdvancedAdaptiveLevinSearchProgramDatabaseQuery whereHumanReadableTopic(string topic) {
            lock(sync) {
                privateEnumerable = privateEnumerable.Where(v => v.humanReableTopic == topic);
            }
            return this;
        }

        public AdvancedAdaptiveLevinSearchProgramDatabaseQuery whereHumanReadableHintsAny(IList<string> hints) {
            lock (sync) {
                privateEnumerable = privateEnumerable.Where(v => listContainsAny(v.problem.humanReadableHints, hints));
            }
            return this;
        }

        public AdvancedAdaptiveLevinSearchProgramDatabaseQuery whereHumanReadableProgramName(string programName) {
            lock (sync) {
                privateEnumerable = privateEnumerable.Where(v => v.problem.humanReadableTaskname == programName);
            }
            return this;
        }


        public IEnumerable<AdvancedAdaptiveLevinSearchProgramDatabaseEntry> enumerable {
            get {
                return privateEnumerable;
            }
        }

        // TODO< move to some list helper class >
        static bool listContainsAny(IList<string> haystack, IList<string> needles) {
            foreach( string iterationNeedle in needles ) {
                if( haystack.Contains(iterationNeedle) ) {
                    return true;
                }
            }

            return false;
        }
        
        IEnumerable<AdvancedAdaptiveLevinSearchProgramDatabaseEntry> privateEnumerable;
        Object sync;
    }

    public class AdvancedAdaptiveLevinSearchProgramDatabaseEntry {
        public AdvancedAdaptiveLevinSearchProblem problem;

        public uint[] program; // found program
        
        public ulong id; // globally unique id

        public string humanReadableName; // can be null
        public string humanReadableDescription; // can be null
        
        public string humanReableTopic; // topic of the program, for example "string handling", "linked list", "set theory", "mathematics"
                                        // can not be null
    }
}
