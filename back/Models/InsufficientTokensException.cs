namespace back.Models;

public class InsufficientTokensException : Exception
{
    public InsufficientTokensException()
        : base("You have used all your tokens for today. To get more tokens, please upgrade your plan or wait until tomorrow.") { }
}
