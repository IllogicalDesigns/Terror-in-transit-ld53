using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FloorTest : MonoBehaviour {
    [SerializeField] private Transform player;
    [SerializeField] private Transform searcher;
    private InfluenceMap3D map3D;

    private int xSize = -1;
    private int ySize = -1;
    private int zSize = -1;

    private Vector3 start;

    [SerializeField] private int PROPSTEPS = 5;
    [SerializeField] private float PROPTIME = 0.05f;

    [SerializeField] private float DotFaceForwardForFill = 0.5f;

    private const byte BARRIER = 0;
    private const int HOT = 10;
    private const byte STARTING = 1;

    private byte[,,] debugGrid;

    private int maxNewHeat = 50;

    // Start is called before the first frame update
    private void Start() {
        map3D = FindObjectOfType<InfluenceMap3D>();
    }

    // Update is called once per frame
    private void Update() {
        if (Input.GetKeyDown(KeyCode.I)) {
            StartCoroutine(HeatWaveChasePropagation(player, searcher, PROPSTEPS, PROPTIME));
        }
    }

    public IEnumerator HeatWaveChasePropagation(Transform target, Transform searcher, int PROPSTEPS = 5, float PROPTIME = 0.05f) {
        byte[,,] grid = map3D.GetGrid();
        debugGrid = grid;

        if (grid == null) {
            Debug.Log("FloorTest:HeatWaveChasePropagation() grid is null, returning");
            yield break;
        }

        Vector3 size = map3D.GetSizeVector();
        xSize = Mathf.RoundToInt(size.x);
        ySize = Mathf.RoundToInt(size.y);
        zSize = Mathf.RoundToInt(size.z);

        start = map3D.start;

        Debug.DrawRay(target.position, searcher.forward, Color.green, 10f);
        var initTargetPosition = map3D.WorldToGrid(target.position);
        var initDirection = target.position - searcher.position;

        //var nonRoundedLocation = map3D.WorldToGrid(initPosition);
        //Vector3Int initGridLocation = new Vector3Int(Mathf.RoundToInt(nonRoundedLocation.x), Mathf.RoundToInt(nonRoundedLocation.y), Mathf.RoundToInt(nonRoundedLocation.y));
        Debug.DrawRay(map3D.GridToWorld(initTargetPosition), Vector3.up * 5f, Color.cyan, 10f);

        List<Vector3Int> newBarriers = new List<Vector3Int>();
        List<Vector3Int> newHeat = new List<Vector3Int>();
        List<Vector3Int> oldHeat = new List<Vector3Int>();

        HeatUpInitLocation(grid, initTargetPosition, newHeat, oldHeat);
        debugGrid = grid;
        GenerateInitBarrier(grid, newBarriers, initTargetPosition, initDirection);
        debugGrid = grid;

        yield return new WaitForEndOfFrame();

        for (int i = 0; i < PROPSTEPS; i++) {
            //TODO calculate centriod as heat is built up
            PropagateHeat(grid, newHeat, oldHeat);
            if (newHeat.Count > maxNewHeat) break;
            CoolOldHeat(grid, oldHeat);
            PropagateBarriers(grid, newBarriers);
            debugGrid = grid;
            yield return new WaitForSeconds(PROPTIME);
        }

        debugGrid = grid;

        var choosenSearchPoint = FindCentroidOrClosest(oldHeat);
        Debug.DrawRay(map3D.GridToWorld(choosenSearchPoint), Vector3.up * 100, Color.red, 10f);

        void GenerateInitBarrier(byte[,,] grid, List<Vector3Int> newBarriers, Vector3Int initPosition, Vector3 initDirection) {
            List<Vector3Int> neighbors = GetNeighbors(grid, initPosition.x, initPosition.y, initPosition.z);
            for (int i = 0; i < neighbors.Count; i++) {
                SetOppositeToBarrier(grid, neighbors[i].x, neighbors[i].y, neighbors[i].z);
            }
        }

        void SetOppositeToBarrier(byte[,,] grid, int x, int y, int z) {
            if (grid[x, y, z] != STARTING) return;

            // Calculate the world position of the current cell
            Vector3 cellPos = map3D.GridToWorld(x, y, z);

            //var playerGrid = map3D.WorldToGrid(initTargetPosition);
            //var playerCellWorldPos = map3D.GridToWorld(playerGrid.x, playerGrid.y, playerGrid.z);
            // Calculate the vector from the current cell to the given world position
            Vector3 toCell = -(cellPos - target.position);
            Vector3 toUs = -(searcher.position - target.position);
            //Debug.DrawRay(searcher.position, toUs * 100, Color.magenta, 20f);

            // Calculate the dot product between the vector and the opposite direction
            float dotProduct = Vector3.Dot(toCell.normalized, toUs.normalized);

            // Check if the dot product is above a certain threshold (e.g., 0.8f)
            if (dotProduct > DotFaceForwardForFill) {
                Debug.DrawRay(target.position, toCell * 100, Color.magenta, 20f);
                //Debug.DrawRay(cellPos, toCell * map3D.size, Color.red, 20f);
                //Debug.DrawLine(cellPos, target.position, Color.red, 20f);
                // Set the cell to 0, orig impl was -1
                grid[x, y, z] = BARRIER;
                newBarriers.Add(new Vector3Int(x, y, z));
            }
            else {
                //Debug.DrawLine(cellPos, target.position, Color.green, 20f);
                //Debug.DrawRay(cellPos, toCell * map3D.size, Color.green, 20f);
            }
        }
    }

    public static Vector3Int FindCentroidOrClosest(List<Vector3Int> vectors) {
        int count = vectors.Count;
        Vector3Int sum = Vector3Int.zero;

        foreach (Vector3Int vector in vectors) {
            sum += vector;
        }

        Vector3Int centroid = sum / count;

        if (vectors.Contains(centroid)) {
            return centroid;
        }

        Vector3Int closestVector = vectors[0];
        float closestDistance = Vector3Int.Distance(centroid, closestVector);

        for (int i = 1; i < count; i++) {
            Vector3Int currentVector = vectors[i];
            float currentDistance = Vector3Int.Distance(centroid, currentVector);

            if (currentDistance < closestDistance) {
                closestVector = currentVector;
                closestDistance = currentDistance;
            }
        }

        return closestVector;
    }

    private Vector3 ChooseChasePointBasedOnCluster(List<Vector3Int> newHeat) {
        List<List<Vector3Int>> clusters = new List<List<Vector3Int>>();
        HashSet<Vector3Int> visited = new HashSet<Vector3Int>();

        foreach (Vector3Int p in newHeat) {
            if (visited.Contains(p))
                continue;

            List<Vector3Int> cluster = new List<Vector3Int>();
            cluster.Add(p);
            visited.Add(p);

            int i = 0;
            while (i < cluster.Count) {
                Vector3Int current = cluster[i];
                foreach (Vector3Int q in newHeat) {
                    if (!visited.Contains(q) && ManhattanDistance(current, q) == 1) {
                        cluster.Add(q);
                        visited.Add(q);
                    }
                }
                i++;
            }

            clusters.Add(cluster);
        }

        var sortedClusters = clusters.OrderByDescending(cluster => cluster.Count).ToList();
        if (sortedClusters.Count > 0 && sortedClusters.First().Count > 0) {
            var bestPos = sortedClusters[0][0];
            return GridToWorld(bestPos.x, bestPos.y, bestPos.z);
        }
        else return Vector3.zero;
    }

    private int ManhattanDistance(Vector3Int p1, Vector3Int p2) {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y) + Mathf.Abs(p1.z - p2.z);
    }

    private void HeatUpInitLocation(byte[,,] grid, Vector3Int initGridLocation, List<Vector3Int> newHeat, List<Vector3Int> oldHeat) {
        grid[initGridLocation.x, initGridLocation.y, initGridLocation.z] = HOT;
        newHeat.Add(new Vector3Int(initGridLocation.x, initGridLocation.y, initGridLocation.z));
        oldHeat.Add(new Vector3Int(initGridLocation.x, initGridLocation.y, initGridLocation.z));
    }

    private void PropagateHeat(byte[,,] grid, List<Vector3Int> newHeat, List<Vector3Int> oldHeat) {
        List<Vector3Int> tempHeat = new List<Vector3Int>(newHeat);
        newHeat.Clear();

        foreach (var item in tempHeat) {
            var neighbors = GetNeighbors(grid, item.x, item.y, item.z);
            foreach (var subItem in neighbors) {
                var val = grid[subItem.x, subItem.y, subItem.z];

                if (val == BARRIER) continue; //converted from -1 in orig impl
                if (val != BARRIER + 1) continue; //converted from 0 in orig impl

                grid[subItem.x, subItem.y, subItem.z] = 10;
                newHeat.Add(new Vector3Int(subItem.x, subItem.y, subItem.z));
                oldHeat.Add(new Vector3Int(subItem.x, subItem.y, subItem.z));
            }
        }
    }

    private void CoolOldHeat(byte[,,] grid, List<Vector3Int> oldHeat) {
        for (int i = 0; i < oldHeat.Count; i++) {
            var x = oldHeat[i].x;
            var y = oldHeat[i].y;
            var z = oldHeat[i].z;
            if (grid[x, y, z] > STARTING + 1) {
                grid[x, y, z] -= 1;
            }
            else {
                // If the value is 1, remove the point from oldHeat.  Orig impl used 0 here
                grid[x, y, z] = STARTING;
                oldHeat.RemoveAt(i);
                i--;  // Decrement i to account for the removed element
            }
        }
    }

    private void PropagateBarriers(byte[,,] grid, List<Vector3Int> newBarriers) {
        List<Vector3Int> tempBarrier = new List<Vector3Int>(newBarriers);
        newBarriers.Clear();

        foreach (var item in tempBarrier) {
            var neighbors = GetNeighbors(grid, item.x, item.y, item.z);
            foreach (var subItem in neighbors) {
                var val = grid[subItem.x, subItem.y, subItem.z];

                if (val == BARRIER) continue;

                if (val == STARTING) {
                    grid[subItem.x, subItem.y, subItem.z] = BARRIER;
                    newBarriers.Add(new Vector3Int(subItem.x, subItem.y, subItem.z));
                }
            }
        }
    }

    private List<Vector3Int> GetNeighbors(byte[,,] grid, int x, int y, int z) {
        List<Vector3Int> neighbors = new List<Vector3Int>();

        for (int i = x - 1; i <= x + 1; i++) {
            for (int j = y - 1; j <= y + 1; j++) {
                for (int k = z - 1; k <= z + 1; k++) {
                    // Skip the current cell
                    if (i == x && j == y && k == z) {
                        continue;
                    }

                    // Check if the cell is inside the grid
                    if (i >= 0 && i < grid.GetLength(0) && j >= 0 && j < grid.GetLength(1) && k >= 0 && k < grid.GetLength(2)) {
                        neighbors.Add(new Vector3Int(i, j, k));
                    }
                }
            }
        }

        return neighbors;
    }

    private void OnDrawGizmos() {
        if (debugGrid == null || debugGrid.GetLength(0) == 0) return;

        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                for (int z = 0; z < zSize; z++) {
                    Vector3 worldPostion = GridToWorld(x, y, z);
                    Gizmos.color = ByteToColor(debugGrid[x, y, z]);
                    Gizmos.DrawWireSphere(worldPostion, 0.25f);
                }
            }
        }
    }

    public Vector3 GridToWorld(int x, int y, int z) {
        return start + (new Vector3(x, y, z) * map3D.size);
    }

    private Color ByteToColor(byte i) {
        switch (i) {
            case STARTING + 9:
                return new Color(1f, 0.20f, 0.2f); // Red

            case STARTING + 8:
                return new Color(0.9f, 0.1f, 0.1f); // Red

            case STARTING + 7:
                return new Color(0.8f, 0f, 0f); // Red

            case STARTING + 6:
                return new Color(0.70f, 0f, 0f); // Red

            case STARTING + 5:
                return new Color(0.60f, 0f, 0f); // Red

            case STARTING + 4:
                return new Color(0.50f, 0f, 0.1f); // Red

            case STARTING + 3:
                return new Color(0.40f, 0f, 0.2f); // Red

            case STARTING + 2:
                return new Color(0.30f, 0f, 0.3f); // Red

            case STARTING + 1:
                return new Color(0.20f, 0f, 0.4f); // Red

            case STARTING:
                return Color.black;

            case BARRIER:
                return Color.clear;

            default:
                return Color.red;
        }
    }
}
