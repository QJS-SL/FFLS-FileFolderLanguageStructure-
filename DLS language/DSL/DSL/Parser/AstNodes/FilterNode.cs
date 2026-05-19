using DSL.Parser.AstNodes;

public class FilterNode : Node
{
    public List<Node> Rules { get; }
    public FilterNode(List<Node> rules) => Rules = rules;
}
