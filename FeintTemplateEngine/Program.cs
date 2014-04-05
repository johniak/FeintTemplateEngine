using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FeintTemplateEngine
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            TextReader reader = File.OpenText("template.jade");
            String text = reader.ReadToEnd();
            String[] texts={"adam","michał","agniszka"};
            TemplateEngine templateEngine = new TemplateEngine(text, new  {collection=texts });
            var rendered = templateEngine.Render();
            rendered = rendered.Replace('\r', '\n');
            Console.WriteLine(rendered);
            Console.Read();
        }
    }
}
