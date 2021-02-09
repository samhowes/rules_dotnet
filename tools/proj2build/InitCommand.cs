using System;
using System.Threading.Tasks;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Definition;
using Microsoft.Extensions.FileSystemGlobbing;
using System.Linq;

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
}