using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SobelOutlineFeature : ScriptableRendererFeature
{
    class SobelPass : ScriptableRenderPass
    {
        private Material sobelMat;
        private RenderTargetIdentifier source;
        private RenderTargetHandle tempTex;
        public SobelPass(Material mat)
        {
            sobelMat = mat;
            tempTex.Init("_TempSobelTex");
        }

        public void Setup(RenderTargetIdentifier src)
        {
            source = src;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (sobelMat == null) return;

            CommandBuffer cmd = CommandBufferPool.Get("Sobel Edge");
            RenderTextureDescriptor desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            cmd.GetTemporaryRT(tempTex.id, desc, FilterMode.Bilinear);

            // aplica el shader Sobel
            Blit(cmd, source, tempTex.Identifier(), sobelMat, 0);
            Blit(cmd, tempTex.Identifier(), source);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    SobelPass sobelPass;
    public Material sobelMaterial;

    public override void Create()
    {
        sobelPass = new SobelPass(sobelMaterial)
        {
            renderPassEvent = RenderPassEvent.AfterRenderingTransparents
        };
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (sobelMaterial == null) return;
        sobelPass.Setup(renderer.cameraColorTarget);
        renderer.EnqueuePass(sobelPass);
    }
}
