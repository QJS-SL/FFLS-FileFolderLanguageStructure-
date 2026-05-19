using DSL.Parser.AstNodes;

public class RemoveNode : BlockNode
{
    public string TargetType { get; }
    public string Name { get; }

    public RemoveNode(string targetType, string name)
    {
        TargetType = targetType;
        Name = name;
    }
}
