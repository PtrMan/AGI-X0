package ptrman.Datastructures;

import java.util.ArrayList;
import java.util.List;

public class TreeNode {
    public enum EnumType {
        LEAF,
        NOLEAF
    }
    public List<TreeNode> childNodes = new ArrayList<>();
    public EnumType type = EnumType.NOLEAF;

    public Variadic value;
    // can be null

    public boolean isLeaf() {
        return type != EnumType.NOLEAF;
    }

    public TreeNode deepCopy() {
        TreeNode result;
        result = new TreeNode();
        result.type = type;
        if (value != null)
        {
            result.value = value.deepCopy();
        }
         
        for (Object __dummyForeachVar0 : childNodes)
        {
            TreeNode iterationChild = (TreeNode)__dummyForeachVar0;
            result.childNodes.add(iterationChild.deepCopy());
        }
        return result;
    }

}


