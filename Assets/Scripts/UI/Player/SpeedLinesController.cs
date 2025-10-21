using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedLinesController : MonoBehaviour
{
    [SerializeField] private Rigidbody playerRb;
    [SerializeField] private ParticleSystem speedLinesPS;
    
    [Header("Speed Settings")]
    public float speedThreshold = 6.0f;
    public float maxSpeed = 20.0f; // Velocidad a la cual el efecto está al máximo
    
    [Header("Emission Settings")]
    public float minEmissionRate = 10f;
    public float maxEmissionRate = 100f;
    
    [Header("Particle Size Settings")]
    public float minParticleSize = 0.5f;
    public float maxParticleSize = 2.0f;
    
    [Header("Particle Speed Settings")]
    public float minParticleSpeed = 5f;
    public float maxParticleSpeed = 15f;
    
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.MainModule mainModule;

    void Start()
    {
        if (speedLinesPS != null)
        {
            emissionModule = speedLinesPS.emission;
            mainModule = speedLinesPS.main;
            emissionModule.enabled = false;
        }
        else Debug.LogError("No se asigno el sistema de particulas");
        
        if (playerRb == null)
            Debug.LogError("No se asigno player RB");
    }

    void Update()
    {
        if (playerRb == null || speedLinesPS == null) return;

        float currentSpeed = playerRb.velocity.magnitude;

        // Si supera el umbral, activamos y ajustamos la intensidad
        if (currentSpeed > speedThreshold)
        {
            if (!emissionModule.enabled)
            {
                emissionModule.enabled = true;
            }
            
            // Calculamos el porcentaje de velocidad entre el threshold y maxSpeed
            float speedPercent = Mathf.InverseLerp(speedThreshold, maxSpeed, currentSpeed);
            
            // Interpolamos la tasa de emisión
            float targetEmissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, speedPercent);
            emissionModule.rateOverTime = targetEmissionRate;
            
            // Interpolamos el tamaño de las partículas
            float targetSize = Mathf.Lerp(minParticleSize, maxParticleSize, speedPercent);
            mainModule.startSize = targetSize;
            
            // Interpolamos la velocidad de las partículas
            float targetParticleSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, speedPercent);
            mainModule.startSpeed = targetParticleSpeed;
        }
        else
        {
            if (emissionModule.enabled)
            {
                emissionModule.enabled = false;
            }
        }
    }
}