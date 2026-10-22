using UnityEngine;
using System.Collections.Generic;

public class MazeGenerator : MonoBehaviour
{
    [Header("Grid")]
    public int width = 13;
    public int height = 13;
    public float cellSize = 2.5f;

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject mushroomPrefab;
    public Material floorMaterial;
    public bool hideFloorRenderer = false;
    public float wallThickness = 0.3f;
    public float wallHeightScale = 1f;

    [Header("Mushrooms")]
    public int mushroomCount = 10;
    public int currentColorIndex;

    [Header("Entrance")]
    public int entranceX = 0;
    public int entranceY = 0;

    private bool[,] visited;
    private bool[,] wallRight; // wall between (x,y) and (x+1,y)
    private bool[,] wallDown;  // wall between (x,y) and (x,y+1)

    private Vector3 naturalWallSize;
    private Quaternion wallBaseRotation;
    private Vector3 wallBaseScale;
    private Quaternion mushroomBaseRotation;
    private Transform wallParent;
    private Transform mushroomParent;

    public Vector3 EntranceWorldPos => CellToWorld(entranceX, entranceY);

    void ComputeNaturalWallSize()
    {
        wallBaseRotation = wallPrefab != null ? wallPrefab.transform.rotation : Quaternion.identity;
        wallBaseScale = wallPrefab != null ? wallPrefab.transform.localScale : Vector3.one;
        mushroomBaseRotation = mushroomPrefab != null ? mushroomPrefab.transform.rotation : Quaternion.identity;

        if (wallPrefab != null)
        {
            var rend = wallPrefab.GetComponentInChildren<Renderer>();
            Vector3 size = rend != null ? rend.bounds.size : Vector3.zero;
            if (size.x > 0.01f && size.z > 0.01f)
            {
                naturalWallSize = size;
                return;
            }
        }
        naturalWallSize = new Vector3(2f, 1.5f, 0.5f);
    }

    public void Generate()
    {
        ComputeNaturalWallSize();
        ClearExisting();

        visited = new bool[width, height];
        wallRight = new bool[width, height];
        wallDown = new bool[width, height];
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                wallRight[x, y] = true;
                wallDown[x, y] = true;
            }

        CarveMaze(entranceX, entranceY);

        wallParent = new GameObject("Walls").transform;
        wallParent.SetParent(transform, false);
        mushroomParent = new GameObject("Mushrooms").transform;
        mushroomParent.SetParent(transform, false);

