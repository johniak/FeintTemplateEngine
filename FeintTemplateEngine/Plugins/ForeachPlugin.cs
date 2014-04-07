using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class ForeachPlugin : TemplatePlugin
    {
        public ForeachPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            RegularExpressionPatterns.Add("^(each[ ]+)(?<statment>.*)$");
        }

        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String statment = lineMatch.Groups["statment"].Value;
            return eachParser(statment, reader, parameters);
        }
        private string eachParser(string statment, TemplateReader reader, Dictionary<string, object> parameters)
        {
            string variable = @"([a-zA-Z_][a-zA-Z0-9_]*)([\.]([a-zA-Z_][a-zA-Z0-9_]*))*";
            string indexPattern = "(,[ ]*(?<index>" + variable + "))";
            string iteratorPattern = "(?<iterator>" + variable + ")";
            string collectionPattern="(?<collection>" + variable + ")";
            string optional = "?";
            string regex = iteratorPattern + "[ ]*" + indexPattern + optional + "[ ]+in[ ]+" + collectionPattern;
            Match m = Regex.Match(statment, regex);
            StringBuilder builder = new StringBuilder();
            if (m.Success)
            {
                String indexName = null;
                bool isWithIndex = m.Groups["index"].Success;
                if (isWithIndex)
                    indexName = m.Groups["index"].Value;
                var iteratorName = m.Groups["iterator"].Value;
                var collectionName = m.Groups["collection"].Value;
                string codeBlock = reader.ReadBlock();
                if (codeBlock.Length == 0)
                {
                    return "";
                }
                int index = 0;
                dynamic collection = parameters[collectionName];
                if (isWithIndex)
                    parameters.Add(indexName, 0);
                foreach (var it in collection)
                {
                    parameters.Add(iteratorName, it);
                    if (isWithIndex)
                        parameters[indexName] = index;
                    index++;
                    builder.Append(renderer.RenderBlock(codeBlock));
                    parameters.Remove(iteratorName);
                }
                if (isWithIndex)
                    parameters.Remove(indexName);
            }
            else
            {
                throw new TemplateException("Can't parse each statment.");
            }
            return builder.ToString();
        }
    }
}
