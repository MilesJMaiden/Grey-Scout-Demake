using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public TMP_Text interactionText;  // Reference to the UI text element

    private Captive closestCaptive;

    private void Start()
    {
        interactionText.enabled = false;
    }
    private void Update()
    {
        CheckForNearbyCaptive();

        if (closestCaptive && Input.GetKeyDown(KeyCode.E))
        {
            closestCaptive.ToggleFollow();
        }
    }

    private void CheckForNearbyCaptive()
    {
        Captive[] captives = FindObjectsOfType<Captive>();
        float minDistance = Mathf.Infinity;

        closestCaptive = null;

        foreach (var npc in captives)
        {
            float distanceToNPC = Vector3.Distance(transform.position, npc.transform.position);
            if (distanceToNPC < minDistance)
            {
                minDistance = distanceToNPC;
                closestCaptive = npc;
            }
        }

        // Since the Captive's IsWithinInteractionRange method is now reliant on a trigger collider,
        // we'll modify the condition here to simply check if interactionText is enabled.
        if (closestCaptive && interactionText.enabled)
        {
            interactionText.text = "Press E to Interact"; // Change this if you want a different message
        }
        else
        {
            interactionText.enabled = false;
        }
    }

    public void ShowInteractionText()
    {
        interactionText.enabled = true;
        interactionText.text = "Press E to Interact"; // Or your desired message
    }

    public void HideInteractionText()
    {
        interactionText.enabled = false;
    }
}