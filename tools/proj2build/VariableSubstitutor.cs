using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace proj2build
{
    public class VariableSubstitutor
    {
        private Regex _regex;

        public VariableSubstitutor()
        {
            _regex = new Regex("#{([^}]+)}#");
        }

        public string Substitute(string input, Dictionary<string, string> variables)
        {
            return _regex.Replace(input, (match) =>
            {
                return variables[match.Groups[1].Value];
            });
        }
    }
}