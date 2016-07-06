using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace treeTransducer {
    class TreeTransducer<TreeElementType> where TreeElementType : ICloneable {
        public static TreeElementType tryToApplyRulesRecursivly(List<Rule<TreeElementType>> rules, TreeElementType apply, ITreeManipulationFacade<TreeElementType> treeManipulationFacade) {
            TreeElementType rewritten = apply;

            foreach (Rule<TreeElementType> iterationRule in rules) {
                if (doesSourceConditionMatch(iterationRule.matching, apply, treeManipulationFacade)) {
                    List<Tuple<string, TreeElementType>> variableAssignments = assignVariables(iterationRule, apply, treeManipulationFacade);

                    rewritten = rewrite(iterationRule.rewriteTarget, variableAssignments, treeManipulationFacade);
                    break;
                }
            }

            for (int i = 0; i < treeManipulationFacade.getNumberOfChildren(rewritten); i++) {
                TreeElementType rwrittenChildren = tryToApplyRulesRecursivly(rules, treeManipulationFacade.getChildren(rewritten, i), treeManipulationFacade);
                treeManipulationFacade.setChildren(rewritten, i, rwrittenChildren);
            }

            return rewritten;
        }

        private static TreeElementType rewrite(TreeElementType apply, List<Tuple<string, TreeElementType>> variables, ITreeManipulationFacade<TreeElementType> treeManipulationFacade) {
            if (treeManipulationFacade.getType(apply) == ITreeManipulationFacade<TreeElementType>.EnumTreeElementType.VARIABLE) {
                TreeElementType variableValue = (TreeElementType)getVariableByName(variables, treeManipulationFacade.getVariableName(apply)).Clone();
                treeManipulationFacade.setType(variableValue, ITreeManipulationFacade<TreeElementType>.EnumTreeElementType.VALUE);
                return variableValue;
            }
            else {
                for (int i = 0; i < treeManipulationFacade.getNumberOfChildren(apply); i++) {
                    TreeElementType iterationApplyChildren = treeManipulationFacade.getChildren(apply, i);
                    TreeElementType rewritten = rewrite(iterationApplyChildren, variables, treeManipulationFacade);
                    treeManipulationFacade.setChildren(apply, i, rewritten);
                }
                return apply;
            }
        }

        private static TreeElementType getVariableByName(List<Tuple<string, TreeElementType>> variableAssignments, string name) {
            foreach (Tuple<string, TreeElementType> iterationVariableAssignment in variableAssignments) {
                if (iterationVariableAssignment.Item1 == name) {
                    return iterationVariableAssignment.Item2;
                }
            }

            throw new Exception("Fatal error: Variable was not found");
        }

        private static bool doesSourceConditionMatch(TreeElementType ruleTreeElement, TreeElementType apply, ITreeManipulationFacade<TreeElementType> treeManipulationFacade) {
            bool isVariable = treeManipulationFacade.getType(ruleTreeElement) == ITreeManipulationFacade<TreeElementType>.EnumTreeElementType.VARIABLE;
            if (isVariable) {
                return true;
            }
            else {
                int ruleChildrenCount = treeManipulationFacade.getNumberOfChildren(ruleTreeElement);
                int applyChildrenCount = treeManipulationFacade.getNumberOfChildren(apply);

                if (treeManipulationFacade.isLeaf(ruleTreeElement) && treeManipulationFacade.isLeaf(apply)) {
                    return treeManipulationFacade.isEqual(ruleTreeElement, apply);
                }

                if (ruleChildrenCount != applyChildrenCount) {
                    return false;
                }

                int childrenCount = ruleChildrenCount;
                for (int i = 0; i < childrenCount; i++) {
                    if (!doesSourceConditionMatch(treeManipulationFacade.getChildren(ruleTreeElement, i), treeManipulationFacade.getChildren(apply, i), treeManipulationFacade)) {
                        return false;
                    }
                }

                return true;
            }
        }

        private static List<Tuple<string, TreeElementType>> assignVariables(Rule<TreeElementType> rule, TreeElementType apply, ITreeManipulationFacade<TreeElementType> treeManipulationFacade) {
            Debug.Assert(doesSourceConditionMatch(rule.matching, apply, treeManipulationFacade));

            List<Tuple<string, TreeElementType>> variablesDestination = new List<Tuple<string, TreeElementType>>();
            assignVariablesHelper(rule.matching, apply, variablesDestination, treeManipulationFacade);
            return variablesDestination;
        }

        private static void assignVariablesHelper(TreeElementType ruleTreeElement, TreeElementType apply, List<Tuple<string, TreeElementType>> variablesDestination, ITreeManipulationFacade<TreeElementType> treeManipulationFacade) {
            if (treeManipulationFacade.getType(ruleTreeElement) == ITreeManipulationFacade<TreeElementType>.EnumTreeElementType.VARIABLE) {
                variablesDestination.Add(new Tuple<string, TreeElementType>(treeManipulationFacade.getVariableName(ruleTreeElement), apply));
            }
            else {
                int childrenCount = treeManipulationFacade.getNumberOfChildren(ruleTreeElement);
                Debug.Assert(treeManipulationFacade.getNumberOfChildren(ruleTreeElement) ==  treeManipulationFacade.getNumberOfChildren(apply));

                for (int i = 0; i < childrenCount; i++) {
                    assignVariablesHelper(treeManipulationFacade.getChildren(ruleTreeElement, i), treeManipulationFacade.getChildren(apply, i), variablesDestination, treeManipulationFacade);
                }
            }

        }
    }
}
