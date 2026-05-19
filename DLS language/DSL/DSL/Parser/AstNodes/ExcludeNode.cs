using DSL.Parser.AstNodes;

public class ExcludeNode : BlockNode
{
    public string Extension { get; }

    public ExcludeNode(string extension)
    {
        Extension = extension;
    }
}
