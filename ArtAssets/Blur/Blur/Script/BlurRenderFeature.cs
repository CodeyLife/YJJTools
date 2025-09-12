using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEditor;
using UnityEngine.Serialization;

public class BlurRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private BlurSettings settings;
    [SerializeField] private Shader blurShader;
    private Material _blurMaterial;
    private BlurRenderPass _blurRenderPass;

    public override void Create()
    {
        if(blurShader==null)
            blurShader=Shader.Find("CustomEffects/GaussianBlur");
        _blurMaterial = new Material(blurShader);
        _blurRenderPass = new BlurRenderPass(_blurMaterial, blurShader);
        _blurRenderPass.renderPassEvent = settings is null ? RenderPassEvent.BeforeRenderingTransparents : settings.renderPassEvent;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_blurRenderPass);
    }

    protected override void Dispose(bool disposing)
    {
        _blurRenderPass.Dispose();
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
            Destroy(_blurMaterial);
        else
            DestroyImmediate(_blurMaterial);
#else
        Destroy(_blurMaterial);
#endif
    }


    [Serializable]
    public class BlurSettings
    {
        public RenderPassEvent renderPassEvent=RenderPassEvent.BeforeRenderingTransparents;
    }
}