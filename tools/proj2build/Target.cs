using System;
using System.Collections.Generic;

namespace proj2build
{
    public class Target
    {
        public string Name { get; set; }
        public List<string> Deps { get; } = new List<string>();
        public string Rule { get; set; }

        public List<TargetAttribute> Attributes { get; } = new List<TargetAttribute>();

        public void Build()
        {
            Attributes.Add(new ListAttribute("deps", Deps));
        }
    }

    public abstract class TargetAttribute
    {
        public string Name { get; set; }

        public abstract void WriteValue(IndentedStringBuilder builder);
    }

    public class ListAttribute : TargetAttribute
    {
        private readonly List<string> _value;

        public ListAttribute(string name, List<string> value)
        {
            _value = value;
            Name = name;
        }

        public override void WriteValue(IndentedStringBuilder builder)
        {
            builder.Append("[");
            IDisposable indent = null;
            if (_value.Count > 1)
            {
                builder.AppendLine();
                indent = builder.Indent();
            }

            foreach (var value in _value)
            {
                builder.AppendQuoted(value);
                if (_value.Count > 1) builder.AppendLine(",");
            }

            indent?.Dispose();
            builder.Append("]");
        }
    }

    public class StringAttribute : TargetAttribute
    {
        public StringAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Value { get; set; }

        public override void WriteValue(IndentedStringBuilder builder)
        {
            builder.Append("\"");
            builder.Append(Value);
            builder.Append("\"");
        }
    }

    public class GlobAttribute : TargetAttribute
    {
        public string Glob { get; }

        public GlobAttribute(string name, string glob)
        {
            Glob = glob;
            Name = name;
        }

        public override void WriteValue(IndentedStringBuilder builder)
        {
            builder.Append("glob([");
            builder.AppendQuoted(Glob);
            builder.Append("])");
        }
    }
}