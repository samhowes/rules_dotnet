using System;
using System.Collections.Generic;
using System.Text;

namespace proj2build
{
    public class IndentedStringBuilder
    {
        private readonly StringBuilder _builder;
        private bool _freshLine = true;
        private int IndentLevel { get; set; }

        public IndentedStringBuilder()
        {
            _builder = new StringBuilder();
        }

        public IDisposable Indent()
        {
            return new Indentor(this);
        }

        public void Append(string value)
        {
            AppendPrefix();

            _builder.Append(value);
        }

        public void AppendLine(string? value = null)
        {
            AppendPrefix();
            _builder.AppendLine(value);
            _freshLine = true;
        }

        public void AppendQuoted(string value)
        {
            Append("\"");
            Append(value);
            Append("\"");
        }

        private void AppendPrefix()
        {
            if (!_freshLine) return;
            for (var i = 0; i < IndentLevel; i++)
            {
                // probably a better way that looping
                _builder.Append("    ");
            }

            _freshLine = false;
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        private class Indentor : IDisposable
        {
            private readonly IndentedStringBuilder _indentedStringBuilder;

            public Indentor(IndentedStringBuilder indentedStringBuilder)
            {
                _indentedStringBuilder = indentedStringBuilder;
                _indentedStringBuilder.IndentLevel++;
            }

            public void Dispose()
            {
                _indentedStringBuilder.IndentLevel--;
            }
        }

        public void AppendJoin(string separator, IEnumerable<string> values)
        {
            AppendPrefix();
            _builder.AppendJoin(separator, values);
        }
    }
}