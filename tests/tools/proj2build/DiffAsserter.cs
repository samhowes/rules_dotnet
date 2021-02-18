using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace proj2build_test
{
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

        public static void AssertFile(string path, Dictionary<string, string> written)
        {
            var workspacePath = Path.GetFullPath(path);

            Assert.Contains(workspacePath, written.Keys);
            var got = written[workspacePath];
            var want = File.ReadAllText(workspacePath + ".want");

            DiffAsserter.AssertSame(want, got, path);
        }
    }
}