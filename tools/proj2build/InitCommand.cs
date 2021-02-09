using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Definition;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Linq;
using System.Text.RegularExpressions;

namespace proj2build
{
    public class InitCommand
    {
        public InitCommand()
        {
        }

        public Task Do(string solutionPath, string testsGlobPattern)
        {
            var testsMatcher = new Matcher(StringComparison.OrdinalIgnoreCase);
            testsMatcher.AddInclude(testsGlobPattern);

            var creator = new WorkspaceCreator(new WorkspaceWriter(), new VariableSubstitutor());
            creator.CreateFromSolutionFile(solutionPath);
            //var options = new ProjectOptions()
            //{
            //    LoadSettings = ProjectLoadSettings.IgnoreMissingImports | ProjectLoadSettings.RejectCircularImports,
            //    ProjectCollection = new ProjectCollection()
            //};
            //foreach (var projectInSolution in solutionFile.ProjectsInOrder)
            //{
            //    var project = Project.FromFile(projectInSolution.RelativePath, options);
            //    var projectDefinedProperties = project.Properties.Where(p =>
            //        p.IsImported == false
            //        && p.IsEnvironmentProperty == false
            //        && p.IsGlobalProperty == false
            //        && p.IsReservedProperty == false)
            //        .ToList();
            //}

            return Task.CompletedTask;
        }
    }

    public class VariableSubstitutor
    {
        private Regex _regex;

        public VariableSubstitutor()
        {
            _regex = new Regex("#{([^}]+)}#");
        }

        public string Substitute(string input, Dictionary<string, string> variables)
        {
            return _regex.Replace(input, (match) =>
            {
                return variables[match.Groups[1].Value];
            });
        }
    }

    public class WorkspaceCreator
    {
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
        }

        public const string WorkspaceTemplatePath = "WORKSPACE.template";
    }

    public class WorkspaceWriter
    {
        public virtual void Write(string path, string workspaceContent)
        {
            File.WriteAllText(path, workspaceContent);
        }
    }
}