using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System.Linq;

public class InfluenceMap : MonoBehaviour {
    [SerializeField] public Collider boundsHint;
    [SerializeField] public float size = 2f;

    private int[,] grid;
    public int[,] searchMap;
    private int[,] debugCopyOfGrid;

    //public Vector3 choosenChasePoint;
    public Vector3 choosenSearchPoint;

    public List<Vector2Int> searchNewHeat = new List<Vector2Int>();
    public List<Vector2Int> searchOldHeat = new List<Vector2Int>();

    //private const float DotFaceForwardForFill = 0.3f;
    [SerializeField] private Transform player;

    //private Vector3 fillStartPost = Vector3.zero;
    public Vector3 start;

    //private List<Vector2> filledGridCells = new List<Vector2>();

    private const int ARBITRARYHOTVALUE = 9;
    private const int BARRIER = -1;
    private const int STARTINGVAL = 0;
    [SerializeField] private float navMeshSampleAllowance = 0.7f;

    [SerializeField] private float Yoffset = 0;

    [SerializeField] private bool isDebug = false;

    [SerializeField] private LayerMask layerMask;

    // Start is called before the first frame update
    private void Start() {
        GenerateGrid();
    }

    public void GenerateGrid() {
        if (isDebug) Debug.Log("InfluenceMap:GenerateGrid() Starting Generation");
        start = boundsHint.bounds.center - (boundsHint.bounds.size * 0.5f);

        GenerateGrid();

        void GenerateGrid() {
            Vector3 start = boundsHint.bounds.center - (boundsHint.bounds.size * 0.5f);

            int xSize = Mathf.RoundToInt(boundsHint.bounds.size.x / size);
            int zSize = Mathf.RoundToInt(boundsHint.bounds.size.z / size);

            int[,] tempArray = new int[xSize, zSize];

            for (int x = 0; x < xSize; x++) {
                for (int z = 0; z < zSize; z++) {
                    Vector3 location = start + (new Vector3(x, Yoffset, z) * size);

                    location.y = boundsHint.transform.position.y + Yoffset;

                    //if (isDebug) Debug.DrawRay(location, Vector3.down * 10f, Color.magenta, 15f);

                    RaycastHit rayHit;
                    if (Physics.Raycast(location, Vector3.down, out rayHit, 5f, layerMask)) {
                        location.y = rayHit.point.y;
                    }

                    Color clr = Color.red;
                    NavMeshHit hit;
                    if (NavMesh.SamplePosition(location, out hit, navMeshSampleAllowance, -1)) {
                        if (isDebug) Debug.DrawRay(hit.position, transform.up * size, Color.red, 15f);
                        // Add to the array
                        tempArray[x, z] = STARTINGVAL;
                    }
                    else {
                        // Dont add to the array
                        tempArray[x, z] = BARRIER;
                    }
                }
            }

            grid = tempArray;
            debugCopyOfGrid = grid;
            if (isDebug) Debug.Log("InfluenceMap:GenerateGrid() Finished Generation");
        }
    }

    public int[,] GetGridCopy() {
        int[,] copy = (int[,])grid.Clone();
        return copy;
    }

    public Vector2Int WorldToGrid(Vector3 worldPos) {
        Vector3 localPos = worldPos - start;
        var colIndex = Mathf.RoundToInt(localPos.x / size);
        var rowIndex = Mathf.RoundToInt(localPos.z / size);
        return new Vector2Int(colIndex, rowIndex);
    }

    public Vector3 GridToWorld(int x, int z, int y = 0) {
        return start + (new Vector3(x, Yoffset, z) * size);
    }

    public Vector3 GridToWorld(Vector2Int vec) {
        return GridToWorld(vec.x, vec.y);
    }

