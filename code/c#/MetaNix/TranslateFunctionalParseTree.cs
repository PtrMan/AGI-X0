﻿using System;
using System.Collections.Generic;

namespace MetaNix {
    public class TranslateFunctionalParseTree {
        public static NodeRefererEntry translateRecursive(Functional.ParseTreeElement entry) {
            return new NodeRefererEntry(translateRecursiveInternal(entry, null));
        }

        public static ImmutableNodeReferer translateRecursiveInternal(Functional.ParseTreeElement entry, ImmutableNodeReferer parent) {
            if( entry.type == Functional.ParseTreeElement.EnumType.SCOPE ) {
                ImmutableNodeReferer resultNode = ImmutableNodeReferer.makeBranch();
                
                Functional.ScopeParseTreeElement castedEntry = (Functional.ScopeParseTreeElement)entry;
                foreach(Functional.ParseTreeElement iterationChildren in castedEntry.children) {
                    ImmutableNodeReferer translatedNode = translateRecursiveInternal(iterationChildren, resultNode);
                    translatedNode.parent = resultNode;
                    resultNode.children.Add(translatedNode);
                }
                
                return resultNode;
            }
            else if( entry.type == Functional.ParseTreeElement.EnumType.IDENTIFIER ) {
                Functional.IdentifierParseTreeElement castedEntry = (Functional.IdentifierParseTreeElement)entry;
                return ImmutableNodeRefererManipulatorHelper.makeString(castedEntry.identifier);
            }
            else if( entry.type == Functional.ParseTreeElement.EnumType.NUMBER ) {
                Functional.NumberParseTreeElement castedEntry = (Functional.NumberParseTreeElement)entry;
                switch(castedEntry.numberType) {
                    case Functional.NumberParseTreeElement.EnumNumberType.FLOAT:
                    return ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(Variant.makeFloat(castedEntry.valueFloat)));
                    case Functional.NumberParseTreeElement.EnumNumberType.INTEGER:
                    return ImmutableNodeReferer.makeNonbranch(ValueNode.makeAtomic(Variant.makeInt(castedEntry.valueInt)));
                }
                throw new Exception("Internal error!"); // hard internal error
            }
            else if( entry.type == Functional.ParseTreeElement.EnumType.ARRAY ) {
                // an array gets transated to an branch with array as the operation

                ImmutableNodeReferer resultNode = ImmutableNodeReferer.makeBranch();

                resultNode.children.Add(ImmutableNodeRefererManipulatorHelper.makeString("array", resultNode)); // pseudo operation "array" which indicates an array


                Functional.ArrayParseTreeElement castedEntry = (Functional.ArrayParseTreeElement)entry;
                foreach (Functional.ParseTreeElement iterationChildren in castedEntry.children) {
                    ImmutableNodeReferer translatedNode = translateRecursiveInternal(iterationChildren, resultNode);
                    translatedNode.parent = resultNode;
                    resultNode.children.Add(translatedNode);
                }
                
                return resultNode;
            }
            else {
                throw new Exception("Not supported parse node!");
            }
        }
    }
}