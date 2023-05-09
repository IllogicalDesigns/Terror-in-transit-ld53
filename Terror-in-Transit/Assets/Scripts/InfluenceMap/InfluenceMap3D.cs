using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class InfluenceMap3D : MonoBehaviour {

    [Header("Grid settings")]
    [SerializeField] private Vector3 offset;

    [SerializeField] public Collider boundsHint;

    [SerializeField] public float size = 2f;
    private byte[,,] grid;
    public Vector3 start;

    private const byte ARBITRARYHOTVALUE = 10;
    private const byte BARRIER = 0;
    private const byte STARTING = 1;

    private int xSize = -1;
    private int ySize = -1;
    private int zSize = -1;

    [Header("Ground check")]
    [SerializeField] private float groundCheckLength = 1f;

    [SerializeField] private LayerMask layerMask;
    [SerializeField] private Vector3 groundCheckOffset = Vector3.up;
    [SerializeField] private bool GroundCheck = true;

    [Header("Nav check")]
    [SerializeField] private float navMeshSampleAllowance = 1.9f;

    [SerializeField] private bool navCheck = true;
    [SerializeField] private bool dropNav = true;
    [SerializeField] private Vector3 dropNavOffset = Vector3.zero;
    [SerializeField] private float dropNavLength = 1f;

    // Start is called before the first frame update
    private void Start() {
        //BuildGrid();
        //boundsHint.enabled = false;
    }

    public byte[,,] GetGrid() {
        if (grid == null) BuildGrid();

        byte[,,] copy = (byte[,,])grid.Clone();
        return copy;
    }

    public Vector3 GetStart() {
        return start;
    }

    public Vector3 GetSizeVector() {
        return new Vector3(xSize, ySize, zSize);
    }

    public void BuildGrid() {
        if (!HasBoundsHint()) return;
        InitializeGridSizeBasedOnBounds();
        InitializeGridStartBasedOnBounds();

        grid = new byte[xSize, ySize, zSize];

        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                for (int z = 0; z < zSize; z++) {
                    Vector3 location = GridToWorldPosition(x, y, z);

                    if (navCheck) {
                        location = DropNavPoints(location);
                        bool mesh = HasNavMesh(location);

                        if (!mesh) {
                            grid[x, y, z] = BARRIER;
                            continue;
                        }
                    }

                    if (GroundCheck) {
                        bool ground = HasGround(location);
                        if (!ground) {
                            grid[x, y, z] = BARRIER;
                            continue;
                        }
                    }

                    grid[x, y, z] = STARTING;
                }
            }
        }
    }

    private Vector3 DropNavPoints(Vector3 location) {
        if (dropNav) {
            RaycastHit rayHit;
            if (Physics.Raycast(location + dropNavOffset, Vector3.down, out rayHit, dropNavLength, layerMask)) {
                //Debug.DrawRay(location + dropNavOffset, Vector3.down * dropNavLength, Color.yellow, 10f);
                location.y = rayHit.point.y;
            }
        }

        return location;
    }

    public void ClearGrid() {
        grid = new byte[0, 0, 0];
    }

    private bool HasGround(Vector3 location) {
        RaycastHit rayHit;
        if (Physics.Raycast(location + groundCheckOffset, Vector3.down, out rayHit, groundCheckLength, layerMask)) {
            Debug.DrawRay(location + groundCheckOffset, Vector3.down * groundCheckLength, Color.cyan, 10f);
            return true;
        }

        return false;
    }

    private bool HasNavMesh(Vector3 location) {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(location, out hit, navMeshSampleAllowance, -1)) {
            //Debug.DrawRay(hit.position, Vector3.up, Color.cyan, 20f);
            return true;
        }
        else {
            return false;
        }
    }

    private bool HasBoundsHint() {
        if (boundsHint == null) {
            Debug.Log("InfluenceMap3D Collider boundsHint is null");
            return false;
        }
        else {
            return true;
        }
    }

    private void InitializeGridStartBasedOnBounds() {
        start = boundsHint.bounds.center - (boundsHint.bounds.size * 0.5f);
    }

    private void InitializeGridSizeBasedOnBounds() {
        xSize = Mathf.RoundToInt(boundsHint.bounds.size.x / size);
        ySize = Mathf.RoundToInt(boundsHint.bounds.size.y / size);
        zSize = Mathf.RoundToInt(boundsHint.bounds.size.z / size);
    }

    private void OnDrawGizmosSelected() {
        if (!HasBoundsHint()) return;
        InitializeGridSizeBasedOnBounds();
        InitializeGridStartBasedOnBounds();

        if (grid.GetLength(0) == 0) return;

        for (int x = 0; x < xSize; x++) {
            for (int y = 0; y < ySize; y++) {
                for (int z = 0; z < zSize; z++) {
                    Vector3 worldPostion = GridToWorldPosition(x, y, z);
                    Gizmos.color = ByteToColor(grid[x, y, z]);
                    Gizmos.DrawWireSphere(worldPostion, 0.25f);
                }
            }
        }
    }

    public Vector3 GridToWorldPosition(int x, int y, int z) {
        return start + offset + (new Vector3(x, y, z) * size);
    }

    public Vector3 GridToWorld(int x, int y, int z) {
        return start + (new Vector3(x, y, z) * size);
    }

    public Vector3 GridToWorld(Vector3 vector) {
        return start + (vector * size);
    }

    public Vector3 GridToWorldPosition(Vector3Int vec) {
        return start + offset + (new Vector3(vec.x, vec.y, vec.z) * size);
    }

    public Vector3Int WorldToGrid(Vector3 worldPos) {
        Vector3 localPos = worldPos - start;
        var x = Mathf.RoundToInt(localPos.x / size);
        var y = Mathf.RoundToInt(localPos.y / size);
        var z = Mathf.RoundToInt(localPos.z / size);
        return new Vector3Int(x, y, z);
    }

    private Color ByteToColor(byte i) {
        switch (i) {
            case STARTING + 9:
                return new Color(1f, 0f, 0f); // Red

            case STARTING + 8:
                return new Color(0.9f, 0f, 0f); // Red

            case STARTING + 7:
                return new Color(0.8f, 0f, 0f); // Red

            case STARTING + 6:
                return new Color(0.70f, 0f, 0f); // Red

            case STARTING + 5:
                return new Color(0.60f, 0f, 0f); // Red

            case STARTING + 4:
                return new Color(0.50f, 0f, 0f); // Red

            case STARTING + 3:
                return new Color(0.40f, 0f, 0f); // Red

            case STARTING + 2:
                return new Color(0.30f, 0f, 0f); // Red

            case STARTING + 1:
                return new Color(0.20f, 0f, 0f); // Red

            case STARTING:
                return Color.black;

            case BARRIER:
                return Color.clear;

            default:
                return Color.red;
        }
    }
}
