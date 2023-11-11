using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    public GameObject textMeshProObject;
    public GameObject objectToRotate; 
    public GameObject objectToActivate;
    public float rotationTime = 2f; 
    public float activationTime = 5f;
    public int scoreValue = 5;

    public float openAngle = 135f;

    private bool isPlayerInRange = false;
    private bool hasInteracted = false;

    private bool interactionComplete = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !interactionComplete)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction)
            {
                playerInteraction.AddChestInRange(this);
            }

            if (textMeshProObject != null)
                textMeshProObject.SetActive(true);
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && !interactionComplete)
        {
            PlayerInteraction playerInteraction = other.GetComponent<PlayerInteraction>();
            if (playerInteraction)
            {
                playerInteraction.AddChestInRange(this);
            }

            if (textMeshProObject != null)
                textMeshProObject.SetActive(false);
            isPlayerInRange = false;
        }
    }

    public void Interact()
    {
        if (!hasInteracted)
        {
            OpenChest();
        }
    }

    public void OpenChest()
    {
        // Increase the player's score by 5
        GameManager.Instance.AddScore(scoreValue);

        hasInteracted = true;
        textMeshProObject.SetActive(false);
        StartCoroutine(RotateObject());
    }

    private IEnumerator RotateObject()
    {
        Destroy(textMeshProObject);
        interactionComplete = true;

        Quaternion startRotation = objectToRotate.transform.rotation;
        Quaternion endRotation = Quaternion.Euler(0, 0, openAngle);
        float timeElapsed = 0;

        while (timeElapsed < rotationTime)
        {
            objectToRotate.transform.rotation = Quaternion.Slerp(startRotation, endRotation, timeElapsed / rotationTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        objectToRotate.transform.rotation = endRotation; 

        // Enable the object and start the second timer
        objectToActivate.SetActive(true);
        StartCoroutine(ActivationTimer());
    }

    private IEnumerator ActivationTimer()
    {
        yield return new WaitForSeconds(activationTime);
        objectToActivate.SetActive(false);
    }
}