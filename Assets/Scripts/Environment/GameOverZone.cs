using UnityEngine;

public class GameOverZone : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        EventsManager.Instance.ActionGameOver();
    }
}
