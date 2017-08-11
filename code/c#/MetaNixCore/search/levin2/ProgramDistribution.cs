using MetaNix.datastructures.compact;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace MetaNix.search.levin2 {
    // implementations sample the programspace acording to an distribution which is stored and updated
    interface IProgramDistribution {
        // \param propabilityVector is an array of the propability [0; 1) for each selection of an instruction
        // \param lengthOfProgram how log is the sampled program, must be <= resultInstructions.Count, can be -1 if the maximal length is allowed
        void sample(double[] propabilityVector, ref uint[] resultInstructions, uint instructionsetCount, int lengthOfProgram, Random random);

        void addProgram(uint[] instructions);

        void multiplyPropabiliyForProgram(uint[] instructions, double multiplicator);
    }

    // stores the instructions and their corresponding propabilities based on an array
    /*
     * in each level in the stored tree the propabilities and the corresponding instructions get stored for all programs which were found by the agent thus far.
     * for all other instructions which were not encountered a "shadowed" propability is stored
     * 
     */
    public class SparseArrayProgramDistribution : IProgramDistribution {
        // \param numberOfInstructions how many instructions exist for the instructionset?
        public void sample(double[] propabilityVector, ref uint[] resultInstructions, uint instructionsetCount, int lengthOfProgram, Random random) {
            // select instruction, look for a children for the node, iterate

            lengthOfProgram = lengthOfProgram == -1 ? propabilityVector.Length : lengthOfProgram;

            SparseArrayProgramDistributionTreeElement iterationElement = root;

            // if the iteration tree element is null we choose an random instruction,
            // because the tree has no preference or knowledge of the instruction distribution

            for (int i = 0; i < lengthOfProgram; i++) {
                // * calculate absolute propability sum
                double propabilityMassForCurrentSparseArrayProgramDistributionTreeElement = iterationElement != null ? iterationElement.getPropabilityMass(instructionsetCount) : instructionsetCount;
                double propabilityForInstructionByIndex = propabilityVector[i];
                double absolutePropabilitySum = propabilityMassForCurrentSparseArrayProgramDistributionTreeElement * propabilityForInstructionByIndex;

                uint selectedInstruction;
                // * select instructions
                if (iterationElement != null) {
                    selectedInstruction = iterationElement.sampleInstructionBasedOnAbsolutePropability(absolutePropabilitySum, instructionsetCount, random);
                }
                else {
                    // fast way to choose a random instruction
                    selectedInstruction = (uint)absolutePropabilitySum;
                }

                // * store instruction
                resultInstructions[i] = selectedInstruction;

                // * choose next tree element
                if (iterationElement != null) {
                    iterationElement = iterationElement.getChildrenElementByInstruction(selectedInstruction);
                }
            }
        }

        public void addProgram(uint[] instructions) {
            double defaultPropabilityOfShadowedInstruction = 1.0; // just a name for an otherwise magical value

            SparseArrayProgramDistributionTreeElement treeElementForCurrentInstruction = root;

            if (root == null) {
                root = new SparseArrayProgramDistributionTreeElement(defaultPropabilityOfShadowedInstruction);
                treeElementForCurrentInstruction = root;
            }

            foreach (uint iterationInstruction in instructions) {
                if (!treeElementForCurrentInstruction.isInstructionKnown(iterationInstruction)) {
                    treeElementForCurrentInstruction.appendInstruction(iterationInstruction, defaultPropabilityOfShadowedInstruction, new SparseArrayProgramDistributionTreeElement(defaultPropabilityOfShadowedInstruction));
                }

                treeElementForCurrentInstruction = treeElementForCurrentInstruction.getChildrenElementByInstruction(iterationInstruction);
            }
        }

        public void multiplyPropabiliyForProgram(uint[] instructions, double multiplicator) {
            var iterationNode = root;
            foreach (uint iterationInstruction in instructions) {
                // Ensure.ensureHard(iterationNode != null);
                iterationNode.multiplyPropabilityForInstruction(iterationInstruction, multiplicator);
                iterationNode = iterationNode.getChildrenElementByInstruction(iterationInstruction);
            }
        }

        SparseArrayProgramDistributionTreeElement root;
    }

    class SparseArrayProgramDistributionElementsWithPropability {
        class RelativePropabilityAndSum {
            public double relativePropability;
            public double propabilitySum;

            public RelativePropabilityAndSum(double relativePropability, double propabilitySum) {
                this.relativePropability = relativePropability;
                this.propabilitySum = propabilitySum;
            }
        }

        CompressedTable compressedTable = new CompressedTable();

        // we store propability and sum for fast sampling
        // sum of propabilities is just relative the propabilities only in this object
        IList<RelativePropabilityAndSum> propabilities = new List<RelativePropabilityAndSum>();

        public void append(uint instruction, double relativePropability) {
            // TODO< in debug mode check for other instructions >
            compressedTable.append(instruction);
            propabilities.Add(new RelativePropabilityAndSum(relativePropability, getPropabilitySum() + relativePropability));
        }

        public bool existInstruction(uint instruction) {
            return compressedTable.hasValue(instruction);
        }

        public uint getInstructionWhichFallsIntoAbsolutePropabilitySum(double absolutePropabilitySum) {
            // TODO< binary search >
            for (int i = propabilities.Count - 1; i >= 0; i--) {
                if (propabilities[i].propabilitySum < absolutePropabilitySum) {
                    return compressedTable.getValueByGlobalIndex((uint)i);
                }
            }

            return compressedTable.getValueByGlobalIndex(0);
        }

        public double getPropabilitySum() {
            // propabilitySum is sum until now or zero
            return propabilities.Count > 0 ? propabilities[propabilities.Count - 1].propabilitySum : 0.0;
        }

        public void multiplyPropabilityForInstruction(uint instruction, double multiplicator) {
            for (uint i = 0; i < count; i++) {
                if (compressedTable.getValueByGlobalIndex(i) == instruction) {
                    propabilities[(int)i].relativePropability *= multiplicator;
                }
                else {
                    propabilities[(int)i].relativePropability /= multiplicator;
                }
            }

            recalcPropabilitySum();
        }

        void recalcPropabilitySum() {
            double sum = 0.0;

            for (int i = 0; i < count; i++) {
                sum += propabilities[i].relativePropability;
                propabilities[i].propabilitySum = sum;
            }
        }


        public uint count {
            get {
                //Ensure.ensureHard(propabilities.Length == compressedTable.usedValues);
                return (uint)propabilities.Count;
            }
        }

    }

    // tree in the sparse program distibution
    // contains the propabilities and instructionnumbers of known instructions and a propability for all unknown remaining instructions
    class SparseArrayProgramDistributionTreeElement {
        public SparseArrayProgramDistributionTreeElement(double propabilityOfShadowedInstruction) {
            this.propabilityOfShadowedInstruction = propabilityOfShadowedInstruction;
        }

        // adds instruction with following tree
        public void appendInstruction(uint instruction, double relativePropability, SparseArrayProgramDistributionTreeElement childrenTreeElement) {
            // ensure hard
            Debug.Assert(!childrenByInstruction.ContainsKey(instruction));
            childrenByInstruction[instruction] = childrenTreeElement;
            tableWithPropability.append(instruction, relativePropability);
        }

        // returns null if there is no children by the selected instruction
        public SparseArrayProgramDistributionTreeElement getChildrenElementByInstruction(uint instruction) {
            if (childrenByInstruction.ContainsKey(instruction)) {
                return childrenByInstruction[instruction];
            }
            return null;
        }

        // if it is not known a random instruction is returned
        // \param instructionsetCount is as parameter that the object doesn't have to carry around the number of instructions
        public uint sampleInstructionBasedOnAbsolutePropability(double absolutePropabilitySum, uint instructionsetCount, Random random) {
            if (absolutePropabilitySum > tableWithPropability.getPropabilitySum()) {
                // propability mass of table is too low, we have to search an instruction which is not mentioned in the table

                for (; ; ) {
                    uint candidateInstruction = (uint)random.Next((int)instructionsetCount);
                    if (!tableWithPropability.existInstruction(candidateInstruction)) {
                        return candidateInstruction;
                    }
                }
            }
            else {
                return tableWithPropability.getInstructionWhichFallsIntoAbsolutePropabilitySum(absolutePropabilitySum);
            }
        }

        public double getPropabilityMass(uint instructionsetCount) {
            double propabilityMassInTable = tableWithPropability.getPropabilitySum();
            double propabilityMassInShadowedInstructions = propabilityOfShadowedInstruction * (double)(instructionsetCount - tableWithPropability.count);

            return propabilityMassInTable + propabilityMassInShadowedInstructions;
        }

        SparseArrayProgramDistributionElementsWithPropability tableWithPropability = new SparseArrayProgramDistributionElementsWithPropability();
        double propabilityOfShadowedInstruction; // of one instruction and not the whole propabilitymass of all shadowed instructions

        // children which describe the nodes after this instruction chosen by the instruction which was chosen
        IDictionary<uint, SparseArrayProgramDistributionTreeElement> childrenByInstruction = new Dictionary<uint, SparseArrayProgramDistributionTreeElement>();

        public bool isInstructionKnown(uint instruction) {
            return tableWithPropability.existInstruction(instruction);
        }

        // multiplies the propability of the instruction and all other instructions by the inverse
        public void multiplyPropabilityForInstruction(uint instruction, double multiplicator) {
            tableWithPropability.multiplyPropabilityForInstruction(instruction, multiplicator);
            propabilityOfShadowedInstruction /= multiplicator; // all other instructions get less propability mass
        }
    }







    // returns the program based on an distribution
    class ProgramSampler {
        public ProgramSampler(IProgramDistribution programDistribution, uint numberOfInstructions, uint instructionsetCount) {
            this.programDistribution = programDistribution;
            this.temporaryChosenInstructions = new uint[numberOfInstructions];
            this.temporaryPropabilityVector = new double[numberOfInstructions];
            this.instructionsetCount = instructionsetCount;
        }

        public void setInstructionsetCount(uint instructionsetCount) {
            this.instructionsetCount = instructionsetCount;
        }

        public uint[] sampleProgram(int programLength = -1) {
            int usedProgramLength = programLength == -1 ? (int)this.programLength : programLength;

            // fill random vector with values
            for (int instructionIndex = 0; instructionIndex < usedProgramLength; instructionIndex++) {
                temporaryPropabilityVector[instructionIndex] = random.NextDouble();
            }

            programDistribution.sample(temporaryPropabilityVector, ref temporaryChosenInstructions, instructionsetCount, usedProgramLength, random);
            return temporaryChosenInstructions;
        }

        private uint programLength {
            get {
                return (uint)temporaryPropabilityVector.Length;
            }
        }

        IProgramDistribution programDistribution;

        uint[] temporaryChosenInstructions;
        double[] temporaryPropabilityVector; // temporary vector for the chosen absolute values in range [0..1) on which the instructions get chosen

        uint instructionsetCount;
        public Random random = new Random();
    }
}
