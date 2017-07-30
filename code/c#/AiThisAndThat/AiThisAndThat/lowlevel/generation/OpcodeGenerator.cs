using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace AiThisAndThat.lowlevel.generation {

    // see my "Smiley" project (which is not public)
    public class OpcodeGenerator {

        public enum EnumConditionalJumpType {
		    JUMPEQUAL,
	    }

	    public enum EnumCodeGenerationHint {
		    DUMMY,
	    }

	    private enum EnumOffsetType {
		    NOOFFSET,
		    SINGLEBYTE,
		    FOURBYTES,
	    }





        
	    private enum EnumMathOperationType {
		    ADD,
		    SUB,
	    }

        // TODO< other widths and combinations >
	    static private IList<byte> generateAddOrSubInstruction(EnumMathOperationType type, AssemblyOperand operandLeft, AssemblyOperand operandRight, bool in64BitMode, uint wishedImmediateLength, EnumCodeGenerationHint[] hints, out bool calleeSuccess) {
		    calleeSuccess = false;
		
		    if( operandRight.type == AssemblyOperand.EnumType.IMMEDIATE && operandRight.immediateWidth == 1 ) {
			    AssemblyOperand noOperand = new AssemblyOperand();
			    int rmType;
			
			    if( type == EnumMathOperationType.ADD ) rmType = 0;
			    else if( type == EnumMathOperationType.SUB ) rmType = 5;
			    else {
				    throw new Exception("Internal error");
			    }
			
			    IList<byte> rmByteAndAddressCalculation = generateRmByteAndAddressCalculation(noOperand, rmType, operandLeft, in64BitMode, hints, out calleeSuccess);
			    if( !calleeSuccess ) {
				    return new List<byte>();
			    }
			
			    calleeSuccess = true;
			
			    // TODO< what to do about negative values? >
			    List<byte> result = new List<byte>();
                result.Add((byte)0x83);
                result.AddRange(rmByteAndAddressCalculation);
                result.Add((byte)operandRight.immediateValue);
                return result;
		    }
		    else if( operandLeft.type == AssemblyOperand.EnumType.GENERALREGISTER ) {
			    IList<byte> rmByteAndAddressCalculation = generateRmByteAndAddressCalculation(operandLeft, -1, operandRight, in64BitMode, hints, out calleeSuccess);
                
			    if( !calleeSuccess ) {
				    return new List<byte>();
			    }
			
			    calleeSuccess = true;
			    
                List<byte> result = new List<byte>();
                
			    switch(type) {
				    case EnumMathOperationType.ADD:
                    result.Add(0x03);
                    result.AddRange(rmByteAndAddressCalculation);
                    return result;
				
				    case EnumMathOperationType.SUB:
                    result.Add(0x2B);
                    result.AddRange(rmByteAndAddressCalculation);
                    return result;
			    }
			    
			    throw new Exception("Unreachable");
		    }
		    else {
			    // TODO
			    throw new Exception("interal error");
		    }
		
		    // TODO
		    return new List<byte>();
	    }

        // TODO< remove the code generation hints >
	    static private IList<byte> generateRmByteAndAddressCalculation(AssemblyOperand operandLeft, int defaultReg, AssemblyOperand operandRight, bool in64BitMode, IEnumerable<EnumCodeGenerationHint> hints, out bool calleeSuccess) {
		    calleeSuccess = false;
            
            List<byte> result = new List<byte>();
		    bool sibByteRequired;
		    EnumOffsetType offsetType;

		    result = new List<byte>(){generateRmByteAndGetOffsetType(operandLeft, defaultReg, operandRight, in64BitMode, out offsetType, out calleeSuccess) };
		    if( !calleeSuccess ) {
			    return new List<byte>();
		    }

		    sibByteRequired = isSibByteRequired(operandRight, hints);
		    if( sibByteRequired ) {
			    // TODO
                throw new NotImplementedException("TODO");
		    }

		    result.AddRange(generateOffset(operandRight, offsetType, out calleeSuccess));
		    if( !calleeSuccess ) {
			    return new List<byte>();
		    }

		    calleeSuccess = true;
		    return result;
	    }

	    static private byte generateRmByteAndGetOffsetType(AssemblyOperand operandLeft, int defaultReg, AssemblyOperand operandRight, bool in64BitMode, out EnumOffsetType offsetType, out bool calleeSuccess) {
		    uint registerMaxIndex;
		    uint reg, mod, rm;

		    offsetType = EnumOffsetType.NOOFFSET;
		    calleeSuccess = false;

		    if( in64BitMode ) registerMaxIndex = 16;
		    else registerMaxIndex = 8;
		
		    if( defaultReg == -1 ) {
			    if( operandLeft.type == AssemblyOperand.EnumType.XMM && operandLeft.xmmRegister <= registerMaxIndex ) {
				    reg = operandLeft.xmmRegister;
			    }
			    else if( operandLeft.type == AssemblyOperand.EnumType.GENERALREGISTER && operandLeft.generalPurposeRegister <= registerMaxIndex ) {
				    reg = operandLeft.generalPurposeRegister;
			    }
			    else {
				    return 0;
			    }
		    }
		    else {
                Debug.Assert(defaultReg >= 0);
			    reg = (uint)defaultReg;
		    }
			
		    if( operandRight.type == AssemblyOperand.EnumType.XMM ) {
			    if( operandRight.xmmRegister >= registerMaxIndex ) {
				    return 0;
			    }

			    mod = 3;
			    rm = operandRight.xmmRegister;
		    }
		    else if( operandRight.type == AssemblyOperand.EnumType.GENERALREGISTER ) {
			    if( operandRight.generalPurposeRegister >= registerMaxIndex ) return 0;

			    mod = 3;
			    rm = operandRight.generalPurposeRegister;
		    }
		    else if( operandRight.type == AssemblyOperand.EnumType.MEMORYSIMPLE ) {
			    if( operandRight.offset == 0 ) offsetType = EnumOffsetType.NOOFFSET;
			    else if( operandRight.offset <= 127 && operandRight.offset >= -128 ) offsetType = EnumOffsetType.SINGLEBYTE;
			    else offsetType = EnumOffsetType.FOURBYTES;

			    // TODO< force offset with hints if possible >


			    if( offsetType == EnumOffsetType.NOOFFSET ) mod = 0;
			    else if( offsetType == EnumOffsetType.SINGLEBYTE ) mod = 1;
			    else if( offsetType == EnumOffsetType.FOURBYTES ) mod = 2;
			    else {
                    throw new Exception("Unreachable!");
			    }


			    if( operandRight.register == AssemblyOperand.EnumRegister.EAX ) rm = 0;
			    else if( operandRight.register == AssemblyOperand.EnumRegister.EBX ) rm = 3;
			    else if( operandRight.register == AssemblyOperand.EnumRegister.ECX ) rm = 1;
			    else if( operandRight.register == AssemblyOperand.EnumRegister.EDX ) rm = 2;
			    else {
				    // possible todo or unimplemented
				    throw new Exception("Internal error!");
			    }
		    }
		    else {
			    // unimplemented, todo, internal error
			    throw new Exception("Internal error!");
		    }

		    calleeSuccess = true;
		    return (byte)(((mod & 3) << 6) | ((reg & 7) << 3) | (rm & 7));
	    }

        
	    static private bool isSibByteRequired(AssemblyOperand operandLeft, IEnumerable<EnumCodeGenerationHint> hints) {
		    // TODO< hints >

		    // TODO
		    return false;
	    }

        
	    static private IList<byte> generateOffset(AssemblyOperand operand, EnumOffsetType offsetType, out bool calleeSuccess) {
		    IList<byte> result = new List<byte>();
            
		    uint rm;

		    calleeSuccess = false;
		
		    if( operand.type == AssemblyOperand.EnumType.MEMORYSIMPLE ) {
			    if( offsetType == EnumOffsetType.NOOFFSET ) {
				    result = new List<byte>();
			    }
			    else if( offsetType == EnumOffsetType.SINGLEBYTE ) {
				    result = new List<byte>(){(byte)operand.offset};
			    }
			    else if( offsetType == EnumOffsetType.FOURBYTES ) {
                    /* commented because not translated from D
				    ubyte *pointerToOffset;

				    pointerToOffset = cast(ubyte*)&operand.offset;

				    result = [pointerToOffset[0], pointerToOffset[1], pointerToOffset[2], pointerToOffset[3]];

				    assert(false, "TODO");
                    */
                    throw new NotImplementedException("TODO");
			    }
			

			    calleeSuccess = true;
			    return result;
		    }
		    else if( operand.type == AssemblyOperand.EnumType.XMM || operand.type == AssemblyOperand.EnumType.GENERALREGISTER ) {
			    // we dont need any offset for an xmm or a general register
			    calleeSuccess = true;
			    return new List<byte>();
		    }
		    else {
			    return new List<byte>();
		    }

		    throw new Exception("Unreachable!");
	    }

    }
}
