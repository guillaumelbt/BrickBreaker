
[UnityEngine.ExecuteInEditMode]
public class LightEventHandler : LoadableBehaviour
{
    [UnityEngine.SerializeField]
    int rendererContextIndex = 0;

    [System.NonSerialized]
    private static System.Collections.Generic.List<LightEventHandler> lightEventHandlerList = new System.Collections.Generic.List<LightEventHandler>();
    private UnityEngine.Light lightComponent;
    
    public UnityEngine.Light LightComponent
    {
        get { return this.lightComponent; }
    }

    public static LightEventHandler GetLightEventHandler(int rendererContextIndex)
    {
        int eventHandlerListCount = LightEventHandler.lightEventHandlerList != null ? LightEventHandler.lightEventHandlerList.Count : 0;
        for (int i = 0; i < eventHandlerListCount; ++i)
        {
            if (LightEventHandler.lightEventHandlerList[i].RendererContextIndex == rendererContextIndex)
            {
                return LightEventHandler.lightEventHandlerList[i];
            }
        }

        return null;
    }

    public int RendererContextIndex
    {
        get { return this.rendererContextIndex; }
    }

    protected void OnEnable()
    {
        this.LoadIFN();
    }

    protected override bool ResolveDependencies()
    {
        this.lightComponent = this.GetComponent<UnityEngine.Light>();
        return this.lightComponent != null;
    }

    protected override void Load()
    {
        if (LightEventHandler.lightEventHandlerList == null)
        {
            LightEventHandler.lightEventHandlerList = new System.Collections.Generic.List<LightEventHandler>();
        }

        int lightEventHandlerListCount = LightEventHandler.lightEventHandlerList.Count;
        for (int i = 0; i < lightEventHandlerListCount; ++i)
        {
            if (LightEventHandler.lightEventHandlerList[i].RendererContextIndex == this.rendererContextIndex)
            {
                UnityEngine.Debug.LogErrorFormat(this.gameObject, "An object of type {0} as been already registered", this.GetType());
            }
        }

        LightEventHandler.lightEventHandlerList.Add(this);
        base.Load();
    }

    protected override void Unload()
    {
        base.Unload();
        int eventHandlerListCount = LightEventHandler.lightEventHandlerList.Count;
        for (int i = 0; i < eventHandlerListCount; ++i)
        {
            if (LightEventHandler.lightEventHandlerList[i] == this)
            {
                LightEventHandler.lightEventHandlerList[i] = LightEventHandler.lightEventHandlerList[LightEventHandler.lightEventHandlerList.Count - 1];
                LightEventHandler.lightEventHandlerList.RemoveAt(LightEventHandler.lightEventHandlerList.Count - 1);
                break;
            }
        }
    }
}
