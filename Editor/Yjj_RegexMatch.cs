using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Yjj_RegexMatch : OdinEditorWindow
{
    public string patten;
    [ListDrawerSettings(ShowFoldout = true)]
    public List<string> inputs = new List<string>() {"在这里输入测试文本\\input text hear"};
    [TableList]
    public List<RegexInfo> infos = new List<RegexInfo>()
    {
        new RegexInfo(@"\b,\B","匹配单词和非单词的开始或结束"),
        new RegexInfo("^,$","开头，结尾"),
        new RegexInfo("[],[^]","方括号内是需要匹配的字符，不需要匹配的字符"),
        new RegexInfo("s,S","	与任何空白字符匹配,非空白字符"),
        new RegexInfo("{}","花括号内是指定匹配字符的数量"),
        new RegexInfo("(","圆括号表示用来分组的"),
        new RegexInfo("d,D","[0-9],非十进制"),
        new RegexInfo("w,W","字符和[a-z][0-9][_](数字、字母、下划线)"),
        new RegexInfo("*,+,?","0次或多次发生,至少一次发生,0次或1次发生"),
        new RegexInfo("(?:str)", "非捕获组"),
        new RegexInfo("(?=...) ","正向先行断言 表示匹配某个模式前面的字符。例如，foo(?=bar) 会匹配 \"foo\"，前提是它后面跟着 \"bar\"，但 \"bar\" 不包含在匹配结果中 "),
        new RegexInfo("(?!...) ","负向先行断言 表示匹配某个模式前面的字符，但该模式必须不能匹配。例如，foo(?!bar) 会匹配 \"foo\"，前提是它后面不跟着 \"bar\" "),
        new RegexInfo("(?<=...) ","正向后行断言 表示匹配某个模式后面的字符。例如，(?<=foo)bar 会匹配 \"bar\"，前提是它前面是 \"foo\"，但 \"foo\" 不包含在匹配结果中  "),
        new RegexInfo("(?<!...) ","负向后行断言 表示匹配某个模式后面的字符，但该模式必须不能匹配。例如，(?<!foo)bar 会匹配 \"bar\"，前提是它前面不是 \"foo\" "),
    };

    [MenuItem("YJJ/正则表达式测试")]
    private static void Open()
    {
        GetWindow<Yjj_RegexMatch>().Show();
    }
    [Button("测试",buttonSize:ButtonSizes.Large),GUIColor(1,0,0)]
    private void Test()
    {
        foreach (var x in inputs)
        {
            Match match = Regex.Match(x, patten);
            if (match.Success)
            {
            
                Debug.Log($"输入:{x}");
                Debug.Log($"匹配结果:<color=yellow>{match.Value}</color>");
                
                for (int i = 0;i<match.Captures.Count;i++)
                {
                    Debug.Log(match.Captures[i].Value);
                }
            }
            else
            {
                Debug.Log($"匹配失败;{x}");
            }

        }
    }
    public class RegexInfo
    {
        [VerticalGroup("字符"),HideLabel,TableColumnWidth(90,resizable:false)]
        public string str;
        [VerticalGroup("用途"), HideLabel]
        public string info;
        public RegexInfo(string s,string i)
        {
            str = s;info = i;
        }
    }
}
