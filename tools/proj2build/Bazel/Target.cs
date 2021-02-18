#nullable enable

using System;
using System.Collections.Generic;

namespace proj2build.Bazel
{
    public class Target
    {
        public string Rule { get; set; }
        public string Name { get; set; }
        public string? TargetFramework { get; set; }
        public List<string> Deps { get; } = new List<string>();
        public List<TargetAttribute> Attributes { get; } = new List<TargetAttribute>();

        public Target(string rule, string name, string extension)
        {
            Name = name;
            Rule = rule;
            Attributes.Add(new StringAttribute("name", Name));
            Attributes.Add(new GlobAttribute("srcs", "**/*." + extension));
        }

        public void Build()
        {
            if (TargetFramework != null)
            {
                Attributes.Add(new StringAttribute("target_framework", TargetFramework));
            }
            Attributes.Add(new ListAttribute("visibility", new List<string>() { "//visibility:public" }));
            Attributes.Add(new ListAttribute("deps", Deps));
        }
    }

    public class CSharpTarget : Target
    {
        public CSharpTarget(string rule, string name) : base(rule, name, "cs")
        { }
    }
}