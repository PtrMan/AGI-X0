using System;
using System.Collections.Generic;

namespace AiThisAndThat {
    // used to store the names of the symbols
    public class PatternSymbolContext {
        public Tuple<ulong, ulong> lookupOrCreateSymbolIdAndUniqueIdForName(string humanReadableName) {
            if( SymbolIdAndUniqueIdByHumanReadableName.ContainsKey(humanReadableName) )   return SymbolIdAndUniqueIdByHumanReadableName[humanReadableName];
            
            ulong symbolId = returnNewSymbolId();
            ulong uniqueId = returnNewUniqueId();
            SymbolIdAndUniqueIdByHumanReadableName[humanReadableName] = new Tuple<ulong, ulong>(symbolId, uniqueId);
            return new Tuple<ulong, ulong>(symbolId, uniqueId);
        }

        public ulong lookupOrCreateUniqueIdForVariable(string humanReadableVariableName) {
            if( uniqueIdOfVariableByHumanReadableName.ContainsKey(humanReadableVariableName) )   return uniqueIdOfVariableByHumanReadableName[humanReadableVariableName];
            
            ulong uniqueId = returnNewUniqueId();
            uniqueIdOfVariableByHumanReadableName[humanReadableVariableName] = uniqueId;
            return uniqueId;
        }

        public ulong lookupUniqueIdForVariable(string humanReadableVariableName) {
            return uniqueIdOfVariableByHumanReadableName[humanReadableVariableName];
        }

        public Tuple<ulong, ulong> createSymbolIdAndUniqueId() {
            return new Tuple<ulong, ulong>(returnNewSymbolId(), returnNewUniqueId());
        }


        ulong returnNewSymbolId() {
            return symbolIdCounter++;
        }

        public ulong returnNewUniqueId() {
            return uniqueIdCounter++;
        }

        IDictionary<string, ulong> uniqueIdOfVariableByHumanReadableName = new Dictionary<string, ulong>();

        IDictionary<string, Tuple<ulong, ulong>> SymbolIdAndUniqueIdByHumanReadableName = new Dictionary<string, Tuple<ulong, ulong>>();
        ulong symbolIdCounter = 0;

        ulong uniqueIdCounter = 0;

        
    }
}
