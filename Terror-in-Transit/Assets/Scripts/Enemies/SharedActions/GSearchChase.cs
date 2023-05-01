using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.AI;

public class GSearchArea : GAction {

    //[Header("Action specific settings")]
    private InfluenceMap influenceMap;

    private int[,] searchMap;

    private const int searchedValue = -1;

    [SerializeField] private int distanceDepth = 3;
    [SerializeField] private LayerMask layerMask = ~8;
    [SerializeField] private float rotateSpeed = 60;

    private float timer;
    private float cleanCloseTime = 1;

    [SerializeField] private float closeDist = 4f;
    [SerializeField] private float speed = 2.5f;

    [SerializeField] private float searchTime = 30f;

    public override void Awake() {
        base.Awake();

        //AddPreconditions("SeenPlayer");
        AddEffects("Search");
    }

    private void Start() {
        influenceMap = FindObjectOfType<InfluenceMap>();
        //searchMap = influenceMap.GetGridCopy();
    }

    private void CleanClosePoints() {
        Vector2Int gridPos = influenceMap.WorldToGrid(transform.position);
        //Vector3 position = transform.position;
        //Vector3 worldPos = transform.position;
        var neigh = GetNeighbors(searchMap, gridPos.x, gridPos.y, 1);
        foreach (var i in neigh) {
            if (searchMap[i.x, i.y] == searchedValue) continue;

            searchMap[i.x, i.y] = searchedValue;
        }
    }

    private bool isBehind(Vector3 vec) {
        Vector3 directionToTarget = vec - transform.position;
        Vector3 forward = transform.forward;
        float dotProduct = Vector3.Dot(directionToTarget.normalized, forward.normalized);
        return dotProduct < 0;
    }

    public override void Interruppted() {
    }

    public override IEnumerator Perform() {
        searchMap = influenceMap.GetGridCopy();

        gameObject.SendMessage("SetLightColor", LightColor.LightAwareness.aware, SendMessageOptions.DontRequireReceiver);

        int r = 0;
        do {
            Vector2Int gridPos = influenceMap.WorldToGrid(transform.position);
            Vector3 worldPos = transform.position;

            SetVisibleNeighboursToVisited(gridPos, worldPos);

            Vector3 position = transform.position;
            position = GetUnsearchedPosition(gridPos, worldPos, position);
            if (position == transform.position) break;  //If no new unsearched points exist, break out

            if (isBehind(position)) {
                //gameObject.BroadcastMessage("SetLookAround", true);
                yield return AgentHelpers.RotateToFaceTarget(transform, position, rotateSpeed);
                //gameObject.BroadcastMessage("SetLookAround", false);
            }

            Vector3 oldPos = transform.position;
            //yield return AgentHelpers.GoToPosition(gAgent.agent, position, closeDist);
            do {
                gAgent.agent.isStopped = false;
                gAgent.agent.SetDestination(position);
                yield return new WaitForSeconds(0.01f);
                //} while (Vector3.Distance(transform.position, new Vector3(position.x, transform.position.y, position.z)) > closeDist);
                if (transform.position == oldPos) break;
                else oldPos = transform.position;
            } while (AgentHelpers.DistanceOnNavMesh(transform, position) > closeDist);

            r++;
        } while (r < 50);

        CompletedAction();
    }

    private Vector3 GetUnsearchedPosition(Vector2Int gridPos, Vector3 worldPos, Vector3 position) {
        var neigh = GetNeighbors(searchMap, gridPos.x, gridPos.y, distanceDepth * 4);

        foreach (var i in neigh) {
            if (searchMap[i.x, i.y] == searchedValue) continue;
            if (!isInfrontOf(influenceMap.GridToWorld(i.x, i.y))) continue;
            if (Physics.Linecast(transform.position, worldPos, layerMask)) continue;

            position = influenceMap.GridToWorld(i.x, i.y);
            break;
        }

        return position;
    }

    private void SetVisibleNeighboursToVisited(Vector2Int gridPos, Vector3 worldPos) {
        var neigh = GetNeighbors(searchMap, gridPos.x, gridPos.y, distanceDepth);
        foreach (var i in neigh) {
            if (searchMap[i.x, i.y] == searchedValue) continue;
            if (!isInfrontOf(influenceMap.GridToWorld(i.x, i.y))) continue;
            if (Physics.Linecast(transform.position, worldPos, layerMask)) continue;

            searchMap[i.x, i.y] = searchedValue;
        }
    }

    private List<Vector2Int> GetNeighbors(int[,] grid, int x, int y, int extend = 1) {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        for (int i = x - extend; i <= x + extend; i++) {
            for (int j = y - extend; j <= y + extend; j++) {
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

    private bool isInfrontOf(Vector3 worldPos, float forwardThreshold = 0.5f) {
        // Calculate the direction from the reference transform to this transform
        Vector3 directionToReference = transform.position - worldPos;
        directionToReference.Normalize();

        // Calculate the dot product between the forward direction of the reference transform and the direction to this transform
        float dotProduct = Vector3.Dot(transform.forward, directionToReference);

        // Check if the dot product is greater than the forward threshold, indicating that the object is in front of the reference transform
        if (dotProduct > forwardThreshold) {
            return false;
        }
        else {
            return true;
        }
    }

    private Vector2Int GetRandomPointOnGrid() {
        int maxX = searchMap.GetLength(0);
        int maxY = searchMap.GetLength(1);

        int randomX = Random.Range(0, maxX);
        int randomY = Random.Range(0, maxY);

        return new Vector2Int(randomX, randomY);
    }

    public override bool PostPerform() {
        gAgent.AddGoal("Search", 3, true);
        return true;
    }

    public override bool PrePerform() {
        gAgent.agent.speed = speed;
        return true;
    }

    public override bool IsAchievable() {
        return base.IsAchievable();
    }

    // Update is called once per frame
    private void Update() {
        if (!running) return;

        searchTime -= Time.deltaTime;
        if (searchTime < 0) {
            CompletedAction();
            gAgent.Replan();
        }

        timer -= Time.deltaTime;
        if (timer < 0) {
            timer = cleanCloseTime;
            CleanClosePoints();
        }
    }

    private void OnDrawGizmos() {
        if (influenceMap == null || searchMap == null || searchMap.Length == 0) return;

        int xSize = searchMap.GetLength(0); // returns 3
        int zSize = searchMap.GetLength(1); // returns 4

        Vector3 start = influenceMap.boundsHint.bounds.center - (influenceMap.boundsHint.bounds.size * 0.5f);

        //if (player != null) {
        //    var pos = influenceMap.WorldToGrid(player.position);
        //    Gizmos.DrawWireSphere(GridToWorld(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y)), 1f);
        //}

        for (int x = 0; x < xSize; x++) {
            for (int z = 0; z < zSize; z++) {
                if (searchMap[x, z] != -1) {
                    Vector3 location = start + (new Vector3(x, 1, z) * influenceMap.size);
                    Gizmos.color = IntToColor(searchMap[x, z]);
                    Gizmos.DrawWireSphere(location, 0.4f);
                }
                else {
                    Vector3 location = start + (new Vector3(x, 1, z) * influenceMap.size);
                    Gizmos.color = IntToColor(searchMap[x, z]);
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
