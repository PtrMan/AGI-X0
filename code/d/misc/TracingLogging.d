module misc.TracingLogging;

class Tracer {
	enum EnumVerbose : bool {
		NO,
		YES,
	}

	// sends an internal event to an eventstore or logs it or whatever
	final void internalEvent(PayloadType)(string humanreadableDescription, PayloadType payload, string sourceFunction, uint sourceLine, EnumVerbose verbose = EnumVerbose.NO) {
		// TODO< send to eventstore if the configuration is set this way >

		if( verbose == EnumVerbose.YES ) {
			import std.format : format;
			reportError(EnumErrorType.NONCRITICAL, format("-verbose %s line %s : %s", sourceFunction, sourceLine, humanreadableDescription));
		}
	}




	protected enum EnumErrorType {
		NONCRITICAL
	}

	protected final void reportError(EnumErrorType errorType, string message) {
		import std.stdio : writeln;
		writeln("[ERROR] noncritical: ", message);
	}
}

