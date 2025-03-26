using UnityEngine;

public class ComputeShaderExample : MonoBehaviour
{
    public ComputeShader computeShader;
    private ComputeBuffer buffer;
    private Vector3[] dataArray;
    private int kernelID;
    
    void Start()
    {
        int count = 1000; // Number of Vector3 elements
        dataArray = new Vector3[count];

        // Initialize with some values
        for (int i = 0; i < count; i++)
            dataArray[i] = new Vector3(i, i * 2, i * 3);

        // Create a ComputeBuffer (3 floats per Vector3)
        buffer = new ComputeBuffer(count, sizeof(float) * 3);
        buffer.SetData(dataArray); // Send data to GPU

        // Get the kernel ID
        kernelID = computeShader.FindKernel("CSMain");

        // Bind the buffer to the compute shader
        computeShader.SetBuffer(kernelID, "transformedVertices", buffer);

        // Dispatch Compute Shader (Divide count by thread group size)
        int threadGroups = Mathf.CeilToInt(count / 256f);
        computeShader.Dispatch(kernelID, threadGroups, 1, 1);

        // Retrieve the modified data from GPU
        buffer.GetData(dataArray);

        // Print some results
        Debug.Log("First element after compute: " + dataArray[0]);
        Debug.Log("Last element after compute: " + dataArray[count - 1]);

        // Release buffer
        buffer.Release();
    }
}
