using UnityEngine;

public class FaceCamera : MonoBehaviour
{
	public Camera mainCamera;

	private void Start()
	{
		mainCamera = Camera.main;
	}

	private void Update()
	{
		Vector3 direction = transform.position - mainCamera.transform.position; // Flipped this line
		//direction.y = 0; // Zero out the Y component

		transform.rotation = Quaternion.LookRotation(direction);
	}
}