namespace XIVHuntUtils.Exceptions;

public class InitializationException : Exception {
	public InitializationException(string message) : base(message) { }
	public InitializationException(string message, Exception cause) : base(message, cause) { }
}
