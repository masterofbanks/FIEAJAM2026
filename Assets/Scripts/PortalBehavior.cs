using UnityEngine;

public class PortalBehavior : MonoBehaviour
{
    public GameManager GameManagementScript;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            GameManagementScript.ProceedToNextLevel();
        }
    }
}
