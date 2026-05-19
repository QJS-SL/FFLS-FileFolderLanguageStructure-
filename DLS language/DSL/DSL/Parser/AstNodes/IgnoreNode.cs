using DSL.Parser.AstNodes;

public class IgnoreNode : Node
{
    public string Pattern { get; }
    public IgnoreNode(string pattern) => Pattern = pattern;
}
