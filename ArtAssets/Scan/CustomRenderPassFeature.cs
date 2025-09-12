using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class CustomRenderPassFeature : ScriptableRendererFeature
{
    class CustomRenderPass : ScriptableRenderPass
    {
        // 这个类存储RenderGraph pass所需的数据。
        // 它作为参数传递给执行RenderGraph pass的委托函数。
        private class PassData
        {
        }

        // 这个静态方法作为RenderFunc委托传递给RenderGraph render pass。
        // 它用于执行绘制命令。
        static void ExecutePass(PassData data, RasterGraphContext context)
        {
        }

        // RecordRenderGraph是访问RenderGraph句柄的地方，通过它可以向图中添加渲染pass。
        // FrameData是一个上下文容器，通过它可以访问和管理URP资源。
        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            const string passName = "Render Custom Pass";

            // 这向图中添加了一个光栅渲染pass，指定了名称和将传递给ExecutePass函数的数据类型。
            using (var builder = renderGraph.AddRasterRenderPass<PassData>(passName, out var passData))
            {
                // 使用这个作用域来设置pass所需的输入和输出，并设置passData在pass执行时所需的属性。

                // 使用frameData通过专用容器访问资源和相机数据。
                // 例如：
                // UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();
                UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

                // 通过builder接口设置pass的输入和输出。
                // 例如：
                // builder.UseTexture(sourceTexture);
                // TextureHandle destination = UniversalRenderer.CreateRenderGraphTexture(renderGraph, cameraData.cameraTargetDescriptor, "Destination Texture", false);

                // 这将pass的渲染目标设置为活动颜色纹理。根据需要将其更改为您自己的渲染目标。
                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);

                // 将ExecutePass函数分配给渲染pass委托。当渲染图执行pass时，这将被调用。
                builder.SetRenderFunc((PassData data, RasterGraphContext context) => ExecutePass(data, context));
            }
        }

        // 注意：这个方法属于兼容性渲染路径，请使用上面的Render Graph API代替。
        // 在执行渲染pass之前调用此方法。
        // 它可用于配置渲染目标和它们的清除状态。还可以创建临时渲染目标纹理。
        // 当为空时，此渲染pass将渲染到活动相机的渲染目标。
        // 您不应调用CommandBuffer.SetRenderTarget。相反，请调用<c>ConfigureTarget</c>和<c>ConfigureClear</c>。
        // 渲染管线将确保目标设置和清除以高效的方式进行。
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
        }

        // 注意：这个方法属于兼容性渲染路径，请使用上面的Render Graph API代替。
        // 在这里您可以实现渲染逻辑。
        // 使用<c>ScriptableRenderContext</c>来发出绘制命令或执行命令缓冲区
        // https://docs.unity3d.com/ScriptReference/Rendering.ScriptableRenderContext.html
        // 您不必调用ScriptableRenderContext.submit，渲染管线会在管线的特定点调用它。
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
        }

        // 注意：这个方法属于兼容性渲染路径，请使用上面的Render Graph API代替。
        // 清理在执行此渲染pass期间分配的所有资源。
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
        }
    }

    CustomRenderPass m_ScriptablePass;

    /// <inheritdoc/>
    public override void Create()
    {
        m_ScriptablePass = new CustomRenderPass();

        // 配置渲染pass应注入的位置。
        m_ScriptablePass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

    // 在这里，您可以将一个或多个渲染pass注入到渲染器中。
    // 这个方法在设置渲染器时每个相机调用一次。
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(m_ScriptablePass);
    }
}