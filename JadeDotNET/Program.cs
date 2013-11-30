using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JadeDotNET
{
    class Program
    {
        static void Main(string[] args)
        {
            TextReader reader = File.OpenText("template.jade");
            String text = reader.ReadToEnd();
            Template template = new Template(text, new { test =true,test2=false });
            Console.WriteLine(template.Parse());
            Console.ReadLine();
        }
    }
}
