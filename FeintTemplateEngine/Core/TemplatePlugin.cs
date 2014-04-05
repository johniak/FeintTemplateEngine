using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Core
{
    public abstract class TemplatePlugin
    {
        protected TemplateRenderer renderer{get;set;}

        private List<string> _regularExpressionPatterns = new List<string>();

        public List<string> RegularExpressionPatterns { get { return _regularExpressionPatterns; } }

        public TemplatePriority Priority { get; protected set;}

        public enum TemplatePriority
        {
            High,Medium,Low
        }

        public TemplatePlugin(TemplateRenderer renderer)
        {
            this.Priority = TemplatePriority.Medium;
            this.renderer=renderer;
        }

        public abstract String RenderTag(String line, TemplateReader reader, Dictionary<string, object> parameters,int patternIndex);
    }
}
