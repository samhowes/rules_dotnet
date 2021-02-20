#nullable enable

using System;
using System.Collections.Generic;
using System.Text;

namespace proj2build
{
    public class StarlarkStringBuilder
    {
        private readonly StringBuilder _builder;
        private bool _freshLine = true;
        private int _indentLevel = 0;
        private int _commentLevel = 0;

        public StarlarkStringBuilder()
        {
            _builder = new StringBuilder();
        }

        public IDisposable Indent()
        {
            return new Indentor(this);
        }

        public IDisposable Comment()
        {
            return new Commentor(this);
        }

        public void Append(string value)
        {
            AppendPrefix();

            _builder.Append(value);
        }

        public void AppendComment(string value)
        {
            Append("# ");
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
            for (var i = 0; i < _indentLevel; i++)
            {
                // probably a better way that looping
                _builder.Append("    ");
            }

            if (_commentLevel > 0)
            {
                _builder.Append("# ");
            }

            _freshLine = false;
        }

        public override string ToString()
        {
            return _builder.ToString();
        }

        public void AppendJoin(string separator, IEnumerable<string> values)
        {
            AppendPrefix();
            _builder.AppendJoin(separator, values);
        }

        private class Indentor : IDisposable
        {
            private readonly StarlarkStringBuilder _builder;

            public Indentor(StarlarkStringBuilder builder)
            {
                _builder = builder;
                _builder._indentLevel++;
            }

            public void Dispose()
            {
                _builder._indentLevel--;
            }
        }

        private class Commentor : IDisposable
        {
            private readonly StarlarkStringBuilder _builder;

            public Commentor(StarlarkStringBuilder builder)
            {
                _builder = builder;
                _builder._commentLevel++;
            }

            public void Dispose()
            {
                _builder._commentLevel--;
            }
        }
    }
}