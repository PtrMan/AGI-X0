using System;
using System.Collections.Generic;

namespace MetaNix {
    public class TranslateFunctionalParseTree {
        public static Node translateRecursive(Functional.ParseTreeElement entry) {
            if( entry.type == Functional.ParseTreeElement.EnumType.SCOPE ) {
                Node resultNode = Node.makeBranch();

                IList<Node> children = new List<Node>();

                Functional.ScopeParseTreeElement castedEntry = (Functional.ScopeParseTreeElement)entry;
                foreach(Functional.ParseTreeElement iterationChildren in castedEntry.children) {
                    Node translatedNode = translateRecursive(iterationChildren);
                    translatedNode.parent = resultNode;
                    children.Add(translatedNode);
                }

                resultNode.children = new Node[children.Count];
                for(int i = 0; i < children.Count; i++) {
                    resultNode.children[i] = children[i];
                }

                return resultNode;
            }
            else if( entry.type == Functional.ParseTreeElement.EnumType.IDENTIFIER ) {
                Functional.IdentifierParseTreeElement castedEntry = (Functional.IdentifierParseTreeElement)entry;
                return NodeHelper.makeString(castedEntry.identifier);
            }
            else if( entry.type == Functional.ParseTreeElement.EnumType.NUMBER ) {
                Functional.NumberParseTreeElement castedEntry = (Functional.NumberParseTreeElement)entry;
                switch(castedEntry.numberType) {
                    case Functional.NumberParseTreeElement.EnumNumberType.FLOAT:
                    return Node.makeAtomic(Variant.makeFloat(castedEntry.valueFloat));
                    case Functional.NumberParseTreeElement.EnumNumberType.INTEGER:
                    return Node.makeAtomic(Variant.makeInt(castedEntry.valueInt));
                }
                throw new Exception("Internal error!"); // hard internal error
            }
            else if( entry.type == Functional.ParseTreeElement.EnumType.ARRAY ) {
                // an array gets transated to an branch with array as the operation

                Node resultNode = Node.makeBranch();

                IList<Node> children = new List<Node>();

                Functional.ArrayParseTreeElement castedEntry = (Functional.ArrayParseTreeElement)entry;
                foreach (Functional.ParseTreeElement iterationChildren in castedEntry.children) {
                    Node translatedNode = translateRecursive(iterationChildren);
                    translatedNode.parent = resultNode;
                    children.Add(translatedNode);
                }

                resultNode.children = new Node[1+children.Count];
                resultNode.children[0] = NodeHelper.makeString("array"); // pseudo operation "array" which indicates an array
                for (int i = 0; i < children.Count; i++) {
                    resultNode.children[1+i] = children[i];
                }

                return resultNode;
            }
            else {
                throw new Exception("Not supported parse node!");
            }
        }
    }
}
