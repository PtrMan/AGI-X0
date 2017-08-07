reasoning

Reasoning will be accomplished by an approximation of solomonov induction and inference with the causal algorithm.
This will be highlighted based on examples.

Approximation of solomonoff induction
----

Before the information is fed to deduction it gets compressed, so the induction has to do far less work.

The actual induction is done by pattern matching with enumeration bitsignatures in fixed intervals and variations of these patterns.
This can be suplemented by a search for turing machines which produce the bitpattern and can be used to continue it.

causal inference algorithm
----

see < TODO link >


candidate: input compressor
----

compresses the visual input to simple commands which losslessly describe the raw input under AIKR.






Working example #1
----

Task: find best action after seeing frames of a tetris game

The compressed representation of the screen, together with the actions up to a past time horizon is fed to the solomonoff induction.
Induction should be able to predict the next action to take.

If this doesn't work (and the agent doesn't get reward) the causal inference algorithm and the genetic algorithm work together.
The task of the genetic algorithm is to find a good representation and interpretation for the causal algorithm to work with the data to get a high reward.
The structur which is fed into the causal algorithm has to include the past reward signals somehow.

If a good strategy (representation and interpretation) for the causal algorithm is found it takes control over the agent.


Working example #2
---
