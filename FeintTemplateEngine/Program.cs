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
            //TextReader reader = File.OpenText("child_template.fte");
            TextReader reader = File.OpenText("child_template.fte");
            String text = reader.ReadToEnd();
            reader.Close();
            String[] friends={"Piotr","Asia","Paweł","Mateusz","Rafał"};
            TemplateEngine templateEngine = new TemplateEngine(text, new { friends = friends });
            var rendered = templateEngine.Render();
            rendered = rendered.Replace('\r', '\n');
            Console.WriteLine(rendered);
            Console.Read();
        }
    }
}
