using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class Yjj_TextureImporter : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        var path = assetPath;
        var list = Yjj_ConfigWindows.Config.autoSpriteList;
        for(int i = 0; i < list.Count; i++)
        {
            var math = Regex.Match(assetPath, $@"(^|/){list[i]}");
            if (math.Success)
            {
                TextureImporter textureImporter = (TextureImporter)assetImporter;
                textureImporter.textureType = TextureImporterType.Sprite;
                textureImporter.spriteImportMode = SpriteImportMode.Single;
                textureImporter.alphaIsTransparency = true;
                textureImporter.mipmapEnabled = false;
                break;
            }
        }
        
    }
}
