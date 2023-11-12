using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    private Captive closestCaptive;
    private Chest closestChest;
    private List<Captive> captivesInRange = new List<Captive>();
    private List<Chest> chestsInRange = new List<Chest>();

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            // If closestCaptive is not null and is closer than the closestChest, interact with it
            if (closestCaptive && (closestChest == null ||
                DistanceToScreenCenter(closestCaptive.transform.position) <
                DistanceToScreenCenter(closestChest.transform.position)))
            {
                closestCaptive.ToggleFollow();
            }
            // Otherwise, if closestChest is not null, interact with it
            else if (closestChest)
            {
                closestChest.Interact();
            }
        }
    }

    private void Update()
    {
        CheckForNearbyInteractables();
    }

    public void AddCaptiveInRange(Captive captive)
    {
        if (!captivesInRange.Contains(captive))
        {
            captivesInRange.Add(captive);
        }
    }

    public void RemoveCaptiveInRange(Captive captive)
    {
        captivesInRange.Remove(captive);
    }

    // Call this method from Chest's OnTriggerEnter
    public void AddChestInRange(Chest chest)
    {
        if (!chestsInRange.Contains(chest))
        {
            chestsInRange.Add(chest);
        }
    }

    // Call this method from Chest's OnTriggerExit
    public void RemoveChestInRange(Chest chest)
    {
        if (chestsInRange.Contains(chest))
        {
            chestsInRange.Remove(chest);
        }
    }

    private float DistanceToScreenCenter(Vector3 worldPosition)
    {
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        return Vector2.Distance(screenPosition, screenCenter);
    }

    private void CheckForNearbyInteractables()
    {
        float closestScreenDistance = float.MaxValue;
        Captive previousClosestCaptive = closestCaptive;
        Chest previousClosestChest = closestChest;
        closestCaptive = null;
        closestChest = null;

        // Check for closest captive
        foreach (var captive in captivesInRange)
        {
            float screenDistance = DistanceToScreenCenter(captive.transform.position);
            if (screenDistance < closestScreenDistance)
            {
                closestScreenDistance = screenDistance;
                closestCaptive = captive;
            }
        }

        // Check for closest chest
        foreach (var chest in chestsInRange)
        {
            float screenDistance = DistanceToScreenCenter(chest.transform.position);
            if (screenDistance < closestScreenDistance)
            {
                closestScreenDistance = screenDistance;
                closestChest = chest;
                closestCaptive = null; // Ensure the closest captive is not considered if a chest is closer
            }
        }

        // Update interaction text for captives
        if (previousClosestCaptive != closestCaptive)
        {
            if (previousClosestCaptive) previousClosestCaptive.HideInteractionText();
            if (closestCaptive) closestCaptive.ShowInteractionText();
        }
    }
}