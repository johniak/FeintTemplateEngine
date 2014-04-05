using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Core
{
    class TemplateException :Exception
    {
        public TemplateException(String message):base(message)
        {

        }
        public TemplateException()
            : base()
        {

        }

    }
}
