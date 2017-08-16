# AGI-X0
Named for a lack of a name

Not that Utterly broken anymore.

The so called Development cycle cisists out of a cycle of 3 months hibernation followed by some activity spike (depressing indeed).


# Implemented

## Artificial Intelligence

### Self delimiting Neural Network (SLIM-RNN) invented by Jürgen Schmidhuber

[SLIM-RNN](https://github.com/PtrMan/AGI-X0/tree/master/code/c%23/MetaNixCore/schmidhuber/slimRnn)

* Turing complete RNN

## Artificial General Intelligence'ish

### Powerplay

[Powerplay](https://github.com/PtrMan/AGI-X0/tree/master/code/c%23/MetaNixCore/schmidhuber/powerplay) invented by Jürgen Schmidhuber.

* RL framework for agents and components

### ALS
[Adaptive Levin Search](https://github.com/PtrMan/AGI-X0/blob/master/code/c%23/MetaNixCore/search/levin2/Levin2.cs)

* with a VM for simple Operations
* Propabilities of instructions are learned

### "Pattern" based representation and matching

[Pattern](https://github.com/PtrMan/AGI-X0/tree/master/code/c%23/MetaNixCore/framework/pattern)

* Uniform way to store and manipulate structural data
* Built with explicit semi functional constructs for simple automated generation and dynamic rewriting of code
* Inspired by the representations and matching mechanisms in Ikon FLux 2.0 and replicode

## Low level

### x86 representation and superoptimizer

[Superoptimizer](https://github.com/PtrMan/AGI-X0/blob/master/code/c%23/MetaNixCore/framework/super/optimization/SuperOptimizerExperiment.cs)

* based on levin search
* explicit x86 representation to allow for later translation/optimization of generated code to x86 assembler
* **Warning** : takes a lot of computational effort to enumerate the shortest/best version
* **Warning** : Immature, only a few (interesting) instructions implemented

# TODO
* CausalReasoningSystem 
* pull in more code
* pull in code/write code for the various NN types + hofstadter networks
