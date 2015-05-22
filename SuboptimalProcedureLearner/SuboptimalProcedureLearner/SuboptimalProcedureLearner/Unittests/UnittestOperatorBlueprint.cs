using System;
using System.Collections.Generic;

namespace SuboptimalProcedureLearner.SuboptimalProcedureLearner.Unittests
{
    class UnittestOperatorBlueprint
    {
        public static void unittest()
        {
            test();
        }

        private static void test()
        {
            OperatorBlueprint planOrginal;

            OperatorBlueprint.TreeElement rootElementOrginal;

            rootElementOrginal = new OperatorBlueprint.TreeElement(OperatorBlueprint.TreeElement.EnumType.BRANCH);
            rootElementOrginal.operatorToInstanciate = new Operators.OperatorAdd();

            rootElementOrginal.childrens = new List<OperatorBlueprint.TreeElement>();
            rootElementOrginal.childrens.Add(new OperatorBlueprint.TreeElement(OperatorBlueprint.TreeElement.EnumType.DUMMY));
            rootElementOrginal.childrens.Add(new OperatorBlueprint.TreeElement(OperatorBlueprint.TreeElement.EnumType.DUMMY));
            rootElementOrginal.childrens[0].apearsInParentParameterInstances = true;
            rootElementOrginal.childrens[1].apearsInParentParameterInstances = true;
            
            List<List<int>> pathsToFreeOperatorsForAdd = new List<List<int>>();
            pathsToFreeOperatorsForAdd.Add(new List<int>{0});
            pathsToFreeOperatorsForAdd.Add(new List<int>{1});

            planOrginal = OperatorBlueprint.createFromTree(rootElementOrginal, pathsToFreeOperatorsForAdd);



            OperatorBlueprint.TreeElement treeElementConstantA;

            treeElementConstantA = new OperatorBlueprint.TreeElement(OperatorBlueprint.TreeElement.EnumType.LEAF);
            treeElementConstantA.operatorToInstanciate = new Operators.OperatorConstant();
            treeElementConstantA.constantsForOperatorInstance = new Datastructures.Variadic[1];
            treeElementConstantA.constantsForOperatorInstance[0] = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            treeElementConstantA.constantsForOperatorInstance[0].valueInt = 1;

            OperatorBlueprint constantABlueprint = OperatorBlueprint.createFromTree(treeElementConstantA, new List<List<int>>());



            OperatorBlueprint.TreeElement treeElementConstantB;

            treeElementConstantB = new OperatorBlueprint.TreeElement(OperatorBlueprint.TreeElement.EnumType.LEAF);
            treeElementConstantB.operatorToInstanciate = new Operators.OperatorConstant();
            treeElementConstantB.constantsForOperatorInstance = new Datastructures.Variadic[1];
            treeElementConstantB.constantsForOperatorInstance[0] = new Datastructures.Variadic(Datastructures.Variadic.EnumType.INT);
            treeElementConstantB.constantsForOperatorInstance[0].valueInt = 2;

            OperatorBlueprint constantBBlueprint = OperatorBlueprint.createFromTree(treeElementConstantB, new List<List<int>>());

            // merge them (with replace)
            OperatorBlueprint compositionTemp = OperatorBlueprint.compose(planOrginal, new List<int> {0}, constantABlueprint, OperatorBlueprint.EnumReplace.YES);
            OperatorBlueprint composition = OperatorBlueprint.compose(compositionTemp, new List<int> { 1 }, constantBBlueprint, OperatorBlueprint.EnumReplace.YES);

            // check if result blueprint has any dummies
            System.Diagnostics.Debug.Assert(OperatorBlueprint.countDummiesRecursive(composition) == 0);
        }
    }
}
