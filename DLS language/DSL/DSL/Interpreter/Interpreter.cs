using DSL.Parser.AstNodes;

namespace DSL.Interpreter;

public class Interpreter
{
    private readonly string ProjectRoot = Path.GetFullPath(
        Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));

    public void Execute(Node root)
    {
        Visit(root);
    }

    private void Visit(Node node)
    {
        switch (node)
        {
            case ProjectNode p: VisitProject(p); break;
            case FolderNode f: VisitFolder(f); break;
            case FileNode f: VisitFile(f); break;
            case RelocateNode r: VisitRelocate(r); break;
            case AddNode a: VisitAdd(a); break;
            case DeleteNode d: VisitDelete(d); break;
            case IgnoreNode ignore: VisitIgnore(ignore); break;
            case CleanNode c: VisitClean(c); break;
            case RemoveNode r: VisitRemove(r); break;
            case ExcludeNode e: VisitExclude(e); break;
            case FilterNode f: VisitFilter(f); break;
            case RenameNode r: VisitRename(r); break;


            default:
                Console.WriteLine($"Unknown node: {node.GetType().Name}");
                break;
        }
    }

    private void VisitProject(ProjectNode node)
    {
        Console.WriteLine("=== Project Structure Check ===");

        foreach (var child in node.Children)
        {
            Visit(child);
        }
    }

    private void VisitFolder(FolderNode node)
    {
        string fullPath = Resolve(node.Name);

        if (Directory.Exists(fullPath))
            Console.WriteLine($"! Folder exists: {node.Name}");
        else
            Console.WriteLine($"? Missing folder: {node.Name}");
    }

    private void VisitFile(FileNode node)
    {
        string fullPath = Resolve(node.Name);

        if (File.Exists(fullPath))
            Console.WriteLine($"! File exists: {node.Name}");
        else
            Console.WriteLine($"? Missing file: {node.Name}");
    }

    private void VisitRelocate(RelocateNode node)
    {
        string sourceFolder = Resolve(node.FromFolder);
        string destinationFolder = Resolve(node.ToFolder);

        if (!Directory.Exists(sourceFolder))
        {
            Console.WriteLine($"? Folder not found: {node.FromFolder}");
            return;
        }

        if (!Directory.Exists(destinationFolder))
        {
            Directory.CreateDirectory(destinationFolder);
            Console.WriteLine($"Created folder: {node.ToFolder}");
        }

        var files = Directory.GetFiles(sourceFolder);

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);

            if (!PatternMatcher.IsMatch(fileName, node.Pattern))
                continue;

            if (ShouldSkip(fileName))
            {
                Console.WriteLine($"~ Skipped (filtered): {fileName}");
                continue;
            }

            string dest = Path.Combine(destinationFolder, fileName);
            File.Move(file, dest, overwrite: true);
            Console.WriteLine($"! Relocated {fileName} → {node.ToFolder}");
        }
    }



    private void VisitAdd(AddNode node)
    {
        foreach (var name in node.Names)
        {
            string fullPath = node.Folder != null
                ? Resolve(Path.Combine(node.Folder, name))
                : Resolve(name);

            if (node.TargetType == "file")
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
                File.WriteAllText(fullPath, "");
                Console.WriteLine($"! Created file: {fullPath}");
            }
            else if (node.TargetType == "folder")
            {
                Directory.CreateDirectory(fullPath);
                Console.WriteLine($"! Created folder: {fullPath}");
            }
        }
    }

    private void VisitDelete(DeleteNode node)
    {
        string fullPath = Resolve(node.Name);

        if (node.TargetType == "file")
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                Console.WriteLine($"! Deleted file: {node.Name}");
            }
            else
            {
                Console.WriteLine($"? File not found: {node.Name}");
            }
        }
        else if (node.TargetType == "folder")
        {
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, recursive: true);
                Console.WriteLine($"! Deleted folder: {node.Name}");
            }
            else
            {
                Console.WriteLine($"? Folder not found: {node.Name}");
            }
        }
        else
        {
            Console.WriteLine($"? Unknown delete target: {node.TargetType}");
        }
    }

    private void VisitClean(CleanNode node)
    {
        string fullPath = Resolve(node.Folder);

        if (!Directory.Exists(fullPath))
        {
            Console.WriteLine($"? Folder not found: {node.Folder}");
            return;
        }

        // Delete all files
        foreach (var file in Directory.GetFiles(fullPath))
        {
            string fileName = Path.GetFileName(file);

            if (IsIgnored(fileName))
            {
                Console.WriteLine($"~ Skipped (ignored): {fileName}");
                continue;
            }

            if (IsExcluded(fileName))
            {
                Console.WriteLine($"~ Skipped (excluded type): {fileName}");
                continue;
            }

            File.Delete(file);
            Console.WriteLine($"! Deleted file: {fileName}");
        }

        // Delete all subfolders
        foreach (var dir in Directory.GetDirectories(fullPath))
        {
            string folderName = Path.GetFileName(dir);

            if (IsIgnored(folderName))
            {
                Console.WriteLine($"~ Skipped (ignored folder): {folderName}");
                continue;
            }

            Directory.Delete(dir, recursive: true);
            Console.WriteLine($"! Deleted folder: {folderName}");
        }

        Console.WriteLine($"? Cleaned folder: {node.Folder}");
    }

    private readonly List<string> IgnorePatterns = new();

    private void VisitIgnore(IgnoreNode node)
    {
        IgnorePatterns.Add(node.Pattern);
        Console.WriteLine($"! Ignoring: {node.Pattern}");
    }

    private bool IsIgnored(string fileName)
    {
        foreach (var pattern in IgnorePatterns)
        {
            if (pattern == fileName)
                return true;
        }

        return false;
    }

    private void VisitRemove(RemoveNode node)
    {
        string fullPath = Resolve(node.Name);

        if (node.TargetType == "file")
        {
            RemoveFileSafe(fullPath);
        }
        else if (node.TargetType == "folder")
        {
            RemoveFolderSafe(fullPath);
        }
        else
        {
            Console.WriteLine($"? Unknown remove target: {node.TargetType}");
        }
    }

    private void RemoveFileSafe(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"? File not found: {filePath}");
            return;
        }

        string dir = Path.GetDirectoryName(filePath)!;
        string name = Path.GetFileName(filePath);

        File.Delete(filePath);
        Console.WriteLine($"! Removed file: {name}");

        string[] patterns = { name + ".bak", name + ".tmp", name + "~" };

        foreach (var pattern in patterns)
        {
            string backup = Path.Combine(dir, pattern);
            if (File.Exists(backup))
            {
                File.Delete(backup);
                Console.WriteLine($"! Removed backup: {pattern}");
            }
        }
    }

    private void RemoveFolderSafe(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            Console.WriteLine($"? Folder not found: {folderPath}");
            return;
        }

        string parent = Directory.GetParent(folderPath)!.FullName;

        foreach (var file in Directory.GetFiles(folderPath))
        {
            string name = Path.GetFileName(file);
            string dest = Path.Combine(parent, name);
            File.Move(file, dest, overwrite: true);
            Console.WriteLine($"! Moved file: {name}");
        }

        foreach (var dir in Directory.GetDirectories(folderPath))
        {
            string name = Path.GetFileName(dir);
            string dest = Path.Combine(parent, name);
            Directory.Move(dir, dest);
            Console.WriteLine($"! Moved folder: {name}");
        }

        Directory.Delete(folderPath);
        Console.WriteLine($"? Removed folder: {Path.GetFileName(folderPath)}");
    }

    private readonly List<string> ExcludedFiles = new();
    private readonly List<string> ExcludedExtensions = new();

    private void VisitExclude(ExcludeNode node)
    {
        // If it starts with a dot → extension
        if (node.Extension.StartsWith("."))
        {
            ExcludedExtensions.Add(node.Extension);
            Console.WriteLine($"! Excluding file type: {node.Extension}");
        }
        else
        {
            ExcludedFiles.Add(node.Extension);
            Console.WriteLine($"! Excluding file: {node.Extension}");
        }
    }


    private bool IsExcluded(string fileName)
    {
        // Exact filename match
        if (ExcludedFiles.Contains(fileName))
            return true;

        // Extension match
        string ext = Path.GetExtension(fileName);
        if (ExcludedExtensions.Contains(ext))
            return true;

        return false;
    }


    private void VisitFilter(FilterNode node)
    {
        foreach (var rule in node.Rules)
            Visit(rule);
    }
    private bool ShouldSkip(string fileName)
    {
        if (IsIgnored(fileName))
            return true;

        if (IsExcluded(fileName))
            return true;

        return false;
    }

    private void VisitRename(RenameNode node)
    {
        string baseFolder = node.Folder != null
            ? Resolve(node.Folder)
            : ProjectRoot;

        if (!Directory.Exists(baseFolder))
        {
            Console.WriteLine($"? Folder not found: {node.Folder}");
            return;
        }

        var files = Directory.GetFiles(baseFolder);

        foreach (var file in files)
        {
            string fileName = Path.GetFileName(file);

            if (!PatternMatcher.IsMatch(fileName, node.Pattern))
                continue;

            if (ShouldSkip(fileName))
            {
                Console.WriteLine($"~ Skipped (filtered): {fileName}");
                continue;
            }

            string newName = ApplyRenamePattern(fileName, node.Pattern, node.Replacement);
            string newPath = Path.Combine(baseFolder, newName);

            File.Move(file, newPath, overwrite: true);
            Console.WriteLine($"! Renamed {fileName} → {newName}");
        }
    }

    private string ApplyRenamePattern(string fileName, string pattern, string replacement)
    {
        // Only handle single * for now: *.txt -> *.bak
        int starIndex = pattern.IndexOf('*');
        if (starIndex == -1)
            return replacement; // no wildcard, just literal

        string prefix = pattern[..starIndex];
        string suffix = pattern[(starIndex + 1)..];

        if (!fileName.StartsWith(prefix) || !fileName.EndsWith(suffix))
            return fileName; // shouldn't happen if IsMatch was true

        string middle = fileName[prefix.Length..(fileName.Length - suffix.Length)];

        // replacement also has one *
        int repStar = replacement.IndexOf('*');
        if (repStar == -1)
            return replacement;

        string repPrefix = replacement[..repStar];
        string repSuffix = replacement[(repStar + 1)..];

        return repPrefix + middle + repSuffix;
    }





    private string Resolve(string path)
    {
        return Path.Combine(ProjectRoot, path);
    }
}
