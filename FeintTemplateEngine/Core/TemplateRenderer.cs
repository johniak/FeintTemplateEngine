using DynamicExpresso;
using FeintTemplateEngine.Plugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Core
{
    public class TemplateRenderer
    {
        protected List<TemplatePlugin> plugins = new List<TemplatePlugin>();
        
        protected Dictionary<Type,TemplatePlugin> pluginsDictionary= new Dictionary<Type,TemplatePlugin>();
        public Dictionary<string, object> parameters { get; protected set; }
        

        private string sourceCode;
        public int LineIndent { get; set;}
        

        public TemplateRenderer(String sourceCode, Dictionary<string, object> parameters)
        {
            initialze();
            this.sourceCode = sourceCode;
            this.parameters = parameters;
        }

        protected void initialze()
        {
            addPlugin(new HtmlTagPlugin(this));
            addPlugin(new TextPlugin(this));
            addPlugin(new ConditionalStatmentPlugin(this));
            addPlugin(new ForeachPlugin(this));
            addPlugin(new ExtendsPlugin(this));
            addPlugin(new BlockPlugin(this));
            addPlugin(new DoctypePlugin(this));
            addPlugin(new MixinPlugin(this));
            plugins.Sort(templatePluginComparator);
        }

        public String RenderBlock(String sourceCode)
        {
            StringBuilder renderedStringBuilder = new StringBuilder();
            TemplateReader reader = new TemplateReader(sourceCode);
            while (reader.CanRead)
            {
                String line = reader.ReadLine();
                line = variablePreRenderer(line);
                String lineTrimed = line.Trim();
                for (int j = 0; j < plugins.Count;j++ )
                {
                    var plugin = plugins[j];
                    for (int i = 0; i < plugin.RegularExpressionPatterns.Count; i++)
                    {
                        string pattern = plugin.RegularExpressionPatterns[i];
                        if (Regex.IsMatch(lineTrimed, pattern))
                        {
                            String renderedCode = plugin.RenderTag(line, reader, parameters,i);
                            renderedStringBuilder.Append(renderedCode);
                            j = plugins.Count;
                            break;
                        }
                    }

                }
            }
            return renderedStringBuilder.ToString();
        }
        private string variablePreRenderer(String line)
        {
            String pattern="^(.*(?<repl>{{(?<expression>.*)}}).*)+$";
            Match match = Regex.Match(line, pattern);
            var repls=match.Groups["repl"].Captures;
            var expressions=match.Groups["expression"].Captures;
            var interpreter = new Interpreter();
            foreach (var element in parameters)
            {
                interpreter.SetVariable(element.Key, element.Value, element.Value.GetType());
            }
            for(int i=0;i<repls.Count;i++)
            {
                var repl = repls[i];
                var expression = expressions[i];
                var value = interpreter.Eval(expression.Value);
                line = line.Replace(repl.Value, value.ToString());
            }
            if (Regex.IsMatch(line, pattern))
            {
                line = variablePreRenderer(line);
            }
            return line;
        }
        public String RenderAllBraceExpressions(String line, Dictionary<String, Object> parameters)
        {
            return line;
        }
        private int templatePluginComparator(TemplatePlugin t1,TemplatePlugin t2)
        {
            if (t1.Priority == t2.Priority)
                return 0;
            else if (t1.Priority > t2.Priority)
                return 1;
            return -1;
        }
        protected void addPlugin(TemplatePlugin plugin)
        {
            pluginsDictionary.Add(plugin.GetType(), plugin);
            plugins.Add(plugin);
        }
        public T GetPlugin<T>()where T:TemplatePlugin
        {
            return (T)pluginsDictionary[typeof(T)];
        }
        

    }
}
