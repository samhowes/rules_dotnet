using CommandLine;

namespace proj2build
{
    [Verb("init", HelpText = "Creates a WORKSPACE file from a solution or project file.")]
    public class InitVerb
    {
        [Value(index: 0,
            Required = true,
            HelpText = "Solution file path.")]
        public string Solution { get; set; }

        [Value(index: 1,
            Required = true,
            HelpText = "Glob pattern for test projects")]
        public string Tests { get; set; }
    }
}