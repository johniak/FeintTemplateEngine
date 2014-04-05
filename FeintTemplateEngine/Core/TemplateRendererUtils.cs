using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Core
{
    public class TemplateRendererUtils
    {
        public static string CreateIndent(int level)
        {
            String str = "";
            for (int i = 0; i < level; i++)
            {
                str += "\t";
            }
            return str;
        }

    }
}
