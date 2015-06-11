package ptrman.agix0.src.java.SuboptimalProcedureLearner;

import java.util.ArrayList;
import java.util.List;

/**
 *
 * instantiated OperatorBluerint
 *
 * contains operatorInstances which get used by the system
 */
public class OperatorPlan {
    public static class TreeElement {
        public enum EnumType {
            LEAF,
            BRANCH
        }
        public EnumType type = EnumType.LEAF;
        public boolean isLeaf() {
            return type == EnumType.LEAF;
        }

        public boolean isBranch() {
            return type == EnumType.BRANCH;
        }

        public List<OperatorPlan.TreeElement> childrens = new ArrayList<>();
        public boolean apearsInParentParameterInstances;
        public OperatorInstance operatorInstance;
    }

    public TreeElement rootTreeElement;
    private static TreeElement walkTreeElementByPath(TreeElement currentTreeElement, final List<Integer> path) {
        for( int currentPathIndex : path ) {
            currentTreeElement = currentTreeElement.childrens.get(currentPathIndex);
        }
        return currentTreeElement;
    }

    /*
      public EnumType getTypeByPath(List<int> path) {
          // TODO
      }
      public Variadic getValueByPath(List<int> path) {
           // TODO
      }
      public OperatorInstance getInstanceByPath(List<int> path) {
            // TODO
      }
     */
    public List<OperatorInstance> getParameterInstancesByPath(final List<Integer> path) {
        TreeElement endTreeElement = walkTreeElementByPath(rootTreeElement, path);
        List<OperatorInstance>resultList = new ArrayList<>();
        if( endTreeElement.isLeaf() ) {
            resultList.add(endTreeElement.operatorInstance);
        }
        else {
            for( TreeElement iterationChildElement : endTreeElement.childrens ) {
                // look for childnodes where the apearsInParentParameterInstances flag is true
                if (iterationChildElement.apearsInParentParameterInstances) {
                    resultList.add(iterationChildElement.operatorInstance);
                }
            }
        } 
        return resultList;
    }

    /*
            private List<OperatorInstance> getParameterInstancesByPathOld(List<int> path)
            {
                // TODO
                if( path.Count == 1 )
                {
                    if( path[0] == 0 )
                    {
                        List<OperatorInstance> resultInstances = new List<OperatorInstance>();
                        resultInstances.Add(new OperatorInstance(new Operators.OperatorConstant()));
                        resultInstances.Add(new OperatorInstance(new Operators.OperatorConstant()));
                        return resultInstances;
                    }
                    else
                    {
                        throw new Exception("ERROR");
                        return null;
                    }
                }
                else if (path.Count == 2)
                {
                    if (path[0] == 0)
                    {
                        List<OperatorInstance> resultInstances = new List<OperatorInstance>();
                        OperatorInstance createdOperatorInstance = new OperatorInstance(new Operators.OperatorConstant());
                        createdOperatorInstance.constants = new Variadic[1];
                        createdOperatorInstance.constants[0] = new Variadic(Variadic.EnumType.FLOAT);
                        createdOperatorInstance.constants[0].valueFloat = 1.0f;
                        resultInstances.Add(createdOperatorInstance);
                        return resultInstances;
                    }
                    else
                    {
                        throw new Exception("ERROR");
                        return null;
                    }
                }
                else
                {
                    throw new Exception("ERROR");
                    return null;
                }
                throw new Exception("ERROR");
                return null;
            }
             */
    public int getParameterNumberByPath(List<Integer> path) {
        return 0;
    }

    // TODO
    public static OperatorPlan instantiateBlueprint(OperatorBlueprint blueprint) {
        OperatorPlan resultPlan;
        resultPlan = new OperatorPlan();
        resultPlan.rootTreeElement = instantiateBlueprintTreeRecursive(blueprint.rootTreeElement);
        return resultPlan;
    }

    private static TreeElement instantiateBlueprintTreeRecursive(OperatorBlueprint.TreeElement treeElement) {
        TreeElement resultTreeElement;
        resultTreeElement = new TreeElement();
        resultTreeElement.apearsInParentParameterInstances = treeElement.apearsInParentParameterInstances;
        resultTreeElement.operatorInstance = treeElement.operatorToInstanciate.createOperatorInstance();
        resultTreeElement.operatorInstance.constants = treeElement.constantsForOperatorInstance;
        if( treeElement.isBranch() ) {
            resultTreeElement.type = TreeElement.EnumType.BRANCH;
        }
        else {
            resultTreeElement.type = TreeElement.EnumType.LEAF;
        } 
        if( treeElement.isBranch() ) {
            resultTreeElement.childrens = new ArrayList<>();
            for( OperatorBlueprint.TreeElement iteratorChildTreeElement : treeElement.childrens ) {
                resultTreeElement.childrens.add(instantiateBlueprintTreeRecursive(iteratorChildTreeElement));
            }
        }
         
        return resultTreeElement;
    }
}
