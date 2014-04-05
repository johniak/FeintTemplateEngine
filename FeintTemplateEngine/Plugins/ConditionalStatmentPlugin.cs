using DynamicExpresso;
using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class ConditionalStatmentPlugin : TemplatePlugin
    {
        public ConditionalStatmentPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            RegularExpressionPatterns.Add("^(if[ ]+)(?<condition>.*)$");
        }

        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String condition = lineMatch.Groups["condition"].Value;
            var interpreter = new Interpreter();
            foreach (var element in parameters)
            {
                interpreter.SetVariable(element.Key, element.Value, element.Value.GetType());
            }
            var result = interpreter.Eval<bool>(condition);
            if (result)
                return renderer.RenderBlock(reader.ReadBlock());
            reader.ReadBlock();
            return "";
        }
    }
}
