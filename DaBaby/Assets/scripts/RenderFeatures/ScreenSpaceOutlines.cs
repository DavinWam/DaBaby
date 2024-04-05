using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ScreenSpaceOutlines : ScriptableRendererFeature
{
    [System.Serializable]
    private class ViewSpaceNormalsTextureSettings
    {
        public RenderTextureFormat colorFormat;
        public int depthBufferBits;
        public Color backgroundColor;
    }
    private class ViewSpaceNormalsTexturePass : ScriptableRenderPass
    {
        private ViewSpaceNormalsTextureSettings normalsTextureSettings;

        private RenderTexture normalText;
        private readonly RenderTargetHandle normals;
        private readonly List<ShaderTagId> shaderTagIdList;
        private readonly Material normalsMaterial;
        private FilteringSettings filteringSettings;
        private FilteringSettings occluderFilteringSettings;

        public ViewSpaceNormalsTexturePass(RenderPassEvent renderPassEvent, LayerMask outlineLayerMask, ViewSpaceNormalsTextureSettings settings, Shader viewSpaceNormalShader, RenderTexture temp)
        {
            normals.Init("_SceneViewSpaceNormals");

            this.renderPassEvent = renderPassEvent;
            this.normalsTextureSettings = settings;
            normalText = temp;
            shaderTagIdList = new List<ShaderTagId> {
                new ShaderTagId("UniversalForward"),
                new ShaderTagId("UniversalForwardOnly"),
                new ShaderTagId("LightweightForward"),
                new ShaderTagId("SRPDefaultUnlit")
            };
            normalsMaterial = CoreUtils.CreateEngineMaterial(viewSpaceNormalShader);

            filteringSettings = new FilteringSettings(RenderQueueRange.opaque, outlineLayerMask);
            // Assuming outlineLayerMask is the layer you want to exclude
            LayerMask invertedMask = ~outlineLayerMask;

            occluderFilteringSettings = new FilteringSettings(RenderQueueRange.opaque, invertedMask);
        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            RenderTextureDescriptor normalsTextureDescriptor = cameraTextureDescriptor;
            normalsTextureDescriptor.colorFormat = normalsTextureSettings.colorFormat;
            normalsTextureDescriptor.depthBufferBits = normalsTextureSettings.depthBufferBits;

            cmd.GetTemporaryRT(normals.id, normalsTextureDescriptor, FilterMode.Point);
            // Set the RenderTexture globally for access in shaders

            ConfigureTarget(normals.Identifier());


            ConfigureClear(ClearFlag.All, normalsTextureSettings.backgroundColor);


        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!normalsMaterial) {
                Debug.Log("couldn't find the normal Material");
                return;
            }



            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("SceneViewSpaceNormalsTextureCreation")))
            {
                context.ExecuteCommandBuffer(cmd);
                cmd.Clear();
                DrawingSettings drawingSettings = CreateDrawingSettings(shaderTagIdList, ref renderingData, 
                    renderingData.cameraData.defaultOpaqueSortFlags);
                drawingSettings.overrideMaterial = normalsMaterial;
               
                context.DrawRenderers(renderingData.cullResults, ref drawingSettings,ref filteringSettings);

                context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref occluderFilteringSettings);

                // After rendering, copy the texture to 'temp' for visualization
                cmd.Blit(normals.Identifier(), normalText);
             
            }

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);

        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            cmd.ReleaseTemporaryRT(normals.id);
        }
    }
    private class ScreenSpaceOutlinesPass : ScriptableRenderPass
    {
        private readonly Material screenSpaceOutlineMaterial;
        private RenderTargetIdentifier cameraColorTarget;
        private RenderTargetIdentifier temporaryBuffer;
        private int temporaryBufferID =
            Shader.PropertyToID("_TemporaryBuffer");

        public ScreenSpaceOutlinesPass(RenderPassEvent renderPassEvent, Shader outlineShader) {
            this.renderPassEvent = renderPassEvent;
            screenSpaceOutlineMaterial = CoreUtils.CreateEngineMaterial(outlineShader);
        
        }
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
        }
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!screenSpaceOutlineMaterial) {
                Debug.Log("Couldn't find outline material");
                return;
            }
            CommandBuffer cmd = CommandBufferPool.Get();
            using (new ProfilingScope(cmd, new ProfilingSampler("ScreenSpaceOutlines"))){
                Blit(cmd, cameraColorTarget, temporaryBuffer);

                Blit(cmd, temporaryBuffer, cameraColorTarget, screenSpaceOutlineMaterial);

            }
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd );
        }
        public override void OnCameraCleanup(CommandBuffer cmd)
        {
          cmd.ReleaseTemporaryRT(temporaryBufferID);
        }



    }

    [SerializeField] private ViewSpaceNormalsTextureSettings viewSpaceNormalsTextureSettings;
    [SerializeField] private LayerMask outlinesLayerMask;
    [SerializeField] private RenderPassEvent renderPassEvent;
    [SerializeField] private Shader viewSpaceNormalShader;
    [SerializeField] private Shader outlineShader;
    [SerializeField] private Shader outlineOccluderShader;
    public RenderTexture temp;

    private ViewSpaceNormalsTexturePass viewSpaceNormalsTexturePass;
    private ScreenSpaceOutlinesPass screenSpaceOutlinesPass;

    public override void Create()
    {

        viewSpaceNormalsTexturePass = new ViewSpaceNormalsTexturePass(renderPassEvent, outlinesLayerMask,viewSpaceNormalsTextureSettings, viewSpaceNormalShader,temp);
        screenSpaceOutlinesPass = new ScreenSpaceOutlinesPass(renderPassEvent, outlineShader);

    }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(viewSpaceNormalsTexturePass);
        renderer.EnqueuePass(screenSpaceOutlinesPass);
    }

}
