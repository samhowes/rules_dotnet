#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Definition;
using Microsoft.Build.Evaluation;

// ReSharper disable StringLiteralTypo

namespace proj2build
{
    public class BuildCreator
    {
        public HashSet<string> Loads { get; set; } = new HashSet<string>();

        public void AddBinary(string name)
        {
            Loads.Add("core_binary");
            var target = new Target() { Name = name + ".exe" };
            target.Attributes.Add(new StringAttribute("name", target.Name));
            target.Attributes.Add(new GlobAttribute("srcs", "**/*.cs"));
            target.Deps.Add("@io_bazel_rules_dotnet//dotnet/stdlib.core:libraryset");
            target.Rule = "core_binary";
            Targets.Add(target);
        }

        public List<Target> Targets { get; } = new List<Target>();

        public string Build()
        {
            var builder = new IndentedStringBuilder();
            builder.Append("load(\"@io_bazel_rules_dotnet//dotnet:defs.bzl\", \"");
            builder.AppendJoin("\", \"", Loads);
            builder.AppendLine("\")");

            foreach (var target in Targets)
            {
                target.Build();
                builder.AppendLine();
                builder.Append(target.Rule);
                builder.AppendLine("(");
                using (var _ = builder.Indent())
                {
                    foreach (var attribute in target.Attributes)
                    {
                        builder.Append(attribute.Name);
                        builder.Append(" = ");
                        attribute.WriteValue(builder);
                        builder.AppendLine(",");
                    }
                }
                builder.AppendLine(")");
                builder.AppendLine();
            }

            return builder.ToString()!;
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
            var template = File.ReadAllText(WorkspaceTemplatePath);

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
                buildCreator.AddBinary(Path.GetFileNameWithoutExtension(project.FullPath));

                var buildPath = Path.Combine(project.DirectoryPath, "BUILD");
                var buildContents = buildCreator.Build();
                _writer.Write(buildPath, buildContents);
            }
        }
    }
}