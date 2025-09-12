using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

public class Yjj_JosnEditor : OdinEditorWindow
{
    [MultiLineProperty(10),HideLabel,Title("json",TitleAlignment = TitleAlignments.Centered)]
    public string json;
    [MenuItem("YJJ/Json工具")]
    private static Yjj_JosnEditor Open()
    {
        var window = GetWindow<Yjj_JosnEditor>();
        window.Show();
        return window;
    }
    [LabelText("生成的类型前缀")]
    public string classHead;
    [LabelText("region注释")]
    public string regionName = "json数据类";
    public bool ignorNull = false;
    [MultiLineProperty(10), HideLabel, Title("生成结果", TitleAlignment = TitleAlignments.Centered)]
    public string result;
    [Button]
    private void Generate()
    {
        var obj = JsonConvert.DeserializeObject<JObject>(json);
        var sb = new StringBuilder();
        sb.AppendLine("");
        sb.AppendLine($"#region {regionName}");
        Parse(obj, sb);
        sb.AppendLine("#endregion");
        result = sb.ToString();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="sb"></param>
    /// <param name="className"></param>
    private void Parse(JObject obj,StringBuilder sb,string className = "Root")
    {
        if (obj == null) return;
        List<JObject> objs = new List<JObject>();
        List<string> names = new List<string>();
        sb.AppendLine($"public class {classHead}{className}");
        sb.AppendLine("{");
        foreach (var p in obj.Properties())
        {
            if(p.Value.Type!= JTokenType.Array)
            {
                //Debug.Log($"{p.Name}  {p.Value.Type}  {p.Value}");
                string type = "string";
                bool add = false;
                switch (p.Value.Type)
                {
                    case JTokenType.Object:
                        var cm = UpperFirst(p.Name);
                        sb.AppendLine($"    public {classHead}{cm} {p.Name};");
                        names.Add(cm);
                        objs.Add(p.Value as JObject);
                        continue;
                    case JTokenType.Integer:
                        if((long)p.Value>int.MaxValue)
                        {
                            type = "long";
                        }
                        else
                        {
                            type = "int";
                        }
                        add = ignorNull ? true : false;
                        break;
                    case JTokenType.Float:
                        type = "float";
                        add = ignorNull ? true : false;
                        break;
                    case JTokenType.Boolean:
                        type = "bool";
                        add = ignorNull ? true : false;
                        break;
                    case JTokenType.TimeSpan:
                        type = "DateTime";
                        add = ignorNull ? true : false;
                        break;
                }
                if (!string.IsNullOrEmpty(p.Value.ToString()))
                {
                    if (p.HasValues && type == "string")
                    {
                        sb.AppendLine("    /// <summary>");
                        sb.AppendLine($"    /// {p.Value}");
                        sb.AppendLine("    /// </summary>");
                    }
                }
 
                if (add) sb.AppendLine($"      [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]");
                sb.AppendLine($"    public {type} {p.Name};");
            }
            else
            {
                var arr = p.Value as JArray;
                if (arr.Count > 0)
                {
                    var arrValue = arr[0];
                    switch (arrValue.Type)
                    {
                        case JTokenType.Object:
                            var cmcmm = UpperFirst(p.Name);
                            sb.AppendLine($"    public List<{classHead}{cmcmm}> {p.Name};");
                            names.Add(cmcmm);
                            objs.Add(arrValue as JObject);
                            break;
                        case JTokenType.String:
                            sb.AppendLine($"    public List<string> {p.Name};");
                            break;
                        case JTokenType.Integer:
                            sb.AppendLine($"    public List<int> {p.Name};");
                            break;
                        case JTokenType.Float:
                            sb.AppendLine($"    public List<float> {p.Name};");
                            break;
                        case JTokenType.Boolean:
                            sb.AppendLine($"    public List<bool> {p.Name};");
                            break;

                    } 
                }
                else
                {
                    sb.AppendLine($"    public List<string> {p.Name};");
                    Debug.Log($"json数组:{p.Name}为空");
                }
            
            }

        }
        sb.AppendLine("}");
        for (int i = 0; i < objs.Count; i++)
        {
            Parse(objs[i], sb, names[i]);
        }
    }
    public static string UpperFirst(string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return "";
        }
        var f = str.Substring(0, 1);
        f = f.ToUpper();
        return f + str.Substring(1, str.Length - 1);
    }
}
