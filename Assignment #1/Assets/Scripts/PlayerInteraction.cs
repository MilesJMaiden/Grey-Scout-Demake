using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public TMP_Text interactionText;  // Reference to the UI text element

    private Captive closestNPC;

    private void Update()
    {
        CheckForNearbyCaptive();

        if (closestNPC && closestNPC.IsWithinInteractionRange() && Input.GetKeyDown(KeyCode.E))
        {
            closestNPC.ToggleFollow();
        }
    }

    private void CheckForNearbyCaptive()
    {
        Captive[] captives = FindObjectsOfType<Captive>();   
        float minDistance = Mathf.Infinity;

        closestNPC = null;

        foreach (var npc in captives)
        {
            float distanceToNPC = Vector3.Distance(transform.position, npc.transform.position);
            if (distanceToNPC < minDistance)
            {
                minDistance = distanceToNPC;
                closestNPC = npc;
            }
        }

        if (closestNPC && closestNPC.IsWithinInteractionRange())
        {
            interactionText.enabled = true;
        }
        else
        {
            interactionText.enabled = false;
        }
    }
}