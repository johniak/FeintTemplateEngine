using DynamicExpresso;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JadeDotNET
{
    class Template
    {
        Dictionary<string, object> parameters = new Dictionary<string, object>();
        List<String> restircedStartString = new List<string>();
        string code;
        int level = -1;


        const int BRACKET_OPEN_VALUE = 0;
        const int OPERATOR_OR_ID = 1;
        const int OPERATOR_AND_ID = 2;
        const int OPERATOR_EQ_ID = 3;
        const int OPERATOR_NEQ_ID = 4;
        const int OPERATOR_G_ID = 5;
        const int OPERATOR_L_ID = 6;
        const int OPERATOR_GE_ID = 7;
        const int OPERATOR_LE_ID = 8;


        public Template(String code, Object parameters)
        {
            this.code = code;
            this.parameters = getVariablesFromObject(parameters);
        }
        public Template(String code, Dictionary<string, object> parameters)
        {
            this.code = code;
            this.parameters = parameters;
        }

        public string Parse()
        {
            string rendered = "";
            rendered = parseBlock(code);
            
            rendered = rendered.Trim();
            
            return rendered;
        }

        private string parseBlock(string codeBlock)
        {

            List<String> tagStack = new List<string>();
            string rendered = "";
            TemplateReader reader = new TemplateReader(codeBlock);
            int level;
            string line;
            StringBuilder stringBuilder = new StringBuilder();
            Match m;
            String paramsRegex = "(\\((?<attr>[a-zA-Z0-9_\\-]+([ ]?=[ ]?(\".*?\")))*[ ]*(,[ ]*(?<attr>[a-zA-Z0-9_\\-]+([ ]?=[ ]?(\".*?\")))*[ ]*)*\\))";
            while (reader.CanRead)
            {
                line = reader.ReadLine();
                level = reader.Level;
                
                stringBuilder.Append("\n");
                stringBuilder.Append(createLevel(level));
                if (level < this.level)
                {
                    stringBuilder.Append("</");
                    stringBuilder.Append(tagStack[tagStack.Count - 1]);
                    stringBuilder.Append(">");
                    this.level = level;
                }
                line = line.Trim();
                m = Regex.Match(line, "^if[ ]+(.*)");
                if (m.Success)
                {
                    stringBuilder.Remove(stringBuilder.Length - level-1, level+1);
                   stringBuilder.Append(ifParser(reader,m.Groups[1].Value));
                    continue;
                }

                m = Regex.Match(line, "^each[ ]+(.*)");
                if (m.Success)
                {
                    stringBuilder.Remove(stringBuilder.Length - level - 1, level + 1);
                    stringBuilder.Append(eachParser(reader, m.Groups[1].Value));
                    continue;
                }



                m = Regex.Match(line, "^(?<tag>[a-zA-Z]+[0-6]?)" + paramsRegex + "?(?<text>.*)$");
                if (m.Success)
                {
                    if (level == this.level)
                    {
                        stringBuilder.Append("</");
                        stringBuilder.Append(tagStack[tagStack.Count - 1]);
                        tagStack.RemoveAt(tagStack.Count - 1);
                        stringBuilder.Append(">");
                        stringBuilder.Append("\n");
                        stringBuilder.Append(createLevel(level));
                        //  this.level--;
                    }
                    else
                    {
                        this.level = level;
                    }
                    String tag = m.Groups["tag"].Value;
                    stringBuilder.Append("<");
                    stringBuilder.Append(tag);
                    tagStack.Add(tag);
                    stringBuilder.Append(" ");
                    List<string> attrs = new List<string>();
                    foreach (Capture cap in m.Groups["attr"].Captures)
                    {
                        attrs.Add(cap.Value);
                        stringBuilder.Append(cap.Value.Trim() + " ");
                    }
                    stringBuilder.Append(">");
                    if (m.Groups["text"].Success)
                    {
                        var expression = m.Groups["text"].Value;
                        if (expression.Trim().StartsWith("="))
                        {
                            expression = parseExpression(expression);
                        }
                        stringBuilder.Append(expression);
                    }
                }
                m = Regex.Match(line, "^(?<tag>[a-zA-Z]+[0-6]?) (?<text>.*)$");
                if (m.Success)
                {
                    if (level == this.level)
                    {
                        stringBuilder.Append("</");
                        stringBuilder.Append(tagStack[tagStack.Count - 1]);
                        tagStack.RemoveAt(tagStack.Count - 1);
                        stringBuilder.Append(">");
                        stringBuilder.Append("\n");
                        stringBuilder.Append(createLevel(level));
                        this.level--;
                    }
                    else
                    {
                        this.level = level;
                    }
                    String tag = m.Groups["tag"].Value;
                    String text = m.Groups["text"].Value;
                    stringBuilder.Append("<");
                    stringBuilder.Append(tag);
                    stringBuilder.Append(" >");
                    stringBuilder.Append(text);
                    stringBuilder.Append("</");
                    stringBuilder.Append(tag);
                    stringBuilder.Append(">");
                }
                else
                {

                }
            }
            rendered = stringBuilder.ToString();
            for (int i = tagStack.Count - 1; i >= 0; i--)
            {
                var tag = tagStack[i];
                line = "\n" + createLevel(this.level--);
                line += "</" + tag + ">";
                rendered += line;
            }
            return rendered;
        }

        string ifParser(TemplateReader reader, string condition)
        {
            String rpnCondition = toRPNLogic(condition);
            List<String> conditionList = new List<string>(rpnCondition.Split(' '));
            bool result = rpnConditionChecker(conditionList);
            int level= reader.Level;
            String ifBlock = reader.ReadBlock();
            String elseBlock = null;
            if (reader.CanRead)
            {
                String lineAfter = reader.ReadLine();
                if (level == reader.Level && lineAfter.Trim() == "else")
                {
                    elseBlock = reader.ReadBlock();
                }
                else
                {
                    reader.MoveToLine(reader.ActualLineIndex - 1);
                }
            }
            if (result)
            {
                return parseBlock(ifBlock);
            }
            else if (elseBlock!=null)
            {
                return parseBlock(elseBlock);
            }

            return "";
        }

        string eachParser(TemplateReader reader, string condition)
        {
            string variable =@"([a-zA-Z_][a-zA-Z0-9_-]*)([\.]([a-zA-Z_][a-zA-Z0-9_\-]*))*";
            string regex = "((?<index>"+variable+")[ ]*,[ ]*)?[ ]*(?<iterator>"+variable+")[ ]+in[ ]+(?<collection>"+variable+")";
            Match m = Regex.Match(condition, regex);
            StringBuilder builder = new StringBuilder();
            if (m.Success)
            {
                var indexName = m.Groups["index"].Value;
                var iteratorName = m.Groups["iterator"].Value;
                var collectionName = m.Groups["collection"].Value;
                int index = 0;
                dynamic collection = getVariable(collectionName);
                string codeBlock = reader.ReadBlock();
                if (codeBlock.Length == 0)
                {
                    return "";
                }
                parameters.Add(indexName, 0);
                foreach (var it in collection)
                {
                    parameters.Add(iteratorName, it);
                    parameters[indexName] = index++;
                    builder.Append(parseBlock(codeBlock));
                    parameters.Remove(iteratorName);
                }
                parameters.Remove(indexName);
            }
            else
            {
                throw new Exception("Can't parse each condition!");
            }
            return builder.ToString();
        }

        bool rpnConditionChecker(List<String> condition)
        {
            List<string> stack = new List<string>();
            bool result = false;
            for (int i = 0; i < condition.Count; i++)
            {
                string cond = condition[i];
                if (cond == "&&" || cond == "||" || cond == "==" || cond == "!=" || cond == ">=" || cond == "<=")
                {
                    dynamic var1 = getVariable(stack[0]);
                    dynamic var2 = getVariable(stack[1]);
                    var operation = cond;
                    switch (operation)
                    {
                        case "&&":
                            result = var1 && var2;
                            break;
                        case "||":
                            result = var1 || var2;
                            break;
                        case "==":
                            result = var1 == var2;
                            break;
                        case "!=":
                            result = var1 == var2;
                            break;
                        case ">=":
                            result = var1 >= var2;
                            break;
                        case "<=":
                            result = var1 <= var2;
                            break;
                    }
                    stack.RemoveAt(0);
                    stack[0] = result.ToString().ToLower();
                }
                else
                {
                    stack.Insert(0, cond);
                }
            }
            return result;
        }

        /// <summary>
        /// Unification method of remeber variables to dictionary
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        Dictionary<string, object> getVariablesFromObject(object obj)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            Type t = obj.GetType();
            FieldInfo[] fields = t.GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var f in fields)
            {
                dict.Add(f.Name, f.GetValue(obj));
            }
            PropertyInfo[] properties = t.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (var p in properties)
            {
                dict.Add(p.Name, p.GetValue(obj));
            }
            return dict;
        }


        /// <summary>
        /// Change condition string to reverse polish notation logic condition string 
        /// Not working if string contains one of operators.
        /// 
        /// </summary>
        /// <param name="condition"></param>
        /// <returns></returns>
        string toRPNLogic(string condition)
        {
            List<int> stack = new List<int>();
            List<string> variables = new List<string>();
            string rpn = "";
            bool wasOperator = false;
            int level = 0;
            for (int i = 0; i < condition.Length; i++)
            {
                if (condition[i] == '(')
                {
                    stack.Add(BRACKET_OPEN_VALUE);
                }
                else if (condition[i] == ')')
                {
                    while (stack.Count > 0 && stack[stack.Count - 1] != BRACKET_OPEN_VALUE)
                    {
                        rpn += " " + getOperatorById(stack[stack.Count - 1]);
                        stack.RemoveAt(stack.Count - 1);
                    }
                    if (stack.Count > 0)
                    {
                        stack.RemoveAt(stack.Count - 1);
                    }
                    else
                        throw new Exception("bracket mismatch");
                    wasOperator = true;
                }
                else if (condition[i] == '=')
                {
                    if (condition[++i] == '=')
                    {
                        while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_EQ_ID)
                        {
                            rpn += " " + getOperatorById(stack[stack.Count - 1]);
                            stack.RemoveAt(stack.Count - 1);
                        }
                        stack.Add(OPERATOR_EQ_ID);
                    }
                    else
                    {
                        throw new Exception("unknown operator");
                    }
                    wasOperator = true;
                }
                else if (condition[i] == '!' && condition[i + 1] == '=')
                {
                    i++;
                    while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_EQ_ID)
                    {
                        rpn += " " + getOperatorById(stack[stack.Count - 1]);
                        stack.RemoveAt(stack.Count - 1);
                    }
                    stack.Add(OPERATOR_NEQ_ID);
                    wasOperator = true;
                }
                else if (condition[i] == '>' && condition[i + 1] != '=')
                {
                    i++;
                    while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_EQ_ID)
                    {
                        rpn += " " + getOperatorById(stack[stack.Count - 1]);
                        stack.RemoveAt(stack.Count - 1);
                    }
                    stack.Add(OPERATOR_GE_ID);
                    wasOperator = true;
                }
                else if (condition[i] == '<' && condition[i + 1] != '=')
                {
                    i++;
                    while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_EQ_ID)
                    {
                        rpn += " " + getOperatorById(stack[stack.Count - 1]);
                        stack.RemoveAt(stack.Count - 1);
                    }
                    stack.Add(OPERATOR_LE_ID);
                    wasOperator = true;
                }
                else if (condition[i] == '>')
                {
                    i++;
                    while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_EQ_ID)
                    {
                        rpn += " " + getOperatorById(stack[stack.Count - 1]);
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(OPERATOR_G_ID);
                    }
                    wasOperator = true;
                }
                else if (condition[i] == '<')
                {
                    i++;
                    while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_EQ_ID)
                    {
                        rpn += " " + getOperatorById(stack[stack.Count - 1]);
                        stack.RemoveAt(stack.Count - 1);
                        stack.Add(OPERATOR_L_ID);
                    }
                    wasOperator = true;
                }
                else if (condition[i] == '&')
                {
                    if (condition[++i] == '&')
                    {
                        while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_AND_ID)
                        {
                            rpn += " " + getOperatorById(stack[stack.Count - 1]);
                            stack.RemoveAt(stack.Count - 1);
                        }
                        stack.Add(OPERATOR_AND_ID);
                    }
                    else
                    {
                        throw new Exception("unknown operator");
                    }
                    wasOperator = true;
                }
                else if (condition[i] == '|')
                {
                    if (condition[++i] == '|')
                    {
                        while (stack.Count > 0 && stack[stack.Count - 1] >= OPERATOR_OR_ID)
                        {
                            rpn += " " + getOperatorById(stack[stack.Count - 1]);
                            stack.RemoveAt(stack.Count - 1);
                        }
                        stack.Add(OPERATOR_OR_ID);
                    }
                    else
                    {
                        throw new Exception("unknown operator");
                    }
                    wasOperator = true;
                }
                else
                {
                    if (wasOperator)
                        rpn += " ";
                    rpn += condition[i];
                    wasOperator = false;
                }
            }
            var st = stack.Count - 1;
            //while (s > 0 && stack[s] != '*' && stack[s] != '/' && stack[s] != '%')
            while (st >= 0)
            {
                rpn += " ";
                if (stack[st] != BRACKET_OPEN_VALUE)
                    rpn += getOperatorById(stack[st]);
                stack.RemoveAt(st);
                st--;
            }
            return rpn;
        }

        string getOperatorById(int id)
        {



            switch (id)
            {
                case OPERATOR_OR_ID: return "||";
                case OPERATOR_AND_ID: return "&&";
                case OPERATOR_EQ_ID: return "==";
                case OPERATOR_NEQ_ID: return "!=";
                case OPERATOR_G_ID: return ">";
                case OPERATOR_L_ID: return "<";
                case OPERATOR_GE_ID: return ">=";
                case OPERATOR_LE_ID: return "<=";
            }
            return "";
        }

        /// <summary>
        /// Getting variable from
        /// -object (nested to)
        /// -string
        /// -bool value
        /// -numbers
        /// -floats
        /// </summary>
        /// <param name="str"></param>
        /// <param name="parameters"></param>
        /// <returns>vraibable which user want</returns>
        object getVariable(string str)
        {
            if (Regex.IsMatch(str, @"^([a-zA-Z_][a-zA-Z0-9_-]*)([\.]([a-zA-Z_][a-zA-Z0-9_\-]*))*$"))
            {

                object variable = getVariable(str.Split('.'));
                return variable;
            }
            else if (Regex.IsMatch(str, "^\".*\"$"))
            {
                return str.Substring(1, str.Length - 2);
            }
            else if (Regex.IsMatch(str, "^[-]?[0-9]+$"))
            {
                return int.Parse(str);
            }
            else if (Regex.IsMatch(str, "^true$"))
                return true;
            else if (Regex.IsMatch(str, "^false$"))
                return false;
            throw new Exception("Cant parse variable");
        }

        /// <summary>
        /// Gets Variable from object (nested to)
        /// </summary>
        /// <param name="pathToVariable"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        object getVariable(string[] pathToVariable)
        {
            object actualVariable;
            if (pathToVariable.Length == 1)
            {
                var variableString = pathToVariable[0];
                Match m = Regex.Match(variableString, "^\"(.*)\"$");
                if (m.Success)
                {
                    return m.Groups[1].Value;
                }
                try
                {
                    return bool.Parse(variableString);
                }
                catch
                {

                }

                try
                {
                    return int.Parse(variableString);
                }
                catch
                {

                }
                try
                {
                    return double.Parse(variableString);
                }
                catch
                {

                }
            }
            try
            {
                actualVariable = parameters[pathToVariable[0]];
            }
            catch (KeyNotFoundException ex)
            {
                throw new Exception("Undefined Variable");
            }
            for (int i = 1; i < pathToVariable.Length; i++)
            {
                var s = pathToVariable[i];

                var info = actualVariable.GetType().GetMember(s).Single();
                if (!(info is PropertyInfo || info is FieldInfo))
                {
                    throw new Exception("Undefined Variable");
                }
                if (info is PropertyInfo)
                    actualVariable = ((PropertyInfo)info).GetValue(actualVariable);

                if (info is PropertyInfo)
                    actualVariable = ((PropertyInfo)info).GetValue(actualVariable);
                else if (info is FieldInfo)
                    actualVariable = ((FieldInfo)info).GetValue(actualVariable);
            }
            return actualVariable;
        }


        string parseExpression(String line)
        {
            var parameters = new Parameter[this.parameters.Count];
            int i =0;
            foreach(var p in this.parameters)
            { 
                parameters[i++]= new Parameter(p.Key,p.Value);
            }
            var interpreter = new Interpreter();
            return interpreter.Eval(line.Trim().Substring(1), parameters).ToString();
        }


        /// <summary>
        /// Add tabs to line 
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        string createLevel(int level)
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
