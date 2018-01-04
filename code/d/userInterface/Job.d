import std.stdio : writeln;

class Job {
	public string name;


	public bool completionTimeKnown;
	public double timeInSecondsTillCompletion;

	public UserMessage[] messagesToBePresentedToUser;

	public final void presentStatusToUser() {
		string statusSummary;

		string completionTimeString;
		if( completionTimeKnown ) {
			completionTimeString = format("%.3f", timeInSecondsTillCompletion);
		}
		else {
			completionTimeString = "[unknown]";
		}

		statusSummary = format("[job] job name=\"%s\", completion time=%s", name, completionTimeString);
		writeln(statusSummary);

		foreach( iterationUserMessage; messagesToBePresentedToUser ) {
			writeln(" ->", iterationUserMessage.humanMessage);
		}

		messagesToBePresentedToUser.length = 0;
	}
}

class UserMessage {
	string humanMessage;
}

