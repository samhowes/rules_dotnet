﻿using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Moq;
using proj2build;
using proj2build.Bazel;
using Xunit;

namespace proj2build_test
{
    public class EverythingT
    {
        private static string SolutionPath = Path.Combine("testdata", "Everything", "Everything.sln");

        [Theory]
        [InlineData("ConsoleApp")]
        [InlineData("ClassLibrary")]
        [InlineData("WebApplication.Api")]
        public void EverythingTest(string directory)
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

            DiffAsserter.AssertFile(Path.Combine(solutionDirectory!, directory, "BUILD"), written);
        }
    }
}