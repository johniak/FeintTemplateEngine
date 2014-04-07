using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class BlockPlugin : TemplatePlugin
    {
        Dictionary<string, string> blocks = new Dictionary<string, string>();
        public BlockPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            RegularExpressionPatterns.Add("^(block[ ]+)(?<statment>.*)[ ]*$");
        }

        /// <summary>
        /// If in child template collect blocks
        /// If in parrent render blocks using child blocks
        /// </summary>
        /// <param name="line"></param>
        /// <param name="reader"></param>
        /// <param name="parameters"></param>
        /// <param name="patternIndex"></param>
        /// <returns></returns>
        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String statment = lineMatch.Groups["statment"].Value;
            if (!renderer.GetPlugin<ExtendsPlugin>().IsInParrent)
            {
                blocks.Add(statment, reader.ReadBlock());
                return "";
            }
            StringBuilder renderedSource = new StringBuilder();
            renderedSource.Append(renderer.RenderBlock(reader.ReadBlock()));
            if (blocks.ContainsKey(statment))
            {
                renderedSource.Append(renderer.RenderBlock(blocks[statment]));
            }
            return renderedSource.ToString();
        }
    }
}
