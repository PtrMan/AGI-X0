module slimRnn.programEncoding.Vliw1;

import memoryLowlevel.StackAllocator;

import slimRnn.SlimRnnStackBasedManipulationInstruction;

// encoding of the instructions to come up with useful programs quickly
// highly biased

// specialized to manipulate  32 neurons (5 bits)

// the encoding is used as instructions for the enumeration of programs with levin search.
// the programs manipulate the SLIM-RNN

// encoded as 12 bits
// data2(4 bit) data(5 bit) 
// followed by 3 bits for the instruction (lowest)

// gets as argument the instruction (usually from levin search), which is 12 bits wide and converts it to zero or more instruction which operate on the SLIM-RNN and manipulate the network
void decodeInstruction(uint instruction, uint delegate(uint index) translateTypeIndexOfPieceToType, ref StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) resultInstructionStack, out bool invalidEncoding) {
	invalidEncoding = true;

	uint data1 = (instruction >> 3) & 0x1f;
	uint data2 = (instruction >> (5+3)) & 0xf;
	uint encodingIndex = instruction & 7;
	if( encodingIndex >= PRIMARYINSTRUCTIONENCODING.length ) {
		return;
	}

	return convertPrimaryEncodingToInstructions(PRIMARYINSTRUCTIONENCODING[encodingIndex], data1, data2, translateTypeIndexOfPieceToType, resultInstructionStack, /*out*/ invalidEncoding);
}

private void convertPrimaryEncodingToInstructions(bool[12] encoding, uint data1, uint data2, uint delegate(uint index) translateTypeIndexOfPieceToType, ref StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) resultInstructionStack, out bool invalidEncoding) {
	bool appendSuccess; // returned by the stack allocator, we throw it away

	if( encoding[0] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makePush(data1), /*out*/appendSuccess);
	}
	if( encoding[1] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makePop(), /*out*/appendSuccess);
	}
	if( encoding[2] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makePop(), /*out*/appendSuccess);
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makePop(), /*out*/appendSuccess);
	}

	if( encoding[3] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetSwitchboardIndexForInputIndexAndPieceAtStackTop(0, 0), /*out*/appendSuccess);
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetSwitchboardIndexForInputIndexAndPieceAtStackTop(1, 1), /*out*/appendSuccess);
	}
	
	if( encoding[4] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetActivateForPieceActivationVariableAtStackTop(true), /*out*/appendSuccess);
	}
	if( encoding[5] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetActivateForPieceActivationVariableAtStackTop(false), /*out*/appendSuccess);
	}

	if( encoding[6] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(translateTypeIndexOfPieceToType(0)), /*out*/appendSuccess);
	}
	if( encoding[7] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(translateTypeIndexOfPieceToType(1)), /*out*/appendSuccess);
	}
	if( encoding[8] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(translateTypeIndexOfPieceToType(2)), /*out*/appendSuccess);
	}
	if( encoding[9] ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(translateTypeIndexOfPieceToType(3)), /*out*/appendSuccess);
	}

	if( encoding[10] ) {
		uint inputIndex = (((data2 >> 3) & 1) == 0) ? 0 : 1;
		float threshold = cast(float)(data2 & (8-1)) / cast(float)(8-1);
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetThresholdForInputIndexForPieceAtStackTop(inputIndex, threshold), /*out*/appendSuccess);
	}

	if( encoding[11] ) {
		interpretAfterTable2(data1, data2, resultInstructionStack, /*out*/invalidEncoding);
	}
}

