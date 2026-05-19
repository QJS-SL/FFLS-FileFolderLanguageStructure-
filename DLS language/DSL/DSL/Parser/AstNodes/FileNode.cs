using DSL.Parser.AstNodes;

public class FileNode : Node
{
    public string Name { get; }
    public FileNode(string name) => Name = name;
}
