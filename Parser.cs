using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;

namespace ScssParser
{
    public class Parser
    {
        public string Selector { get; set; } = string.Empty;
        public Dictionary<string, string> Props { get; set; } = new Dictionary<string, string>();
        public Collection<Parser> Objs { get; set; } = new Collection<Parser>();

        public Parser ReadFileText(string scss) => Read($"{{{scss}}}");

        public Parser Read(string scss)
        {
            Contract.Requires(scss != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(scss));

            if (scss == null || string.IsNullOrWhiteSpace(scss)) return this;

            int start = 0;
            bool needFix = false;
            do
            {
                start = scss.IndexOf('{', start);
                if (start == -1) break;
                needFix = needToFixIndex(scss, start);
                if (needFix)
                    start++;
            }
            while (needFix);
            int end = JumpToEndOfCloseTag(start, scss) + 1;
            string style = scss.Substring(start, end - start);
            string styleTrimd = style.TrimStart().TrimEnd();

            string noChilds = removeChilds(styleTrimd.Substring(1, styleTrimd.Length - 2));
            Props = GenerateStylsFromString(noChilds);

            string childs;
            if (!string.IsNullOrWhiteSpace(noChilds))
                childs = styleTrimd.Substring(1, styleTrimd.Length - 2).Replace(noChilds, "");
            else childs = styleTrimd.Substring(1, styleTrimd.Length - 2);

            var childsCollection = GetChilds(childs);
            foreach (var item in childsCollection)
            {
                Objs.Add(new Parser().Read(item.TrimStart().TrimEnd()));
            }

            if (start > 0)
                Selector = scss.Substring(0, start - 1).TrimStart().TrimEnd();

            return this;
        }
        private Collection<string> GetChilds(string style)
        {
            Collection<string> childs = new Collection<string>();
            bool needFix = false;
            string temp = "";
            int end = 0;
            int start = 0;
            while (true)
            {
                do
                {
                    start = style.IndexOf('{', start);
                    if (start == -1) break;
                    needFix = needToFixIndex(style, start);
                    if (needFix)
                        start++;
                }
                while (needFix);

                if (start == -1)
                    break;
                end = JumpToEndOfCloseTag(start, style);
                start = findSelectorStart(start - 1, style) + 1;
                temp = style.Substring(start, end + 1 - start);
                if (string.IsNullOrWhiteSpace(temp))
                    continue;
                childs.Add(temp);
                style = style.Replace(temp, "");
            }

            return childs;
        }

        private Dictionary<string, string> GenerateStylsFromString(string style)
        {
            if (string.IsNullOrEmpty(style)) return new Dictionary<string, string>();
            if (!style.Contains(";") || !style.Contains(":")) return new Dictionary<string, string>();

            Dictionary<string, string> props = new Dictionary<string, string>();
            var arr = style.Split(';');
            string[] temp;
            string temp2;
            foreach (var item in arr)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                temp = item.Split(':');
                try
                {
                    temp2 = "";
                    for (int i = 1; i < temp.Length; i++)
                    {
                        temp2 += temp[i];
                        if (i + 1 < temp.Length)
                            temp2 += ':';
                    }
                    props.Add(temp[0], temp2);
                }
                catch (Exception ex) { }
            }
            return props;
        }

        private string removeChilds(string style)
        {
            int end = 0;
            bool needFix = false;
            int start = 0;
            while (true)
            {
                do
                {
                    start = style.IndexOf('{', start);
                    if (start == -1) break;
                    needFix = needToFixIndex(style, start);
                    if (needFix)
                        start++;
                }
                while (needFix);

                if (start == -1) break;
                end = JumpToEndOfCloseTag(start, style);
                start = findSelectorStart(start - 1, style) + 1;
                style = style.Replace(style.Substring(start, end + 1 - start), "");
            }
            return style.TrimStart().TrimEnd();
        }

        private bool needToFixIndex(string style, int index)
        {
            int start = jumpWhiteSpace(style, index, -1);
            bool needFix = false;
            if (start == 0 && start == '#')
            {
                needFix = true;
            }
            else if (start > 0 && style[start - 1] == '#')
            {
                needFix = true;
            }

            return needFix;
        }

        private int jumpWhiteSpace(string style, int index, int value)
        {
            int i = index;
            while (style[i] == '\n' || style[i] == '\r' || style[i] == '\t' || style[i] == ' ')
                i += value;

            return i;
        }



        private int findSelectorStart(int start, string style)
        {
            for (int i = start; i > -1; i--)
            {
                if (style[i] == ')')
                {
                    while (i > -1 && style[i] != '(')
                        i--;
                }
                else if (style[i] == ';' || style[i] == '{' || style[i] == '}')
                {
                    return i;
                }
            }
            return 0;
        }
        private int JumpToEndOfCloseTag(int start, string scss)
        {
            int end = -1;
            int opens = 0;
            for (int i = start; i < scss.Length; i++)
            {
                if (scss[i] == '{')
                    opens++;
                else if (scss[i] == '}')
                    opens--;

                if (scss[i] == '}' && opens == 0)
                {
                    end = i;
                    break;
                }
            }
            return end;
        }

        public override string ToString()
        {
            string str = string.Empty;
            if (!string.IsNullOrWhiteSpace(Selector))
                str = $"{Selector}{{{string.Join("", Props.Select(p => $"{p.Key}:{p.Value};"))} {string.Join("", Objs.Select(o => o.ToString()))}}}";
            else
                str = $"{string.Join("", Props.Select(p => $"{p.Key}:{p.Value};"))} {string.Join("", Objs.Select(o => o.ToString()))}";

            return str;
        }
    }
}
