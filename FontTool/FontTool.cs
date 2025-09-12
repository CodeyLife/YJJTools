using Sirenix.OdinInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YjjTool
{
    public class FontTool : MonoBehaviour
    {
        public Font font;
        public List<TMP_FontAsset> generateAssets;
        private void Awake()
        {
            InitDynamicFont();
        }

        [Button]
        void Test()
        {
            //
            Debug.Log(generateAssets[0].sourceFontFile);
        }
        private void InitDynamicFont()
        {
            foreach (var asset in generateAssets)
            {

                // 创建支持动态扩展的字体资产
                var dynamicFont = TMP_FontAsset.CreateFontAsset(
                font,
                40, // 字体大小
                2,  // 填充
                UnityEngine.TextCore.LowLevel.GlyphRenderMode.SDFAA,
                2048, 2048, // 图集大小
                AtlasPopulationMode.Dynamic
                );
                asset.fallbackFontAssetTable = new List<TMP_FontAsset>() { dynamicFont };
            }

        }
    }
#if UNITY_EDITOR
    public static class TextUpgrade
    {
        [UnityEditor.MenuItem("CONTEXT/Text/用TextMeshPro替换", priority = 1000)]
        private static void RectTransfromSetZero(UnityEditor.MenuCommand command)
        {
            UnityEditor.Undo.IncrementCurrentGroup();
            int groupIndex = UnityEditor.Undo.GetCurrentGroup();
            var undoName = "UpgradeCreat";
            UnityEditor.Undo.SetCurrentGroupName(undoName);
            var text = (Text)command.context;

            var pro = new GameObject(text.name, typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            UnityEditor.Undo.RegisterCreatedObjectUndo(pro, undoName);
            UnityEditor.Undo.SetTransformParent(pro.transform, text.transform.parent, undoName);

            pro.text = text.text;
            pro.fontSize = text.fontSize;
            pro.enableAutoSizing = text.resizeTextForBestFit; pro.fontSizeMax = text.resizeTextMaxSize;
            pro.fontSizeMin = text.resizeTextMinSize;
            pro.color = text.color;
            pro.font = YjjConfigs.Instance.tmpFont;
            pro.maskable = text.maskable;
            switch (text.alignment)
            {
                case TextAnchor.UpperLeft:
                    pro.alignment = TextAlignmentOptions.TopLeft;
                    break;
                case TextAnchor.UpperCenter:
                    pro.alignment = TextAlignmentOptions.Top;
                    break;
                case TextAnchor.UpperRight:
                    pro.alignment = TextAlignmentOptions.TopRight;
                    break;
                case TextAnchor.MiddleLeft:
                    pro.alignment = TextAlignmentOptions.MidlineLeft;
                    break;
                case TextAnchor.MiddleCenter:
                    pro.alignment = TextAlignmentOptions.Center;
                    break;
            }
        }
    }

#endif
}
