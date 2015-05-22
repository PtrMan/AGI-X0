using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    /**
     * 
     * instantiated OperatorBluerint
     * 
     * contains operatorInstances which get used by the system
     */
    class OperatorPlan
    {
        public class TreeElement
        {
            public enum EnumType
            {
                LEAF,
                BRANCH
            }

            public EnumType type;

            public bool isLeaf()
            {
                return type == EnumType.LEAF;
            }

            public bool isBranch()
            {
                return type == EnumType.BRANCH;
            }

            public List<TreeElement> childrens;

            public bool apearsInParentParameterInstances;
            public OperatorInstance operatorInstance;
        }

        internal TreeElement rootTreeElement;

        private static TreeElement walkTreeElementByPath(TreeElement currentTreeElement, List<int> path)
        {
            foreach (int currentPathIndex in path)
            {
                currentTreeElement = currentTreeElement.childrens[currentPathIndex];
            }

            return currentTreeElement;
        }


        /*
        public EnumType getTypeByPath(List<int> path)
        {
            // TODO
        }

        public Variadic getValueByPath(List<int> path)
        {
            // TODO
        }

        public OperatorInstance getInstanceByPath(List<int> path)
        {
            // TODO
        }
        */

        public List<OperatorInstance> getParameterInstancesByPath(List<int> path)
        {
            TreeElement endTreeElement;
            List<OperatorInstance> resultList;

            endTreeElement = walkTreeElementByPath(rootTreeElement, path);

            resultList = new List<OperatorInstance>();

            if (endTreeElement.isLeaf())
            {
                resultList.Add(endTreeElement.operatorInstance);
            }
            else
            {
                // look for childnodes where the apearsInParentParameterInstances flag is true
                foreach (TreeElement iterationChildElement in endTreeElement.childrens)
                {
                    if (iterationChildElement.apearsInParentParameterInstances)
                    {
                        resultList.Add(iterationChildElement.operatorInstance);
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

        public int getParameterNumberByPath(List<int> path)
        {
            // TODO
            return 0;
        }

        public static OperatorPlan instantiateBlueprint(OperatorBlueprint blueprint)
        {
            OperatorPlan resultPlan;

            resultPlan = new OperatorPlan();
            resultPlan.rootTreeElement = instantiateBlueprintTreeRecursive(blueprint.rootTreeElement);

            return resultPlan;
        }

        private static TreeElement instantiateBlueprintTreeRecursive(OperatorBlueprint.TreeElement treeElement)
        {
            TreeElement resultTreeElement;

            resultTreeElement = new TreeElement();
            resultTreeElement.apearsInParentParameterInstances = treeElement.apearsInParentParameterInstances;

            resultTreeElement.operatorInstance = treeElement.operatorToInstanciate.createOperatorInstance();
            resultTreeElement.operatorInstance.constants = treeElement.constantsForOperatorInstance;

            if( treeElement.isBranch() )
            {
                resultTreeElement.type = TreeElement.EnumType.BRANCH;
            }
            else
            {
                resultTreeElement.type = TreeElement.EnumType.LEAF;
            }
            
            if( treeElement.isBranch() )
            {
                resultTreeElement.childrens = new List<TreeElement>();
                
                foreach( OperatorBlueprint.TreeElement iteratorChildTreeElement in treeElement.childrens )
                {
                    resultTreeElement.childrens.Add(instantiateBlueprintTreeRecursive(iteratorChildTreeElement));
                }
            }

            return resultTreeElement;
        }
    }
}
