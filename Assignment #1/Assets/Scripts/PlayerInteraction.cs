using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    private Captive closestCaptive;

	private List<Captive> captivesInRange = new List<Captive>();

	public void OnInteract(InputAction.CallbackContext context)
	{
		if (context.started && closestCaptive)
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

	public void AddCaptiveInRange(Captive captive)
	{
		if (!captivesInRange.Contains(captive))
		{
			captivesInRange.Add(captive);
		}
	}

	public void RemoveCaptiveInRange(Captive captive)
	{
		if (captivesInRange.Contains(captive))
		{
			captivesInRange.Remove(captive);
		}
	}

	private float DistanceToScreenCenter(Vector3 worldPosition)
	{
		Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
		Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
		return Vector2.Distance(screenPosition, screenCenter);
	}
	private void CheckForNearbyCaptive()
	{
		float closestScreenDistance = float.MaxValue;

		Captive previousClosest = closestCaptive; // Store the previous closest captive
		closestCaptive = null;

		foreach (var captive in captivesInRange)  // Only consider captives in range
		{
			float screenDistance = DistanceToScreenCenter(captive.transform.position);
			if (screenDistance < closestScreenDistance)
			{
				closestScreenDistance = screenDistance;
				closestCaptive = captive;
			}
		}

		if (previousClosest != closestCaptive)
		{
			if (previousClosest) previousClosest.HideInteractionText();
			if (closestCaptive) closestCaptive.ShowInteractionText();
		}
	}

}