using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.Text;
using System;

public class AutoCopy2Txt:OdinEditorWindow
{
    [MenuItem("YJJ/拷贝所有脚本进文本(代码交付使用)")]
    public static void Menu()
    {
        GetWindow<AutoCopy2Txt>().Show();
    }
    [Button]
    private async void BeginWrite()
    {
       var  target =  Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
        target = Path.Combine(target, "脚本.txt");
        var path = Application.dataPath;
        List<string> files = null;
        await Task.Run(() =>
        {
            List<string> fs = new List<string>();
            PathUtility.GetAllDirectoryFiles(path, fs);
            var collection = from f in fs
                             where f.EndsWith(".cs")
                             select f;
            files = collection.ToList();
        });
        //var fileStream = File.Open(target, FileMode.OpenOrCreate);
        //fileStream.Seek(0, SeekOrigin.Begin);
        //fileStream.SetLength(0);
        for (int i = 0; i < files.Count; i++)
        {
            EditorUtility.DisplayProgressBar("写入中", $"{i + 1}/{files.Count}", (float)i / files.Count);
            //await Task.Run(() =>
            //{
            //    //var data = File.ReadAllBytes(files[i]);
            //    //var name ="\n" +Path.GetFileName(files[i])+"\n";
            //    //var namedata = Encoding.UTF8.GetBytes(name);
            //    //fileStream.Write(namedata, 0, namedata.Length);
            //    //fileStream.Write(data, 0, data.Length);
            //    File.AppendAllText(target, File.ReadAllText(files[i]));
            //});
            File.AppendAllText(target, File.ReadAllText(files[i]));
        }
        EditorUtility.ClearProgressBar();
      //  fileStream.Dispose();
    }
}
