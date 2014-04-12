using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class DoctypePlugin: TemplatePlugin
    {
        public DoctypePlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            RegularExpressionPatterns.Add("^(doctype[ ]+)(?<statment>.*)[ ]*$");
        }
        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match m = Regex.Match(line.Trim(),RegularExpressionPatterns[patternIndex]);
            String type = m.Groups["statment"].Value;
            type = type.Trim();
            string realType;
            switch (type)
            {
                case "xml":
                    realType = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
                    break;
                case "transitional":
                    realType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";
                    break;
                case "strict":
                    realType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";
                    break;
                case "frameset":
                    realType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Frameset//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-frameset.dtd\">";
                    break;
                case "1.1":
                    realType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.1//EN\" \"http://www.w3.org/TR/xhtml11/DTD/xhtml11.dtd\">";
                    break;
                case "basic":
                    realType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML Basic 1.1//EN\" \"http://www.w3.org/TR/xhtml-basic/xhtml-basic11.dtd\">";
                    break;
                case "mobile":
                    realType = "<!DOCTYPE html PUBLIC \"-//WAPFORUM//DTD XHTML Mobile 1.2//EN\" \"http://www.openmobilealliance.org/tech/DTD/xhtml-mobile12.dtd\">";
                    break;
                default:
                    realType = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML Basic 1.1//EN\" \"http://www.w3.org/TR/xhtml-basic/xhtml-basic11.dtd\">";
                    break;
            }
            return realType + "\n";
        }
    }
}
