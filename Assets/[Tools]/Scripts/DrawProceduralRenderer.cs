
//[UnityEngine.ExecuteInEditMode]
public class DrawProceduralRenderer : LoadableBehaviour
{
    public int VertexCount = 6;
    public int MaxVertexCount = 0;
    public int InstanceCount = 1;
    public UnityEngine.Material material = null;

    public int PassIndex = 0;
    public UnityEngine.Material[] materials = null;
    public UnityEngine.MeshTopology meshTopology = UnityEngine.MeshTopology.Triangles;
    public UnityEngine.Rendering.CameraEvent CameraEvent = UnityEngine.Rendering.CameraEvent.AfterForwardAlpha;

    [UnityEngine.SerializeField]
    private int rendererContextIndex = 0;

    [UnityEngine.SerializeField]
    private bool drawInScene = true;
    
    public bool DrawInLight = false;
    public UnityEngine.Rendering.LightEvent LightEvent = UnityEngine.Rendering.LightEvent.AfterShadowMapPass;

    private UnityEngine.Rendering.CommandBuffer commandBuffer = null;
    private UnityEngine.Rendering.CameraEvent usedCameraEvent = UnityEngine.Rendering.CameraEvent.AfterEverything;
    private UnityEngine.MaterialPropertyBlock materialPropertyBlock;

    [System.NonSerialized]
    CameraEventHandler cameraEventHandler;

    [System.NonSerialized]
    LightEventHandler lightEventHandler;
    private UnityEngine.Rendering.LightEvent usedLightEvent = UnityEngine.Rendering.LightEvent.BeforeShadowMap;

    public UnityEngine.MaterialPropertyBlock MaterialPropertyBlock
    {
        get
        {
            if (this.materialPropertyBlock == null)
            {
                this.materialPropertyBlock = new UnityEngine.MaterialPropertyBlock();
            }

            return this.materialPropertyBlock;
        }
    }

    protected void OnEnable()
    {
        this.LoadIFN();
    }

    protected void OnDisable()
    {
        this.UnloadIFN();
    }

    protected virtual void AddCommands(UnityEngine.Rendering.CommandBuffer commandBuffer)
    {
    }

    protected override bool ResolveDependencies()
    {
        this.cameraEventHandler = CameraEventHandler.GetCameraEventHandler(this.rendererContextIndex);
        this.lightEventHandler = LightEventHandler.GetLightEventHandler(this.rendererContextIndex);
        return this.cameraEventHandler != null &&
               (this.lightEventHandler != null || !this.DrawInLight);
    }

    protected override void Load()
    {
        this.cameraEventHandler.OnPreCullEvents += this.OnPreCullEvent;

        if (this.commandBuffer == null)
        {
            this.commandBuffer = new UnityEngine.Rendering.CommandBuffer();
            this.commandBuffer.name = this.name;

            if (this.DrawInLight)
            {
                this.AddCommandBuffer(this.LightEvent);
            }
            else
            {
                this.AddCommandBuffer(this.CameraEvent);
            }
        }

        base.Load();
    }

    protected override void Unload()
    {
        base.Unload();
        if (this.commandBuffer != null)
        {
            if (this.cameraEventHandler != null && this.cameraEventHandler.CameraComponent)
            {
                this.RemoveCommandBuffer();
            }

            this.commandBuffer.Clear();
            this.commandBuffer.Release();
            this.commandBuffer = null;
        }
            // Here a tricks : a reference to a component that has been destroyed act as null on a comparaison.
        if (this.cameraEventHandler != null)
        {
            this.cameraEventHandler.OnPreCullEvents -= this.OnPreCullEvent;
        }

        this.cameraEventHandler = null;
        this.lightEventHandler = null;
    }

    private void OnPreCullEvent(UnityEngine.Camera camera)
    {
        if (this.DrawInLight)
        {
            // handling public change of this.CameraEvent
            if (this.usedLightEvent != this.LightEvent)
            {
                this.RemoveCommandBuffer();
                this.AddCommandBuffer(this.LightEvent);
            }
        }
        else
        {
            // handling public change of this.CameraEvent
            if (this.usedCameraEvent != this.CameraEvent)
            {
                this.RemoveCommandBuffer();
                this.AddCommandBuffer(this.CameraEvent);
            }
        }

        // we could have a component that do refresh only when needed but it add a bunch of code so....
        this.commandBuffer.Clear();
        
        this.AddCommands(this.commandBuffer);
        if (this.VertexCount > 0 && this.InstanceCount > 0)
        {
            int vertexCount = (this.MaxVertexCount > 0) ? System.Math.Min(this.InstanceCount * this.VertexCount, this.InstanceCount * this.MaxVertexCount) : this.InstanceCount * this.VertexCount;
            this.MaterialPropertyBlock.SetInt("_VertexCount", vertexCount);
            this.materialPropertyBlock.SetInt("_InstanceCount", this.InstanceCount);

            if (this.material != null)
            {
                this.commandBuffer.DrawProcedural(this.transform.localToWorldMatrix, this.material, this.PassIndex, this.meshTopology, vertexCount, 1, this.MaterialPropertyBlock);
            }

            int materialsCount = this.materials != null ? this.materials.Length : 0;
            
            for (int i = 0; i < materialsCount; ++i)
            {
                this.commandBuffer.DrawProcedural(this.transform.localToWorldMatrix, this.materials[i], this.PassIndex, this.meshTopology, vertexCount, 1, this.MaterialPropertyBlock);
            }
        }
    }

    private void RemoveCommandBuffer()
    {
        if (this.DrawInLight)
        {
            this.lightEventHandler.LightComponent.RemoveCommandBuffer(this.usedLightEvent, this.commandBuffer);
        }
        else
        {
            this.cameraEventHandler.CameraComponent.RemoveCommandBuffer(this.usedCameraEvent, this.commandBuffer);
#if UNITY_EDITOR
            if (this.drawInScene)
            {
                int sceneViewCount = UnityEditor.SceneView.sceneViews.Count;
                for (int i = 0; i < sceneViewCount; ++i)
                {
                    UnityEditor.SceneView sceneView = UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;
                    sceneView.camera.RemoveCommandBuffer(this.usedCameraEvent, this.commandBuffer);
                }
            }
#endif

        }
    }

    private void AddCommandBuffer(UnityEngine.Rendering.CameraEvent cameraEvent)
    {
        this.usedCameraEvent = cameraEvent;
        this.cameraEventHandler.CameraComponent.AddCommandBuffer(this.usedCameraEvent, this.commandBuffer);

#if UNITY_EDITOR
        if (this.drawInScene)
        {
            int sceneViewCount = UnityEditor.SceneView.sceneViews.Count;
            for (int i = 0; i < sceneViewCount; ++i)
            {
                UnityEditor.SceneView sceneView =  UnityEditor.SceneView.sceneViews[i] as UnityEditor.SceneView;
                sceneView.camera.AddCommandBuffer(this.usedCameraEvent, this.commandBuffer);
            }
        }
#endif
    }

    private void AddCommandBuffer(UnityEngine.Rendering.LightEvent lightEvent)
    {
        this.usedLightEvent = lightEvent;
        this.lightEventHandler.LightComponent.AddCommandBuffer(this.usedLightEvent, this.commandBuffer);
    }
}
