#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    [SerializeField] private string saveToDiskPath = "Assets/YJJTools/ArtAssets/NoiseGenerator/NoiseTextures/Noise";
    [SerializeField] private TextureSize textureSize = TextureSize.Size256;
    [SerializeField] [Range(1, 32)] private float frequency = 4;
    [SerializeField] private bool is3D = false;
    [SerializeField] private bool isTilable = true;
    [SerializeField] [Range(1, 100)] private float randomSeed = 1;
    [SerializeField] private bool autoReseed = false;
    [SerializeField] private Vector3 evolution = Vector3.zero;
    [SerializeField] private TextureImporterCompression compression = TextureImporterCompression.Uncompressed;
    [SerializeField] [Range(0, 8)] private int fbmIteration = 0;
    [SerializeField] private bool remapTo01 = true;
    [SerializeField] private bool invert = false;
    [SerializeField] private bool changeContrast = false;
    [SerializeField] [Range(0, 5)] private float contrast = 1;
    [SerializeField] private WorleyReturnType returnType = WorleyReturnType.Cell;

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
    [SerializeField] private int current3DSlice = 0; // 3D纹理当前显示的切片

    // UI
    private Vector2 scrollPosition;
    private const int PREVIEW_SIZE = 256;
    private bool isGenerating = false;
    
    
    // UI折叠状态
    [SerializeField] private bool foldoutBasicParams = true;
    [SerializeField] private bool foldoutFBMParams = true;
    [SerializeField] private bool foldoutWorleyParams = true;
    [SerializeField] private bool foldoutPostProcessParams = true;
    [SerializeField] private bool foldoutPresets = false;
    
    // 预设相关
    private const string PRESET_KEY_PREFIX = "NoiseGenerator_Preset_";
    private const string PRESET_NAMES_KEY = "NoiseGenerator_PresetNames";
    private string newPresetName = "";
    
    // EditorPrefs键
    private const string PREFS_KEY_PATH = "NoiseGenerator_SavePath";
    private const string PREFS_KEY_COMPRESSION = "NoiseGenerator_Compression";

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
        LoadParameters();
    }

    private void OnDisable()
    {
        SaveParameters();
        ReleaseTempResources();
    }
    
    private void SaveParameters()
    {
        // 保存保存路径和压缩设置到EditorPrefs
        EditorPrefs.SetString(PREFS_KEY_PATH, saveToDiskPath);
        EditorPrefs.SetInt(PREFS_KEY_COMPRESSION, (int)compression);
    }
    
    private void LoadParameters()
    {
        // 从EditorPrefs加载保存路径和压缩设置
        if (EditorPrefs.HasKey(PREFS_KEY_PATH))
        {
            saveToDiskPath = EditorPrefs.GetString(PREFS_KEY_PATH);
        }
        if (EditorPrefs.HasKey(PREFS_KEY_COMPRESSION))
        {
            compression = (TextureImporterCompression)EditorPrefs.GetInt(PREFS_KEY_COMPRESSION);
        }
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
    
    // 预设系统方法
    private List<string> GetPresetNames()
    {
        if (EditorPrefs.HasKey(PRESET_NAMES_KEY))
        {
            string namesJson = EditorPrefs.GetString(PRESET_NAMES_KEY);
            if (!string.IsNullOrEmpty(namesJson))
            {
                return JsonUtility.FromJson<PresetNamesList>(namesJson).names;
            }
        }
        return new List<string>();
    }
    
    private void SavePresetNames(List<string> names)
    {
        PresetNamesList list = new PresetNamesList { names = names };
        EditorPrefs.SetString(PRESET_NAMES_KEY, JsonUtility.ToJson(list));
    }
    
    private void SavePreset(string presetName)
    {
        if (string.IsNullOrEmpty(presetName))
        {
            EditorUtility.DisplayDialog("错误", "预设名称不能为空", "确定");
            return;
        }
        
        PresetData preset = new PresetData
        {
            textureSize = textureSize,
            frequency = frequency,
            is3D = is3D,
            isTilable = isTilable,
            randomSeed = randomSeed,
            autoReseed = autoReseed,
            evolution = evolution,
            fbmIteration = fbmIteration,
            remapTo01 = remapTo01,
            invert = invert,
            changeContrast = changeContrast,
            contrast = contrast,
            returnType = returnType
        };
        
        string presetJson = JsonUtility.ToJson(preset);
        EditorPrefs.SetString(PRESET_KEY_PREFIX + presetName, presetJson);
        
        List<string> presetNames = GetPresetNames();
        if (!presetNames.Contains(presetName))
        {
            presetNames.Add(presetName);
            SavePresetNames(presetNames);
        }
        
        EditorUtility.DisplayDialog("成功", $"预设 '{presetName}' 已保存", "确定");
    }
    
    private void LoadPreset(string presetName)
    {
        string key = PRESET_KEY_PREFIX + presetName;
        if (!EditorPrefs.HasKey(key))
        {
            EditorUtility.DisplayDialog("错误", $"预设 '{presetName}' 不存在", "确定");
            return;
        }
        
        string presetJson = EditorPrefs.GetString(key);
        PresetData preset = JsonUtility.FromJson<PresetData>(presetJson);
        
        textureSize = preset.textureSize;
        frequency = preset.frequency;
        is3D = preset.is3D;
        isTilable = preset.isTilable;
        randomSeed = preset.randomSeed;
        autoReseed = preset.autoReseed;
        evolution = preset.evolution;
        fbmIteration = preset.fbmIteration;
        remapTo01 = preset.remapTo01;
        invert = preset.invert;
        changeContrast = preset.changeContrast;
        contrast = preset.contrast;
        returnType = preset.returnType;
        
        // 加载后自动生成
        Generate();
    }
    
    private void DeletePreset(string presetName)
    {
        if (EditorUtility.DisplayDialog("确认删除", $"确定要删除预设 '{presetName}' 吗？", "删除", "取消"))
        {
            EditorPrefs.DeleteKey(PRESET_KEY_PREFIX + presetName);
            List<string> presetNames = GetPresetNames();
            presetNames.Remove(presetName);
            SavePresetNames(presetNames);
        }
    }
    
    [System.Serializable]
    private class PresetData
    {
        public TextureSize textureSize;
        public float frequency;
        public bool is3D;
        public bool isTilable;
        public float randomSeed;
        public bool autoReseed;
        public Vector3 evolution;
        public int fbmIteration;
        public bool remapTo01;
        public bool invert;
        public bool changeContrast;
        public float contrast;
        public WorleyReturnType returnType;
    }
    
    [System.Serializable]
    private class PresetNamesList
    {
        public List<string> names = new List<string>();
    }
    
    // 参数验证
    private bool ValidateParameters()
    {
        if (string.IsNullOrEmpty(saveToDiskPath))
        {
            EditorUtility.DisplayDialog("错误", "保存路径不能为空", "确定");
            return false;
        }
        
        if ((int)textureSize <= 0)
        {
            EditorUtility.DisplayDialog("错误", "分辨率无效", "确定");
            return false;
        }
        
        return true;
    }
    
    // 立即生成
    private void ScheduleGenerate()
    {
        if (!isGenerating)
        {
            EditorApplication.delayCall += () =>
            {
                if (!isGenerating)
                {
                    Generate();
                }
            };
        }
    }

    private void OnGUI()
    {
        // 处理快捷键
        Event e = Event.current;
        if (e.type == EventType.KeyDown)
        {
            if (e.control || e.command)
            {
                if (e.keyCode == KeyCode.G)
                {
                    Generate();
                    e.Use();
                }
                else if (e.keyCode == KeyCode.S)
                {
                    if (tempComputeBuffer != null)
                    {
                        SaveToDisk();
                        e.Use();
                    }
                }
            }
        }
        
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("噪声图生成器", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);
        
        // 快捷键提示
        EditorGUILayout.HelpBox("快捷键: Ctrl+G 生成 | Ctrl+S 保存", MessageType.Info);

        // 检查Compute Shader是否加载成功
        if (cs_core == null || cs_postProcess == null)
        {
            EditorGUILayout.HelpBox("Compute Shader未加载成功，请检查以下文件是否存在：\n- WorleyNoise.compute\n- PostProcessNoise.compute\n\n请确保这些文件在项目中的Assets目录下。", MessageType.Error);
            if (GUILayout.Button("重新加载"))
            {
                LoadComputeShaders();
            }
            EditorGUILayout.EndScrollView();
            return;
        }

        // 预设系统
        foldoutPresets = EditorGUILayout.Foldout(foldoutPresets, "预设管理", true);
        if (foldoutPresets)
        {
            EditorGUI.indentLevel++;
            List<string> presetNames = GetPresetNames();
            
            EditorGUILayout.BeginHorizontal();
            newPresetName = EditorGUILayout.TextField("预设名称", newPresetName);
            if (GUILayout.Button("保存预设", GUILayout.Width(100)))
            {
                SavePreset(newPresetName);
                newPresetName = "";
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
            if (presetNames.Count > 0)
            {
                EditorGUILayout.LabelField("已保存的预设:", EditorStyles.boldLabel);
                foreach (string presetName in presetNames)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button(presetName, EditorStyles.linkLabel))
                    {
                        LoadPreset(presetName);
                    }
                    if (GUILayout.Button("删除", GUILayout.Width(60)))
                    {
                        DeletePreset(presetName);
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("暂无保存的预设", MessageType.Info);
            }
            EditorGUI.indentLevel--;
            EditorGUILayout.Space(10);
        }

        // 开始检测参数变化（排除保存路径和压缩设置）
        EditorGUI.BeginChangeCheck();
        
        // 保存路径和压缩设置单独处理（不触发自动生成）
        EditorGUILayout.BeginHorizontal();
        saveToDiskPath = EditorGUILayout.TextField("保存路径", saveToDiskPath);
        if (GUILayout.Button("选择", GUILayout.Width(60)))
        {
            string defaultPath = saveToDiskPath;
            if (string.IsNullOrEmpty(defaultPath) || !defaultPath.StartsWith("Assets/"))
            {
                defaultPath = "Assets/YJJTools/ArtAssets/NoiseGenerator/NoiseTextures/";
            }
            
            string directory = Path.GetDirectoryName(defaultPath).Replace('\\', '/');
            string fileName = Path.GetFileNameWithoutExtension(defaultPath);
            string extension = is3D ? "asset" : "png";
            
            string selectedPath = EditorUtility.SaveFilePanel("选择保存位置", directory, fileName, extension);
            if (!string.IsNullOrEmpty(selectedPath))
            {
                // 转换为Unity相对路径
                if (selectedPath.StartsWith(Application.dataPath))
                {
                    saveToDiskPath = "Assets" + selectedPath.Substring(Application.dataPath.Length);
                    saveToDiskPath = saveToDiskPath.Replace('\\', '/');
                    // 移除扩展名
                    saveToDiskPath = Path.Combine(Path.GetDirectoryName(saveToDiskPath), Path.GetFileNameWithoutExtension(saveToDiskPath)).Replace('\\', '/');
                }
                else
                {
                    EditorUtility.DisplayDialog("错误", "请选择Assets目录下的路径", "确定");
                }
            }
        }
        EditorGUILayout.EndHorizontal();
        
        EditorGUI.EndChangeCheck(); // 结束对保存路径的检测
        
        // 压缩设置单独处理
        EditorGUI.BeginChangeCheck();
        compression = (TextureImporterCompression)EditorGUILayout.EnumPopup("压缩", compression);
        EditorGUI.EndChangeCheck(); // 结束对压缩设置的检测

        EditorGUILayout.Space(10);

        // 基础参数（折叠）
        foldoutBasicParams = EditorGUILayout.Foldout(foldoutBasicParams, "基础参数", true);
        if (foldoutBasicParams)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            
            textureSize = (TextureSize)EditorGUILayout.EnumPopup("分辨率", textureSize);
            frequency = EditorGUILayout.Slider("频率", frequency, 1, 32);
            is3D = EditorGUILayout.Toggle("3D纹理", is3D);
            isTilable = EditorGUILayout.Toggle("无缝纹理", isTilable);
            randomSeed = EditorGUILayout.Slider("随机种子", randomSeed, 1, 100);
            autoReseed = EditorGUILayout.Toggle("自动重新生成种子", autoReseed);
            evolution = EditorGUILayout.Vector3Field("Evolution", evolution);
            
            bool basicParamsChanged = EditorGUI.EndChangeCheck();
            if (basicParamsChanged && !isGenerating)
            {
                ScheduleGenerate();
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // FBM参数（折叠）
        foldoutFBMParams = EditorGUILayout.Foldout(foldoutFBMParams, "FBM参数", true);
        if (foldoutFBMParams)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            
            fbmIteration = EditorGUILayout.IntSlider("FBM迭代", fbmIteration, 0, 8);
            
            bool fbmParamsChanged = EditorGUI.EndChangeCheck();
            if (fbmParamsChanged && !isGenerating)
            {
                ScheduleGenerate();
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // Worley噪声参数（折叠）
        foldoutWorleyParams = EditorGUILayout.Foldout(foldoutWorleyParams, "Worley噪声参数", true);
        if (foldoutWorleyParams)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            
            returnType = (WorleyReturnType)EditorGUILayout.EnumPopup("返回类型", returnType);
            
            bool worleyParamsChanged = EditorGUI.EndChangeCheck();
            if (worleyParamsChanged && !isGenerating)
            {
                ScheduleGenerate();
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(5);

        // 后处理参数（折叠）
        foldoutPostProcessParams = EditorGUILayout.Foldout(foldoutPostProcessParams, "后处理参数", true);
        if (foldoutPostProcessParams)
        {
            EditorGUI.indentLevel++;
            EditorGUI.BeginChangeCheck();
            
            remapTo01 = EditorGUILayout.Toggle("重映射到[0,1]", remapTo01);
            invert = EditorGUILayout.Toggle("反转", invert);
            changeContrast = EditorGUILayout.Toggle("调整对比度", changeContrast);
            if (changeContrast)
            {
                contrast = EditorGUILayout.Slider("对比度", contrast, 0, 5);
            }
            
            bool postProcessParamsChanged = EditorGUI.EndChangeCheck();
            if (postProcessParamsChanged && !isGenerating)
            {
                ScheduleGenerate();
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(20);

        // 预览区域
        EditorGUILayout.LabelField("预览", EditorStyles.boldLabel);
        if (is3D && previewTexture3D != null)
        {
            // 3D纹理预览：显示可切换的切片
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("深度切片:", GUILayout.Width(80));
            int maxSlice = previewTexture3D.depth - 1;
            current3DSlice = EditorGUILayout.IntSlider(current3DSlice, 0, maxSlice);
            EditorGUILayout.LabelField($"{current3DSlice}/{maxSlice}", GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();
            
            Texture2D sliceTexture = Create3DPreviewSlice(previewTexture3D, current3DSlice);
            if (sliceTexture != null)
            {
                int displayWidth = Mathf.Min(sliceTexture.width, PREVIEW_SIZE);
                int displayHeight = Mathf.Min(sliceTexture.height, PREVIEW_SIZE);
                GUILayout.Label(sliceTexture, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
            }
        }
        else if (!is3D && previewTexture2D != null)
        {
            int displayWidth = Mathf.Min(previewTexture2D.width, PREVIEW_SIZE);
            int displayHeight = Mathf.Min(previewTexture2D.height, PREVIEW_SIZE);
            GUILayout.Label(previewTexture2D, GUILayout.Width(displayWidth), GUILayout.Height(displayHeight));
        }
        else
        {
            EditorGUILayout.HelpBox("点击生成按钮生成预览", MessageType.Info);
        }
        
        // 状态信息
        if (isGenerating)
        {
            EditorGUILayout.HelpBox("正在生成...", MessageType.Info);
        }
        else if (tempComputeBuffer != null)
        {
            int resolution = (int)textureSize;
            int pixelCount = is3D ? resolution * resolution * resolution : resolution * resolution;
            EditorGUILayout.HelpBox($"已生成纹理 | 分辨率: {resolution}x{resolution}" + (is3D ? $"x{resolution}" : "") + $" | 像素数: {pixelCount}", MessageType.Info);
        }

        EditorGUILayout.Space(20);

        // 操作按钮
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("生成 (Ctrl+G)", GUILayout.Height(30)))
        {
            if (ValidateParameters())
            {
                Generate();
            }
        }
        EditorGUI.BeginDisabledGroup(tempComputeBuffer == null);
        if (GUILayout.Button("保存到磁盘 (Ctrl+S)", GUILayout.Height(30)))
        {
            if (ValidateParameters())
            {
                SaveToDisk();
            }
        }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndScrollView();
    }

    private void Generate()
    {
        if (isGenerating) return;
        if (cs_core == null || cs_postProcess == null)
        {
            EditorUtility.DisplayDialog("错误", "Compute Shader未加载，请检查文件是否存在", "确定");
            return;
        }
        
        isGenerating = true;

        try
        {
            ReleaseTempResources();

            if (autoReseed)
            {
                randomSeed = UnityEngine.Random.Range(1f, 100f);
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
                if (!tempRenderTexture2D.IsCreated())
                {
                    tempRenderTexture2D.Create();
                }

                tempRenderTexture3D = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
                tempRenderTexture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
                tempRenderTexture3D.volumeDepth = resolution;
                tempRenderTexture3D.wrapMode = TextureWrapMode.Repeat;
                tempRenderTexture3D.filterMode = FilterMode.Bilinear;
                tempRenderTexture3D.enableRandomWrite = true;
                if (!tempRenderTexture3D.IsCreated())
                {
                    tempRenderTexture3D.Create();
                }
            }
            else
            {
                tempRenderTexture2D = new RenderTexture(resolution, resolution, 0, RenderTextureFormat.R8);
                tempRenderTexture2D.wrapMode = TextureWrapMode.Repeat;
                tempRenderTexture2D.filterMode = FilterMode.Bilinear;
                tempRenderTexture2D.enableRandomWrite = true;
                if (!tempRenderTexture2D.IsCreated())
                {
                    tempRenderTexture2D.Create();
                }

                tempRenderTexture3D = new RenderTexture(4, 4, 0, RenderTextureFormat.R8);
                tempRenderTexture3D.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
                tempRenderTexture3D.enableRandomWrite = true;
                if (!tempRenderTexture3D.IsCreated())
                {
                    tempRenderTexture3D.Create();
                }
            }

            // 先找到 kernel
            int kernel = cs_core.FindKernel("Main");
            if (kernel < 0)
            {
                throw new System.Exception("找不到Compute Shader的Main kernel");
            }
            
            // 设置所有参数（先设置全局参数，再设置 kernel 特定的资源）
            cs_core.SetInt("_ReturnType", (int)returnType);
            cs_core.SetInt("_Resolution", resolution);
            cs_core.SetFloat("_Frequency", frequency);
            cs_core.SetBool("_Is3D", is3D);
            cs_core.SetBool("_IsTilable", isTilable);
            cs_core.SetFloat("_RandomSeed", randomSeed);
            cs_core.SetVector("_Evolution", evolution);
            cs_core.SetInt("_FBMIteration", fbmIteration);
            
            // 设置 kernel 特定的资源
            cs_core.SetBuffer(kernel, "_Colors", tempComputeBuffer);
            cs_core.SetTexture(kernel, "_Texture2D", tempRenderTexture2D);
            cs_core.SetTexture(kernel, "_Texture3D", tempRenderTexture3D);

            int dispatchX = Mathf.CeilToInt(resolution / 16f);
            int dispatchY = Mathf.CeilToInt(resolution / 16f);

            cs_core.Dispatch(kernel, dispatchX, dispatchY, resolutionZ);

            if (ShouldPostProcess())
            {
                int postProcessKernel = cs_postProcess.FindKernel("Main");
                if (postProcessKernel < 0)
                {
                    throw new System.Exception("找不到PostProcess Compute Shader的Main kernel");
                }
                
                // 先设置全局参数
                cs_postProcess.SetInt("_Resolution", resolution);
                cs_postProcess.SetBool("_Is3D", is3D);
                cs_postProcess.SetBool("_RemapTo01", remapTo01);
                cs_postProcess.SetBool("_Invert", invert);
                cs_postProcess.SetBool("_ChangeContrast", changeContrast);
                
                // 再设置 kernel 特定的资源
                cs_postProcess.SetBuffer(postProcessKernel, "_Colors", tempComputeBuffer);
                cs_postProcess.SetTexture(postProcessKernel, "_Texture2D", tempRenderTexture2D);
                cs_postProcess.SetTexture(postProcessKernel, "_Texture3D", tempRenderTexture3D);

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

                cs_postProcess.Dispatch(postProcessKernel, dispatchX, dispatchY, resolutionZ);
            }

            // 更新预览
            UpdatePreview();
            
            // 重置3D切片到中间位置
            if (is3D && previewTexture3D != null)
            {
                current3DSlice = previewTexture3D.depth / 2;
            }
        }
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("生成错误", $"生成噪声纹理时发生错误：\n{ex.Message}", "确定");
            Debug.LogError($"生成噪声纹理错误: {ex}");
            ReleaseTempResources();
        }
        finally
        {
            isGenerating = false;
            Repaint();
        }
    }

    private void SaveToDisk()
    {
        if (tempComputeBuffer == null)
        {
            EditorUtility.DisplayDialog("错误", "请先生成噪声纹理", "确定");
            return;
        }

        if (string.IsNullOrEmpty(saveToDiskPath))
        {
            EditorUtility.DisplayDialog("错误", "保存路径不能为空", "确定");
            return;
        }

        try
        {
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
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(path).Replace('\\', '/');
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 删除已存在的资源
                if (File.Exists(path))
                {
                    AssetDatabase.DeleteAsset(path);
                }
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
                
                // 确保目录存在
                string directory = Path.GetDirectoryName(path).Replace('\\', '/');
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                byte[] pngData = texture.EncodeToPNG();
                if (pngData == null || pngData.Length == 0)
                {
                    throw new System.Exception("PNG编码失败");
                }
                
                File.WriteAllBytes(path, pngData);
                AssetDatabase.Refresh();

                TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                {
                    throw new System.Exception("无法获取TextureImporter");
                }
                
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
        catch (System.Exception ex)
        {
            EditorUtility.DisplayDialog("保存错误", $"保存纹理时发生错误：\n{ex.Message}", "确定");
            Debug.LogError($"保存纹理错误: {ex}");
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

