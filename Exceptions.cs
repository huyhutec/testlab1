using System;

namespace MangaReader2026.DomainCommon;

public class NetworkException : Exception
{
    public NetworkException(string message) : base(message) { }
}

public class ParseException : Exception { }