#if UNITY_EDITOR
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YjjTool
{
    public class YjjToolExample : MonoBehaviour
    {
        public ChartV2Base randomChartDemo;
        public ChartV2Base addDataChartDemo;


        //ChartV2图表随机生成数据
        public void RandomData()
        {
            //随机数据的数量
            var count = Random.Range(50, 100);
            //随机多少列数据
            var column = Random.Range(2, 6);
            //生成数据
            var datas = new List<List<float>>();
            for (int i = 0; i < column; i++)
            {
                var data = new List<float>();
                for (int j = 0; j < count; j++)
                {
                    data.Add(Random.Range(0, 100f));
                }
                datas.Add(data);
            }
            //添加标题
            var titles = new List<string>();
            for (int i = 0; i < count; i++)
            {
                titles.Add($"数据{i + 1}");
            }
            //更新图表
            randomChartDemo.SetGraph(datas, titles);
        }

        //ChartV2图表，额外添加数据并刷新
        public void AddData()
        {
            //在原数据里添加一个随机数据
            var datas = addDataChartDemo.datas;
            for (int i = 0; i < datas.Count; i++)
            {
                datas[i].datas.Add(Random.Range(0, 100));
            }
            //添加标题
            addDataChartDemo.names.Add($"随机数据:{datas[0].datas.Count + 1}");

            //刷新图表 
            addDataChartDemo.RefreshGraph(true);
        }

        //随机生成列表示意
        public void RandomItem(Transform root)
        {
            var item1 = root.GetComponentInChildren<UIItemOptimization>();
            var count = Random.Range(100, 200);
            item1.SetGraph(count, (i, t) =>
            {
                t.Find("name_Mask").SetText("这里是循环滚动的介绍文本，在需要滚动的文本上添加Roll脚本就可以实现这个功能");
                t.Find("id").SetText(i + 1);
            });
            var item2 = root.GetComponentInChildren<UIItemManager>();
            count = 12;
            item2.SetGraph(count, (i, t) =>
            {
                t.Find("Text").SetText($"一次性生成的选项{i + 1}");
            });
        }

        //编辑器功能示意 （对应功能都有菜单或者快捷键，这里只是Demo示范）
        public void OpenConfig()
        {
            Yjj_ConfigWindows.OpenWindow();
        }

        public void ReferenceTest()
        {
            // 通过反射调用编辑器功能
            var editorAssembly = System.AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "Assembly-CSharp-Editor");
            
            if (editorAssembly != null)
            {
                var windowType = editorAssembly.GetType("YjjTool.Yjj_ReferenceWindows");
                if (windowType != null)
                {
                    // 获取Instance属性
                    var instanceProperty = windowType.GetProperty("Instance",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProperty != null)
                    {
                        var instance = instanceProperty.GetValue(null);
                        if (instance != null)
                        {
                            // 设置script
                            var scriptField = windowType.GetField("script");
                            scriptField?.SetValue(instance, this);
                            
                            // 调用Search方法
                            var searchMethod = windowType.GetMethod("Search",
                                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                            searchMethod?.Invoke(instance, null);
                        }
                    }
                }
                else
                {
                    Debug.LogError("找不到 Yjj_ReferenceWindows 类型");
                }
            }
            else
            {
                Debug.LogError("找不到 Assembly-CSharp-Editor 程序集");
            }
        }
    }
}

#endif