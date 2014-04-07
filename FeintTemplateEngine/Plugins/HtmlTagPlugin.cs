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
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + classPattern + optional + idPattern + optional + attributes + "?\\.[w]*$");
            base.RegularExpressionPatterns.Add("^" + tagNamePattern + idPattern + optional + classPattern + optional + attributes + "?\\.[w]*$");
            base.RegularExpressionPatterns.Add("^" + idPattern + classPattern + optional + attributes + optional + textPattern + optional + "$");
            base.RegularExpressionPatterns.Add("^" + classPattern + idPattern + optional + attributes + optional + textPattern + optional + "$");


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
            }
            throw new TemplateException("Can't parse html tag.");
        }
        private string renderStandardHtmlTag(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = lineMatch.Groups["tag"].Value;
            String text = "";
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder renderedBuilder = new StringBuilder();
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("<");
            renderedBuilder.Append(tag);
            if (lineMatch.Groups["id"].Success)
            {
                renderedBuilder.Append(" id=\"");
                renderedBuilder.Append(lineMatch.Groups["id"].Value);
                renderedBuilder.Append("\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                renderedBuilder.Append(" class=\"");
                renderedBuilder.Append(lineMatch.Groups["class"].Value);
                renderedBuilder.Append("\"");
            }
            renderedBuilder.Append(">\n");
            if (text.Length > 0)
            {
                renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
                renderedBuilder.Append(text);
                renderedBuilder.Append("\n");
            }
            String sourceBlock = reader.ReadBlock();
            renderer.LineIndent++;
            renderedBuilder.Append(renderer.RenderBlock(sourceBlock));
            renderer.LineIndent--;
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("</");
            renderedBuilder.Append(tag);
            renderedBuilder.Append(">\n");
            return renderedBuilder.ToString();
        }
        private string renderTagWithAttributes(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = lineMatch.Groups["tag"].Value;
            String text = "";
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder renderedBuilder = new StringBuilder();
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("<");
            renderedBuilder.Append(tag);
            if (lineMatch.Groups["id"].Success)
            {
                renderedBuilder.Append(" id=\"");
                renderedBuilder.Append(lineMatch.Groups["id"].Value);
                renderedBuilder.Append("\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                renderedBuilder.Append(" class=\"");
                renderedBuilder.Append(lineMatch.Groups["class"].Value);
                renderedBuilder.Append("\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                renderedBuilder.Append(" ");
                renderedBuilder.Append(cap.Value.Trim());
            }
            renderedBuilder.Append(">\n");
            if (text.Length > 0)
            {
                renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
                renderedBuilder.Append(text);
                renderedBuilder.Append("\n");
            }

            String sourceBlock = reader.ReadBlock();
            renderer.LineIndent++;
            renderedBuilder.Append(renderer.RenderBlock(sourceBlock));
            renderer.LineIndent--;
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("</");
            renderedBuilder.Append(tag);
            renderedBuilder.Append(">\n");
            return renderedBuilder.ToString();
        }
        private string renderTagWithAttributesWithTextBlock(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = lineMatch.Groups["tag"].Value;
            String text = "";
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder renderedBuilder = new StringBuilder();
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("<");
            renderedBuilder.Append(tag);
            if (lineMatch.Groups["id"].Success)
            {
                renderedBuilder.Append(" id=\"");
                renderedBuilder.Append(lineMatch.Groups["id"].Value);
                renderedBuilder.Append("\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                renderedBuilder.Append(" class=\"");
                renderedBuilder.Append(lineMatch.Groups["class"].Value);
                renderedBuilder.Append("\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                renderedBuilder.Append(" ");
                renderedBuilder.Append(cap.Value.Trim());
            }
            renderedBuilder.Append(">\n");
            if (text.Length > 0)
            {
                renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
                renderedBuilder.Append(text);
                renderedBuilder.Append("\n");
            }
            renderer.LineIndent++;
            String sourceBlock = reader.ReadBlockWithIndent(renderer.LineIndent);
            sourceBlock = sourceBlock.TrimEnd('\n');
            renderedBuilder.Append(sourceBlock);
            renderedBuilder.Append("\n");
            renderer.LineIndent--;
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("</");
            renderedBuilder.Append(tag);
            renderedBuilder.Append(">\n");
            return renderedBuilder.ToString();
        }
        private string renderShortcutDiv(string line, TemplateReader reader, Dictionary<string, object> parameters, int patternIndex)
        {
            Match lineMatch = Regex.Match(line.Trim(), RegularExpressionPatterns[patternIndex]);
            String tag = "div";
            String text = "";
            int level = reader.Level;
            if (lineMatch.Groups["text"].Success)
            {
                text = lineMatch.Groups["text"].Value;
            }
            StringBuilder renderedBuilder = new StringBuilder();
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("<");
            renderedBuilder.Append(tag);
            if (lineMatch.Groups["id"].Success)
            {
                renderedBuilder.Append(" id=\"");
                renderedBuilder.Append(lineMatch.Groups["id"].Value);
                renderedBuilder.Append("\"");
            }
            if (lineMatch.Groups["class"].Success)
            {
                renderedBuilder.Append(" class=\"");
                renderedBuilder.Append(lineMatch.Groups["class"].Value);
                renderedBuilder.Append("\"");
            }
            foreach (Capture cap in lineMatch.Groups["attr"].Captures)
            {
                renderedBuilder.Append(" ");
                renderedBuilder.Append(cap.Value.Trim());
            }
            renderedBuilder.Append(">\n");
            if (text.Length > 0)
            {
                renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent + 1));
                renderedBuilder.Append(text);
                renderedBuilder.Append("\n");
            }

            String sourceBlock = reader.ReadBlock();
            renderer.LineIndent++;
            renderedBuilder.Append(renderer.RenderBlock(sourceBlock));
            renderer.LineIndent--;
            renderedBuilder.Append(TemplateRendererUtils.CreateIndent(renderer.LineIndent));
            renderedBuilder.Append("</");
            renderedBuilder.Append(tag);
            renderedBuilder.Append(">\n");
            return renderedBuilder.ToString();
        }
    }
}
