using System.Collections.Generic;
using UnityEngine;

public class CreateGrass : MonoBehaviour
{
    public GameObject grassPrefab;
    public List<GrassArea> grassAreas;

    public float distanceApart = 2f;
    public float randomDistanceOffset = 0.25f;

    public float shortestGrass = 1.2f;
    public float tallestGrass = 1.6f;

    public Material shaderMaterial;
    public Transform player;

    [System.Serializable]
    public class GrassArea
    {
        public Vector2 originPoint;
        public Vector2 areaSize; // Used for square and rectangle
        public float radius; // Used for circle
        public GrassShape shape;

        public GrassArea(Vector2 origin, Vector2 size, float rad, GrassShape shapeType)
        {
            originPoint = origin;
            areaSize = size;
            radius = rad;
            shape = shapeType;
        }
    }

    public enum GrassShape
    {
        Square,
        Circle,
    }

    void Start()
    {
        foreach (GrassArea grassArea in grassAreas)
        {
            CreateGrassArea(grassArea);
        }
    }

    private void Update()
    {
        if (shaderMaterial != null && player != null)
            shaderMaterial.SetVector("_TramplePosition", player.position);
    }

    void CreateGrassArea(GrassArea grassArea)
    {
        // Calculate the start position based on the origin point and area size
        Vector3 centerPosition = new Vector3(grassArea.originPoint.x, 0, grassArea.originPoint.y);

        int grassCountX = Mathf.RoundToInt(grassArea.areaSize.x / distanceApart);
        int grassCountZ = Mathf.RoundToInt(grassArea.areaSize.y / distanceApart);

        for (int z = 0; z < grassCountZ; z++)
        {
            for (int x = 0; x < grassCountX; x++)
            {
                Vector3 positionOffset = new Vector3(
                    x * distanceApart + Random.Range(-randomDistanceOffset, randomDistanceOffset),
                    0,
                    z * distanceApart + Random.Range(-randomDistanceOffset, randomDistanceOffset)
                );

                Vector3 grassPosition = centerPosition + positionOffset;

                // Check if the position is within the shape's bounds
                if (IsWithinShapeBounds(grassPosition, centerPosition, grassArea))
                {
                    GameObject grass = Instantiate(grassPrefab, grassPosition, Quaternion.identity, transform);
                    grass.transform.localScale = new Vector3(1, Random.Range(shortestGrass, tallestGrass), 1);
                }
            }
        }
    }

    bool IsWithinShapeBounds(Vector3 position, Vector3 center, GrassArea area)
    {
        switch (area.shape)
        {
            case GrassShape.Circle:
                return Vector3.Distance(position, center) <= area.radius;
            case GrassShape.Square:
                return Mathf.Abs(position.x - center.x) <= area.areaSize.x / 2 &&
                       Mathf.Abs(position.z - center.z) <= area.areaSize.y / 2;
            default:
                return false;
        }
    }
}