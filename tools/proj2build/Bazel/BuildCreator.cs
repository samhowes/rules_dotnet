using System.Collections.Generic;
using System.Linq;
using Microsoft.Build.Construction;

namespace proj2build.Bazel
{
    public class BuildCreator
    {
        public List<TranslationError> TranslationErrors { get; } = new List<TranslationError>();
        public List<Target> Targets { get; } = new List<Target>();

        public CSharpTarget AddTarget(string name)
        {
            var target = new CSharpTarget(name);
            Targets.Add(target);
            return target;
        }

        public void AddTranslationError(ElementLocation location, string message)
        {
            TranslationErrors.Add(new TranslationError(location, message));
        }

        public string Build()
        {
            var loads = new HashSet<string>();
            foreach (var target in Targets)
            {
                target.Build();
                loads.Add(target.Rule);
            }

            var builder = new StarlarkStringBuilder();
            builder.Append("load(\"@io_bazel_rules_dotnet//dotnet:defs.bzl\", \"");
            builder.AppendJoin("\", \"", loads);
            builder.AppendLine("\")");

            if (TranslationErrors.Any())
            {
                builder.AppendLine();
                using var _ = builder.Comment();
                builder.AppendLine("The following attributes were lost in translation");
                foreach (var error in TranslationErrors)
                {
                    builder.Append(error.Location.LocationString);
                    builder.Append(" ");
                    builder.AppendLine(error.Message);
                }
            }

            foreach (var target in Targets)
            {
                builder.AppendLine();
                builder.Append(target.Rule!);
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

    public class TranslationError
    {
        public ElementLocation Location { get; }
        public string Message { get; }

        public TranslationError(ElementLocation location, string message)
        {
            Location = location;
            Message = message;
        }
    }
}