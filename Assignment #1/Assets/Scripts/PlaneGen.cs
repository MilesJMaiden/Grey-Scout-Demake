using System.Collections.Generic;
using UnityEngine;

public class PlaneGen : MonoBehaviour
{
    public GameObject grassPrefab;
    public List<GrassArea> grassAreas;

    public float distanceApart = 2f;
    public float randomDistanceOffset = 0.25f;

    public float shortestGrass = 1.2f;
    public float tallestGrass = 1.6f;

    public GameObject treePrefab;
    public float treeEdgeBuffer = 5f;
    public float treeDistanceApart = 5f; // Distance between each tree
    public float treeRandomDistanceOffset = 1f; // Random offset for tree placement
    public float forestEdgeWidth = 20f;

    public float planeSize = 250f;

    public Material shaderMaterial;
    public Transform player;

    public bool showGizmos = true; // Toggle this in the inspector to show or hide gizmos
    public Color grassGizmoColor = Color.green;
    public Color treeGizmoColor = Color.yellow;

    [System.Serializable]
    public class GrassArea
    {
        public Vector2 originPoint;
        public Vector2 areaSize;
        public GrassShape shape;

        public GrassArea(Vector2 origin, Vector2 size, float rad, GrassShape shapeType)
        {
            originPoint = origin;
            areaSize = size;
            shape = shapeType;
        }
    }

    public enum GrassShape
    {
        Square,
        //Circle,
    }

    private void OnEnable()
    {
        GameManager.OnPlayerRespawned += HandlePlayerRespawned;
    }

    private void OnDisable()
    {
        GameManager.OnPlayerRespawned -= HandlePlayerRespawned;
    }

    private void HandlePlayerRespawned(GameObject respawnedPlayer)
    {
        player = respawnedPlayer.transform;
    }

    void Start()
    {
        // Create a master container for all grass area containers
        GameObject masterGrassContainer = new GameObject("MasterGrassContainer");
        masterGrassContainer.transform.SetParent(transform, false); // Set as child of the plane

        foreach (GrassArea grassArea in grassAreas)
        {
            CreateGrassArea(grassArea, masterGrassContainer);
        }

        CreateForest();
    }

    private void Update()
    {
        if (shaderMaterial != null)
        {
            List<Vector4> positions = new List<Vector4>();

            // Add the player's position if the player exists
            if (player != null)
            {
                positions.Add(new Vector4(player.position.x, player.position.y, player.position.z, 1));
            }

            // Add the positions of all active enemies
            foreach (Enemy enemy in EnemyManager.Instance.activeEnemies)
            {
                positions.Add(new Vector4(enemy.transform.position.x, enemy.transform.position.y, enemy.transform.position.z, 1));
            }

            // Now, convert the List to an array and pass it to the shader
            shaderMaterial.SetVectorArray("_TramplePosition", positions.ToArray());
        }
    }

    void CreateGrassArea(GrassArea grassArea, GameObject masterGrassContainer)
    {
        // Calculate the center position based on the origin point and area size
        Vector3 centerPosition = new Vector3(grassArea.originPoint.x, 0, grassArea.originPoint.y);

        // Create a new container GameObject for this grass area
        GameObject grassAreaContainer = new GameObject("GrassAreaContainer_" + grassArea.originPoint);
        grassAreaContainer.transform.SetParent(masterGrassContainer.transform, false); // Set as child of the master container
        grassAreaContainer.transform.localPosition = centerPosition - new Vector3(0, 0, 0); // Adjust local position relative to the master container

        int grassCountX = Mathf.CeilToInt(grassArea.areaSize.x / distanceApart);
        int grassCountZ = Mathf.CeilToInt(grassArea.areaSize.y / distanceApart);

        // Start at the bottom-left corner of the grass area
        Vector3 startPosition = centerPosition - new Vector3(grassArea.areaSize.x / 2, 0, grassArea.areaSize.y / 2);

        for (int x = 0; x < grassCountX; x++)
        {
            for (int z = 0; z < grassCountZ; z++)
            {
                Vector3 positionOffset = new Vector3(
                    x * distanceApart + Random.Range(-randomDistanceOffset, randomDistanceOffset),
                    0,
                    z * distanceApart + Random.Range(-randomDistanceOffset, randomDistanceOffset)
                );

                Vector3 grassPosition = startPosition + positionOffset; // Position relative to the grass area's bottom-left corner

                GameObject grass = Instantiate(grassPrefab, grassPosition, Quaternion.identity, grassAreaContainer.transform);
                grass.transform.localScale = new Vector3(1, Random.Range(shortestGrass, tallestGrass), 1);
            }
        }

        // Add and configure the box collider for this grass area
        BoxCollider areaCollider = grassAreaContainer.AddComponent<BoxCollider>();
        areaCollider.size = new Vector3(grassArea.areaSize.x, tallestGrass, grassArea.areaSize.y);
        areaCollider.center = new Vector3(0, tallestGrass / 2, 0); // Adjust collider center
        areaCollider.isTrigger = true;
        areaCollider.tag = "HideZone";
    }

    void CreateForest()
    {
        // Create a container for the trees
        GameObject treeContainer = new GameObject("TreeContainer");
        treeContainer.transform.parent = transform; // Set the plane as the parent

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
                    // Instantiate the tree as a child of the tree container
                    GameObject tree = Instantiate(treePrefab, treePosition, Quaternion.identity, treeContainer.transform);
                }
            }
        }
    }

    void OnDrawGizmos()
    {
        if (!showGizmos) return;

        // Draw the border area where trees will be instantiated
        Gizmos.color = treeGizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(planeSize - 2 * forestEdgeWidth, 1f, planeSize - 2 * forestEdgeWidth));

        // Draw the inner area to show where trees won't be placed
        Gizmos.color = new Color(0, 1, 0, 0.5f); // Semi-transparent green
        Gizmos.DrawWireCube(transform.position, new Vector3(planeSize - 2 * (forestEdgeWidth + treeEdgeBuffer), 1f, planeSize - 2 * (forestEdgeWidth + treeEdgeBuffer)));

        // Draw gizmos for grass areas
        foreach (GrassArea grassArea in grassAreas)
        {
            Vector3 grassCenterPosition = new Vector3(grassArea.originPoint.x, 0, grassArea.originPoint.y);
            Vector3 startGizmoPosition = grassCenterPosition - new Vector3(grassArea.areaSize.x / 2, 0, grassArea.areaSize.y / 2);

            Gizmos.color = grassGizmoColor;

            int grassCountX = Mathf.CeilToInt(grassArea.areaSize.x / distanceApart);
            int grassCountZ = Mathf.CeilToInt(grassArea.areaSize.y / distanceApart);

            for (int x = 0; x < grassCountX; x++)
            {
                for (int z = 0; z < grassCountZ; z++)
                {
                    Vector3 positionOffset = new Vector3(
                        x * distanceApart,
                        0,
                        z * distanceApart
                    );

                    Vector3 gizmoPosition = startGizmoPosition + positionOffset;
                    Gizmos.DrawWireCube(gizmoPosition, new Vector3(1, tallestGrass, 1) * 0.1f); // Scale down the gizmo representation
                }
            }
        }
    }
}