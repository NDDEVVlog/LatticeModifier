using UnityEngine;
[ExecuteInEditMode]
public class MotorShaderController : MonoBehaviour
{   

    [Header("Material Reference")]
    public Material motorMaterial;

    [Header("Rotation")]
    [Range(0f, 50f)]
    public float rotationSpeed = 1.0f;

    [Header("Piston Settings")]
    [Range(0f, 10f)]
    public float pistonBlueSpeed = 1.0f;
    
    [Range(0f, 10f)]
    public float pistonRedSpeed = 1.0f;
    
    [Range(-10f, 10f)]
    public float pistonBluePhase = 0.0f;
    
    [Range(-10f, 10f)]
    public float pistonRedPhase = 0.0f;

    [Header("Shake Settings")]
    [Range(0f, 100f)]
    public float shakeFrequency = 50.0f;

    [Range(0f, 0.1f)]
    public float shakeAmplitude = 0.0f;

    void Update()
    {
        if (motorMaterial == null) return;

        motorMaterial.SetFloat("_RotationSpeed", rotationSpeed);
        motorMaterial.SetFloat("_PistonBlueSpeed", pistonBlueSpeed);
        motorMaterial.SetFloat("_PistonRedSpeed", pistonRedSpeed);
        motorMaterial.SetFloat("_PistonBluePhase", pistonBluePhase*Mathf.PI);
        motorMaterial.SetFloat("_PistonRedPhase", pistonRedPhase*Mathf.PI);
        motorMaterial.SetFloat("_ShakeFreq", shakeFrequency);
        motorMaterial.SetFloat("_ShakeAmplitude", shakeAmplitude);
    }
}
