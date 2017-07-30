namespace AiThisAndThat.lowlevel.generation {
    public class AssemblyOperand {
	    public enum EnumType {
		    XMM,
		    MEMORYSIMPLE, // base register + offset
		    IMMEDIATE,
		    GENERALREGISTER
	    }

	    public enum EnumRegister {
		    EAX = 0,
		    EBX,
		    ECX,
		    EDX
		    // TODO
	    }
	
	    static private string[] registersStrings = new string[]{"EAX", "EBX", "ECX", "EDX" };

	    public static AssemblyOperand createMemorySimpleOperand(EnumRegister register, int offset) {
		    AssemblyOperand result = new AssemblyOperand();

		    result.type = EnumType.MEMORYSIMPLE;
		    result.register = register;
		    result.offset = offset;

		    return result;
	    }

	    public static AssemblyOperand createXmmRegister(uint xmmRegister) {
		    AssemblyOperand result = new AssemblyOperand();

		    result.type = EnumType.XMM;
		    result.xmmRegister = xmmRegister;

		    return result;
	    }
	
	    public static AssemblyOperand createImmediate(int value, uint width) {
		    AssemblyOperand result = new AssemblyOperand();
		
		    result.type = EnumType.IMMEDIATE;
		    result.immediateValue = value;
		    result.immediateWidth = width;
		
		    return result;
	    }
	
	    public static AssemblyOperand createGeneralPurpose(uint register) {
		    AssemblyOperand result = new AssemblyOperand();

		    result.type = EnumType.GENERALREGISTER;
		    result.generalPurposeRegister = register;

		    return result;
	    }
	    
        /* commented because it has to be translated to C# from D
	    public string getAsString()
	    {
		    if( this.type == EnumType.XMM )
		    {
			    return "XMM" ~ convertTo!(string)(this.xmmRegister);
		    }
		    else if( this.type == EnumType.MEMORYSIMPLE )
		    {
			    return "[" ~ registersStrings[cast(uint)this.register] ~ "+" ~ convertTo!(string)(this.offset) ~ "]";
		    }
		    else if( this.type == EnumType.IMMEDIATE )
		    {
			    return convertTo!(string)(this.immediateValue);
		    }
		    else if( this.type == EnumType.GENERALREGISTER )
		    {
			    return "GPR" ~ convertTo!(string)(this.generalPurposeRegister);
		    }
		    else
		    {
			    assert(false, "Unreachable!");
		    }
	    }
        */

	    public EnumType type;
	
	    // union
	    public uint xmmRegister;
	    public int immediateValue;
	    public EnumRegister register;
	    public uint generalPurposeRegister;

	    public int offset;
	
	    public uint immediateWidth;
    }

}
