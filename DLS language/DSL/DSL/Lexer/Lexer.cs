using System;
using System.Collections.Generic;

namespace DSL.Lexer;

public class Lexer
{
    private readonly string _source;

    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private readonly List<Token> _tokens = new();

    public Lexer(string source)
    {
        _source = source;
    }

    public List<Token> Tokenize()
    {
        while (!IsAtEnd())
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.EndOfFile, "", _line));
        return _tokens;
    }

    private bool IsAtEnd() => _current >= _source.Length;

    private char Advance() => _source[_current++];

    private char Peek() => IsAtEnd() ? '\0' : _source[_current];

    private bool Match(char expected)
    {
        if (IsAtEnd()) return false;
        if (_source[_current] != expected) return false;

        _current++;
        return true;
    }

    private void AddToken(TokenType type)
    {
        string text = _source.Substring(_start, _current - _start);
        _tokens.Add(new Token(type, text, _line));
    }

    private void ScanToken()
    {
        char c = Advance();

        switch (c)
        {
            case '{': AddToken(TokenType.LeftBrace); break;
            case '}': AddToken(TokenType.RightBrace); break;

            case ' ':
            case '\r':
            case '\t':
                break;

            case '\n':
                _line++;
                break;

            default:
                if (char.IsLetter(c))
                {
                    Identifier();
                }
                else
                {
                    // Unknown character for now
                }
                break;
        }
    }

    private void Identifier()
    {
        while (
            char.IsLetterOrDigit(Peek()) ||
             Peek() == '.' ||
             Peek() == '/' ||
             Peek() == '\\' ||
             Peek() == '*' ||
             Peek() == '?' ||
             Peek() == '!' ||
             Peek() == '[' ||
             Peek() == ']' ||
             Peek() == '<' ||
             Peek() == '>' ||
             Peek() == '-')
        {
            Advance();
        }



        string text = _source.Substring(_start, _current - _start);

        TokenType type = text switch
        {
            "project" => TokenType.Project,
            "folder" => TokenType.Folder,
            "file" => TokenType.File,
            "clean" => TokenType.Clean,
            "remove" => TokenType.Remove,
            "add" => TokenType.Add,
            "forbid" => TokenType.Forbid,
            "search" => TokenType.Search,
            "exclude" => TokenType.Exclude,
            "ignore" => TokenType.Ignore,
            "include" => TokenType.Include,
            "all" => TokenType.All,
            "make" => TokenType.Make,
            "relocate" => TokenType.Relocate,
            "to" => TokenType.To,
            "in" => TokenType.In,
            "delete" => TokenType.Delete,
            "filter" => TokenType.Filter,
            "from" => TokenType.From,
            "rename" => TokenType.Rename,

            _ => TokenType.Identifier
        };

        AddToken(type);
    }
}
