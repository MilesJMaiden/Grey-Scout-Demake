using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrass : MonoBehaviour
{

    public GameObject grassPrefab;

    public int grassSize = 20;

    public float distanceApart = 2f;
    public float randomDistanceOffset = 0.25f;

    public float shortestGrass = 1.2f;
    public float tallestGrass = 1.6f;


    void Start()
    {
        for (int z = -grassSize; z <= grassSize; z++)
        {
            for (int x = -grassSize; x <= grassSize; x++)
            {
                //plant apart
                Vector3 position = new Vector3(x / distanceApart + Random.Range(-randomDistanceOffset, randomDistanceOffset), 0, z / distanceApart + Random.Range(-randomDistanceOffset, randomDistanceOffset));
                GameObject grass = Instantiate(grassPrefab, position, Quaternion.identity);
                grass.transform.localScale = new Vector3(1, Random.Range(shortestGrass, tallestGrass), 1);
            }
        }
    }

}
