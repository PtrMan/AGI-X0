using System;
using System.Linq;
using System.Collections.Generic;

using MetaNix.framework.misc;
using MetaNix.framework.representation.x86;
using MetaNix.datastructures;

namespace AiThisAndThat.prototyping {
    // calculates the utility
    public interface IUtilityTreeElement {
        IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent);

        double propability {
            get;
        }
    }

    // Expected Utility maximization tree element which doesn't do anything
    public class NullUtilityTreeElement : IUtilityTreeElement {
        public double propability => 1.0;

        public IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent) {
            yield return new Tuple<int[], double>(path, propabilityOfParent*propability);
        }
    }

    // just for testing
    // terminates the descision tree search
    class TestingTerminalUtilityTreeElement : IUtilityTreeElement {
        public TestingTerminalUtilityTreeElement(IArchitectureRecoverable archRecoverable, double propability) {
            this.archRecoverable = archRecoverable;
            this.privatePropability = propability;
        }

        public double propability => privatePropability;

        public IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent) {
            archRecoverable.commit();
            yield return new Tuple<int[], double>(path, propabilityOfParent*propability);
            archRecoverable.rollback();
        }

        double privatePropability;
        
        protected IArchitectureRecoverable archRecoverable; // used to do changes to the arhitecture and roll it back (so it is as if the change was never done)
    }




    // used to hold information of a program change and roll it back
    public abstract class RecoverableProgramChange<ProgramType, ChangeRecordType> : IArchitectureRecoverable {
        public RecoverableProgramChange(ProgramType mutatedProgram, ChangeRecordType changeRecords) {
            this.mutatedProgram = mutatedProgram;
            this.changeRecords = changeRecords;
        }

        public abstract void commit();
        public abstract void rollback();

        protected ProgramType mutatedProgram;
        protected ChangeRecordType changeRecords;
    }

    public abstract class AbstractProgram<InstructionType, ExecutionContextType> {
        public MutableArray<InstructionType> instructions = new MutableArray<InstructionType>();

        public abstract void interpret(ExecutionContextType ctx);
    }

    // used to hold the information of a change to a X86Program and roll it back
    public class RecoverableProgramChangeWithRecords<InstructionType, ProgramType, ExecutionContextType>
    : RecoverableProgramChange<ProgramType, RecoverableProgramChangeWithRecords<InstructionType, ProgramType, ExecutionContextType>.ChangeRecords>
    where ProgramType : AbstractProgram<InstructionType, ExecutionContextType>
    {
    
        public class ChangeRecords {
            public IList<ChangeRecord> arr = new List<ChangeRecord>();
        }

        public class ChangeRecord {
            public enum EnumType {
                ADD,
                REMOVE,
                MOVE, // move without replace
            }

            public ChangeRecord(EnumType type) {
                this.type = type;
            }

            public EnumType type;
            public int idxDest, idxSource; // index at which the operation takes place
            public InstructionType instructionToAdd = default(InstructionType);

            public InstructionType storedInstruction; // used for rollback
        }

        public RecoverableProgramChangeWithRecords(ProgramType mutatedProgram, ChangeRecords changeRecords) : base(mutatedProgram, changeRecords) {}

        public override void commit() {
            foreach( var iChangeRecord in changeRecords.arr ) {
                if( iChangeRecord.type == ChangeRecord.EnumType.ADD ) {
                    mutatedProgram.instructions.appendAt(iChangeRecord.idxDest, iChangeRecord.instructionToAdd);
                }
                else if( iChangeRecord.type == ChangeRecord.EnumType.REMOVE ) {
                    iChangeRecord.storedInstruction = mutatedProgram.instructions[iChangeRecord.idxDest];
                    mutatedProgram.instructions.removeAt(iChangeRecord.idxDest);
                }
                else if( iChangeRecord.type == ChangeRecord.EnumType.MOVE ) {
                    InstructionType instr = mutatedProgram.instructions[iChangeRecord.idxSource];
                    mutatedProgram.instructions.removeAt(iChangeRecord.idxSource);
                    mutatedProgram.instructions[iChangeRecord.idxDest] = instr;
                }
                else {
                    throw new NotImplementedException(); // is todo or bug
                }
            }
        }

        public override void rollback() {
            foreach( var iChangeRecord in changeRecords.arr.Reverse() ) {
                if( iChangeRecord.type == ChangeRecord.EnumType.ADD ) { // do add in reverse
                    mutatedProgram.instructions.removeAt(iChangeRecord.idxDest);
                }
                else if( iChangeRecord.type == ChangeRecord.EnumType.REMOVE ) { // do remove in reverse
                    mutatedProgram.instructions.appendAt(iChangeRecord.idxDest, iChangeRecord.storedInstruction);
                }
                else if( iChangeRecord.type == ChangeRecord.EnumType.MOVE ) {
                    InstructionType instr = mutatedProgram.instructions[iChangeRecord.idxDest];
                    mutatedProgram.instructions.removeAt(iChangeRecord.idxDest);
                    mutatedProgram.instructions[iChangeRecord.idxSource] = instr;
                }
                else {
                    throw new NotImplementedException(); // is todo or bug
                }
            }
        }

        
    }





    // executes all changes of a program for all virtual children branches with the propability
    public class ProgramChangeBranchUtilityTreeElement : IUtilityTreeElement {

        // /param propability the propability of this node
        // /param childrenPropabilityChangeAndNode all changes with the propabilities for the (virtual) children
        // /param program the modified program
        public ProgramChangeBranchUtilityTreeElement(
            double propability,
            IList<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>> childrenPropabilityChangeAndNode
        ) {

            this.privatePropability = propability;
            this.childrenPropabilityChangeAndNode = childrenPropabilityChangeAndNode;
        }

        public double propability {
            get {
                return privatePropability;
            }
        }

        public IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent) {
            int idx = 0;
            foreach ( var iPropabilityWithChange in childrenPropabilityChangeAndNode ) {
                int[] childrenPath = new int[path.Length+1];
                Array.Copy(path, childrenPath, path.Length);
                childrenPath[childrenPath.Length-1] = idx;
                
                var resultFromChildren = iPropabilityWithChange.Item3.calcUtility(childrenPath, propabilityOfParent*propability);

                // for each change we must do the change, yield, reverse the change
                iPropabilityWithChange.Item2.commit();
                
                foreach ( var iResultFromChildren in resultFromChildren ) {
                    //iPropabilityWithChange.Item2.commit();
                    yield return iResultFromChildren;
                    //iPropabilityWithChange.Item2.rollback();
                }

                iPropabilityWithChange.Item2.rollback();

                idx++;
            }
        }

        double privatePropability;

        IList<Tuple<double, IArchitectureRecoverable, IUtilityTreeElement>> childrenPropabilityChangeAndNode;
    }




    // just for testing
    /* commented because it was tested with this class
    class TestingTreeUtilityTreeElement : IUtilityTreeElement {
        public IList<IUtilityTreeElement> children;

        public double propability => 0.5;

        public IEnumerable<Tuple<int[], double>> calcUtility(int[] path, double propabilityOfParent) {
            int idx = 0;
            foreach( var iChildren in children ) {
                int[] childrenPath = new int[path.Length+1];
                Array.Copy(path, childrenPath, path.Length);
                childrenPath[childrenPath.Length-1] = idx;
                
                var resultFromChildren = iChildren.calcUtility(childrenPath, propabilityOfParent*propability);
                foreach( var iResultFromChildren in resultFromChildren )   yield return iResultFromChildren;

                idx++;
            }
        }
    }
    
     */
}
