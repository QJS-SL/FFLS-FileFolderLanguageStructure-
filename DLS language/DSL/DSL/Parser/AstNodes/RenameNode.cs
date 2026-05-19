using DSL.Parser.AstNodes;

public class RenameNode : Node
{
    // The pattern or literal name on the left side
    public string Pattern { get; }

    // The replacement pattern or literal name on the right side
    public string Replacement { get; }

    // Optional folder block: from folder { src }
    public string? Folder { get; }

    public RenameNode(string pattern, string? folder, string replacement)
    {
        Pattern = pattern;
        Folder = folder;
        Replacement = replacement;
    }
}