    public IEnumerator HeatWaveChasePropagation(Transform target, Transform searcher, GSearchAfterChase.OnCoroutineFinished onFinished, int PROPSTEPS = 5, float PROPTIME = 0.05f) {
        int[,] copyOfGrid = GetGridCopy();
        debugCopyOfGrid = copyOfGrid;

        Debug.DrawRay(target.position, searcher.forward, Color.green, 10f);
        var initPosition = target.position;
        var initDirection = target.position - searcher.position;

        var nonRoundedLocation = WorldToGrid(initPosition);
        Vector2Int initGridLocation = new Vector2Int(Mathf.RoundToInt(nonRoundedLocation.x), Mathf.RoundToInt(nonRoundedLocation.y));

        List<Vector2Int> newBarriers = new List<Vector2Int>();
        List<Vector2Int> newHeat = new List<Vector2Int>();
        List<Vector2Int> oldHeat = new List<Vector2Int>();

        HeatUpInitLocation(copyOfGrid, ref initGridLocation, newHeat, oldHeat);
        GenerateInitBarrier();

        for (int i = 0; i < PROPSTEPS; i++) {
            //TODO calculate centriod as heat is built up
            PropagateHeat(copyOfGrid, newHeat, oldHeat);
            CoolOldHeat(copyOfGrid, oldHeat);
            PropagateBarriers(copyOfGrid, newBarriers);
            yield return new WaitForSeconds(PROPTIME);
        }

        onFinished(ChooseChasePointCentroid(oldHeat, copyOfGrid));
        choosenSearchPoint = ChooseChasePointBasedOnCluster(newHeat); //This is used for searching chase as a starting point

        void GenerateInitBarrier() {
            ForceALineBehindThePlayer(newBarriers);

            List<Vector2Int> neighbors = GetNeighbors(copyOfGrid, initGridLocation.x, initGridLocation.y);
            for (int i = 0; i < neighbors.Count; i++) {
                SetOppositeToBarrier(neighbors[i].x, neighbors[i].y);
            }

            void ForceALineBehindThePlayer(List<Vector2Int> newBarriers) {
                Vector3 behindPlayer = initPosition + -initDirection * size;
                var behindPlayerGrid = WorldToGrid(behindPlayer);
                if (behindPlayerGrid.x >= 0 && behindPlayerGrid.x < copyOfGrid.GetLength(0) && behindPlayerGrid.y >= 0 && behindPlayerGrid.y < copyOfGrid.GetLength(1)) {
                    copyOfGrid[behindPlayerGrid.x, behindPlayerGrid.y] = BARRIER;
                    newBarriers.Add(new Vector2Int(behindPlayerGrid.x, behindPlayerGrid.y));
                }

                behindPlayer = initPosition + -initDirection * size + -target.right * size;
                behindPlayerGrid = WorldToGrid(behindPlayer);
                if (behindPlayerGrid.x >= 0 && behindPlayerGrid.x < copyOfGrid.GetLength(0) && behindPlayerGrid.y >= 0 && behindPlayerGrid.y < copyOfGrid.GetLength(1)) {
                    copyOfGrid[behindPlayerGrid.x, behindPlayerGrid.y] = BARRIER;
                    newBarriers.Add(new Vector2Int(behindPlayerGrid.x, behindPlayerGrid.y));
                }

                behindPlayer = initPosition + -initDirection * size + target.right * size;
                behindPlayerGrid = WorldToGrid(behindPlayer);
                if (behindPlayerGrid.x >= 0 && behindPlayerGrid.x < copyOfGrid.GetLength(0) && behindPlayerGrid.y >= 0 && behindPlayerGrid.y < copyOfGrid.GetLength(1)) {
                    copyOfGrid[behindPlayerGrid.x, behindPlayerGrid.y] = BARRIER;
                    newBarriers.Add(new Vector2Int(behindPlayerGrid.x, behindPlayerGrid.y));
                }
            }
        }

        void SetOppositeToBarrier(int row, int col) {
            const float DotFaceForwardForFill = 0.5f;

            // Calculate the world position of the current cell
            Vector3 cellPos = start + new Vector3(row * size + size / 2f, 0f, col * size + size / 2f);

            var playerGrid = WorldToGrid(initPosition);
            var playerCellWorldPos = GridToWorld(playerGrid.x, playerGrid.y);
            // Calculate the vector from the current cell to the given world position
            Vector3 toCell = cellPos - playerCellWorldPos;

            // Calculate the dot product between the vector and the opposite direction
            float dotProduct = Vector3.Dot(toCell.normalized, -initDirection.normalized);

            // Check if the dot product is above a certain threshold (e.g., 0.8f)
            if (dotProduct > DotFaceForwardForFill) {
                // Set the cell to -1
                copyOfGrid[row, col] = BARRIER;
                newBarriers.Add(new Vector2Int(row, col));
            }
        }
    }

