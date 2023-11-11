using UnityEngine;

public class Gem : MonoBehaviour
{
    public float rotationSpeed = 40f;
    public float bobbingAmount = 0.1f;
    public float bobbingSpeed = 2f;

    private float startY;
    private float timer = 0;

    private void Start()
    {
        // Store the initial position
        startY = transform.position.y;
    }

    private void Update()
    {
        // Rotate around the Y axis
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        // Calculate the bobbing
        float newY = startY + Mathf.Sin(timer * bobbingSpeed) * bobbingAmount;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        // Increment timer
        timer += Time.deltaTime;
    }
}