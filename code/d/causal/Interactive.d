import CausalCore;
import CausalDag;

import std.stdio;
import std.format : formattedRead, format;
import std.conv : to;

string prompt(string text) {
	write(text);
	string input = readln();
	return input[0..input.length-1]; // remove \r\n
}

const string PROMPTCOMMANDREQUEST = "cmd$causal#";
const string PROMPTCAUSALNORMALIZEDINPUT = "CN~";
const string PROMPTCAUSALLANGUAGEINPUT = "Clanguage~";

class MatchingErrorException : Exception {
	final this() {
		super("");
	}
}

struct Settings {
	uint limitRepetitionCount = 60000; // just for testing   60000
	uint startRepetitionCount = 10000; // just for testing 10000
	uint resetRepetitionCounter = 10;
}

// hold the context of the current user provided causal input
class UserInputCausalContext {
	size_t[string] nameMap; // map of names to the indices in the causal graph

	Dag *dag;

	final this() {
		dag = new Dag;
	}

	final void addCausalChain(string source, string target) {
		size_t sourceElementIndex = lookupOrAddElement(source);
		size_t targetElementIndex = lookupOrAddElement(target);

		// add the connection
		dag.elements[sourceElementIndex].childrenIndices ~= targetElementIndex;
	}

	final void addCausalChain(size_t sourceElementIndex, string target) {
		size_t targetElementIndex = lookupOrAddElement(target);

		// add the connection
		dag.elements[sourceElementIndex].childrenIndices ~= targetElementIndex;
	}

	final size_t addAnonymous() {
		size_t index = dag.elements.length;
		dag.elements ~= DagElement();
		return index;
	}

	final private size_t lookupOrAddElement(string name) {
		if( name in nameMap ) {
			return nameMap[name];
		}
		else {
			nameMap[name] = dag.elements.length;
			dag.elements ~= DagElement();
			return nameMap[name];
		}
	}
}

enum EnumInputMode {
	COMMAND,
	CAUSALNOMRALIZED, // input of causal connections
	CAUSALLANGUAGE, // language, NLP input
}

void main() {
	UserInputCausalContext userInputCausalContext = new UserInputCausalContext;

	Settings *settings = new Settings;

	EnumInputMode inputMode = EnumInputMode.COMMAND;

	for(;;) {
		if( inputMode == EnumInputMode.COMMAND ) {
			string commandInput = prompt(PROMPTCOMMANDREQUEST);

			string settingName, settingValue;
			uint numberOfMatches;

			// TODO< command to load text from file >

			if( commandInput == "causalNormalized" ) {
				inputMode = EnumInputMode.CAUSALNOMRALIZED;
				continue;
			}
			else if( commandInput == "causalLanguage") {
				inputMode = EnumInputMode.CAUSALLANGUAGE;
				continue;
			}
			else if( commandInput.length > 4 && commandInput[0..4] == "set " && (numberOfMatches = formattedRead(commandInput, "set %s=%s", &settingName, &settingValue)) == 2 ) {
				switch( settingName ) {
					case "limitRepetitionCount": settings.limitRepetitionCount = settingValue.to!uint; break;
					case "startRepetitionCount": settings.startRepetitionCount = settingValue.to!uint; break;
					case "resetRepetitionCounter": settings.resetRepetitionCounter = settingValue.to!uint; break;
					default:
					writeln("Error: unknown variable name!");
				}
			}
			else if( commandInput == "sampleResetCountingBatch(...)" ) { // user requested a number of runs of the energy/entropy minimization
				SampleWithStrategyBatchArguments sampleBatchArguments;
				sampleBatchArguments.dag = userInputCausalContext.dag;
				sampleBatchArguments.numberOfBatchSteps = 1000;
				sampleBatchArguments.sampleContext = new SampleContext;


				sampleWithResetCountingStrategyBatchSetup(sampleBatchArguments, settings.startRepetitionCount, settings.resetRepetitionCounter, settings.limitRepetitionCount);

				for(;;) {
					double resultEnergy;
					size_t[] resultSequence;
					bool terminated;

					writeln("running sampleResetCountingBatch(...) job with id=%s and name=%s".format("?", "<NULL>"));

					sampleWithResetCountingStrategyBatch(sampleBatchArguments, /*out*/ resultEnergy, /*out*/ resultSequence, /*out*/ terminated);
					if( terminated ) {
						uint repetitionCounter = sampleBatchArguments.sampleContext.value!(uint, "counter");

						writeln("terminated sampleResetCountingBatch(...) job with id=%s and name=%s".format("?", "<NULL>"));
						writeln("   result: sequence=%s resultEnergy=%s repetitionCounter=%s".format(resultSequence, resultEnergy, repetitionCounter));

						break;
					}
				}
			}
			else if( commandInput == "debug" ) {
				userInputCausalContext.dag.debugDag();
			}
			else {
				writeln("Error: Unknown command!");
			}
		}
		else if( inputMode == EnumInputMode.CAUSALNOMRALIZED ) {
			string causalInput = prompt(PROMPTCAUSALNORMALIZEDINPUT);

			if( causalInput.isEmpty ) {
				inputMode = EnumInputMode.COMMAND;
				continue;
			}

			try {
				string sourceName, targetName;
				{
					const uint numberOfMatches = formattedRead(causalInput, "%s->%s", &sourceName, &targetName);
					if( numberOfMatches != 2 ) {
						throw new MatchingErrorException();
					}

					userInputCausalContext.addCausalChain(sourceName, targetName);
					writeln("ok:%s->%s".format(sourceName, targetName));
				}

			}
			catch( MatchingErrorException e ) {
				writeln("Error: Parsing failed!");
			}

		}
		else if( inputMode == EnumInputMode.CAUSALLANGUAGE ) {
			string causalInput = prompt(PROMPTCAUSALLANGUAGEINPUT);

			if( causalInput.isEmpty ) {
				inputMode = EnumInputMode.COMMAND;
				continue;
			}

			foreach( i; 0..causalInput.length-1 ) {
				string
					nameLetter0 = causalInput[i].to!string,
					nameLetter1 = causalInput[i+1].to!string;

				size_t binder = userInputCausalContext.addAnonymous(); // we add an node nto the dag which is the binder, the binder connects the two letters without breaking the DAG invariant

				userInputCausalContext.addCausalChain(binder, nameLetter0);
				userInputCausalContext.addCausalChain(binder, nameLetter1);
			}

			writeln("ok");
		}
	}
}
