using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JadeDotNET
{
    class TemplateReader
    {
        string code;
        public int ActualLineIndex { get; private set; }
        string[] lines;
        public int Level { get { return getLevel(lines[ActualLineIndex]); } }
        int readedLength;
        public bool CanRead { get { return ActualLineIndex + 1 < lines.Length; } }


        public TemplateReader(string code)
        {
            if (code[code.Length - 1] == '\n')
                this.code = code.Substring(0,code.Length - 1);
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

        public string ReadBlock()
        {
            StringBuilder builder = new StringBuilder();
            int blockLevel = Level;
            while(CanRead)
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
