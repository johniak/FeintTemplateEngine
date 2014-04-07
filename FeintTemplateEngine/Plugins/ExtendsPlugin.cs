using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class ExtendsPlugin: TemplatePlugin
    {
        public String Parent { get; protected set; }

        public bool IsInParrent { get; protected set;}
        public ExtendsPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            RegularExpressionPatterns.Add("^(extends[ ]+)(?<statment>.*)[ ]*$");
        }

        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String statment = lineMatch.Groups["statment"].Value;
            if (statment.Contains(" "))
                throw new TemplateException("Name of template can't contains space");
            Parent = statment;
            IsInParrent = false;
            String childSource = reader.ReadToEnd();
            renderer.RenderBlock(childSource);
            TextReader fileReader = File.OpenText(statment+".fte");
            String parrentSource = fileReader.ReadToEnd();
            fileReader.Close();
            IsInParrent = true;
            return renderer.RenderBlock(parrentSource);           
        }
    }
}
