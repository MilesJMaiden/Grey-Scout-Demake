using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Captive : MonoBehaviour
{
    public float interactionRange = 3f;  // Distance within which player can interact with the NPC
    public float followDistance = 2f;  // Distance at which NPC will follow the player

    private bool isFollowing = false;
    private Transform playerTransform;

    private void Start()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (isFollowing)
        {
            // Follow the player with a certain distance
            Vector3 followPosition = playerTransform.position - playerTransform.forward * followDistance;
            transform.position = Vector3.Lerp(transform.position, followPosition, Time.deltaTime);
        }
    }

    public void ToggleFollow()
    {
        isFollowing = !isFollowing;
    }

    public bool IsWithinInteractionRange()
    {
        return Vector3.Distance(transform.position, playerTransform.position) <= interactionRange;
    }

    // This will draw a sphere in the Unity editor when the NPC is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;  // Set the color to green (or any color you prefer)
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}
