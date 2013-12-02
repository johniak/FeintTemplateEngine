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
            int[] tab = {4,5,6,7,8,9};
            Template template = new Template(text, new { test =true,test2=false,tab=tab });
            Console.WriteLine(template.Parse());
            Console.ReadLine();
        }
    }
}
