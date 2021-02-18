using System.IO;

namespace proj2build.Bazel
{
    public class WorkspaceWriter
    {
        public virtual void Write(string path, string workspaceContent)
        {
            File.WriteAllText(path, workspaceContent);
        }
    }
}