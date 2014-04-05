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
            return eachParser(statment, reader,parameters);
        }
        private string eachParser(string statment, TemplateReader reader, Dictionary<string, object> parameters)
        {
            string variable = @"([a-zA-Z_][a-zA-Z0-9_]*)([\.]([a-zA-Z_][a-zA-Z0-9_]*))*";
            string regex = "((?<iterator>" + variable + ")[ ]*,[ ]*)?[ ]*(?<index>" + variable + ")[ ]+in[ ]+(?<collection>" + variable + ")";
            Match m = Regex.Match(statment, regex);
            StringBuilder builder = new StringBuilder();
            if (m.Success)
            {
                var indexName = m.Groups["index"].Value;
                var iteratorName = m.Groups["iterator"].Value;
                var collectionName = m.Groups["collection"].Value;
                string codeBlock = reader.ReadBlock();
                if (codeBlock.Length == 0)
                {
                    return "";
                }
                int index = 0;
                dynamic collection = parameters[collectionName];
                parameters.Add(indexName, 0);
                foreach (var it in collection)
                {
                    parameters.Add(iteratorName, it);
                    parameters[indexName] = index++;
                    builder.Append(renderer.RenderBlock(codeBlock));
                    parameters.Remove(iteratorName);
                }
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
