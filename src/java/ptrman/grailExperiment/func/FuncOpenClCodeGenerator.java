package ptrman.grailExperiment.func;

import com.syncleus.ferma.traversals.EdgeTraversal;
import com.tinkerpop.blueprints.Direction;
import org.apache.commons.lang.StringUtils;

import java.util.*;

/**
 * Created by r0b3 on 16.06.15.
 */
public class FuncOpenClCodeGenerator {
    /*
    public class VariableBinding {
        String name;
        String type;
        String value;
    }
    */

    private static class StringBuilderTreeElement {
        public enum EnumType {
            LEAF,
            BRANCH
        }

        public EnumType type;

        public String template; // only value for branches
        public String value; // only valid for LEAFs

        public List<StringBuilderTreeElement> childrens;
    }

    private static class WaitingStackElement {
        public StringBuilderTreeElement treeElement; // tree element where the transformed data should be placed
        public FuncVertex vertex;

        public WaitingStackElement(final StringBuilderTreeElement treeElement, final FuncVertex vertex) {
            this.treeElement = treeElement;
            this.vertex = vertex;
        }
    }

    public static String generateCode(FuncVertex entryVertex) {
        Stack<WaitingStackElement> stack = new Stack<>();

        StringBuilderTreeElement rootTreeElement = new StringBuilderTreeElement();

        insertNodeIntoTreeFrom(stack, rootTreeElement, entryVertex);

        for(;;) {
            if( stack.isEmpty() ) {
                break;
            }

            WaitingStackElement currentWaitingStackElement = stack.pop();
            insertNodeIntoTreeFrom(stack, currentWaitingStackElement.treeElement, currentWaitingStackElement.vertex);
        }

        return generateStringFromTreeRecursive(rootTreeElement);
    }

    private static void insertNodeIntoTreeFrom(Stack<WaitingStackElement> stack, StringBuilderTreeElement treeElement, final FuncVertex vertex) {
        if( vertex instanceof FuncOperationVertex) {
            // TODO< generalize to any number of parameters >

            FuncOperationVertex asOperationVertex = (FuncOperationVertex)vertex;

            FuncVertex left = (FuncVertex)asOperationVertex.out("0").next();
            FuncVertex right = (FuncVertex)asOperationVertex.out("1").next();

            treeElement.type = StringBuilderTreeElement.EnumType.BRANCH;
            treeElement.template = "({0}" + asOperationVertex.getOperation() + "{1})";
            treeElement.childrens = new ArrayList<>();
            treeElement.childrens.add(new StringBuilderTreeElement());
            treeElement.childrens.add(new StringBuilderTreeElement());

            stack.push(new WaitingStackElement(treeElement.childrens.get(0), left));
            stack.push(new WaitingStackElement(treeElement.childrens.get(1), right));
        }
        else if( vertex instanceof FuncConditionVertex ) {
            FuncConditionVertex asCondition = (FuncConditionVertex)vertex;

            FuncVertex condition = (FuncVertex)asCondition.out("condition").next();
            FuncVertex body = (FuncVertex)asCondition.out("body").next();

            treeElement.type = StringBuilderTreeElement.EnumType.BRANCH;
            treeElement.template = "if ({0}) {{1}}";
            treeElement.childrens = new ArrayList<>();
            treeElement.childrens.add(new StringBuilderTreeElement());
            treeElement.childrens.add(new StringBuilderTreeElement());

            stack.push(new WaitingStackElement(treeElement.childrens.get(0), condition));
            stack.push(new WaitingStackElement(treeElement.childrens.get(1), body));
        }
        else if( vertex instanceof FuncAssignmentVertex ) {
            FuncAssignmentVertex asAssignment = (FuncAssignmentVertex)vertex;

            FuncVertex left = (FuncVertex)asAssignment.out("left").next();
            FuncVertex right = (FuncVertex)asAssignment.out("right").next();

            treeElement.type = StringBuilderTreeElement.EnumType.BRANCH;
            treeElement.template = "{0} = {1};";
            treeElement.childrens = new ArrayList<>();
            treeElement.childrens.add(new StringBuilderTreeElement());
            treeElement.childrens.add(new StringBuilderTreeElement());

            stack.push(new WaitingStackElement(treeElement.childrens.get(0), left));
            stack.push(new WaitingStackElement(treeElement.childrens.get(1), right));
        }
        else if( vertex instanceof FuncFunctionCallVertex ) {
            FuncFunctionCallVertex asFunctionCall = (FuncFunctionCallVertex)vertex;

            Map<String, FuncVertex> bindingEdgeTargetMap = new HashMap<>();

            EdgeTraversal<?, ?, ?> edgeTraversal = asFunctionCall.outE("binding");

            for(;;) {
                if( !edgeTraversal.hasNext() ) {
                    break;
                }

                FuncValueBindingEdge currentEdge = (FuncValueBindingEdge)edgeTraversal.next();

                //Vertex x = (Vertex) currentEdge.outV().toList();

                // confusion up here >>>>
                bindingEdgeTargetMap.put(currentEdge.getSite(), (FuncVertex) currentEdge.getElement().getVertex(Direction.OUT));
            }

            List<String> listOfArgumentTemplates = new ArrayList<>();
            List<FuncVertex> childFuncVertices = new ArrayList<>();

            for( int parameterI = 0; parameterI < bindingEdgeTargetMap.size(); parameterI++ ) {
                listOfArgumentTemplates.add("{" + Integer.toString(parameterI) + "}");

                childFuncVertices.add(bindingEdgeTargetMap.get(Integer.toString(parameterI)));
            }

            final String parametersAsString = getCommaSeperated(listOfArgumentTemplates);

            treeElement.type = StringBuilderTreeElement.EnumType.BRANCH;
            treeElement.template = "(" + asFunctionCall.getName() + "(" + parametersAsString + "))";
            treeElement.childrens = new ArrayList<>();

            for( int parameterI = 0; parameterI < bindingEdgeTargetMap.size(); parameterI++ ) {
                treeElement.childrens.add(new StringBuilderTreeElement());

                stack.push(new WaitingStackElement(treeElement.childrens.get(parameterI), childFuncVertices.get(parameterI)));
            }
        }
        else if( vertex instanceof FuncConstantVertex ) {
            FuncConstantVertex asConstant = (FuncConstantVertex)vertex;

            treeElement.type = StringBuilderTreeElement.EnumType.LEAF;
            treeElement.template = asConstant.getValue();
        }
        else if( vertex instanceof FuncVariableVertex ) {
            FuncVariableVertex asVariable = (FuncVariableVertex)vertex;

            treeElement.type = StringBuilderTreeElement.EnumType.LEAF;
            treeElement.template = asVariable.getName();
        }
        else {
            throw new RuntimeException("Internal Error!");
        }
    }

    // TODO< replace this with non recursive version
    private static String generateStringFromTreeRecursive(StringBuilderTreeElement treeElement) {
        if( treeElement.type == StringBuilderTreeElement.EnumType.LEAF ) {
            return treeElement.template;
        }
        else {
            String templateInProcessing = treeElement.template;

            int childrenI = 0;

            for( StringBuilderTreeElement iterationChildren : treeElement.childrens ) {
                String recursiveResultOfChildren = generateStringFromTreeRecursive(iterationChildren);

                templateInProcessing = templateInProcessing.replace("{" + Integer.toString(childrenI) + "}", recursiveResultOfChildren);

                childrenI++;
            }

            return templateInProcessing;
        }
    }

    private static String getCommaSeperated(List<String> strings) {
        return StringUtils.join(strings, ',');
    }
}
