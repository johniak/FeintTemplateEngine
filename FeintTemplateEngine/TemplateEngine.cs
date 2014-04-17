using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace FeintTemplateEngine
{
    public class TemplateEngine
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        string sourceCode;
        TemplateRenderer renderer;
        public TemplateEngine(String sourceCode, Object parameters)
        {
            this.sourceCode = sourceCode;
            this.parameters = getVariablesFromObject(parameters);
            initialze();
        }
        public TemplateEngine(String sourceCode, Dictionary<string, object> parameters)
        {
            this.sourceCode = sourceCode;
            this.parameters = parameters;
            initialze();
        }
        protected void initialze()
        {
            renderer = new TemplateRenderer(sourceCode, parameters);
        }


        public String Render()
        {
            var rendered = renderer.RenderBlock(sourceCode);
            rendered = rendered.Replace('\r', '\n');
            return rendered;
        }

        Dictionary<string, object> getVariablesFromObject(object obj)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var f in fields)
            {
                dict.Add(f.Name, f.GetValue(obj));
            }
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in properties)
            {
                dict.Add(p.Name, p.GetValue(obj));
            }
            return dict;
        }
        public static TemplateEngine FromFile(String filename, Dictionary<string, object> parameters)
        {
            return new TemplateEngine(readFile(filename), parameters);
        }

        public static TemplateEngine FromFile(String filename, Object parameters)
        {
            return new TemplateEngine(readFile(filename), parameters);
        }
        private static string readFile(String filename)
        {
            var reader = new StreamReader(filename, Encoding.UTF8,true);
            return reader.ReadToEnd();
        }
    }
}
