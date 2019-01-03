using UnityEngine;

public class ExampleClass : MonoBehaviour {

    public int instanceCount = 100000;
    public Mesh instanceMesh;
    public Material instanceMaterial;

    private int cachedInstanceCount = -1;
    private ComputeBuffer positionBuffer;
    private ComputeBuffer argsBuffer;
    private uint[] args = new uint[5] { 0, 0, 0, 0, 0 };
    public float spacing = 0.07f;
    private float cachedSpacing = 0.07f;
    
    public float amplitude = 1.0f;
    private float cachedAmplitude = 1.0f;
    void Start() {

        argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        UpdateBuffers();
    }

    void Update()
    {
        elapsedTime += Time.deltaTime;
        // Update starting position buffer
        if (cachedInstanceCount != instanceCount || cachedSpacing != spacing || amplitude != cachedAmplitude)
        {
            UpdateBuffers();
        }

        // Pad input
        if (Input.GetAxisRaw("Horizontal") != 0.0f)
            instanceCount = (int)Mathf.Clamp(instanceCount + Input.GetAxis("Horizontal") * 40000, 1.0f, 5000000.0f);

        // Render
        Graphics.DrawMeshInstancedIndirect(instanceMesh, 0, instanceMaterial, new Bounds(Vector3.zero, new Vector3(100.0f, 100.0f, 100.0f)), argsBuffer);
    }

    void OnGUI() {

        GUI.Label(new Rect(265, 25, 200, 30), "Instance Count: " + instanceCount.ToString());
        instanceCount = (int)GUI.HorizontalSlider(new Rect(25, 20, 200, 30), (float)instanceCount, 1.0f, 5000000.0f);
    }

    private float elapsedTime = 0.0f;

    void UpdateBuffers() {

        // positions
        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = new ComputeBuffer(instanceCount, 16);
        Vector4[] positions = new Vector4[instanceCount];


        float width = 20.0f;
        float height = 20.0f;
        float size = 0.05f;
        
        for (int i = 0; i < instanceCount; i++)
        {

            float x = (i % 100) * size + (i % 100) * spacing + size / 2.0f;
            float z = -Mathf.Floor(i / 100.0f) * size - Mathf.Floor(i / 100.0f) * spacing + size / 2.0f;
            float y = amplitude * Mathf.Sin(elapsedTime / 2) * 5 * Mathf.Sin(x * z);
            
            positions[i] = new Vector4(x, y, z, size);
        }
        positionBuffer.SetData(positions);
        instanceMaterial.SetBuffer("positionBuffer", positionBuffer);

        // indirect args
        uint numIndices = (instanceMesh != null) ? (uint)instanceMesh.GetIndexCount(0) : 0;
        args[0] = numIndices;
        args[1] = (uint)instanceCount;
        argsBuffer.SetData(args);

        cachedInstanceCount = instanceCount;
        cachedSpacing = spacing;

        cachedAmplitude = amplitude;
    }

    void OnDisable() {

        if (positionBuffer != null)
            positionBuffer.Release();
        positionBuffer = null;

        if (argsBuffer != null)
            argsBuffer.Release();
        argsBuffer = null;
    }
}