    public IEnumerator SearchPropagationStep(int[,] map, List<Vector2Int> newHeat, List<Vector2Int> oldHeat, Vector3 targetPosition, Vector3 searchingPoint /*, GSearch.OnCoroutineFinished onFinished*/, bool reset = false, int steps = 1) {
        if (reset) {
            map = GetGridCopy();
            debugCopyOfGrid = map;

            var initPosition = targetPosition;

            var nonRoundedLocation = WorldToGrid(initPosition);
            Vector2Int initGridLocation = new Vector2Int(Mathf.RoundToInt(nonRoundedLocation.x), Mathf.RoundToInt(nonRoundedLocation.y));

            List<Vector2Int> newBarriers = new List<Vector2Int>();
            newHeat = new List<Vector2Int>();
            oldHeat = new List<Vector2Int>();

            //GenerateInitBarrier(initGridLocation, newBarriers);
            HeatUpInitLocation(map, ref initGridLocation, newHeat, oldHeat);
        }

        for (int i = 0; i < steps; i++) {
            //TODO calculate centriod as heat is built up
            PropagateHeat(map, newHeat, oldHeat);
            CoolOldHeatSearcher(map, oldHeat, searchingPoint);
            //CoolOldHeat(searchMap, searchOldHeat);
            yield return new WaitForEndOfFrame();
        }

        //TODO move this to another function
        //TODO call this async from search propagation
        //TODO convert search prop to continous
        //if (newHeat.Count > 1)
        //    onFinished(CreateClustersFromHeat(newHeat));
    }

    //public IEnumerator HeatWaveSearchPropagationStep(Vector3 targetPosition, Vector3 searcherPos, GSearch gSearch, float PROPTIME = 1f, bool firstTime = false) {
    //    targetPosition = choosenSearchPoint;
    //    if (searchMap == null || searchMap.Length == 0) searchMap = GetGridCopy();

    //    if (firstTime) {
    //        debugCopyOfGrid = searchMap;

    //        var initPosition = targetPosition;

    //        var nonRoundedLocation = WorldToGrid(initPosition);
    //        Vector2Int initGridLocation = new Vector2Int(Mathf.RoundToInt(nonRoundedLocation.x), Mathf.RoundToInt(nonRoundedLocation.y));

    //        searchNewHeat = new List<Vector2Int>();
    //        searchOldHeat = new List<Vector2Int>();

    //        HeatUpInitLocation(searchMap, ref initGridLocation, searchNewHeat, searchOldHeat);
    //    }

    //    PropagateHeat(searchMap, searchNewHeat, searchOldHeat);
    //    if (searchOldHeat.Count > 0) CoolOldHeatSearcher(searchMap, searchOldHeat, searcherPos);

    //    if (searchOldHeat.Count > 1)
    //        gSearch.searchPosition = ChooseChasePointCentroid(searchOldHeat, searchMap);

    //    yield return new WaitForEndOfFrame();
    //}

    #region Region1

    private static void HeatUpInitLocation(int[,] copyOfGrid, ref Vector2Int initGridLocation, List<Vector2Int> newHeat, List<Vector2Int> oldHeat) {
        copyOfGrid[initGridLocation.x, initGridLocation.y] = ARBITRARYHOTVALUE;
        newHeat.Add(new Vector2Int(initGridLocation.x, initGridLocation.y));
        oldHeat.Add(new Vector2Int(initGridLocation.x, initGridLocation.y));
    }

    private void PropagateHeat(int[,] copyOfGrid, List<Vector2Int> newHeat, List<Vector2Int> oldHeat) {
        List<Vector2Int> tempHeat = new List<Vector2Int>(newHeat);
        newHeat.Clear();

        foreach (var item in tempHeat) {
            var neighbors = GetNeighbors(copyOfGrid, item.x, item.y);
            foreach (var subItem in neighbors) {
                var val = copyOfGrid[subItem.x, subItem.y];
                if (val == -1) continue;
                if (val != 0) continue;

                copyOfGrid[subItem.x, subItem.y] = ARBITRARYHOTVALUE;
                newHeat.Add(new Vector2Int(subItem.x, subItem.y));
                oldHeat.Add(new Vector2Int(subItem.x, subItem.y));
            }
        }
    }