// instructions which get executed in order from left to right
private const bool[12][] PRIMARYINSTRUCTIONENCODING = [

// TODO< push next free  neuron index (as combined operation) >
//       this should generalize by far bettern than pushing a constant


// push data1 | pop     | pop2     | rewire input#0 to index 0 and input#1 to index 1 for neuron at index stack(top)|  set activation variable to activate neuron at index stack(top) | set activation variable to deactivate neuron at index stack(top) | set type for neuron at stack(#top) to 0 | set type for neuron at stack(#top) to 1 | set type for neuron at stack(#top) to 2 | set type for neuron at stack(#top) to 3 | set threshold for piece index #stack(top) for input data2 at bit #3 to data2[0..2] | interpret data2 after 2nd table
[      false,      false,     false,      false,                                                                           false,                                                            false,                                                            false,                                    false,                                    false,                                    false,                               /*x*/ true,                                                                               false,                     ],
[      false,      false,     false,      false,                                                                           false,                                                            false,                                                            false,                                    false,                                    false,                                    false,                                     false,                                                                         /*x*/true,                      ],

[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                       /*x*/true,                                     false,                                    false,                                    false,                               /*x*/ true,                                                                               false,                     ],
[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                            false,                               /*x*/true,                                     false,                                    false,                               /*x*/ true,                                                                               false,                     ],
[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                            false,                                    false,                               /*x*/true,                                     false,                               /*x*/ true,                                                                               false,                     ],
[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                            false,                                    false,                                    false,                               /*x*/true,                                /*x*/ true,                                                                               false,                     ],
];

// TODO instructions
//      - // TODO : set input#data[4..3] to data[0..2]                this works just for up to 8 switchboard elements!   TODO< find better solution to this >
//      -           
// 0010 | set type of neuron to data1  and activate


//table 2: interpretation for data2
// 0000 | set output strength of piece stack(top) to data1
// 0001 | interpret data table 3
// 0010 | set type of neuron to data1
// 0011 | set output index of piece stack(top) to data1

// TODO : set input#data[4..3] to data[0..2]                this works just for up to 8 switchboard elements!   TODO< find better solution to this >
// TODO : put neuron stack(top) into WTA-group data1

// --- TODO: other instruction
private void interpretAfterTable2(uint data1, uint data2, ref StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) resultInstructionStack, out bool invalidEncoding) {
	bool appendSuccess; // returned by the stack allocator, we throw it away

	invalidEncoding = true;
	if( data2 == 0 ) {
		invalidEncoding = false;
		float outputStrength = cast(float)(data1) / cast(float)((1 << 5) - 1);
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetOutputStrengthForPieceAtStackTop(outputStrength), /*out*/appendSuccess);
	}
	else if( data2 == 1 ) {
		interpretAfterTable3(data1, resultInstructionStack, /*out*/invalidEncoding);
	}
	else if( data2 == 2 ) {
		invalidEncoding = false;
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(data1), /*out*/appendSuccess);
	}
	else if( data2 == 3 ) {
		invalidEncoding = false;
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetSwitchboardIndexForOutputForPieceAtStackTop(data1), /*out*/appendSuccess);
	}
	else {
		return;
	}
}

/*
table 3: interpretation for data
* interpret data as bit pattern
  vvvrp
  - vvv : 0 : nop
          1 : reset neuron activation
          2 : set neuron activation to deactivate neuron
          3 : TODO : switch pieces at stack(top) and stack(top-1)
  - r: reset set type variable
  - p: pop
*/
private void interpretAfterTable3(uint data1, ref StackAllocator!(8, SlimRnnStackBasedManipulationInstruction) resultInstructionStack, out bool invalidEncoding) {
	bool appendSuccess; // returned by the stack allocator, we throw it away

	invalidEncoding = true;

	uint vvv = (data1 >> 2) & 7;
	bool r = ((data1 >> 1) & 1) != 0;
	bool p = (data1 & 1) != 0;

	if( vvv == 0 ) {
		// nop;
	}
	else if(vvv == 1) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeResetNeuronActivation(), /*out*/appendSuccess);
	}
	else if(vvv == 2) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makeSetActivateForPieceActivationVariableAtStackTop(false), /*out*/appendSuccess);
	}
	else {
		// invalid encoding
		return;
	}

	if( p ) {
		resultInstructionStack.append(SlimRnnStackBasedManipulationInstruction.makePop(), /*out*/appendSuccess);
	}

	invalidEncoding = false;
	return;
}
