using System;
using System.Collections.Generic;

namespace proj2build.Bazel
{
    public abstract class TargetAttribute
    {
        public string Name { get; set; }

        protected TargetAttribute(string name)
        {
            Name = name;
        }

        public abstract void WriteValue(IndentedStringBuilder builder);
    }

    public class ListAttribute : TargetAttribute
    {
        private readonly List<string> _value;

        public ListAttribute(string name, List<string> value) : base(name)
        {
            _value = value;
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
        public string Value { get; set; }

        public StringAttribute(string name, string value) : base(name)
        {
            Value = value;
        }

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

        public GlobAttribute(string name, string glob) : base(name)
        {
            Glob = glob;
        }

        public override void WriteValue(IndentedStringBuilder builder)
        {
            builder.Append("glob([");
            builder.AppendQuoted(Glob);
            builder.Append("])");
        }
    }
}