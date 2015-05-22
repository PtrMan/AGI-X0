using System;
using System.Collections.Generic;

namespace Datastructures
{
    class TreeNode
    {
        public enum EnumType
        {
            LEAF,
            NOLEAF
        }
        
        public List<TreeNode> childNodes = new List<TreeNode>();
        public EnumType type = EnumType.NOLEAF;
        public Variadic value; // can be null

        public bool isLeaf()
        {
            return type != EnumType.NOLEAF;
        }

        public TreeNode deepCopy()
        {
            TreeNode result;

            result = new TreeNode();
            result.type = type;

            if( value != null )
            {
                result.value = value.deepCopy();
            }
            
            foreach( TreeNode iterationChild in childNodes )
            {
                result.childNodes.Add(iterationChild.deepCopy());
            }

            return result;
        }
    }
}
