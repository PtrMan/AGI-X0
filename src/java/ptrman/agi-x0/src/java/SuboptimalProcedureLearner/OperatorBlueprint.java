package SuboptimalProcedureLearner;

import Datastructures.Variadic;
import ptrman.misc.Deepcopy;
import ptrman.misc.ListTools;
import sun.reflect.generics.reflectiveObjects.NotImplementedException;

import java.util.ArrayList;
import java.util.List;

/**
 *
 * one plan can be combined into bigger plans (composition)
 *
 * does also contain the positions(as paths) to operators which need to be replaced
 */
public class OperatorBlueprint {
    public static class TreeElement {
        public enum EnumType {
            LEAF,
            DUMMY,
            // if the operator to instanciate is not set and if this is free and deferable to a later time
            BRANCH
        }
        public TreeElement(EnumType type) {
            this.type = type;
        }

        public EnumType type = EnumType.LEAF;
        public boolean isLeaf() {
            return type == EnumType.LEAF || type == EnumType.DUMMY;
        }

        public boolean isBranch() {
            return type == EnumType.BRANCH;
        }

        public boolean isDummy() {
            return type == EnumType.DUMMY;
        }

        public List<TreeElement> childrens = new ArrayList<>();
        // -----
        // only valid for leafs
        public boolean apearsInParentParameterInstances = false;
        //public OperatorInstance operatorInstance;
        public Operator operatorToInstanciate;
        public Variadic[] constantsForOperatorInstance;
    }

    public TreeElement rootTreeElement;
    // paths to Operators which are dummy operators, which need to be replaced with real operators for a useful Operator which has a function
    private List<List<Integer>> pathsToFreeOperators = new ArrayList<>();

    public enum EnumType {
        INSTANCE,
        VALUE,
        EMPTY
    }
    public enum EnumReplace {
        // if the place doesn't have a value or a operator attached
        YES,
        NO
    }
    private OperatorBlueprint(TreeElement tree, List<List<Integer>> pathsToFreeOperators) {
        rootTreeElement = tree;
        this.pathsToFreeOperators = pathsToFreeOperators;
    }

    public static OperatorBlueprint compose(OperatorBlueprint orginal, List<Integer> pathInOrginal, OperatorBlueprint composeWith, EnumReplace replace) {
        TreeElement currentTreeElement;
        OperatorBlueprint result = orginal.clone();
        if (replace == EnumReplace.YES) {
            List<Integer> pathWithoutLast = pathInOrginal.subList(0, pathInOrginal.size() - 1);
            int lastPathIndex = pathInOrginal.get(pathInOrginal.size() - 1);
            // check if the pathInOrginal is one of pathToFreeOperators, if this is the case, remove it
            for( int pathToFreeOperatorsI = 0; pathToFreeOperatorsI < result.pathsToFreeOperators.size(); pathToFreeOperatorsI++ ) {
                List<Integer> pathsToFreeOperator = result.pathsToFreeOperators.get(pathToFreeOperatorsI);
                if (ListTools.isListTheSameInt(pathsToFreeOperator, pathInOrginal)) {
                    result.pathsToFreeOperators.remove(pathToFreeOperatorsI);
                    pathToFreeOperatorsI--;
                    continue;
                }
                 
            }
            // include path to free operators from composeWith
            List<List<Integer>> pathsWithAppendedPath = appendPathsToPath(pathInOrginal, composeWith.pathsToFreeOperators);
            result.pathsToFreeOperators.addAll(pathsWithAppendedPath);
            // walk the path in the orginal/result
            // without the last because we replace it
            currentTreeElement = walkTreeElementByPath(result.rootTreeElement, pathWithoutLast);
            currentTreeElement.childrens.set(lastPathIndex, cloneTreeElement(composeWith.rootTreeElement));
        }
        else if (replace == EnumReplace.NO) {
            throw new NotImplementedException();
            // TODO
            // walk the path in the orginal/result
            // without the last because we replace it
            //currentTreeElement = walkTreeElementByPath(result.rootTreeElement, pathInOrginal);
            // must be a branch because we add the other plan of the operator/scaffold


            /*
            uncommented because it is unreachable

            currentTreeElement.type = TreeElement.EnumType.BRANCH;
            for (TreeElement iterationChildElement : composeWith.rootTreeElement.childrens) {
                // add
                currentTreeElement.childrens.add(cloneTreeElement(iterationChildElement));
            }
            */
        }
        else {
            throw new RuntimeException("internal error");
        }  
        return result;
    }

    public OperatorBlueprint clone() {
        OperatorBlueprint cloned = new OperatorBlueprint(cloneTreeElement(this.rootTreeElement),deepcopyPaths(this.pathsToFreeOperators));
        return cloned;
    }

    private static TreeElement cloneTreeElement(TreeElement element) {
        TreeElement clone;
        clone = new TreeElement(element.type);
        clone.apearsInParentParameterInstances = element.apearsInParentParameterInstances;
        clone.operatorToInstanciate = element.operatorToInstanciate;
        clone.constantsForOperatorInstance = element.constantsForOperatorInstance;
        clone.type = element.type;
        if (element.isBranch()) {
            clone.childrens = new ArrayList<>();
            for (Object __dummyForeachVar1 : element.childrens) {
                TreeElement iterationChildElement = (TreeElement)__dummyForeachVar1;
                clone.childrens.add(cloneTreeElement(iterationChildElement));
            }
        }
         
        return clone;
    }

    private static TreeElement walkTreeElementByPath(TreeElement currentTreeElement, List<Integer> path) {
        for( int currentPathIndex : path) {
            currentTreeElement = currentTreeElement.childrens.get(currentPathIndex);
        }
        return currentTreeElement;
    }

    private static List<List<Integer>> deepcopyPaths(List<List<Integer>> paths) {
        List<List<Integer>> result = new ArrayList<>();
        for( List<Integer> path : paths ) {
            result.add(Deepcopy.deepCopyList(path));
        }
        return result;
    }

    public static OperatorBlueprint createFromTree(TreeElement tree, List<List<Integer>> pathsToFreeOperators) {
        return new OperatorBlueprint(tree, pathsToFreeOperators);
    }

    public static int countDummiesRecursive(OperatorBlueprint blueprint) {
        return countDummiesOfTreeRecursive(blueprint.rootTreeElement);
    }

    private static int countDummiesOfTreeRecursive(OperatorBlueprint.TreeElement treeElement) {
        if( treeElement.isLeaf() ) {
            if( treeElement.isDummy() ) {
                return 1;
            }
             
            return 0;
        }
        else {
            int count = 0;
            for (Object __dummyForeachVar4 : treeElement.childrens) {
                TreeElement iteratorTree = (OperatorBlueprint.TreeElement)__dummyForeachVar4;
                count += countDummiesOfTreeRecursive(iteratorTree);
            }
            return count;
        } 
    }

    private static List<List<Integer>> appendPathsToPath(final List<Integer> basePath, final List<List<Integer>> appendingPaths) {
        List<List<Integer>> resultList = new ArrayList<>();
        for( final List<Integer> iterationpath : appendingPaths ) {
            List<Integer> workingCopy = Deepcopy.deepCopyList(basePath);
            workingCopy.addAll(iterationpath);
            resultList.add(workingCopy);
        }
        return resultList;
    }
}
