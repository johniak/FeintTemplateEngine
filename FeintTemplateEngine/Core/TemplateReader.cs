using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Core
{
    public class TemplateReader
    {
        string code;
        public int ActualLineIndex { get; private set; }
        
        string[] lines;
        public int Level { get { return getLevel(lines[ActualLineIndex]); } }
        
        int readedLength;
        public bool CanRead { get { return ActualLineIndex + 1 < lines.Length; } }


        public TemplateReader(string code)
        {
            code=code.Replace("\r","");
            if (code.Length>0&&code[code.Length - 1] == '\n')
                this.code = code.Substring(0, code.Length - 1);
            else
                this.code = code;
            this.lines = this.code.Split('\n');
            ActualLineIndex = -1;
        }

        public string ReadLine()
        {
            readedLength = lines[++ActualLineIndex].Length + 1;
            return lines[ActualLineIndex];
        }

        public string ReadToEnd()
        {
            StringBuilder builder = new StringBuilder();
            while (CanRead)
            {
                builder.Append(ReadLine());
                builder.Append("\n");
            }
            return builder.ToString();
        }

        public string ReadBlock()
        {
            StringBuilder builder = new StringBuilder();
            int blockLevel = Level;
            while (CanRead)
            {
                var line = ReadLine();
                if (Level <= blockLevel)
                {
                    ActualLineIndex--;
                    return builder.ToString();
                }
                builder.Append(line);
                builder.Append("\n");
            }
            if (builder.Length >= 1)
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }
        public string ReadBlockWithIndent(int indent)
        {
            StringBuilder builder = new StringBuilder();
            int blockLevel = Level;
            while (CanRead)
            {
                var line = ReadLine();
                if (Level <= blockLevel)
                {
                    ActualLineIndex--;
                    return builder.ToString();
                }
                builder.Append(TemplateRendererUtils.CreateIndent(indent));
                builder.Append(line.TrimStart());
                builder.Append("\n");
            }
            if (builder.Length >= 1)
                builder.Remove(builder.Length - 1, 1);
            return builder.ToString();
        }

        public void MoveToLine(int index)
        {
            ActualLineIndex = index;
        }




        /// <summary>
        /// Gets nested level
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        int getLevel(string line)
        {
            for (int i = 0; i < line.Length; i++)
            {
                if (line[i] != '\t')
                {
                    return i;
                }
            }
            return 0;
        }
    }
}
