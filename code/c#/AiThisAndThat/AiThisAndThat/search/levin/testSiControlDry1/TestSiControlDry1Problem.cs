using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/**
 * 
 * Example which is used to test if the Levin search can be used to generate a program.
 * This program is then used to control an abstract virtual machine for selfimprovement and finding new algorithms.
 * This machine works by viewing the program and the data and the change to its data as a pure functional
 * datastructure, so all changes are revertable and revisable and introspectiable by any algorithms.
 * 
 * 
 */
class TestSiControlDry1Problem : LevinProblem {
    class ExecutionContext {
        public struct IpElement {
            public IpElement(uint index, Element element) {
                this.index = index;
                this.element = element;
            }

            public uint index;
            public Element element;
        }
        
        // points at the poisition in from the root to the current element which is changed
        List<IpElement> ip;

        public Element root;



        public void reset() {
            root = new Brace();
            ip = new List<IpElement>(){ new IpElement(0, root) };
        }
        
        // action write repeat
        // action write op1
        // action write new brace
        // action write add
        // action write op1
        // action write op2

        public void actionWriteNewBrace(out bool executionFault) {
            executionFault = true;

            Brace topBrace;
            uint topIpIndex;

            getTopBraceWithIndex(out topBrace, out topIpIndex, out executionFault);
            if (executionFault) {
                return;
            }
            executionFault = true;

            Element newBrace = new Brace();

            topBrace.content.Insert((int)topIpIndex, newBrace);
            
            
            //topIp.index++;

            //ip.Add(new IpElement(0, newBrace));
            executionFault = false;
        }

        public void actionWriteEnterBrace(out bool executionFault) {
            executionFault = true;

            Brace topBrace;
            uint topIpIndex;
            getTopBraceWithIndex(out topBrace, out topIpIndex, out executionFault);
            if (executionFault) {
                return;
            }
            executionFault = true;

            ip.Add(new IpElement(0, topBrace));

            executionFault = false;
        }

        public void actionIpGoBack(out bool executionFault) {
            executionFault = true;

            IpElement? topIpNullable = getTopIp();
            if( !topIpNullable.HasValue ) {
                return;
            }


            IpElement topIp = topIpNullable.Value;
            if (topIp.index == 0) {
                return;
            }

            topIp.index--;
            executionFault = false;
        }

        public void actionWriteExitBrace(out bool executionFault) {
            executionFault = true;

            if (ip.Count == 0) {
                return;
            }

            ip.RemoveAt(ip.Count - 1);
            executionFault = false;

        }


        public void actionWriteAddAndIncrementIp(out bool executionFault) {
            executionFault = true;

            topIpInsertElement(Element.EnumType.ADD, out executionFault);
            if (executionFault) {
                return;
            }

            actionIncrementIp(out executionFault);
        }


        public void actionWriteOp1AndIncrementIp(out bool executionFault) {
            executionFault = true;

            topIpInsertElement(Element.EnumType.OP1, out executionFault);
            if (executionFault) {
                return;
            }

            actionIncrementIp(out executionFault);
        }

        public void actionWriteOp2AndIncrementIp(out bool executionFault) {
            executionFault = true;

            topIpInsertElement(Element.EnumType.OP2, out executionFault);
            if (executionFault) {
                return;
            }

            actionIncrementIp(out executionFault);
        }

        public void actionWriteRepeatAndIncrementIp(out bool executionFault) {
            executionFault = true;

            topIpInsertElement(Element.EnumType.REPEAT, out executionFault);
            if (executionFault) {
                return;
            }

            actionIncrementIp(out executionFault);
        }


        protected IpElement? getTopIp() {
            if (ip.Count == 0) {
                return null;
            }

            return ip[ip.Count - 1];
        }

        protected void getTopBraceWithIndex(out Brace topBrace, out uint index, out bool executionFault) {
            executionFault = true;
            topBrace = null;
            index = 0;

            IpElement? topIpNullable = getTopIp();
            if (!topIpNullable.HasValue) {
                return;
            }
            IpElement topIp = topIpNullable.Value;
            
            if (topIp.element.type != Element.EnumType.BRACE) {
                return;
            }

            executionFault = false;
            topBrace = (Brace)topIp.element;
            index = topIp.index;
        }

        protected void topIpInsertElement(Element.EnumType type, out bool executionFault) {
            executionFault = true;

            Brace topBrace;
            uint topIpIndex;
            getTopBraceWithIndex(out topBrace, out topIpIndex, out executionFault);
            if (executionFault) {
                return;
            }
            executionFault = true;

            topBrace.content.Insert((int)topIpIndex, new Element(type));

            executionFault = false;
        }

        private void actionIncrementIp(out bool executionFault) {
            executionFault = true;

            IpElement? topIpNullable = getTopIp();
            if (!topIpNullable.HasValue) {
                return;
            }

            IpElement topIp = topIpNullable.Value;

            topIp.index++;

            executionFault = false;
        }


        public class Element {
            public enum EnumType {
                BRACE,
                ADD,
                OP1,
                OP2,
                REPEAT
            }

            public Element(EnumType type) {
                this.type = type;
            }

            public EnumType type;
        }

        public class Brace : Element {
            public Brace() : base(Element.EnumType.BRACE) {
            }

            public List<Element> content = new List<Element>();
        }


    }
    
    public override void executeProgram(LevinProgram program, uint maxNumberOfStepsToExecute, out bool hasHalted) {
        hasHalted = false;
        
        ExecutionContext executionContext = new ExecutionContext();

        executionContext.reset();
        bool executionFault = true;

        foreach (uint iterationInstruction in program.instructions) {
            switch (iterationInstruction) {
                case 0: executionContext.actionWriteAddAndIncrementIp(out executionFault); break;
                case 1: executionContext.actionWriteEnterBrace(out executionFault); break;
                case 2: executionContext.actionWriteExitBrace(out executionFault); break;
                case 3: executionContext.actionWriteNewBrace(out executionFault); break;
                case 4: executionContext.actionWriteOp1AndIncrementIp(out executionFault); break;
                case 5: executionContext.actionWriteOp2AndIncrementIp(out executionFault); break;
                case 6: executionContext.actionWriteRepeatAndIncrementIp(out executionFault); break;
                case 7: executionContext.actionIpGoBack(out executionFault); break;
            }

            if (executionFault) {
                return;
            }
        }

        // execute program
        // TODO TODO TODO

        // instead we just test for all valid combinations
        // TODO TODO TODO TODO TODO
    }


}

