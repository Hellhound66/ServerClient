namespace Server.Exceptions;

internal class InvalidNetworkMessageException(string errorMessage) : Exception(errorMessage);