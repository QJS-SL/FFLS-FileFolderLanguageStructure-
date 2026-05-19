using DSL.Parser.AstNodes;

public class RelocateNode : Node
{
    public string Pattern { get; }
    public string FromFolder { get; }
    public string ToFolder { get; }

    public RelocateNode(string pattern, string fromFolder, string toFolder)
    {
        Pattern = pattern;
        FromFolder = fromFolder;
        ToFolder = toFolder;
    }
}
