using System;
using System.Collections.Generic;

using Datastructures;
using Misc;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner
{
    /**
     * 
     * one plan can be combined into bigger plans (composition)
     * 
     * does also contain the positions(as paths) to operators which need to be replaced
     */
    class OperatorBlueprint
    {
        public class TreeElement
        {
            public enum EnumType
            {
                LEAF,
                DUMMY, // if the operator to instanciate is not set and if this is free and deferable to a later time
                BRANCH
            }

            public TreeElement(EnumType type)
            {
                this.type = type;
            }

            public EnumType type;

            public bool isLeaf()
            {
                return type == EnumType.LEAF || type == EnumType.DUMMY;
            }

            public bool isBranch()
            {
                return type == EnumType.BRANCH;
            }

            public bool isDummy()
            {
                return type == EnumType.DUMMY;
            }

            public List<TreeElement> childrens;

            // -----
            // only valid for leafs
            public bool apearsInParentParameterInstances = false;

            //public OperatorInstance operatorInstance;
            public Operator operatorToInstanciate;
            public Variadic[] constantsForOperatorInstance;
        }

        internal TreeElement rootTreeElement;

        // paths to Operators which are dummy operators, which need to be replaced with real operators for a useful Operator which has a function
        private List<List<int>> pathsToFreeOperators;

        public enum EnumType
        {
            INSTANCE,
            VALUE,
            EMPTY     // if the place doesn't have a value or a operator attached
        }

        public enum EnumReplace
        {
            YES,
            NO
        }

        private OperatorBlueprint(TreeElement tree, List<List<int>> pathsToFreeOperators)
        {
            rootTreeElement = tree;
            this.pathsToFreeOperators = pathsToFreeOperators;
        }

        
        public static OperatorBlueprint compose(OperatorBlueprint orginal, List<int> pathInOrginal, OperatorBlueprint composeWith, EnumReplace replace)
        {
            OperatorBlueprint result;
            TreeElement currentTreeElement;

            result = orginal.clone();

            if( replace == EnumReplace.YES )
            {
                List<int> pathWithoutLast = pathInOrginal.GetRange(0, pathInOrginal.Count - 1);
                int lastPathIndex = pathInOrginal[pathInOrginal.Count-1];

                // check if the pathInOrginal is one of pathToFreeOperators, if this is the case, remove it
                int pathToFreeOperatorsI;

                for( pathToFreeOperatorsI = 0; pathToFreeOperatorsI < result.pathsToFreeOperators.Count; pathToFreeOperatorsI++ )
                {
                    List<int> pathsToFreeOperator;

                    pathsToFreeOperator = result.pathsToFreeOperators[pathToFreeOperatorsI];

                    if( ListTools.isListTheSameInt(pathsToFreeOperator, pathInOrginal) )
                    {
                        result.pathsToFreeOperators.RemoveAt(pathToFreeOperatorsI);
                        pathToFreeOperatorsI--;
                        continue;
                    }
                }

                // include path to free operators from composeWith
                List<List<int>> pathsWithAppendedPath = appendPathsToPath(pathInOrginal, composeWith.pathsToFreeOperators);
                result.pathsToFreeOperators.AddRange(pathsWithAppendedPath);

                // walk the path in the orginal/result
                // without the last because we replace it
                currentTreeElement = walkTreeElementByPath(result.rootTreeElement, pathWithoutLast);

                currentTreeElement.childrens[lastPathIndex] = cloneTreeElement(composeWith.rootTreeElement);
            }
            else if( replace == EnumReplace.NO )
            {
                // TODO
                throw new NotImplementedException();

                // walk the path in the orginal/result
                // without the last because we replace it
                //currentTreeElement = walkTreeElementByPath(result.rootTreeElement, pathInOrginal);

                // must be a branch because we add the other plan of the operator/scaffold
                currentTreeElement.type = TreeElement.EnumType.BRANCH;

                // add 
                foreach (TreeElement iterationChildElement in composeWith.rootTreeElement.childrens)
                {
                    currentTreeElement.childrens.Add(cloneTreeElement(iterationChildElement));
                }
            }
            else
            {
                throw new Exception("internal error");
            }

            return result;
        }

        public OperatorBlueprint clone()
        {
            OperatorBlueprint cloned;

            cloned = new OperatorBlueprint(cloneTreeElement(this.rootTreeElement), deepcopyPaths(this.pathsToFreeOperators));

            return cloned;
        }

        private static TreeElement cloneTreeElement(TreeElement element)
        {
            TreeElement clone;

            clone = new TreeElement(element.type);
            clone.apearsInParentParameterInstances = element.apearsInParentParameterInstances;
            clone.operatorToInstanciate = element.operatorToInstanciate;
            clone.constantsForOperatorInstance = element.constantsForOperatorInstance;
            clone.type = element.type;

            if( element.isBranch() )
            {
                clone.childrens = new List<TreeElement>();

                foreach (TreeElement iterationChildElement in element.childrens)
                {
                    clone.childrens.Add(cloneTreeElement(iterationChildElement));
                }
            }

            return clone;
        }

        private static TreeElement walkTreeElementByPath(TreeElement currentTreeElement, List<int> path)
        {
            foreach( int currentPathIndex in path )
            {
                currentTreeElement = currentTreeElement.childrens[currentPathIndex];
            }

            return currentTreeElement;
        }

        private static List<List<int>> deepcopyPaths(List<List<int>> paths)
        {
            List<List<int>> result;

            result = new List<List<int>>();

            foreach(List<int> path in paths)
            {
                result.Add(Deepcopy.deepCopyListInt(path));
            }

            return result;
        }

        public static OperatorBlueprint createFromTree(TreeElement tree, List<List<int>> pathsToFreeOperators)
        {
            return new OperatorBlueprint(tree, pathsToFreeOperators);
        }

        public static int countDummiesRecursive(OperatorBlueprint blueprint)
        {
            return countDummiesOfTreeRecursive(blueprint.rootTreeElement);
        }

        private static int countDummiesOfTreeRecursive(OperatorBlueprint.TreeElement treeElement)
        {
            if( treeElement.isLeaf() )
            {
                if( treeElement.isDummy() )
                {
                    return 1;
                }

                return 0;
            }
            else
            {
                int count;

                count = 0;

                foreach( OperatorBlueprint.TreeElement iteratorTree in treeElement.childrens )
                {
                    count += countDummiesOfTreeRecursive(iteratorTree);
                }

                return count;
            }
        }

        private static List<List<int>> appendPathsToPath(List<int> basePath, List<List<int>> appendingPaths)
        {
            List<List<int>> resultList;

            resultList = new List<List<int>>();

            foreach(List<int> iterationpath in appendingPaths)
            {
                List<int> workingCopy;

                workingCopy = Deepcopy.deepCopyListInt(basePath);
                workingCopy.AddRange(iterationpath);

                resultList.Add(workingCopy);
            }

            return resultList;
        }
    }
}
