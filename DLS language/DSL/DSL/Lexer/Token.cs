namespace DSL.Lexer;

public class Token
{
    public TokenType Type { get; }
    public string Lexeme { get; }
    public int Line { get; }

    public Token(TokenType type, string lexeme, int line)
    {
        Type = type;
        Lexeme = lexeme;
        Line = line;
    }

    public override string ToString() => $"{Type} '{Lexeme}'";
}