        BuildWalls();
        BuildFloor();
        SpawnMushrooms();
    }

    void ClearExisting()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }
    }

    void CarveMaze(int startX, int startY)
    {
        var stack = new Stack<Vector2Int>();
        visited[startX, startY] = true;
        stack.Push(new Vector2Int(startX, startY));

        var dirs = new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(-1, 0), new Vector2Int(0, 1), new Vector2Int(0, -1) };

        while (stack.Count > 0)
        {
            var cur = stack.Peek();
            var order = new List<int> { 0, 1, 2, 3 };
            Shuffle(order);

            bool carved = false;
            foreach (var idx in order)
            {
                var d = dirs[idx];
                int nx = cur.x + d.x;
                int ny = cur.y + d.y;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;
                if (visited[nx, ny]) continue;

                if (d.x == 1) wallRight[cur.x, cur.y] = false;
                else if (d.x == -1) wallRight[nx, ny] = false;
                else if (d.y == 1) wallDown[cur.x, cur.y] = false;
                else if (d.y == -1) wallDown[nx, ny] = false;

                visited[nx, ny] = true;
                stack.Push(new Vector2Int(nx, ny));
                carved = true;
                break;
            }

            if (!carved) stack.Pop();
        }
    }

    void Shuffle(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    Vector3 CellToWorld(int x, int y)
    {
        return transform.position + new Vector3(x * cellSize, 0f, y * cellSize);
    }

    void PlaceWallSegment(Vector3 worldPos, bool vertical)
    {
        if (wallPrefab == null) return;

        Quaternion rot = vertical ? Quaternion.Euler(0f, 90f, 0f) * wallBaseRotation : wallBaseRotation;
        var go = Instantiate(wallPrefab, worldPos, rot, wallParent);
        go.name = "WallSegment";

        float scaleXRatio = Mathf.Clamp(cellSize / Mathf.Max(0.0001f, naturalWallSize.x), 0.05f, 50f);
        float scaleZRatio = Mathf.Clamp(wallThickness / Mathf.Max(0.0001f, naturalWallSize.z), 0.05f, 50f);
        // wallBaseRotation (-90 on local X) swaps local Y<->world Z and local Z<->world Y,
        // so the thickness ratio goes on local Y and the height scale goes on local Z.
        go.transform.localScale = new Vector3(wallBaseScale.x * scaleXRatio, wallBaseScale.y * scaleZRatio, wallBaseScale.z * wallHeightScale);

        var meshFilter = go.GetComponentInChildren<MeshFilter>();
        if (meshFilter != null && meshFilter.GetComponent<Collider>() == null)
        {
            var box = meshFilter.gameObject.AddComponent<BoxCollider>();
            box.center = meshFilter.sharedMesh.bounds.center;
            box.size = meshFilter.sharedMesh.bounds.size;
        }
    }

    void BuildWalls()
    {
        // internal walls
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = CellToWorld(x, y);

                if (x < width - 1 && wallRight[x, y])
                {
                    Vector3 pos = center + new Vector3(cellSize * 0.5f, 0f, 0f);
                    PlaceWallSegment(pos, true);
                }
                if (y < height - 1 && wallDown[x, y])
                {
                    Vector3 pos = center + new Vector3(0f, 0f, cellSize * 0.5f);
                    PlaceWallSegment(pos, false);
                }
            }
        }

        // boundary walls
        for (int x = 0; x < width; x++)
        {
            Vector3 top = CellToWorld(x, 0) + new Vector3(0f, 0f, -cellSize * 0.5f);
            Vector3 bottom = CellToWorld(x, height - 1) + new Vector3(0f, 0f, cellSize * 0.5f);
            bool skipTop = (x == entranceX && entranceY == 0);
            bool skipBottom = (x == entranceX && entranceY == height - 1);
            if (!skipTop) PlaceWallSegment(top, false);
            if (!skipBottom) PlaceWallSegment(bottom, false);
        }
        for (int y = 0; y < height; y++)
        {
            Vector3 left = CellToWorld(0, y) + new Vector3(-cellSize * 0.5f, 0f, 0f);
            Vector3 right = CellToWorld(width - 1, y) + new Vector3(cellSize * 0.5f, 0f, 0f);
            bool skipLeft = (y == entranceY && entranceX == 0);
            bool skipRight = (y == entranceY && entranceX == width - 1);
            if (!skipLeft) PlaceWallSegment(left, true);
            if (!skipRight) PlaceWallSegment(right, true);
        }
    }

    void BuildFloor()
    {
        var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "MazeFloor";
        floor.transform.SetParent(transform, false);
        float sizeX = width * cellSize;
        float sizeZ = height * cellSize;
        floor.transform.position = transform.position + new Vector3((width - 1) * cellSize * 0.5f, -0.05f, (height - 1) * cellSize * 0.5f);
        floor.transform.localScale = new Vector3(sizeX / 10f, 1f, sizeZ / 10f);

        var floorRend = floor.GetComponent<Renderer>();
        if (hideFloorRenderer)
        {
            // a dedicated MazeTerrain object renders the visible ground instead; keep this plane for collision only
            floorRend.enabled = false;
        }
        else if (floorMaterial != null)
        {
            floorRend.sharedMaterial = floorMaterial;
            // keep texture density consistent regardless of floor size (coarse tiling to avoid a busy/realistic look)
            floorRend.sharedMaterial.mainTextureScale = new Vector2(sizeX * 0.075f, sizeZ * 0.075f);
        }
    }

    void SpawnMushrooms()
    {
        if (mushroomPrefab == null) return;

        var candidates = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (x == entranceX && y == entranceY) continue;
                candidates.Add(new Vector2Int(x, y));
            }

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int count = Mathf.Min(mushroomCount, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = CellToWorld(candidates[i].x, candidates[i].y);
            Quaternion rot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) * mushroomBaseRotation;
            var go = Instantiate(mushroomPrefab, pos, rot, mushroomParent);
            go.name = "Mushroom_" + i;

            var col = go.GetComponent<Collider>();
            if (col == null) col = go.AddComponent<SphereCollider>();
            col.isTrigger = true;

            var mushroom = go.GetComponent<Mushroom>();
            if (mushroom == null) mushroom = go.AddComponent<Mushroom>();
            mushroom.colorIndex = currentColorIndex;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetMazeMushroomTotal(count, currentColorIndex);
        }
    }
}
