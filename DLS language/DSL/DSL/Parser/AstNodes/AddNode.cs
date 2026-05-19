using DSL.Parser.AstNodes;

public class AddNode : Node
{
    public string TargetType { get; }
    public List<string> Names { get; }
    public string? Folder { get; }

    public AddNode(string targetType, List<string> names, string? folder)
    {
        TargetType = targetType;
        Names = names;
        Folder = folder;
    }
}
