using DSL.Parser.AstNodes;

public class ForbidNode : BlockNode
{
    public string Extension { get; }

    public ForbidNode(string extension)
    {
        Extension = extension;
    }
}
