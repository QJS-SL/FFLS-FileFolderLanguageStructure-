namespace DSL.Parser.AstNodes;

public class CleanNode : BlockNode
{
    public string Folder { get; }

    public CleanNode(string folder)
    {
        Folder = folder;
    }
}
