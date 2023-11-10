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

    public GameObject treePrefab; // Assign your tree prefab here in the Inspector
    public float treeEdgeBuffer = 5f; // Minimum distance from the edge to start placing trees
    public float treeDistanceApart = 5f; // Distance between each tree
    public float treeRandomDistanceOffset = 1f; // Random offset for tree placement
    public float forestEdgeWidth = 20f;

    public float planeSize = 250f; // The size of your plane

    public Material shaderMaterial;
    public Transform player;

    public bool showGizmos = true; // Toggle this in the inspector to show or hide gizmos
    public Color grassGizmoColor = Color.green;
    public Color treeGizmoColor = Color.yellow;

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

        CreateForest();
    }

    private void Update()
    {
        if (shaderMaterial != null && player != null)
            shaderMaterial.SetVector("_TramplePosition", player.position);
    }

    void CreateGrassArea(GrassArea grassArea)
    {
        // Calculate the center position based on the origin point and area size
        Vector3 centerPosition = new Vector3(grassArea.originPoint.x, tallestGrass / 2, grassArea.originPoint.y);
        GameObject grassAreaContainer = new GameObject("GrassAreaContainer");
        grassAreaContainer.transform.position = centerPosition;
        grassAreaContainer.transform.parent = transform; // Set the plane as the parent

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

                Vector3 grassPosition = centerPosition + positionOffset - new Vector3(0, tallestGrass / 2, 0); // Adjust position for grass instantiation

                GameObject grass = Instantiate(grassPrefab, grassPosition, Quaternion.identity, grassAreaContainer.transform); // Set as child of container
                grass.transform.localScale = new Vector3(1, Random.Range(shortestGrass, tallestGrass), 1);
            }
        }

        // Add and configure the box collider
        BoxCollider areaCollider = grassAreaContainer.AddComponent<BoxCollider>();
        areaCollider.size = new Vector3(grassArea.areaSize.x, tallestGrass, grassArea.areaSize.y);
        areaCollider.center = new Vector3(0, tallestGrass / 2, 0); // Move collider center up
        areaCollider.isTrigger = true;
        areaCollider.tag = "HideZone";
    }

    void CreateForest()
    {
        // Assuming the size of the plane is 250 x 250 in world units.

        // Calculate the bounds of the plane
        float halfPlaneSize = planeSize / 2;
        float minX = transform.position.x - halfPlaneSize;
        float maxX = transform.position.x + halfPlaneSize;
        float minZ = transform.position.z - halfPlaneSize;
        float maxZ = transform.position.z + halfPlaneSize;

        // Calculate the border area where trees will be placed
        float borderMinX = minX + forestEdgeWidth;
        float borderMaxX = maxX - forestEdgeWidth;
        float borderMinZ = minZ + forestEdgeWidth;
        float borderMaxZ = maxZ - forestEdgeWidth;

        // Instantiate trees within the border width
        for (float x = minX; x <= maxX; x += treeDistanceApart)
        {
            for (float z = minZ; z <= maxZ; z += treeDistanceApart)
            {
                // Check if the position is within the border width
                bool onBorderX = x <= borderMinX || x >= borderMaxX;
                bool onBorderZ = z <= borderMinZ || z >= borderMaxZ;
                if (onBorderX || onBorderZ)
                {
                    Vector3 treePosition = new Vector3(
                        x + Random.Range(-treeRandomDistanceOffset, treeRandomDistanceOffset),
                        0, // Assuming the ground is at y = 0
                        z + Random.Range(-treeRandomDistanceOffset, treeRandomDistanceOffset)
                    );
                    Instantiate(treePrefab, treePosition, Quaternion.identity, transform);
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

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw gizmos for the border area where trees will be instantiated
        float planeSize = 250f; // The size of your plane
        Vector3 planeCenter = transform.position;
        float borderSize = planeSize - 2 * forestEdgeWidth;

        // Draw the border area where trees will be instantiated
        Gizmos.color = treeGizmoColor;
        Gizmos.DrawWireCube(planeCenter, new Vector3(borderSize, 1f, borderSize));

        // Draw the inner area to show where trees won't be placed
        Gizmos.color = new Color(0, 1, 0, 0.5f); // Semi-transparent green
        Gizmos.DrawWireCube(planeCenter, new Vector3(borderSize, 1f, borderSize));

        // Draw gizmos for grass areas
        Gizmos.color = grassGizmoColor;
        foreach (GrassArea grassArea in grassAreas)
        {
            Vector3 grassCenterPosition = new Vector3(grassArea.originPoint.x, 0, grassArea.originPoint.y);
            if (grassArea.shape == GrassShape.Circle)
            {
                Gizmos.DrawWireSphere(grassCenterPosition, grassArea.radius);
            }
            else
            {
                // Draw a cube for each grass patch
                int grassCountX = Mathf.CeilToInt(grassArea.areaSize.x / distanceApart);
                int grassCountZ = Mathf.CeilToInt(grassArea.areaSize.y / distanceApart);

                for (int x = 0; x < grassCountX; x++)
                {
                    for (int z = 0; z < grassCountZ; z++)
                    {
                        Vector3 positionOffset = new Vector3(
                            (x * distanceApart + distanceApart / 2f) - grassArea.areaSize.x / 2,
                            0,
                            (z * distanceApart + distanceApart / 2f) - grassArea.areaSize.y / 2
                        );

                        Vector3 grassPosition = grassCenterPosition + positionOffset;
                        Gizmos.DrawCube(grassPosition, new Vector3(0.1f, 0.1f, 0.1f)); // Adjust the size as needed
                    }
                }
            }
        }
    }
}