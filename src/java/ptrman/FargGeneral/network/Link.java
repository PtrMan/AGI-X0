package ptrman.FargGeneral.network;

public class Link
{
    public Link(EnumType type)
    {
        this.type = type;
    }
    
    public float strength; // [0.0, 1.0]
    public Node target;
    
    //public Node source; // can be null for unidirectional edges
        
    public Node[] hyperNodes;
    
    /*public boolean isBidirectional()
    {
        return this.source != null;
    }*/
    
    public EnumType type;
    
    public enum EnumType
    {
        ISPARTOF("ISPARTOF"),
        CONTAINS("CONTAINS"),
        HASFEATURE("HASFEATURE"), // used for (platonic) features of platonic primitives in ltm
        HASATTRIBUTE("HASATTRIBUTE"),
        ISA("ISA"), // "is a"
        HASNODE("HASNODE"); // inverse of ISPARTOF, weaker than HAS
        
        private String string;
        
        private EnumType(String name){string = name;}

        public String toString() {
            return string;
        }
    }
}
