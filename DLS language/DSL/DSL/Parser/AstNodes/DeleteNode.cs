using DSL.Parser.AstNodes;

public class DeleteNode : Node
{
    public string TargetType { get; }
    public string Name { get; }
    public DeleteNode(string targetType, string name)
    {
        TargetType = targetType;
        Name = name;
    }
}
