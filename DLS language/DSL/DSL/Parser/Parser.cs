using DSL.Lexer;
using DSL.Parser.AstNodes;

namespace DSL.Parser;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _current = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public Node Parse()
    {
        return ParseProject();
    }

    private Node ParseProject()
    {
        Consume(TokenType.Project, "Expected 'project' keyword.");
        Consume(TokenType.LeftBrace, "Expected '{' after project.");

        var children = new List<Node>();

        while (!Check(TokenType.RightBrace) && !IsAtEnd())
        {
            children.Add(ParseStatement());
        }

        Consume(TokenType.RightBrace, "Expected '}' after project body.");

        return new ProjectNode(children);
    }

    private Node ParseStatement()
    {
        // filter { ... }
        if (Match(TokenType.Filter))
        {
            Consume(TokenType.LeftBrace, "Expected '{' after filter.");

            var rules = new List<Node>();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                rules.Add(ParseStatement());
            }

            Consume(TokenType.RightBrace, "Expected '}' after filter block.");

            return new FilterNode(rules);
        }

        // folder name
        if (Match(TokenType.Folder))
        {
            var name = Consume(TokenType.Identifier, "Expected folder name.").Lexeme;
            return new FolderNode(name);
        }

        // file name
        if (Match(TokenType.File))
        {
            var name = Consume(TokenType.Identifier, "Expected file name.").Lexeme;
            return new FileNode(name);
        }

        // relocate <pattern> {
        if (Match(TokenType.Relocate))
        {
            // Accept ANY identifier or pattern token
            string pattern = Consume(TokenType.Identifier, "Expected pattern after 'relocate'.").Lexeme;

            Consume(TokenType.LeftBrace, "Expected '{' after relocate.");

            // --- from folder { src } ---
            Consume(TokenType.From, "Expected 'from'.");
            Consume(TokenType.Folder, "Expected 'folder' after 'from'.");
            Consume(TokenType.LeftBrace, "Expected '{' after 'from folder'.");
            string fromFolder = Consume(TokenType.Identifier, "Expected folder name.").Lexeme;
            Consume(TokenType.RightBrace, "Expected '}' after from folder block.");

            // --- to folder { src2 } ---
            Consume(TokenType.To, "Expected 'to'.");
            Consume(TokenType.Folder, "Expected 'folder' after 'to'.");
            Consume(TokenType.LeftBrace, "Expected '{' after 'to folder'.");
            string toFolder = Consume(TokenType.Identifier, "Expected folder name.").Lexeme;
            Consume(TokenType.RightBrace, "Expected '}' after to folder block.");

            Consume(TokenType.RightBrace, "Expected '}' after relocate block.");

            return new RelocateNode(pattern, fromFolder, toFolder);
            Console.WriteLine("Parsed relocate: " + pattern);

        }





        if (Match(TokenType.Add))
        {
            Token targetToken = Consume(TokenType.File, "Expected 'file'.");
            string targetType = targetToken.Lexeme;

            Consume(TokenType.LeftBrace, "Expected '{' after add file.");

            var names = new List<string>();

            while (!Check(TokenType.RightBrace) && !IsAtEnd())
            {
                if (Match(TokenType.Folder))
                {
                    string folderName = Consume(TokenType.Identifier, "Expected folder name.").Lexeme;
                    Consume(TokenType.LeftBrace, "Expected '{' after folder name.");

                    while (!Check(TokenType.RightBrace) && !IsAtEnd())
                    {
                        string fileName = Consume(TokenType.Identifier, "Expected file name.").Lexeme;
                        names.Add($"{folderName}/{fileName}");
                    }

                    Consume(TokenType.RightBrace, "Expected '}' after folder block.");
                }
                else
                {
                    throw Error(Peek(), "Expected 'folder' block inside add file.");
                }
            }

            Consume(TokenType.RightBrace, "Expected '}' after add file block.");

            return new AddNode(targetType, names, null);
        }


        // delete file x.txt / delete folder src
        if (Match(TokenType.Delete))
        {
            Token targetToken = Advance(); // accept file/folder as any token
            string targetType = targetToken.Lexeme;

            var name = Consume(TokenType.Identifier, "Expected name.").Lexeme;

            return new DeleteNode(targetType, name);
        }

        // ignore c.txt   (no block form yet)
        if (Match(TokenType.Ignore))
        {
            var pattern = Consume(TokenType.Identifier, "Expected pattern to ignore.").Lexeme;
            return new IgnoreNode(pattern);
        }

        // clean folder src
        if (Match(TokenType.Clean))
        {
            Consume(TokenType.Folder, "Expected 'folder' keyword.");
            var folderName = Consume(TokenType.Identifier, "Expected folder name.").Lexeme;

            return new CleanNode(folderName);
        }

        // remove file x.txt / remove folder src
        if (Match(TokenType.Remove))
        {
            Token targetToken = Advance(); // file/folder
            string targetType = targetToken.Lexeme;

            var name = Consume(TokenType.Identifier, "Expected name.").Lexeme;

            return new RemoveNode(targetType, name);
        }

        // exclude .png
        if (Match(TokenType.Exclude))
        {
            var ext = Consume(TokenType.Identifier, "Expected file extension to exclude.").Lexeme;
            return new ExcludeNode(ext);
        }

        // rename <pattern> {
        if (Match(TokenType.Rename))
        {
            // Accept ANY identifier or symbol sequence as the pattern
            Token patternToken = Consume(TokenType.Identifier, "Expected file, folder, or pattern.");
            string pattern = patternToken.Lexeme;

            Consume(TokenType.LeftBrace, "Expected '{' after rename.");

            string? folder = null;

            // Optional: from folder { src }
            if (Match(TokenType.From))
            {
                Consume(TokenType.Folder, "Expected 'folder' after 'from'.");
                Consume(TokenType.LeftBrace, "Expected '{' after from folder.");
                folder = Consume(TokenType.Identifier, "Expected folder name.").Lexeme;
                Consume(TokenType.RightBrace, "Expected '}' after from folder block.");
            }

            // Now parse: <pattern> to <replacement>
            string oldPattern = Consume(TokenType.Identifier, "Expected old name or pattern.").Lexeme;
            Consume(TokenType.To, "Expected 'to'.");
            string newPattern = Consume(TokenType.Identifier, "Expected new name or pattern.").Lexeme;

            Consume(TokenType.RightBrace, "Expected '}' after rename block.");

            return new RenameNode(oldPattern, folder, newPattern);
        }

        throw Error(Peek(), "Unexpected statement.");
    }

    // Utility methods

    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _current++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EndOfFile;

    private Token Peek() => _tokens[_current];

    private Token Previous() => _tokens[_current - 1];

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private Exception Error(Token token, string message)
    {
        return new Exception($"[Line {token.Line}] Error at '{token.Lexeme}': {message}");
    }
}
