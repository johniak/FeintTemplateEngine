using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class TextPlugin : TemplatePlugin
    {
        public TextPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            Priority = TemplatePriority.Low;
            RegularExpressionPatterns.Add("^(\\|)(?<text>.*)$");
        }

        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String text = lineMatch.Groups["text"].Value;
            return TemplateRendererUtils.CreateIndent(renderer.LineIndent) + text + "\n";
        }
    }
}
