module misc.TracingLogging;

interface IReport {
	enum EnumErrorType {
		NONCRITICAL
	}

	void reportError(EnumErrorType errorType, string message);

	void report(string prefix, string message);
}

class Tracer {
	enum EnumVerbose : bool {
		NO,
		YES,
	}

	final this(IReport report) {
		this.report = report;
	}

	// sends an internal event to an eventstore or logs it or whatever
	final void internalEvent(PayloadType)(string humanreadableDescription, PayloadType payload, string sourceFunction, uint sourceLine, EnumVerbose verbose = EnumVerbose.NO) {
		// TODO< send to eventstore if the configuration is set this way >

		if( verbose == EnumVerbose.YES ) {
			import std.format : format;
			report.reportError(IReport.EnumErrorType.NONCRITICAL, format("-verbose %s line %s : %s", sourceFunction, sourceLine, humanreadableDescription));
		}
	}

	protected IReport report;
}

