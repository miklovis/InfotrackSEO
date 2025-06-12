namespace ScrapingGoogle.Exceptions;

public class GoogleSearchException : Exception
{
    public GoogleSearchException(string message) : base(message) { }

    public GoogleSearchException(string message, Exception inner)
        : base(message, inner) { }
}