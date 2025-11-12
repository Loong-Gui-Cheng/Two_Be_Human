using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SobelOutlineRF : ScriptableRendererFeature
{
    [Header("Settings")]
    [SerializeField] private Shader sobelOutlineShader;
    [SerializeField] private RenderPassEvent renderEvent;

    private Material material;
    private SobelOutlineRP sobelOutlinePass = null;

    public class SobelOutlineRP : ScriptableRenderPass
    {
        private readonly ProfilingSampler m_ProfileSampler = new("Sobel Outline Pass");
        private readonly Material m_Material;
        private SobelOutlinePP sobelOutlinePP = null;

        private RenderTargetIdentifier m_CameraColorTarget;

        private int texID = Shader.PropertyToID("_MainTex");

        public SobelOutlineRP(Material material, RenderPassEvent renderEvent)
        {
            m_Material = material;
            renderPassEvent = renderEvent;
        }

        [System.Obsolete]
        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            sobelOutlinePP = VolumeManager.instance.stack.GetComponent<SobelOutlinePP>();
            RenderTextureDescriptor buffer = renderingData.cameraData.cameraTargetDescriptor;

            m_CameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
            cmd.GetTemporaryRT(texID, buffer, FilterMode.Bilinear);
        }

        [System.Obsolete]
        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (!renderingData.cameraData.postProcessEnabled)
                return;

            if (m_Material == null)
                return;

            if (sobelOutlinePP == null || !sobelOutlinePP.IsActive())
                return;

            CommandBuffer cmd = CommandBufferPool.Get();

            using (new ProfilingScope(cmd, m_ProfileSampler))
            {
                // Shader shinengans here
                m_Material.SetColor("_OutlineColor", sobelOutlinePP.outlineColor.value);
                m_Material.SetColor("_OutlineColor", sobelOutlinePP.outlineColor.value);
                m_Material.SetFloat("_OutlineThickness", sobelOutlinePP.outlineThickness.value);
                m_Material.SetFloat("_DepthMultiplier", sobelOutlinePP.depthMultiplier.value);
                m_Material.SetFloat("_DepthBias", sobelOutlinePP.depthBias.value);
                m_Material.SetFloat("_NormalMultiplier", sobelOutlinePP.normalMultiplier.value);
                m_Material.SetFloat("_NormalBias", sobelOutlinePP.normalBias.value);

                // Copy scene camera texture and normals into temporary buffers.
                cmd.Blit(m_CameraColorTarget, texID);

                // Blit back result.
                cmd.Blit(texID, m_CameraColorTarget, m_Material, 0);
            }

            context.ExecuteCommandBuffer(cmd);
            cmd.Clear();
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd)
        {
            // Release the temporary render target
            cmd.ReleaseTemporaryRT(texID);
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (sobelOutlineShader == null) return;

        renderer.EnqueuePass(sobelOutlinePass);
    }
    public override void Create()
    {
        if (sobelOutlineShader == null) return;

        material = CoreUtils.CreateEngineMaterial(sobelOutlineShader);
        sobelOutlinePass = new SobelOutlineRP(material, renderEvent);
    }
    protected override void Dispose(bool disposing)
    {
        if (material == null) return;

        CoreUtils.Destroy(material);
    }
}


