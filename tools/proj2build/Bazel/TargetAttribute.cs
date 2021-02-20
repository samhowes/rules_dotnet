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

        public abstract void WriteValue(StarlarkStringBuilder builder);
    }

    public class ListAttribute : TargetAttribute
    {
        private readonly bool _expand;
        private readonly List<string> _value;

        public ListAttribute(string name, bool expand, List<string> value) : base(name)
        {
            _expand = expand;
            _value = value;
        }

        public override void WriteValue(StarlarkStringBuilder builder)
        {
            builder.Append("[");
            IDisposable indent = null;
            if (_value.Count > 1 || _expand)
            {
                builder.AppendLine();
                indent = builder.Indent();
            }

            foreach (var value in _value)
            {
                builder.AppendQuoted(value);
                if (_value.Count > 1 || _expand) builder.AppendLine(",");
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

        public override void WriteValue(StarlarkStringBuilder builder)
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

        public override void WriteValue(StarlarkStringBuilder builder)
        {
            builder.Append("glob([");
            builder.AppendQuoted(Glob);
            builder.Append("])");
        }
    }

    public class AdditionAttribute : TargetAttribute
    {
        public TargetAttribute[] Operands { get; }

        public AdditionAttribute(string name, params TargetAttribute[] operands) : base(name)
        {
            Operands = operands;
        }

        public override void WriteValue(StarlarkStringBuilder builder)
        {
            for (int i = 0; i < Operands.Length; i++)
            {
                if (i > 0)
                {
                    builder.Append(" + ");
                }
                Operands[i].WriteValue(builder);
            }
        }
    }
}