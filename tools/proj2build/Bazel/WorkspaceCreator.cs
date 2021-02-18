#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;

// ReSharper disable StringLiteralTypo

namespace proj2build.Bazel
{
    public static class NamespacePathResolver
    {
        public static string Resolve<T>(string path)
        {
            var nsParts = typeof(T).Namespace!.Split('.').Skip(1).Append(path).ToArray();
            var assetPath = Path.Combine(nsParts);
            return assetPath;
        }
    }

    public class WorkspaceCreator
    {
        public const string WorkspaceTemplatePath = "WORKSPACE.template";
        private readonly WorkspaceWriter _writer;
        private readonly VariableSubstitutor _substitutor;

        public WorkspaceCreator(WorkspaceWriter writer, VariableSubstitutor substitutor)
        {
            _writer = writer;
            _substitutor = substitutor;
        }

        public void CreateFromSolutionFile(string path)
        {
            var fullPath = Path.GetFullPath(path);
            var directoryPath = Path.GetDirectoryName(fullPath);
            var solutionFile = SolutionFile.Parse(fullPath);
            var solutionName = Path.GetFileNameWithoutExtension(path);
            var template = File.ReadAllText(NamespacePathResolver.Resolve<WorkspaceCreator>(WorkspaceTemplatePath));

            var contents = _substitutor.Substitute(template,
                new Dictionary<string, string>() { { "workspace.name", solutionName.ToLower() } });

            _writer.Write(Path.Combine(directoryPath, "WORKSPACE"), contents);

            var collection = new ProjectCollection();
            var options = new ProjectOptions()
            {
                ProjectCollection = collection,
                LoadSettings = ProjectLoadSettings.IgnoreInvalidImports | ProjectLoadSettings.RejectCircularImports | ProjectLoadSettings.IgnoreMissingImports
            };
            foreach (var projectInSolution in solutionFile.ProjectsInOrder)
            {
                var project = Project.FromFile(projectInSolution.AbsolutePath, options);
                var buildCreator = new BuildCreator();

                var targetName = Path.GetFileNameWithoutExtension(project.FullPath);

                var outputType = project.GetPropertyValue("OutputType");
                Target target;
                target = String.Equals(outputType, "exe", StringComparison.OrdinalIgnoreCase)
                    ? buildCreator.AddBinary(targetName)
                    : buildCreator.AddLibrary(targetName);

                target.TargetFramework = project.GetPropertyValue("TargetFramework");

                var buildPath = Path.Combine(project.DirectoryPath, "BUILD");
                var buildContents = buildCreator.Build();
                _writer.Write(buildPath, buildContents);
            }
        }
    }
}