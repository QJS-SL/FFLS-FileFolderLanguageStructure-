using DSL.Parser.AstNodes;

public class LiteralNode : Node
{
    public string Value { get; }

    public LiteralNode(string value)
    {
        Value = value;
    }
}
