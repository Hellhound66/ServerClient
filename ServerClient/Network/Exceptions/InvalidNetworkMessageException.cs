namespace Messages.Exceptions;

internal class InvalidNetworkMessageException(string errorMessage) : Exception(errorMessage);