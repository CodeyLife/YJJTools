using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class ScanRenderFeature : ScriptableRendererFeature
{
    [SerializeField] private float speed = 10;
    [SerializeField] private Shader shader;
    [SerializeField] private Material material;
    private ScanRenderPass pass;

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (pass != null)
        {
            renderer.EnqueuePass(pass);
        }
    }

    public override void Create()
    {
        if(shader == null)
        {
            shader = Shader.Find("Shader Graphs/ScannerEffect");
        }
        material = new Material(shader);
        pass = new ScanRenderPass(material);
    }
    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        pass?.Dispose();
        if (Application.isPlaying)
        {
            GameObject.Destroy(material);
        }
        else
        {
            GameObject.DestroyImmediate(material);
        }
    }
}
public class ScanRenderPass: ScriptableRenderPass
{
    private Material mat;
    private RTHandle handle;
    public ScanRenderPass(Material material)
    {
        mat = material;
       
    }
    public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
    {
        UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
        UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

        if (cameraData.camera.cameraType != CameraType.Game)
            return;
        var source = resourceData.activeColorTexture;

        var desc = cameraData.cameraTargetDescriptor;
        desc.depthBufferBits = 0;
        desc.msaaSamples = 1;
        RenderingUtils.ReAllocateHandleIfNeeded(ref handle, desc);
        var target = renderGraph.ImportTexture(handle);
        RenderGraphUtils.BlitMaterialParameters blit = new RenderGraphUtils.BlitMaterialParameters(source, target, mat, 0);
        renderGraph.AddBlitPass(blit, "scan");
        renderGraph.AddCopyPass(target, source);
    }
    public void Dispose()
    {
        handle?.Release();
    }
}
