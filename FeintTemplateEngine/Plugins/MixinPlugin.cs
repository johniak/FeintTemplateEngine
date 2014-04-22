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
    class MixinPlugin : TemplatePlugin
    {
        Dictionary<string, Mixin> mixins = new Dictionary<string, Mixin>();
        string namePattern = "(?<name>[a-zA-Z][a-zA-Z0-9_]*)";
        string attributeName = "(?<attr_name>[a-zA-Z][a-zA-Z0-9_]*)";
        string zeroOrMore = "*";
        string optional = "?";
        public MixinPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            // string oneOrZeroAttribute = "\\((" + attributeName + optional + ")\\)";
            string lp = "\\(";
            string rp = "\\)";
            string emptyAttributes = "(" + lp + "[ ]*" + rp + ")";
            string oneOrMoreAttributes = "(" + lp + attributeName + "(," + attributeName + ")*" + rp + ")";
            string parameters = "(" + lp + "(?<param>[^,]*)(,(?<param>[^,]*))*" + rp + ")";
            RegularExpressionPatterns.Add("^mixin[ ]+" + namePattern + "[ ]*$");
            RegularExpressionPatterns.Add("^mixin[ ]+" + namePattern + emptyAttributes + "$");
            RegularExpressionPatterns.Add("^mixin[ ]+" + namePattern + oneOrMoreAttributes + "$");

            RegularExpressionPatterns.Add("^\\+" + namePattern + emptyAttributes + "$");
            RegularExpressionPatterns.Add("^\\+" + namePattern + parameters + "$");
        }
        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            if (patternIndex < 3)
            {
                return declareMixin(line, reader, patternIndex);
            }
            else
            {
                return callMixin(line, reader, parameters, patternIndex);
            }
        }
        public string declareMixin(string line, TemplateReader reader, int patternIndex)
        {
            Match m = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            Mixin mixin = new Mixin();
            String mixinName = m.Groups["name"].Value;
            foreach (Capture cap in m.Groups["attr_name"].Captures)
            {
                mixin.parameters.Add(cap.Value);
            }
            String block = reader.ReadBlockWithIndent(0);
            mixin.block = block;
            mixins.Add(mixinName, mixin);
            return "";
        }
        public string callMixin(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match m = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            List<object> mixinParams = new List<object>();
            String mixinName = m.Groups["name"].Value;
            Mixin mixin = mixins[mixinName];
            var interpreter = new Interpreter();
            Dictionary<string, object> outerParameters = new Dictionary<string, object>();
            foreach (var element in parameters)
            {
                interpreter.SetVariable(element.Key, element.Value, element.Value.GetType());
            }
            foreach (Capture cap in m.Groups["param"].Captures)
            {
                var result = interpreter.Eval(cap.Value);
                mixinParams.Add(result);
            }
            if (mixinParams.Count != mixin.parameters.Count)
                throw new TemplateException("Missmatch number of mixin arguments");
            for (int i = 0; i < mixinParams.Count; i++)
            {
                if (parameters.ContainsKey(mixin.parameters[i]))
                {
                    outerParameters.Add(mixin.parameters[i], parameters[mixin.parameters[i]]);
                    parameters[mixin.parameters[i]] = mixinParams[i];
                }
                else
                {
                    parameters.Add(mixin.parameters[i], mixinParams[i]);
                }
            }
            String blockRendered = renderer.RenderBlock(mixin.block);
            foreach(var n in mixin.parameters)
            {
                parameters.Remove(n);
            }
            foreach(var p in outerParameters)
            {
                parameters.Add(p.Key, p.Value);
            }
            return blockRendered;
        }
    }
    class Mixin
    {
        public List<String> parameters { get; private set; }
        public String block { get; set; }

        public Mixin()
        {
            parameters = new List<string>();
        }
    }
}
