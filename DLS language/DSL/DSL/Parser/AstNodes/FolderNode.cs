using DSL.Parser.AstNodes;

public class FolderNode : Node
{
    public string Name { get; }
    public FolderNode(string name) => Name = name;
}