    private List<Vector2Int> GetNeighbors(int[,] grid, int x, int y) {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        for (int i = x - 1; i <= x + 1; i++) {
            for (int j = y - 1; j <= y + 1; j++) {
                // Skip the current cell
                if (i == x && j == y) {
                    continue;
                }

                // Check if the cell is inside the grid
                if (i >= 0 && i < grid.GetLength(0) && j >= 0 && j < grid.GetLength(1)) {
                    neighbors.Add(new Vector2Int(i, j));
                }
            }
        }

        return neighbors;
    }

    private void PropagateBarriers(int[,] copyOfGrid, List<Vector2Int> newBarriers) {
        List<Vector2Int> tempBarrier = new List<Vector2Int>(newBarriers);
        newBarriers.Clear();

        foreach (var item in tempBarrier) {
            var neighbors = GetNeighbors(copyOfGrid, item.x, item.y);
            foreach (var subItem in neighbors) {
                var val = copyOfGrid[subItem.x, subItem.y];
                if (val == -1) continue;
                if (val == 0) {
                    copyOfGrid[subItem.x, subItem.y] = BARRIER;
                    newBarriers.Add(new Vector2Int(subItem.x, subItem.y));
                }
            }
        }
    }

    private void CoolOldHeat(int[,] copyOfGrid, List<Vector2Int> oldHeat) {
        for (int i = 0; i < oldHeat.Count; i++) {
            var x = oldHeat[i].x;
            var y = oldHeat[i].y;
            if (copyOfGrid[x, y] > 0) {
                copyOfGrid[x, y] -= 1;
            }
            else {
                // If the value is 0, remove the point from oldHeat
                oldHeat.RemoveAt(i);
                i--;  // Decrement i to account for the removed element
            }
        }
    }

    private void CoolOldHeatSearcher(int[,] copyOfGrid, List<Vector2Int> oldHeat, Vector3 searcher, float distance = 4f) {
        for (int i = 0; i < oldHeat.Count; i++) {
            var x = oldHeat[i].x;
            var y = oldHeat[i].y;

            if (copyOfGrid[x, y] > 0) {
                var worldPos = GridToWorld(x, y);

                if (copyOfGrid[x, y] > 2) copyOfGrid[x, y] -= 1;

                if (copyOfGrid[x, y] == 0 || Vector3.Distance(searcher, worldPos) < distance || !Physics.Linecast(searcher, worldPos)) {
                    copyOfGrid[x, y] = -1;
                    oldHeat.RemoveAt(i);
                    i--;  // Decrement i to account for the removed element
                }
            }
            else {
                // If the value is 0, remove the point from oldHeat
                oldHeat.RemoveAt(i);
                i--;  // Decrement i to account for the removed element
            }
        }
    }

    private Vector3 ChooseChasePointCentroid(List<Vector2Int> oldHeat, int[,] copyOfGrid) {
        // Initialize variables for the weighted sum and the total weight
        float weightedSumX = 0;
        float weightedSumY = 0;
        float totalWeight = 0;

        // Iterate through each element in the list
        foreach (Vector2Int cell in oldHeat) {
            // Get the weight of the current cell (you will need to define your own weighting function)
            int weight = copyOfGrid[cell.x, cell.y];

            // Update the weighted sum and the total weight
            weightedSumX += cell.x * weight;
            weightedSumY += cell.y * weight;
            totalWeight += weight;
        }

        // Calculate the centroid by dividing the weighted sum by the total weight
        Vector2Int centroid = new Vector2Int(Mathf.RoundToInt(weightedSumX / totalWeight), Mathf.RoundToInt(weightedSumY / totalWeight));

        if (centroid.x < 0 || centroid.x >= copyOfGrid.GetLength(0) || centroid.y < 0 || centroid.y >= copyOfGrid.GetLength(1) || copyOfGrid[centroid.x, centroid.y] == -1 || copyOfGrid[centroid.x, centroid.y] == 0) {
            var sortedHeat = oldHeat.OrderBy(x => ManhattanDistance(x, centroid));

            if (sortedHeat.Count() > 0) {
                var closestHeatedCell = sortedHeat.First();
                return GridToWorld(closestHeatedCell.x, closestHeatedCell.y);
            }
            else
                return GridToWorld(centroid.x, centroid.y);
        }
        else {
            return GridToWorld(centroid.x, centroid.y);
        }
    }

