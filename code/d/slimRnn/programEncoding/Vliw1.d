module slimRnn.programEncoding.Vliw1;

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
SlimRnnStackBasedManipulationInstruction[] decodeInstruction(uint instruction, out bool invalidEncoding) {
	invalidEncoding = true;

	uint data1 = (instruction >> 3) & 0x1f;
	uint data2 = (instruction >> (5+3)) & 0xf;
	uint encodingIndex = instruction & 7;
	if( encodingIndex >= PRIMARYINSTRUCTIONENCODING.length ) {
		return [];
	}

	return convertPrimaryEncodingToInstructions(PRIMARYINSTRUCTIONENCODING[encodingIndex], data1, data2, /*out*/ invalidEncoding);
}

private SlimRnnStackBasedManipulationInstruction[] convertPrimaryEncodingToInstructions(bool[12] encoding, uint data1, uint data2, out bool invalidEncoding) {
	SlimRnnStackBasedManipulationInstruction[] resultInstructions;
	if( encoding[0] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makePush(data1);
	}
	if( encoding[1] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makePop();
	}
	if( encoding[2] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makePop();
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makePop();
	}

	if( encoding[3] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetSwitchboardIndexForInputIndexAndPieceAtStackTop(0, 0);
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetSwitchboardIndexForInputIndexAndPieceAtStackTop(1, 1);
	}
	
	if( encoding[4] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetActivateForPieceActivationVariableAtStackTop(true);
	}
	if( encoding[5] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetActivateForPieceActivationVariableAtStackTop(false);
	}

	if( encoding[6] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(0);
	}
	if( encoding[7] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(1);
	}
	if( encoding[8] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(2);
	}
	if( encoding[9] ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetTypeVariableForPieceAtStackTop(3);
	}

	if( encoding[10] ) {
		uint inputIndex = (((data2 >> 3) & 1) == 0) ? 0 : 1;
		float threshold = cast(float)(data2 & (8-1)) / cast(float)(8-1);
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetThresholdForInputIndexForPieceAtStackTop(inputIndex, threshold);
	}

	if( encoding[11] ) {
		// TODO< interpret after table >
		resultInstructions ~= interpretAfterTable2(data1, data2, /*out*/invalidEncoding);
	}

	return resultInstructions;
}

// instructions which get executed in order from left to right
private const bool[12][] PRIMARYINSTRUCTIONENCODING = [

// push data1 | pop     | pop2     | rewire input#0 to index 0 and input#1 to index 1 for neuron at index stack(top)|  set activation variable to activate neuron at index stack(top) | set activation variable to deactivate neuron at index stack(top) | set type for neuron at stack(#top) to 0 | set type for neuron at stack(#top) to 1 | set type for neuron at stack(#top) to 2 | set type for neuron at stack(#top) to 3 | set threshold for piece index #stack(top) for input data2 at bit #3 to data2[0..2] | interpret data2 after 2nd table
[      false,      false,     false,      false,                                                                           false,                                                            false,                                                            false,                                    false,                                    false,                                    false,                               /*x*/ true,                                                                               false,                     ],
[      false,      false,     false,      false,                                                                           false,                                                            false,                                                            false,                                    false,                                    false,                                    false,                                     false,                                                                         /*x*/true,                      ],

[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                       /*x*/true,                                     false,                                    false,                                    false,                               /*x*/ true,                                                                               false,                     ],
[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                            false,                               /*x*/true,                                     false,                                    false,                               /*x*/ true,                                                                               false,                     ],
[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                            false,                                    false,                               /*x*/true,                                     false,                               /*x*/ true,                                                                               false,                     ],
[  /*x*/true,      false,     false, /*x*/true,                                                                       /*x*/true,                                                             false,                                                            false,                                    false,                                    false,                               /*x*/true,                                /*x*/ true,                                                                               false,                     ],
];



//table 2: interpretation for data2
// 0000 | set output strength of piece stack(top) to data1
// 0001 | interpret data table 3
// 0010 | set function type of neuron to data1
// 0011 | set output index of piece stack(top) to data1

// --- TODO: other instruction
private SlimRnnStackBasedManipulationInstruction[] interpretAfterTable2(uint data1, uint data2, out bool invalidEncoding) {
	invalidEncoding = true;
	if( data2 == 0 ) {
		invalidEncoding = false;
		float outputStrength = cast(float)(data1) / cast(float)((1 << 5) - 1);
		return [SlimRnnStackBasedManipulationInstruction.makeSetOutputStrengthForPieceAtStackTop(outputStrength)];
	}
	else if( data2 == 1 ) {
		return interpretAfterTable3(data1, /*out*/invalidEncoding);
	}
	else if( data2 == 2 ) {
		invalidEncoding = false;
		return  [SlimRnnStackBasedManipulationInstruction.makeSetFunctionTypeForPieceAtStackTop(data1)];
	}
	else if( data2 == 3 ) {
		invalidEncoding = false;
		return  [SlimRnnStackBasedManipulationInstruction.makeSetSwitchboardIndexForOutputForPieceAtStackTop(data1)];
	}
	else {
		return [];
	}
}

/*
table 3: interpretation for data
* interpret data as bit pattern
  vvvrp
  - vvv : 0 : nop
          1 : reset neuron activation
          2 : set neuron activation to deactivate neuron
  - r: reset set type variable
  - p: pop
*/
private SlimRnnStackBasedManipulationInstruction[] interpretAfterTable3(uint data1, out bool invalidEncoding) {
	invalidEncoding = true;

	SlimRnnStackBasedManipulationInstruction[] resultInstructions;

	uint vvv = (data1 >> 2) & 7;
	bool r = ((data1 >> 1) & 1) != 0;
	bool p = (data1 & 1) != 0;

	if( vvv == 0 ) {
		// nop;
	}
	else if(vvv == 1) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeResetNeuronActivation();
	}
	else if(vvv == 2) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makeSetActivateForPieceActivationVariableAtStackTop(false);
	}
	else {
		// invalid encoding
		return [];
	}

	if( p ) {
		resultInstructions ~= SlimRnnStackBasedManipulationInstruction.makePop();
	}

	invalidEncoding = false;
	return resultInstructions;
}
