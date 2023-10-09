using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    private Captive closestCaptive;

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && closestCaptive) // If the interaction button is pressed and there's a captive nearby
        {
            closestCaptive.ToggleFollow();
        }
    }

    private void Start()
    {
        
    }
    private void Update()
    {
        CheckForNearbyCaptive();
    }

    private void CheckForNearbyCaptive()
    {
        Captive[] captives = FindObjectsOfType<Captive>();
        float minDistance = Mathf.Infinity;

        Captive previousClosest = closestCaptive; // Store the previous closest captive
        closestCaptive = null;

        foreach (var captive in captives)
        {
            float distanceToCaptive = Vector3.Distance(transform.position, captive.transform.position);
            if (distanceToCaptive < minDistance)
            {
                minDistance = distanceToCaptive;
                closestCaptive = captive;
            }
        }
    }
}