using DSL.Parser.AstNodes;

public abstract class BlockNode : Node
{
    public List<Node> Children { get; } = new();
}

