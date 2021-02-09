using CommandLine;
using CommandLine.Text;
using System;
using System.Threading.Tasks;

namespace proj2build
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var parsed = Parser.Default.ParseArguments<InitVerb>(args);
            var result = await parsed.MapResult<InitVerb, Task<int>>(
                async (InitVerb opts) =>
                {
                    await new InitCommand().Do(opts.Solution, opts.Tests);
                    return 0;
                },
                errs =>
                {
                    HelpText.AutoBuild(parsed);
                    return Task.FromResult(-1);
                });

            Environment.Exit(result);
        }
    }
}