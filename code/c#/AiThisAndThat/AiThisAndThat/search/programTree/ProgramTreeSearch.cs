using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using helper;

// uses a propabilistic context free grammar (PCFG) for the creation of the most propable programs
namespace Search.ProgramTree {
    class GrammarRule {
        public GrammarRule(GrammarRuleAlternative leftSide, List<GrammarRuleAlternative> rightSideAlternatives) {
            this.leftSide = leftSide;
            this.rightSideAlternatives = rightSideAlternatives;
        }

        public GrammarRuleAlternative leftSide;

        public List<GrammarRuleAlternative> rightSideAlternatives;
    }

    // is an possibility of an rule
    /*
    class Alternative {
        List<AlternativeElement> elements = new List<AlternativeElement>();
    }
     */

    class GrammarRuleAlternative {
        public enum EnumType {
            TERMINAL,
            VARIABLE,
            BRACE,
            SEQUENCE // sequence of AlternativeElements
        }

        protected GrammarRuleAlternative(EnumType type) {
            this.type = type;
        }

        public static GrammarRuleAlternative makeVariable(uint variable) {
            GrammarRuleAlternative result = new GrammarRuleAlternative(EnumType.VARIABLE);
            result.variable = variable;
            return result;
        }

        public static GrammarRuleAlternative makeTerminal(ProgramTreeElementType.EnumType terminalType) {
            GrammarRuleAlternative result = new GrammarRuleAlternative(EnumType.TERMINAL);
            result.terminalType = terminalType;
            return result;
        }

        public static GrammarRuleAlternative makeBrace(List<GrammarRuleAlternative> braceContent) {
            GrammarRuleAlternative result = new GrammarRuleAlternative(EnumType.BRACE);
            result.braceContent = braceContent;
            return result;
        }

        public static GrammarRuleAlternative makeSequence(List<GrammarRuleAlternative> sequenceContent) {
            GrammarRuleAlternative result = new GrammarRuleAlternative(EnumType.SEQUENCE);
            result.sequenceContent = sequenceContent;
            return result;
        }

        public EnumType type;

        public ProgramTreeElementType.EnumType terminalType;

        public uint variable;

        public List<GrammarRuleAlternative> braceContent;
        public List<GrammarRuleAlternative> sequenceContent;
    }
    
    class ProgramTreeSearch {
        List<GrammarRule> rules;
        
        enum EnumVariables {
                S = 0,
                T,
                OPA,
                A
            }


        protected void fillRules() {            
            rules = new List<GrammarRule>();

            rules.Add(new GrammarRule(
                GrammarRuleAlternative.makeVariable((uint)EnumVariables.S),

                new List<GrammarRuleAlternative>(){
                    GrammarRuleAlternative.makeBrace(new List<GrammarRuleAlternative>(){
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.T),
                    }),

                    GrammarRuleAlternative.makeBrace(new List<GrammarRuleAlternative>(){
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.OPA),
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.A),
                    }),

                    GrammarRuleAlternative.makeBrace(new List<GrammarRuleAlternative>(){
                        GrammarRuleAlternative.makeTerminal(ProgramTreeElementType.EnumType.REPEAT),
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.S),
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.S),
                    }),
                }
            ));

            rules.Add(new GrammarRule(
                GrammarRuleAlternative.makeVariable((uint)EnumVariables.OPA),

                new List<GrammarRuleAlternative>() {
                    GrammarRuleAlternative.makeTerminal(ProgramTreeElementType.EnumType.ADD),
                    GrammarRuleAlternative.makeTerminal(ProgramTreeElementType.EnumType.SUB),
                    GrammarRuleAlternative.makeTerminal(ProgramTreeElementType.EnumType.MUL),
                }
            ));

            rules.Add(new GrammarRule(
                GrammarRuleAlternative.makeVariable((uint)EnumVariables.A),

                new List<GrammarRuleAlternative>() {
                    GrammarRuleAlternative.makeVariable((uint)EnumVariables.T),
                    GrammarRuleAlternative.makeSequence(new List<GrammarRuleAlternative>(){
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.T),
                        GrammarRuleAlternative.makeVariable((uint)EnumVariables.A),
                    }),
                }
            ));

            rules.Add(new GrammarRule(
                GrammarRuleAlternative.makeVariable((uint)EnumVariables.T),

                new List<GrammarRuleAlternative>() {
                    GrammarRuleAlternative.makeTerminal(ProgramTreeElementType.EnumType.A),
                    GrammarRuleAlternative.makeTerminal(ProgramTreeElementType.EnumType.B),
                }
            ));

        }

    }


    class ProgramTreeElementType {
        public ProgramTreeElementType(EnumType value) {
            this.value = value;
        }

        public enum EnumType {
            BRACE,
            REPEAT,
            ADD,
            SUB,
            MUL,
            A, // variable access
            B, // variable access
        }
        
        public EnumType value;
    }


    ///////////////
    /// used for deriving the grammar tree from the current enumeration value

    class CurrentGrammarTreeElement {
        public enum EnumType {
            ADD = 0,
            SUB,
            MUL,
            A, // variable
            B, // variable
            REPEAT,
            BRACE, // functional brace
        }

        public static uint getMaxEnumValue() {
            return (uint)EnumType.BRACE + 1;
        }

        public List<CurrentGrammarTreeElement> childrens = new List<CurrentGrammarTreeElement>();
    }


    ///////////////
    /// used for storing the functional program



    class ProgramTreeElement : TreeElement<ProgramTreeElementType> {
        public ProgramTreeElement(ProgramTreeElementType.EnumType type) : base(new ProgramTreeElementType(type)) { }
    }

    class ProgramTreeBraceElement : ProgramTreeElement {
        public ProgramTreeBraceElement() : base(ProgramTreeElementType.EnumType.BRACE) { }
    }
}
