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
        GameManager.Instance.AddScore(scoreValue);

        hasInteracted = true;
        textMeshProObject.SetActive(false);
        StartCoroutine(RotateObject());
    }

    private IEnumerator RotateObject()
    {
        Destroy(textMeshProObject);
        interactionComplete = true;

        Vector3 startRotation = objectToRotate.transform.eulerAngles;
        Vector3 endRotation = new Vector3(startRotation.x, startRotation.y, startRotation.z + openAngle);

        float timeElapsed = 0;

        while (timeElapsed < rotationTime)
        {
            // Calculate the next rotation frame
            Vector3 nextRotation = new Vector3(
                startRotation.x,
                startRotation.y,
                Mathf.Lerp(startRotation.z, endRotation.z, timeElapsed / rotationTime)
            );

            objectToRotate.transform.eulerAngles = nextRotation;
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        objectToRotate.transform.eulerAngles = endRotation;

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