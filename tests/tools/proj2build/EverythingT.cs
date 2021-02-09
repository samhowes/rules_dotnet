using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using proj2build;
using Xunit;

namespace proj2build_test
{
    public class EverythingT
    {
        private static string SolutionPath = Path.Combine("testdata", "Everything", "Everything.sln");

        [Fact]
        public void EverythingTest()
        {
            var written = new Dictionary<string, string>();
            var writer = new Mock<WorkspaceWriter>();
            writer.Setup(w => w.Write(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((path, content) =>
                {
                    written[path] = content;
                });

            var creator = new WorkspaceCreator(writer.Object, new VariableSubstitutor());
            creator.CreateFromSolutionFile(SolutionPath);
            var solutionDirectory = Path.GetDirectoryName(SolutionPath);

            AssertFile(Path.Combine(solutionDirectory!, "WORKSPACE"), written);
            AssertFile(Path.Combine(solutionDirectory!, "ConsoleApp", "BUILD"), written);
        }

        private static void AssertFile(string path, Dictionary<string, string> written)
        {
            var workspacePath = Path.GetFullPath(path);

            Assert.Contains(workspacePath, written.Keys);
            var got = written[workspacePath];
            var want = File.ReadAllText(workspacePath + ".want");

            DiffAsserter.AssertSame(want, got, path);
        }
    }

    public class DiffAsserter
    {
        public static void AssertSame(string want, string got, string context)
        {
            var wantLines = want.Split(Environment.NewLine);
            var gotLines = got.Split(Environment.NewLine);

            var loopCount = Math.Min(wantLines.Length, gotLines.Length);

            for (var i = 0; i < loopCount; i++)
            {
                Assert.True(wantLines[i] == gotLines[i],
                    $"[{context}]\n" +
                    $"Difference at line #{i + 1}\n" +
                    $"Expected '{wantLines[i]}'\n" +
                    $"Actual   '{gotLines[i]}'");
            }
        }
    }
}