using FeintTemplateEngine.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FeintTemplateEngine.Plugins
{
    class HtmlTagPlugin : TemplatePlugin
    {

        public HtmlTagPlugin(TemplateRenderer renderer)
            : base(renderer)
        {
            Priority = TemplatePriority.Low;
            string tagNamePattern = "(?<tag>[a-zA-Z][a-zA-Z0-9_]*)";
            string attributes = "(\\((?<attr>[a-zA-Z0-9_\\-]+([ ]?=[ ]?(\".*?\")))*[ ]*(,[ ]*(?<attr>[a-zA-Z0-9_\\-]+([ ]?=[ ]?(\".*?\")))*[ ]*)*\\))";
            string idPattern = "(#(?<id>[a-zA-Z_][a-zA-Z_\\-09]*))";
            string classPattern = "(\\.(?<class>[a-zA-Z_][a-zA-Z_\\-09]*))";
            string textPattern = "( (?<text>[^\\(]*.*)|$)";
            string optional = "?";
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + idPattern + optional + classPattern + optional + textPattern + "$");
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + classPattern + optional + idPattern + optional + textPattern + "$");
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + idPattern + optional + classPattern + optional + attributes + textPattern + "$");
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + classPattern + optional + idPattern + optional + attributes + textPattern + "$");
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + classPattern + optional + idPattern + optional + attributes +optional+"\\.[w]*$");
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + idPattern + optional + classPattern + optional + attributes + optional+"\\.[w]*$");
            base.RegularExpressionPatterns.Add("^" + idPattern + classPattern + optional + attributes + optional + textPattern + optional + "$");
            base.RegularExpressionPatterns.Add("^" + classPattern + idPattern + optional + attributes + optional + textPattern + optional + "$");
            base.RegularExpressionPatterns.Add("^" + idPattern + classPattern + optional + attributes + optional + textPattern + optional + "\\.[w]*$");
            base.RegularExpressionPatterns.Add("^" + classPattern + idPattern + optional + attributes + optional + textPattern + optional + "\\.[w]*$");


        }

        public override string RenderTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            switch (patternIndex)
            {
                case 0:
                    return renderStandardHtmlTag(line, reader, parameters, patternIndex);
                case 1:
                    return renderStandardHtmlTag(line, reader, parameters, patternIndex);
                case 2:
                    return renderTagWithAttributes(line, reader, parameters, patternIndex);
                case 3:
                    return renderTagWithAttributes(line, reader, parameters, patternIndex);
                case 4:
                    return renderTagWithAttributesWithTextBlock(line, reader, parameters, patternIndex);
                case 5:
                    return renderTagWithAttributesWithTextBlock(line, reader, parameters, patternIndex);
                case 6:
                    return renderShortcutDiv(line, reader, parameters, patternIndex);
                case 7:
                    return renderShortcutDiv(line, reader, parameters, patternIndex);
                case 8:
                    return renderShortcutDivWithTextBlock(line, reader, parameters, patternIndex);
                case 9:
                    return renderShortcutDivWithTextBlock(line, reader, parameters, patternIndex);
            }
            throw new TemplateException("Can't parse html tag.");
        }
        private string renderStandardHtmlTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = lineMatch.Groups["tag"].Value;
            String text = "";
            List<string> attributes = new List<string>();
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value + "\n";
            }
            StringBuilder textBuilder = new StringBuilder();
            if (text.Length > 0)
                textBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent+1));
            textBuilder.Append(text);
            String sourceBlock = reader.ReadBlock();
            renderer.LineIndent++;
            textBuilder.Append(renderer.RenderBlock(sourceBlock));
            renderer.LineIndent--;

            if (lineMatch.Groups["id"].Success)
            {
                attributes.Add("id=\"" + lineMatch.Groups["id"].Value + "\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                attributes.Add("class=\"" + lineMatch.Groups["class"].Value + "\"");
            }
            return generateTag(tag, attributes, textBuilder.ToString());
        }
        private string renderTagWithAttributes(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = lineMatch.Groups["tag"].Value;
            String text = "";
            List<string> attributes = new List<string>();
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder textBuilder = new StringBuilder();
            if (text.Length > 0)
                textBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
            textBuilder.Append(text);
            String sourceBlock = reader.ReadBlock();
            renderer.LineIndent++;
            textBuilder.Append(renderer.RenderBlock(sourceBlock));
            renderer.LineIndent--;
            if (lineMatch.Groups["id"].Success)
            {
                attributes.Add("id=\"" + lineMatch.Groups["id"].Value + "\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                attributes.Add("class=\"" + lineMatch.Groups["class"].Value + "\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                attributes.Add(cap.Value.Trim());
            }
            return generateTag(tag, attributes, textBuilder.ToString());
        }
        private string renderTagWithAttributesWithTextBlock(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = lineMatch.Groups["tag"].Value;
            String text = "";
            int level = reader.Level;
            List<string> attributes = new List<string>();
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder textBuilder = new StringBuilder();
            if (text.Length > 0)
                textBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
            textBuilder.Append(text);
            renderer.LineIndent++;
            String sourceBlock = reader.ReadBlockWithIndent(renderer.LineIndent);
            sourceBlock = sourceBlock.TrimEnd('\n');
            textBuilder.Append(sourceBlock);
            textBuilder.Append("\n");
            renderer.LineIndent--;
            if (lineMatch.Groups["id"].Success)
            {
                attributes.Add("id=\"" + lineMatch.Groups["id"].Value + "\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                attributes.Add("class=\"" + lineMatch.Groups["class"].Value + "\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                attributes.Add(cap.Value.Trim());
            }
            return generateTag(tag, attributes, textBuilder.ToString());
        }
        private string renderShortcutDiv(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = "div";
            String text = "";
            List<string> attributes = new List<string>();
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder textBuilder = new StringBuilder();
            if (text.Length > 0)
                textBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
            textBuilder.Append(text);
            String sourceBlock = reader.ReadBlock();
            renderer.LineIndent++;
            textBuilder.Append(renderer.RenderBlock(sourceBlock));
            renderer.LineIndent--;
            if (lineMatch.Groups["id"].Success)
            {
                attributes.Add("id=\"" + lineMatch.Groups["id"].Value + "\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                attributes.Add("class=\"" + lineMatch.Groups["class"].Value + "\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                attributes.Add(cap.Value.Trim());
            }
            return generateTag(tag, attributes, textBuilder.ToString());
        }
        private string renderShortcutDivWithTextBlock(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = "div";
            String text = "";
            List<string> attributes = new List<string>();
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder textBuilder = new StringBuilder();
            if (text.Length > 0)
                textBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
            textBuilder.Append(text);
            renderer.LineIndent++;
            String sourceBlock = reader.ReadBlockWithIndent(renderer.LineIndent);
            sourceBlock = sourceBlock.TrimEnd('\n');
            textBuilder.Append(sourceBlock);
            textBuilder.Append("\n");
            renderer.LineIndent--;
            if (lineMatch.Groups["id"].Success)
            {
                attributes.Add("id=\"" + lineMatch.Groups["id"].Value + "\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                attributes.Add("class=\"" + lineMatch.Groups["class"].Value + "\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                attributes.Add(cap.Value.Trim());
            }
            return generateTag(tag, attributes, textBuilder.ToString());
        }


        private string generateTag(String tag, List<String> attributes, String inside)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            sb.Append("<");
            sb.Append(tag);
            if (attributes != null)
            {
                foreach (var a in attributes)
                {
                    sb.Append(" ");
                    sb.Append(a);
                }
            }
            if (inside != null && inside.Length != 0)
            {
                sb.Append(">\n");
                sb.Append(inside);
                sb.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
                sb.Append("</");
                sb.Append(tag);
                sb.Append(">\n");
            }
            else
            {
                sb.Append("/>\n");
            }
            return sb.ToString();
        }

    }
}
