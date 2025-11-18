using System;

namespace PRN232_FA25_Assignment_G7.Services.Exceptions;

public class DuplicateUsernameException : Exception
{
    public DuplicateUsernameException()
    {
    }

    public DuplicateUsernameException(string message)
        : base(message)
    {
    }

    public DuplicateUsernameException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
