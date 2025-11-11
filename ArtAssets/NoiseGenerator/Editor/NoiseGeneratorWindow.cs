#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public class NoiseGeneratorWindow : EditorWindow
{
    /// <summary>
    /// 纹理尺寸枚举
    /// </summary>
    public enum TextureSize
    {
        Size32 = 32,
        Size64 = 64,
        Size128 = 128,
        Size256 = 256,
        Size512 = 512
    }

    /// <summary>
    /// Worley噪声返回类型枚举
    /// </summary>
    public enum WorleyReturnType
    {
        /// <summary>
        /// 欧几里得距离 + 最近距离（Cell）
        /// </summary>
        Cell = 0,
        /// <summary>
        /// 曼哈顿距离 + 最近距离
        /// </summary>
        F1_Manhattan = 1,
        /// <summary>
        /// 欧几里得距离 + F2-F1
        /// </summary>
        F2F1_Euclidean = 2,
        /// <summary>
        /// 曼哈顿距离 + F2-F1
        /// </summary>
        F2F1_Manhattan = 3
    }

    // 参数（完全对应BaseNoise的字段）
    private string saveToDiskPath = "Assets/YJJTools/ArtAssets/NoiseGenerator/NoiseTextures/Noise";
    private TextureSize textureSize = TextureSize.Size256;
    [Range(1, 32)] private float frequency = 4;
    private bool is3D = false;
    private bool isTilable = true;
    [Range(1, 100)] private float randomSeed = 1;
    private bool autoReseed = false;
    private Vector3 evolution = Vector3.zero;
    private TextureImporterCompression compression = TextureImporterCompression.Uncompressed;
    [Range(0, 8)] private int fbmIteration = 0;
    private bool remapTo01 = true;
    private bool invert = false;
    private bool changeContrast = false;
    [Range(0, 5)] private float contrast = 1;
    private WorleyReturnType returnType = WorleyReturnType.Cell;

    // Compute Shaders
    private ComputeShader cs_core;
    private ComputeShader cs_postProcess;

    // 临时资源
    private ComputeBuffer tempComputeBuffer;
    private RenderTexture tempRenderTexture2D;
    private RenderTexture tempRenderTexture3D;

    // 预览
    private Texture2D previewTexture2D;
    private Texture3D previewTexture3D;

    // UI
    private Vector2 scrollPosition;
    private const int PREVIEW_SIZE = 256;
    private bool isGenerating = false;

    [MenuItem("YJJ/噪声图生成器")]
    public static void OpenWindow()
    {
        NoiseGeneratorWindow window = GetWindow<NoiseGeneratorWindow>("噪声图生成器");
        window.minSize = new Vector2(400, 600);
        window.Show();
    }

    private void OnEnable()
    {
        LoadComputeShaders();
    }

    private void OnDisable()
    {
        ReleaseTempResources();
    }

    private void LoadComputeShaders()
    {
        // 查找并加载WorleyNoise.compute
        string[] worleyGuids = AssetDatabase.FindAssets("WorleyNoise t:ComputeShader");
        if (worleyGuids.Length > 0)
        {
            string worleyPath = AssetDatabase.GUIDToAssetPath(worleyGuids[0]);
            cs_core = AssetDatabase.LoadAssetAtPath<ComputeShader>(worleyPath);
            if (cs_core == null)
            {
                Debug.LogError($"无法加载Compute Shader: {worleyPath}");
            }
        }
        else
        {
            Debug.LogError("未找到 WorleyNoise.compute 文件");
        }

        // 查找并加载PostProcessNoise.compute
        string[] postProcessGuids = AssetDatabase.FindAssets("PostProcessNoise t:ComputeShader");
        if (postProcessGuids.Length > 0)
        {
            string postProcessPath = AssetDatabase.GUIDToAssetPath(postProcessGuids[0]);
            cs_postProcess = AssetDatabase.LoadAssetAtPath<ComputeShader>(postProcessPath);
            if (cs_postProcess == null)
            {
                Debug.LogError($"无法加载Compute Shader: {postProcessPath}");
            }
        }
        else
        {
            Debug.LogError("未找到 PostProcessNoise.compute 文件");
        }
    }

    private void OnGUI()
    {
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("噪声图生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        // 检查Compute Shader是否加载成功
        if (cs_core == null || cs_postProcess == null)
        {
            EditorGUILayout.HelpBox("Compute Shader未加载成功，请检查路径是否正确", MessageType.Error);
            if (GUILayout.Button("重新加载"))
            {
                LoadComputeShaders();
            }
            EditorGUILayout.EndScrollView();
            return;
        }

        // 开始检测参数变化
        EditorGUI.BeginChangeCheck();

        // 基础参数
        EditorGUILayout.LabelField("基础参数", EditorStyles.boldLabel);
        saveToDiskPath = EditorGUILayout.TextField("保存路径", saveToDiskPath);
        textureSize = (TextureSize)EditorGUILayout.EnumPopup("分辨率", textureSize);
        frequency = EditorGUILayout.Slider("频率", frequency, 1, 32);
        is3D = EditorGUILayout.Toggle("3D纹理", is3D);
        isTilable = EditorGUILayout.Toggle("无缝纹理", isTilable);
        randomSeed = EditorGUILayout.Slider("随机种子", randomSeed, 1, 100);
        autoReseed = EditorGUILayout.Toggle("自动重新生成种子", autoReseed);
        evolution = EditorGUILayout.Vector3Field("Evolution", evolution);
        compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("压缩", compression);

        EditorGUILayout.Space(10);

        // FBM参数
        EditorGUILayout.LabelField("FBM参数", EditorStyles.boldLabel);
        fbmIteration = EditorGUILayout.IntSlider("FBM迭代", fbmIteration, 0, 8);

        EditorGUILayout.Space(10);

        // Worley噪声参数
        EditorGUILayout.LabelField("Worley噪声参数", EditorStyles.boldLabel);
        returnType = (WorleyReturnType)EditorGUILayout.EnumPopup("返回类型", returnType);

        EditorGUILayout.Space(10);

        // 后处理参数
        EditorGUILayout.LabelField("后处理参数", EditorStyles.boldLabel);
        remapTo01 = EditorGUILayout.Toggle("重映射到[0,1]", remapTo01);
        invert = EditorGUILayout.Toggle("反转", invert);
        changeContrast = EditorGUILayout.Toggle("调整对比度", changeContrast);
        if (changeContrast)
        {
            contrast = EditorGUILayout.Slider("对比度", contrast, 0, 5);
        }

        // 检测参数是否变化（排除保存路径和压缩设置的变化）
        bool parametersChanged = EditorGUI.EndChangeCheck();
        if (parametersChanged && !isGenerating)
        {
            // 延迟生成，避免在GUI绘制过程中生成
            EditorApplication.delayCall += () =>
            {
                if (!isGenerating)
                {
                    Generate();
                }
            };
        }

        EditorGUILayout.Space(20);

        // 预览区域
        EditorGUILayout.LabelField("预览", EditorStyles.boldLabel);
        if (is3D && previewTexture3D != null)
        {
            // 3D纹理预览：显示中间切片
            int slice = previewTexture3D.depth / 2;
            Texture2D sliceTexture = Create3DPreviewSlice(previewTexture3D, slice);
            if (sliceTexture != null)
            {
                // 按实际大小显示，但限制最大尺寸
                int displayWidth = Mathf.Min(sliceTexture.width, PREVIEW_SIZE);
                int displayHeight = Mathf.Min(sliceTexture.height, PREVIEW_SIZE);
                GUILayout.Label(sliceTexture, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
                EditorGUILayout.LabelField($"3D纹理预览（深度切片 {slice}/{previewTexture3D.depth - 1}）");
            }
        }
        else if (!is3D && previewTexture2D != null)
        {
            // 按实际大小显示，但限制最大尺寸
            int displayWidth = Mathf.Min(previewTexture2D.width, PREVIEW_SIZE);
            int displayHeight = Mathf.Min(previewTexture2D.height, PREVIEW_SIZE);
            GUILayout.Label(previewTexture2D, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
        }
        else
        {
            EditorGUILayout.HelpBox("点击生成按钮生成预览", MessageType.Info);
        }

        EditorGUILayout.Space(20);

        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成", GUILayout.Height(30)))
        {
            Generate();
        }
        EditorGUI.BeginDisabledGroup(tempComputeBuffer == null);
        if (GUILayout.Button("保存到磁盘", GUILayout.Height(30)))
        {
            SaveToDisk();
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        if (isGenerating) return;
        isGenerating = true;

        ReleaseTempResources();

        if (autoReseed)
        {
            randomSeed = Random.Range(1f, 100f);
        }

        int resolution = (int)textureSize;
        int resolutionZ = 1;
        if (is3D)
        {
            resolutionZ = resolution;
        }

        tempComputeBuffer = new ComputeBuffer(resolution * resolution * resolutionZ, 16);

        if (is3D)
        {
            tempRenderTexture2D = new RenderTexture(4, 4, 0, RenderTextureFormat.R8);
            tempRenderTexture2D.enableRandomWrite = true;

            tempRenderTexture3D = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
            tempRenderTexture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            tempRenderTexture3D.volumeDepth = resolution;
            tempRenderTexture3D.wrapMode = TextureWrapMode.Repeat;
            tempRenderTexture3D.filterMode = FilterMode.Bilinear;
            tempRenderTexture3D.enableRandomWrite = true;
        }
        else
        {
            tempRenderTexture2D = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
            tempRenderTexture2D.wrapMode = TextureWrapMode.Repeat;
            tempRenderTexture2D.filterMode = FilterMode.Bilinear;
            tempRenderTexture2D.enableRandomWrite = true;

            tempRenderTexture3D = new RenderTexture(4, 4, 0, RenderTextureFormat.R8);
            tempRenderTexture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            tempRenderTexture3D.enableRandomWrite = true;
        }

        // 设置 _ReturnType（在找到 kernel 之前，与 WorleyNoise.Generate() 保持一致）
        cs_core.SetInt("_ReturnType", (int)returnType);

        int kernel = cs_core.FindKernel("Main");
        cs_core.SetBuffer(kernel, "_Colors", tempComputeBuffer);
        cs_core.SetTexture(kernel, "_Texture2D", tempRenderTexture2D);
        cs_core.SetTexture(kernel, "_Texture3D", tempRenderTexture3D);

        cs_core.SetInt("_Resolution", resolution);
        cs_core.SetFloat("_Frequency", frequency);
        cs_core.SetBool("_Is3D", is3D);
        cs_core.SetBool("_IsTilable", isTilable);
        cs_core.SetFloat("_RandomSeed", randomSeed);
        cs_core.SetVector("_Evolution", evolution);
        cs_core.SetInt("_FBMIteration", fbmIteration);

        int dispatchX = Mathf.CeilToInt(resolution / 16f);
        int dispatchY = Mathf.CeilToInt(resolution / 16f);

        cs_core.Dispatch(kernel, dispatchX, dispatchY, resolutionZ);

        if (ShouldPostProcess())
        {
            cs_postProcess.SetBuffer(kernel, "_Colors", tempComputeBuffer);
            cs_postProcess.SetTexture(kernel, "_Texture2D", tempRenderTexture2D);
            cs_postProcess.SetTexture(kernel, "_Texture3D", tempRenderTexture3D);

            cs_postProcess.SetInt("_Resolution", resolution);
            cs_postProcess.SetBool("_Is3D", is3D);
            cs_postProcess.SetBool("_RemapTo01", remapTo01);
            cs_postProcess.SetBool("_Invert", invert);
            cs_postProcess.SetBool("_ChangeContrast", changeContrast);

            if (remapTo01)
            {
                Color[] colors = new Color[tempComputeBuffer.count];
                tempComputeBuffer.GetData(colors);
                float min = float.PositiveInfinity;
                float max = float.NegativeInfinity;
                for (int i = 0; i < colors.Length; i++)
                {
                    min = Mathf.Min(min, colors[i].r);
                    max = Mathf.Max(max, colors[i].r);
                }

                cs_postProcess.SetFloat("_MinValue", min);
                cs_postProcess.SetFloat("_MaxValue", max);
            }

            if (changeContrast)
            {
                cs_postProcess.SetFloat("_Contrast", contrast);
            }

            cs_postProcess.Dispatch(kernel, dispatchX, dispatchY, resolutionZ);
        }

        // 更新预览
        UpdatePreview();
        
        isGenerating = false;
        Repaint();
    }

    private void SaveToDisk()
    {
        if (tempComputeBuffer == null)
        {
            EditorUtility.DisplayDialog("错误", "请先生成噪声纹理", "确定");
            return;
        }

        int resolution = (int)textureSize;

        if (is3D)
        {
            Texture3D texture = new Texture3D(resolution, resolution, resolution, TextureFormat.R8, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[tempComputeBuffer.count];
            tempComputeBuffer.GetData(colors);
            texture.SetPixels(colors);
            texture.Apply();

            string path = saveToDiskPath + ".asset";

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();

            AssetDatabase.CreateAsset(texture, path);
            AssetDatabase.Refresh();
        }
        else
        {
            Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.R8, false);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;

            Color[] colors = new Color[tempComputeBuffer.count];
            tempComputeBuffer.GetData(colors);
            texture.SetPixels(colors);
            texture.Apply();

            string path = saveToDiskPath + ".png";

            File.WriteAllBytes(path, texture.EncodeToPNG());
            AssetDatabase.Refresh();

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.singleChannelComponent = TextureImporterSingleChannelComponent.Red;
            importer.SetTextureSettings(settings);
            importer.textureType = TextureImporterType.SingleChannel;
            importer.mipmapEnabled = false;
            importer.textureCompression = compression;
            importer.SaveAndReimport();
        }
    }

    private void UpdatePreview()
    {
        int resolution = (int)textureSize;

        if (is3D)
        {
            if (tempComputeBuffer != null)
            {
                previewTexture3D = new Texture3D(resolution, resolution, resolution, TextureFormat.R8, false);
                Color[] colors = new Color[tempComputeBuffer.count];
                tempComputeBuffer.GetData(colors);
                previewTexture3D.SetPixels(colors);
                previewTexture3D.Apply();
            }
        }
        else
        {
            if (tempComputeBuffer != null)
            {
                previewTexture2D = new Texture2D(resolution, resolution, TextureFormat.R8, false);
                Color[] colors = new Color[tempComputeBuffer.count];
                tempComputeBuffer.GetData(colors);
                previewTexture2D.SetPixels(colors);
                previewTexture2D.Apply();
            }
        }
    }

    private Texture2D Create3DPreviewSlice(Texture3D texture3D, int slice)
    {
        if (texture3D == null || tempComputeBuffer == null) return null;

        int width = texture3D.width;
        int height = texture3D.height;
        int depth = texture3D.depth;

        if (slice < 0 || slice >= depth) return null;

        Texture2D sliceTexture = new Texture2D(width, height, TextureFormat.R8, false);
        Color[] pixels = new Color[width * height];

        // 从ComputeBuffer读取数据
        Color[] allPixels = new Color[tempComputeBuffer.count];
        tempComputeBuffer.GetData(allPixels);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index3D = x + y * width + slice * width * height;
                pixels[x + y * width] = allPixels[index3D];
            }
        }

        sliceTexture.SetPixels(pixels);
        sliceTexture.Apply();
        return sliceTexture;
    }

    private void ReleaseTempResources()
    {
        if (tempRenderTexture2D != null)
        {
            tempRenderTexture2D.Release();
            tempRenderTexture2D = null;
        }

        if (tempRenderTexture3D != null)
        {
            tempRenderTexture3D.Release();
            tempRenderTexture3D = null;
        }

        if (tempComputeBuffer != null)
        {
            tempComputeBuffer.Release();
            tempComputeBuffer = null;
        }

        if (previewTexture2D != null)
        {
            DestroyImmediate(previewTexture2D);
            previewTexture2D = null;
        }

        if (previewTexture3D != null)
        {
            DestroyImmediate(previewTexture3D);
            previewTexture3D = null;
        }
    }

    private bool ShouldPostProcess()
    {
        if (remapTo01)
        {
            return true;
        }

        if (invert)
        {
            return true;
        }

        if (changeContrast)
        {
            return true;
        }

        return false;
    }
}
#endif

