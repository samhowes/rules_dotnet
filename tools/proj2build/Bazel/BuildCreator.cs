using System.Collections.Generic;
using System.Linq;

namespace proj2build.Bazel
{
    public class BuildCreator
    {
        public List<Target> Targets { get; } = new List<Target>();

        public CSharpTarget AddBinary(string name)
        {
            return AddTarget(new CSharpTarget("csharp_binary", name + ".exe"));
        }

        public CSharpTarget AddLibrary(string name)
        {
            return AddTarget(new CSharpTarget("csharp_library", name + ".dll"));
        }

        private CSharpTarget AddTarget(CSharpTarget target)
        {
            target.Deps.Add("@io_bazel_rules_dotnet//dotnet/stdlib.core:libraryset");
            Targets.Add(target);
            return target;
        }

        public string Build()
        {
            var loads = Targets.Select(t => t.Rule).Distinct();
            var builder = new IndentedStringBuilder();
            builder.Append("load(\"@io_bazel_rules_dotnet//dotnet:defs.bzl\", \"");
            builder.AppendJoin("\", \"", loads);
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
}