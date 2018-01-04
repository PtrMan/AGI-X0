module lang.ParserException;

class ParserException : Exception {
	final this(string message) {
		super("ParserException: " ~ message);
	}
}
