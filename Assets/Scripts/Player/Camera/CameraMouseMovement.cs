using UnityEngine;

public class CameraMouseMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Sensibilidad del movimiento horizontal")]
    [Range(0.1f, 5f)]
    public float sensibilidadX = 1f;
    
    [Tooltip("Sensibilidad del movimiento vertical")]
    [Range(0.1f, 5f)]
    public float sensibilidadY = 1f;
    
    [Tooltip("Suavizado del movimiento (más alto = más suave)")]
    [Range(1f, 20f)]
    public float suavizado = 5f;
    
    [Header("Límites de Rotación")]
    [Tooltip("Ángulo máximo de rotación horizontal")]
    [Range(0f, 45f)]
    public float limiteRotacionX = 10f;
    
    [Tooltip("Ángulo máximo de rotación vertical")]
    [Range(0f, 45f)]
    public float limiteRotacionY = 10f;
    
    private Vector3 rotacionOriginal;
    private Vector3 rotacionObjetivo;
    
    void Start()
    {
        // Guardamos la rotación inicial de la cámara
        rotacionOriginal = transform.localEulerAngles;
        rotacionObjetivo = rotacionOriginal;
    }
    
    void Update()
    {
        // Obtenemos la posición del mouse normalizada (-1 a 1)
        float mouseX = (Input.mousePosition.x / Screen.width - 0.5f) * 2f;
        float mouseY = (Input.mousePosition.y / Screen.height - 0.5f) * 2f;
        
        // Calculamos la rotación objetivo basada en la posición del mouse
        float rotX = mouseY * limiteRotacionY * sensibilidadY;
        float rotY = mouseX * limiteRotacionX * sensibilidadX;
        
        // Aplicamos la rotación con respecto a la rotación original
        rotacionObjetivo = rotacionOriginal + new Vector3(-rotX, rotY, 0f);
        
        // Interpolamos suavemente hacia la rotación objetivo
        transform.localEulerAngles = Vector3.Lerp(
            transform.localEulerAngles, 
            rotacionObjetivo, 
            Time.deltaTime * suavizado
        );
    }
    
    // Método opcional para resetear la cámara a su posición original
    public void ResetearCamara()
    {
        transform.localEulerAngles = rotacionOriginal;
        rotacionObjetivo = rotacionOriginal;
    }
}