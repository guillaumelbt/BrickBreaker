

using UnityEngine.Rendering;

public class GPUBrick : DrawProceduralRenderer
{
    public UnityEngine.ComputeShader computeShader;
    public string EvolveKernelName = "CSEvolve";
    public string ResetKernelName = "CSReset";

    public string EvolveWorldKernelName = "CSWorldEvolve";
    public string ResetWorldKernelName = "CSWorldReset";

    public int kernelSizeX = 32;

    public int BrickX = 20;
    public int BrickY = 5;

    public UnityEngine.Texture2D BrickSetup;

    private UnityEngine.ComputeBuffer inputAndTime;
    private float[] inputAndTimeDatas;
    private UnityEngine.ComputeBuffer worldData;
    private UnityEngine.ComputeBuffer ballData;
    private UnityEngine.ComputeBuffer brickData;
    private bool doReset = true;

    private int evolveKernelIndex = 0;
    private int resetKernelIndex = 1;

    private int evolveWorldKernelIndex = 0;
    private int resetWorldKernelIndex = 1;

    public void OnGUI()
    {
        if (UnityEngine.GUILayout.Button("Reset"))
        {
            this.doReset = true;
        }
    }

    protected override void Load()
    {
        base.Load();
        this.doReset = true;
        this.evolveKernelIndex = this.computeShader.FindKernel(this.EvolveKernelName);
        this.resetKernelIndex = this.computeShader.FindKernel(this.ResetKernelName);
        this.evolveWorldKernelIndex = this.computeShader.FindKernel(this.EvolveWorldKernelName);
        this.resetWorldKernelIndex = this.computeShader.FindKernel(this.ResetWorldKernelName);
    }
    protected override void Unload()
    {
        base.Unload();
        if (this.inputAndTime != null)
        {
            this.inputAndTime.Release();
        }

        if (this.ballData != null)
        {
            this.ballData.Release();
        }

        if (this.brickData != null)
        {
            this.brickData.Release();
        }

        if (this.worldData != null)
        {
            this.worldData.Release();
        }

        this.inputAndTime = null;
        this.ballData = null;
        this.brickData = null;
        this.worldData = null;
    }

    protected override void AddCommands(CommandBuffer commandBuffer)
    {
        this.CreateBuffersIFN();

        base.AddCommands(commandBuffer);
        this.FillInputAndTime(commandBuffer);
        commandBuffer.SetGlobalBuffer("_InputAndTime", this.inputAndTime);
        commandBuffer.SetGlobalBuffer("_WorldData", this.worldData);
        commandBuffer.SetGlobalBuffer("_BallData", this.ballData);
        commandBuffer.SetGlobalInt("_BallDataSize", this.ballData.count);
        commandBuffer.SetGlobalBuffer("_BrickData", this.brickData);
        commandBuffer.SetGlobalInt("_BrickDataSize", this.brickData.count);
        commandBuffer.SetGlobalInt("_BrickX", this.BrickX);
        commandBuffer.SetGlobalInt("_BrickY", this.BrickY);

        if (this.BrickSetup != null)
        {
            commandBuffer.SetGlobalTexture("_BrickSetup", this.BrickSetup);
        }
        
        if (this.computeShader != null)
        {
            int kernelX = this.ballData.count / this.kernelSizeX;
            if (kernelSizeX * kernelX <  this.ballData.count)
            {
                ++kernelX;
            }

            if (this.doReset)
            {
                commandBuffer.DispatchCompute(this.computeShader, this.resetWorldKernelIndex, 1, 1, 1);
                commandBuffer.DispatchCompute(this.computeShader, this.resetKernelIndex, kernelX, 1, 1);
                this.doReset = false;
            }

            commandBuffer.DispatchCompute(this.computeShader, this.evolveWorldKernelIndex, 1, 1, 1);
            commandBuffer.DispatchCompute(this.computeShader, this.evolveKernelIndex, kernelX, 1, 1);
        }
    }

    private void CreateBuffersIFN()
    {
        if (this.inputAndTime == null)
        {
            // 2 touches 2 fois (4) + souris (2) + bouton souris (2) + dt (1) = 
            this.inputAndTime = new UnityEngine.ComputeBuffer(16, 4);
            this.inputAndTimeDatas = new float[this.inputAndTime.count];
        }

        if (this.worldData == null)
        {
            int sizeofOfStruct = System.Runtime.InteropServices.Marshal.SizeOf(typeof(WorldData));
            this.worldData = new UnityEngine.ComputeBuffer(1, sizeofOfStruct);
        }

        if (this.ballData == null || this.ballData.count != this.InstanceCount)
        {
            this.ballData?.Release();
            int sizeofOfStruct = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BallData));
            this.ballData = new UnityEngine.ComputeBuffer(this.InstanceCount, sizeofOfStruct);
            this.doReset = true;
        }

        int brickCount = this.BrickX * this.BrickY;
        if (this.brickData == null || this.brickData.count != brickCount)
        {
            this.brickData?.Release();
            int sizeofOfStruct = System.Runtime.InteropServices.Marshal.SizeOf(typeof(BrickData));
            this.brickData = new UnityEngine.ComputeBuffer(brickCount, sizeofOfStruct);
            this.doReset = true;
        }
    }

    private void FillInputAndTime(CommandBuffer commandBuffer)
    {
        this.inputAndTimeDatas[0] = UnityEngine.Time.deltaTime;
        this.inputAndTimeDatas[1] = UnityEngine.Time.smoothDeltaTime;

        this.inputAndTimeDatas[2] = UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftArrow) ? 1 : 0;
        this.inputAndTimeDatas[3] = UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightArrow) ? 1 : 0;

        this.inputAndTimeDatas[4] = UnityEngine.Input.GetKey(UnityEngine.KeyCode.Q) ? 1 : 0;
        this.inputAndTimeDatas[5] = UnityEngine.Input.GetKey(UnityEngine.KeyCode.D) ? 1 : 0;

        this.inputAndTimeDatas[6] = UnityEngine.Input.mousePosition.x;
        this.inputAndTimeDatas[7] = UnityEngine.Input.mousePosition.y;
        this.inputAndTimeDatas[8] = UnityEngine.Input.GetKey(UnityEngine.KeyCode.Mouse0) ? 1 : 0;
        this.inputAndTimeDatas[9] = UnityEngine.Input.GetKey(UnityEngine.KeyCode.Mouse1) ? 1 : 0;

        commandBuffer.SetBufferData(this.inputAndTime, this.inputAndTimeDatas);
    }

    private struct BallData
    {
        UnityEngine.Vector2 pos;
        UnityEngine.Vector2 speed;
        uint status;
    }

    private struct WorldData
    {
        UnityEngine.Vector2 handlePos;
        UnityEngine.Vector2 handleSize;

        UnityEngine.Vector2 worldSize;

        uint lostBallCount;
    }

    private struct BrickData
    {
        uint hp;
        uint damage;
        UnityEngine.Vector4 color;
    }
}