    private Vector3 ChooseChasePointBasedOnCluster(List<Vector2Int> newHeat) {
        List<List<Vector2Int>> clusters = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Vector2Int p in newHeat) {
            if (visited.Contains(p))
                continue;

            List<Vector2Int> cluster = new List<Vector2Int>();
            cluster.Add(p);
            visited.Add(p);

            int i = 0;
            while (i < cluster.Count) {
                Vector2Int current = cluster[i];
                foreach (Vector2Int q in newHeat) {
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
            return GridToWorld(bestPos.x, bestPos.y);
        }
        else return Vector3.zero;
    }

    private List<List<Vector2Int>> CreateClustersFromHeat(List<Vector2Int> newHeat) {
        List<List<Vector2Int>> clusters = new List<List<Vector2Int>>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        foreach (Vector2Int p in newHeat) {
            if (visited.Contains(p))
                continue;

            List<Vector2Int> cluster = new List<Vector2Int>();
            cluster.Add(p);
            visited.Add(p);

            int i = 0;
            while (i < cluster.Count) {
                Vector2Int current = cluster[i];
                foreach (Vector2Int q in newHeat) {
                    if (!visited.Contains(q) && ManhattanDistance(current, q) == 1) {
                        cluster.Add(q);
                        visited.Add(q);
                    }
                }
                i++;
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    private int ManhattanDistance(Vector2Int p1, Vector2Int p2) {
        return Mathf.Abs(p1.x - p2.x) + Mathf.Abs(p1.y - p2.y);
    }

    #endregion Region1

    private void OnDrawGizmos() {
        if (debugCopyOfGrid == null) return;
        if (!isDebug) return;

        int xSize = debugCopyOfGrid.GetLength(0); // returns 3
        int zSize = debugCopyOfGrid.GetLength(1); // returns 4

        Vector3 start = boundsHint.bounds.center - (boundsHint.bounds.size * 0.5f);

        if (player != null) {
            var pos = WorldToGrid(player.position);
            Gizmos.DrawWireSphere(GridToWorld(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)), 1f);
        }

        for (int x = 0; x < xSize; x++) {
            for (int z = 0; z < zSize; z++) {
                if (debugCopyOfGrid[x, z] != -1) {
                    Vector3 location = start + (new Vector3(x, boundsHint.transform.position.y + Yoffset, z) * size);
                    Gizmos.color = IntToColor(debugCopyOfGrid[x, z]);
                    Gizmos.DrawWireSphere(location, 0.4f);
                }
                else {
                    Vector3 location = start + (new Vector3(x, boundsHint.transform.position.y + Yoffset, z) * size);
                    Gizmos.color = IntToColor(debugCopyOfGrid[x, z]);
                    Gizmos.DrawWireSphere(location, 0.2f);
                }
            }
        }

        Color IntToColor(int i) {
            switch (i) {
                case 9:
                    return new Color(0.90f, 0.33f, 0.32f); // Red

                case 8:
                    return new Color(0.98f, 0.51f, 0.28f); // Orange

                case 7:
                    return new Color(0.99f, 0.83f, 0.23f); // Yellow

                case 6:
                    return new Color(0.51f, 0.79f, 0.24f); // Lime Green

                case 5:
                    return new Color(0.20f, 0.69f, 0.60f); // Teal

                case 4:
                    return new Color(0.08f, 0.45f, 0.51f); // Deep Blue

                case 3:
                    return new Color(0.05f, 0.28f, 0.68f); // Blue

                case 2:
                    return new Color(0.52f, 0.33f, 0.75f); // Purple

                case 1:
                    return new Color(0.81f, 0.47f, 0.69f); // Pink

                case 0:
                    return Color.black;

                case -1:
                    return Color.magenta;

                default:
                    return Color.red;
            }
        }
    }
}
