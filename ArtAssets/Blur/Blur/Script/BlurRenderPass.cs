using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEditor;

public class BlurRenderPass : ScriptableRenderPass
{
    private static readonly int BlurRadiusId = Shader.PropertyToID("_blurRadius");
    private static readonly int BlurBufferID = Shader.PropertyToID("_BlurBuffer");


    private Material _blurMaterial;
    private readonly Shader _blursShader;
    private RenderTextureDescriptor _blurRTDescriptor;
    private RTHandle _cameraTargetHandle;
    private RTHandle _blurRTHandlePing;
    private RTHandle _blurRTHandlePong;
    private float _blurRadius;
    private int _iteration;
    private BlurVolumeComponent _blurVolumeComponent;


    public BlurRenderPass(Material blurMaterial, Shader blursShader)
    {
        _blurMaterial = blurMaterial;
        _blursShader = blursShader;
        _blurRTDescriptor = new RenderTextureDescriptor(Screen.width, Screen.height, RenderTextureFormat.ARGB2101010, 0);
    }

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        _blurRTDescriptor.width = cameraTextureDescriptor.width / 2;
        _blurRTDescriptor.height = cameraTextureDescriptor.height / 2;
        _blurRTDescriptor.colorFormat = RenderTextureFormat.ARGB2101010; //10bit 
        RenderingUtils.ReAllocateIfNeeded(ref _blurRTHandlePing, _blurRTDescriptor);
        RenderingUtils.ReAllocateIfNeeded(ref _blurRTHandlePong, _blurRTDescriptor);

    }

    private void UpdateBlurSettings()
    {
        if (_blurMaterial == null)
            _blurMaterial = new Material(_blursShader);
        _blurVolumeComponent = VolumeManager.instance.stack.GetComponent<BlurVolumeComponent>();
        _blurRadius = _blurVolumeComponent.blurRadius.value;
        _iteration = _blurVolumeComponent.iteration.value;
        _blurMaterial.SetFloat(BlurRadiusId, _blurRadius);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
      
        UpdateBlurSettings();
        var cmd = CommandBufferPool.Get("Blur");
        var cameraTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
        if (cameraTargetHandle.rt == null) return;
        if (_blurVolumeComponent.isActive.value)
        {
            Blitter.BlitCameraTexture(cmd, cameraTargetHandle, _blurRTHandlePing, _blurMaterial, 0);
            Blitter.BlitCameraTexture(cmd, _blurRTHandlePing, _blurRTHandlePong, _blurMaterial, 1);

            for (var i = 1; i <= _iteration * 2; i++)
            {
                //Dual Blur
                if (i <= _iteration)
                {
                    _blurRTDescriptor.width /= 2;
                    _blurRTDescriptor.height /= 2;
                }
                else
                {
                    _blurRTDescriptor.width *= 2;
                    _blurRTDescriptor.height *= 2;
                }

             //   RenderingUtils.ReAllocateIfNeeded(ref _blurRTHandlePing, _blurRTDescriptor, FilterMode.Bilinear, TextureWrapMode.Mirror);
                RenderingUtils.ReAllocateIfNeeded(ref _blurRTHandlePing, _blurRTDescriptor, FilterMode.Bilinear, TextureWrapMode.Mirror);
                Blitter.BlitCameraTexture(cmd, _blurRTHandlePong, _blurRTHandlePing, _blurMaterial, 0);
                RenderingUtils.ReAllocateIfNeeded(ref _blurRTHandlePong, _blurRTDescriptor, FilterMode.Bilinear, TextureWrapMode.Mirror);
                Blitter.BlitCameraTexture(cmd, _blurRTHandlePing, _blurRTHandlePong, _blurMaterial, 1);
            }
        }

        cmd.SetGlobalTexture(BlurBufferID, _blurRTHandlePing);
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

  
    //public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    //{
     
    //    UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
    //    UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
  
    //    if (cameraData.camera.cameraType != CameraType.Game)
    //        return;
    //    UpdateBlurSettings();
    
    //    var source = resourceData.activeColorTexture;

    //    var desc = cameraData.cameraTargetDescriptor;
    //    desc.depthBufferBits = 0;
    //    desc.msaaSamples = 1;
    //    RenderingUtils.ReAllocateHandleIfNeeded(ref _blurRTHandlePing, desc, FilterMode.Bilinear, TextureWrapMode.Clamp);
    //    RenderingUtils.ReAllocateHandleIfNeeded(ref _blurRTHandlePong, desc, FilterMode.Bilinear, TextureWrapMode.Clamp);
    //    TextureHandle ping = renderGraph.ImportTexture(_blurRTHandlePing);
    //    TextureHandle pong = renderGraph.ImportTexture(_blurRTHandlePong);
    //    Shader.SetGlobalTexture(BlurBufferID, _blurRTHandlePong);
    //    if (_blurVolumeComponent.isActive.value)
    //    {
    //        RenderGraphUtils.BlitMaterialParameters para =  new(source, ping, _blurMaterial,0);
    //        RenderGraphUtils.BlitMaterialParameters para1 = new(ping, pong, _blurMaterial, 1);
    //        renderGraph.AddBlitPass(para, "blurCopy1");
    //        renderGraph.AddBlitPass(para1, "blurCopy2");

    //        for (var i = 1; i <= _iteration * 2; i++)
    //        {
    //            //Dual Blur
    //            if (i <= _iteration)
    //            {
    //                _blurRTDescriptor.width /= 2;
    //                _blurRTDescriptor.height /= 2;
    //            }
    //            else
    //            {
    //                _blurRTDescriptor.width *= 2;
    //                _blurRTDescriptor.height *= 2;
    //            }

    //            RenderingUtils.ReAllocateHandleIfNeeded(ref _blurRTHandlePing, _blurRTDescriptor, FilterMode.Bilinear, TextureWrapMode.Mirror);
    //            RenderGraphUtils.BlitMaterialParameters end1 = new(pong, ping, _blurMaterial, 0);
    //            renderGraph.AddBlitPass(end1, "blur1");
    //            RenderingUtils.ReAllocateHandleIfNeeded(ref _blurRTHandlePong, _blurRTDescriptor, FilterMode.Bilinear, TextureWrapMode.Mirror);
    //            RenderGraphUtils.BlitMaterialParameters end2 = new(ping, pong, _blurMaterial, 1);
    //            renderGraph.AddBlitPass(end2, "blur2");
    //        }
    //    }
    //    //cmd.SetGlobalTexture(BlurBufferID, _blurRTHandlePing);
    //    _blurRTHandlePong.Release();
    //}



    public void Dispose()
    {
#if UNITY_EDITOR
        if (EditorApplication.isPlaying)
            Object.Destroy(_blurMaterial);
        else
            Object.DestroyImmediate(_blurMaterial);
#else
        Object.Destroy(_blurMaterial);
#endif
        _blurRTHandlePing?.Release();
        _blurRTHandlePong?.Release();
    }
}