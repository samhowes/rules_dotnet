#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace proj2build.Bazel
{
    public class Target
    {
        private static readonly Regex TfmRegex = new Regex(@"(?<name>\w+)(?<version>\d.*)");

        public string? Rule { get; set; }
        public bool Executable { get; set; }
        public string BaseName { get; set; }
        public string SrcExtension { get; }
        public string? TargetFramework { get; set; }
        public List<string> Deps { get; } = new List<string>();
        public TargetAttribute? Data { get; set; }
        public List<TargetAttribute> Attributes { get; } = new List<TargetAttribute>();

        public Target(string baseName, string srcExtension)
        {
            BaseName = baseName;
            SrcExtension = srcExtension;
        }

        public void SetTargetFramework(string tfm)
        {
            var match = TfmRegex.Match(tfm);
            if (!match.Success)
                throw new ArgumentException(
                    $"Invalid Target Framework Moniker '{tfm}' see https://docs.microsoft.com/en-us/dotnet/standard/frameworks for " +
                    $"valid frameworks.");

            TargetFramework = tfm;
            var name = match.Groups["name"].Value;
            if (name.StartsWith("netcoreapp", StringComparison.OrdinalIgnoreCase))
            {
                Executable = true;
            }
        }

        public bool AddSdk(string sdkName)
        {
            // ReSharper disable once StringLiteralTypo
            const string stdLibrarySet = "libraryset";

            var lower = sdkName.ToLower();
            var label = lower switch
            {
                "microsoft.net.sdk" => stdLibrarySet,
                "microsoft.net.sdk.web" => "Microsoft.AspNetCore.App",
                _ => null
            };
            if (label == null) return false;

            if (!ReferenceEquals(stdLibrarySet, label))
            {
                AddSdkDep(stdLibrarySet);
            }
            AddSdkDep(label);
            return true;
        }

        private void AddSdkDep(string label)
        {
            Deps.Add($"@core_sdk_stdlib//:{label}");
        }

        public void Build()
        {
            var ruleType = Executable ? "binary" : "library";
            Rule = $"{SrcExtension}harp_{ruleType}";

            Attributes.Add(new StringAttribute("name", BaseName + (Executable ? ".exe" : ".dll")));
            Attributes.Add(new GlobAttribute("srcs", "**/*." + SrcExtension));

            if (Data != null)
            {
                Attributes.Add(Data);
            }

            if (TargetFramework != null)
            {
                Attributes.Add(new StringAttribute("target_framework", TargetFramework));
            }
            Attributes.Add(new ListAttribute("visibility", false, new List<string>() { "//visibility:public" }));
            Attributes.Add(new ListAttribute("deps", true, Deps));
        }
    }

    public class CSharpTarget : Target
    {
        public CSharpTarget(string baseName) : base(baseName, "cs")
        { }
    }
}