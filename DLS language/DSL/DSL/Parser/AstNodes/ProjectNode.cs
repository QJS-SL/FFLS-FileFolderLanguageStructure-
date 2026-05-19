using DSL.Parser.AstNodes;

public class ProjectNode : Node
{
    public List<Node> Children { get; }
    public ProjectNode(List<Node> children) => Children = children;
}
