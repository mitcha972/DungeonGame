using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DungeonGenerator : MonoBehaviour
{
    public class Cell
    {
        public bool visited;
        public bool[] status = new bool[4];
        public bool startCell;
        public bool endCell;
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;
        public bool obligatory;
        public enum RoomType { Enemy, Chest }
        public RoomType roomType;
        public int numb;
        public int ProbabilityOfSpawning(int x, int y)
        {
            return x >= minPosition.x && x <= maxPosition.x && y >= minPosition.y && y <= maxPosition.y ? (obligatory ? 2 : 1) : 0;
        }
    }

    public Vector2Int size;
    public int startPos;
    public Rule[] rooms;
    public Vector2 offset;
    public GameObject startRoom;
    public GameObject endRoom;
    public GameObject player;
    [SerializeField] private Unity.AI.Navigation.NavMeshSurface navigationSurface;
    [SerializeField] private float spawnSearchRadius = 4f;
    public bool IsGenerating { get; private set; }
    public bool IsReady { get; private set; }
    public string LastFailure { get; private set; }
    public Vector3 PlayerSpawnPosition { get; private set; }
    public IReadOnlyList<Cell> Cells => board;
    public event System.Action Generated;
    private List<Cell> board = new List<Cell>();
    private GameObject _generatedRoot;
    private NavMeshData ownedNavigationData;
    private IEnumerator activeGeneration;
    private bool advancingFloor;
    private int floorBeforeGeneration;

    private void Start() => ReStart();

    public void ReStart()
    {
        if (isActiveAndEnabled && !IsGenerating) StartCoroutine(TT());
    }

    public void AdvanceFloor()
    {
        if (isActiveAndEnabled && !IsGenerating && IsReady)
            StartCoroutine(RunGeneration(true));
    }

    public IEnumerator TT() => RunGeneration(false);

    private void OnDisable()
    {
        StopAllCoroutines();
        // Explicitly dispose the iterator; coroutine cancellation alone need not run its finally.
        if (activeGeneration == null) return;
        try { (activeGeneration as System.IDisposable)?.Dispose(); }
        finally
        {
            activeGeneration = null;
            if (advancingFloor) GameController.floor = floorBeforeGeneration;
            advancingFloor = false;
            IsGenerating = false;
            if (LastFailure == null) LastFailure = "Dungeon generation was interrupted.";
        }
    }

    private void OnDestroy()
    {
        if (ownedNavigationData == null) return;
        if (navigationSurface != null && navigationSurface.navMeshData == ownedNavigationData)
            navigationSurface.RemoveData();
        Destroy(ownedNavigationData);
        ownedNavigationData = null;
    }

    private IEnumerator RunGeneration(bool advanceFloor)
    {
        if (IsGenerating) yield break;
        bool previousReady = IsReady;
        int previousFloor = GameController.floor;
        IsGenerating = true;
        IsReady = false;
        LastFailure = null;
        advancingFloor = advanceFloor;
        floorBeforeGeneration = previousFloor;
        if (advanceFloor) GameController.floor++;
        bool completed = false;
        IEnumerator generation = GenerateDungeon(previousReady);
        activeGeneration = generation;
        try
        {
            while (true)
            {
                bool hasNext;
                try
                {
                    hasNext = generation.MoveNext();
                }
                catch (System.Exception exception)
                {
                    Fail("Dungeon generation failed: " + exception.Message);
                    Debug.LogException(exception, this);
                    break;
                }
                if (!hasNext) break;
                yield return generation.Current;
            }
            completed = LastFailure == null && IsReady;
        }
        finally
        {
            try { (generation as System.IDisposable)?.Dispose(); }
            finally
            {
                if (!completed && advanceFloor) GameController.floor = previousFloor;
                if (!completed && LastFailure == null) LastFailure = "Dungeon generation was interrupted.";
                activeGeneration = null;
                advancingFloor = false;
                IsGenerating = false;
            }
        }
        if (completed)
        {
            Generated?.Invoke();
            Debug.Log("Dungeon ready: " + board.Count + " connected cells.", this);
        }
    }

    private IEnumerator GenerateDungeon(bool previousReady)
    {
        if (size.x < 2 || size.y < 2 || size.x > 16 || size.y > 16 || offset.x <= 0 || offset.y <= 0 || player == null || navigationSurface == null || startRoom == null || endRoom == null || rooms == null || rooms.Length == 0)
        {
            Fail("Invalid dungeon configuration: assign player, surface, rooms and a grid between 2 and 16 cells per axis.");
            IsReady = previousReady;
            yield break;
        }
        foreach (var rule in rooms)
        {
            if (rule == null)
            {
                Fail("Dungeon room rules must not contain null entries.");
                IsReady = previousReady;
                yield break;
            }
        }
        var controller = player.GetComponent<CharacterController>();
        var movement = player.GetComponent<StarterAssets.ThirdPersonController>();
        var previousRoot = _generatedRoot;
        var previousBoard = board;
        var previousNavigationData = navigationSurface.navMeshData;
        Vector3 previousPosition = player.transform.position;
        Vector3 previousSpawn = PlayerSpawnPosition;
        bool previousRootActive = previousRoot != null && previousRoot.activeSelf;
        bool controllerEnabled = controller != null && controller.enabled;
        bool movementEnabled = movement != null && movement.canMove;
        bool rotationEnabled = movement != null && movement.canRotate;
        bool committed = false;
        try
        {
            if (controller != null) controller.enabled = false;
            if (movement != null) movement.canMove = movement.canRotate = false;
            navigationSurface.RemoveData();
            if (previousRoot != null)
            {
                previousRoot.SetActive(false);
                yield return null;
            }

            BuildLayout();
            _generatedRoot = new GameObject("GeneratedRooms");
            _generatedRoot.transform.SetParent(transform, false);
            _generatedRoot.SetActive(false);
            int[] remaining = new int[rooms.Length];
            for (int i = 0; i < rooms.Length; i++) remaining[i] = rooms[i].numb;
            var enemies = new List<GameObject>();
            Transform spawn = null;
            DungeonExit exit = null;
            for (int cellIndex = 0; cellIndex < board.Count; cellIndex++)
            {
                int x = cellIndex % size.x;
                int y = cellIndex / size.x;
                Cell cell = board[cellIndex];
                GameObject prefab = cell.startCell ? startRoom : cell.endCell ? endRoom : SelectRoom(x, y, remaining);
                if (prefab == null || prefab.GetComponent<RoomBehaviour>() == null)
                {
                    Fail("A room rule has no valid RoomBehaviour prefab.");
                    yield break;
                }
                var room = Instantiate(prefab, _generatedRoot.transform);
                room.transform.localPosition = new Vector3(x * offset.x, 0f, -y * offset.y);
                room.transform.localRotation = Quaternion.identity;
                room.name = prefab.name + " " + x + "-" + y;
                room.GetComponent<RoomBehaviour>().UpdateRoom(cell.status);
                if (cell.endCell) exit = room.GetComponentInChildren<DungeonExit>(true);
                if (cell.startCell)
                {
                    foreach (var marker in room.GetComponentsInChildren<Transform>(true))
                        if (marker.name == "PlayerSpawn") { spawn = marker; break; }
                }
                // Old room assets contain many small surfaces. Only the aggregate surface owns this floor's data.
                foreach (var smallSurface in room.GetComponentsInChildren<Unity.AI.Navigation.NavMeshSurface>(true)) smallSurface.enabled = false;
                foreach (var legacyWall in room.GetComponentsInChildren<WallCollision>(true))
                {
                    legacyWall.enabled = false;
                    var wallCollider = legacyWall.GetComponent<Collider>();
                    if (wallCollider != null) wallCollider.enabled = true;
                }
                foreach (var agent in room.GetComponentsInChildren<NavMeshAgent>(true))
                {
                    if (!agent.gameObject.activeSelf) continue;
                    enemies.Add(agent.gameObject);
                    agent.gameObject.SetActive(false);
                }
            }
            if (spawn == null) { Fail("Start room requires a PlayerSpawn marker."); yield break; }
            if (exit == null) { Fail("End room requires an active DungeonExit."); yield break; }
            _generatedRoot.SetActive(true);
            if (!exit.isActiveAndEnabled) { Fail("End room requires an active DungeonExit."); yield break; }
            foreach (var light in _generatedRoot.GetComponentsInChildren<Light>())
            {
                light.lightmapBakeType = LightmapBakeType.Realtime;
                light.shadows = LightShadows.None;
            }
            foreach (var door in _generatedRoot.GetComponentsInChildren<DoorScript>())
            {
                var doorAnimator = door.GetComponent<Animator>();
                if (doorAnimator != null) doorAnimator.Rebind();
                door.SetOpen(true, true);
            }
            Physics.SyncTransforms();
            navigationSurface.BuildNavMesh();
            if (navigationSurface.navMeshData == null || navigationSurface.navMeshData == previousNavigationData)
            {
                Fail("Dungeon NavMesh build did not produce replacement data.");
                yield break;
            }
            var filter = new NavMeshQueryFilter { agentTypeID = navigationSurface.agentTypeID, areaMask = NavMesh.AllAreas };
            if (!NavMesh.SamplePosition(spawn.position, out NavMeshHit playerHit, spawnSearchRadius, filter))
            {
                Fail("PlayerSpawn is not on the generated NavMesh.");
                yield break;
            }
            var playerInteraction = player.GetComponent<PlayerController>();
            float exitRange = playerInteraction != null ? playerInteraction.InteractionDistance : spawnSearchRadius;
            var exitPath = new NavMeshPath();
            if (!NavMesh.SamplePosition(exit.transform.position, out var exitHit, exitRange, filter) ||
                Vector3.Distance(exit.transform.position, exitHit.position) >= exitRange ||
                !NavMesh.CalculatePath(playerHit.position, exitHit.position, filter, exitPath) ||
                exitPath.status != NavMeshPathStatus.PathComplete)
            {
                Fail("Dungeon exit is not reachable from PlayerSpawn on the generated NavMesh.");
                yield break;
            }
            PlayerSpawnPosition = playerHit.position;
            player.transform.position = PlayerSpawnPosition + Vector3.up * 0.05f;
            if (controller != null) controller.enabled = true;
            if (movement != null) movement.canMove = movement.canRotate = true;
            foreach (var enemy in enemies)
            {
                var agent = enemy.GetComponent<NavMeshAgent>();
                var enemyFilter = new NavMeshQueryFilter { agentTypeID = agent.agentTypeID, areaMask = agent.areaMask };
                if (!NavMesh.SamplePosition(enemy.transform.position, out NavMeshHit enemyHit, spawnSearchRadius, enemyFilter))
                {
                    Debug.LogWarning("Skipped enemy spawn outside the NavMesh: " + enemy.name, enemy);
                    continue;
                }
                enemy.transform.position = enemyHit.position;
                var body = enemy.GetComponent<Rigidbody>();
                if (body != null) { body.isKinematic = true; body.useGravity = false; }
                var ai = enemy.GetComponent<EnemyAi>();
                if (ai != null) ai.target = player.transform;
                enemy.SetActive(true);
                agent.Warp(enemyHit.position);
            }
            committed = true;
            IsReady = true;
            if (ownedNavigationData != null && ownedNavigationData != navigationSurface.navMeshData)
                Destroy(ownedNavigationData);
            ownedNavigationData = navigationSurface.navMeshData;
            if (previousRoot != null) Destroy(previousRoot);
        }
        finally
        {
            if (!committed)
            {
                if (LastFailure == null) LastFailure = "Dungeon generation was interrupted.";
                if (_generatedRoot != null && _generatedRoot != previousRoot)
                {
                    _generatedRoot.SetActive(false);
                    Destroy(_generatedRoot);
                }
                _generatedRoot = previousRoot;
                board = previousBoard;
                PlayerSpawnPosition = previousSpawn;
                bool navigationRestored = false;
                try
                {
                    if (navigationSurface != null)
                    {
                        var failedNavigationData = navigationSurface.navMeshData;
                        navigationSurface.RemoveData();
                        navigationSurface.navMeshData = previousNavigationData;
                        if (navigationSurface.isActiveAndEnabled) navigationSurface.AddData();
                        navigationRestored = previousNavigationData != null;
                        if (failedNavigationData != null && failedNavigationData != previousNavigationData)
                            Destroy(failedNavigationData);
                    }
                }
                catch (System.Exception exception)
                {
                    LastFailure += " Navigation rollback failed: " + exception.Message;
                    Debug.LogException(exception, this);
                }
                if (previousRoot != null) previousRoot.SetActive(previousRootActive);
                if (controller != null) controller.enabled = false;
                if (player != null) player.transform.position = previousPosition;
                if (controller != null) controller.enabled = controllerEnabled;
                var stats = player != null ? player.GetComponent<CharacterStats>() : null;
                if (movement != null)
                {
                    movement.canMove = movementEnabled && (stats == null || !stats.IsDead);
                    movement.canRotate = rotationEnabled && (stats == null || !stats.IsDead);
                }
                Physics.SyncTransforms();
                IsReady = previousReady && previousRoot != null && navigationRestored;
            }
        }
    }

    private void Fail(string message)
    {
        LastFailure = message;
        Debug.LogError(message, this);
    }

    private GameObject SelectRoom(int x, int y, int[] remaining)
    {
        var candidates = new List<int>();
        for (int i = 0; i < rooms.Length; i++)
        {
            int probability = rooms[i].ProbabilityOfSpawning(x, y);
            if (rooms[i].room == null || remaining[i] <= 0 || probability == 0) continue;
            if (probability == 2) { remaining[i]--; return rooms[i].room; }
            candidates.Add(i);
        }
        if (candidates.Count == 0) return null;
        int chosen = candidates[Random.Range(0, candidates.Count)];
        remaining[chosen]--;
        return rooms[chosen].room;
    }

    private void BuildLayout()
    {
        board = new List<Cell>(size.x * size.y);
        for (int i = 0; i < size.x * size.y; i++) board.Add(new Cell());
        int start = Mathf.Clamp(startPos, 0, board.Count - 1);
        var stack = new Stack<int>();
        stack.Push(start);
        board[start].visited = true;
        while (stack.Count > 0)
        {
            int current = stack.Peek();
            var neighbors = CheckNeighbors(current);
            if (neighbors.Count == 0) { stack.Pop(); continue; }
            int next = neighbors[Random.Range(0, neighbors.Count)];
            int direction = next == current - size.x ? 0 : next == current + size.x ? 1 : next == current + 1 ? 2 : 3;
            board[current].status[direction] = true;
            board[next].status[direction ^ 1] = true;
            board[next].visited = true;
            stack.Push(next);
        }
        board[start].startCell = true;
        // The exit is the furthest reachable cell, not a recursively selected random coordinate.
        var distances = new int[board.Count];
        for (int i = 0; i < distances.Length; i++) distances[i] = -1;
        var queue = new Queue<int>();
        queue.Enqueue(start);
        distances[start] = 0;
        int furthest = start;
        int[] steps = { -size.x, size.x, 1, -1 };
        while (queue.Count > 0)
        {
            int current = queue.Dequeue();
            for (int direction = 0; direction < 4; direction++)
            {
                if (!board[current].status[direction]) continue;
                int next = current + steps[direction];
                if (distances[next] >= 0) continue;
                distances[next] = distances[current] + 1;
                if (distances[next] > distances[furthest]) furthest = next;
                queue.Enqueue(next);
            }
        }
        board[furthest].endCell = true;
    }

    private List<int> CheckNeighbors(int cell)
    {
        var neighbors = new List<int>(4);
        if (cell >= size.x && !board[cell - size.x].visited) neighbors.Add(cell - size.x);
        if (cell + size.x < board.Count && !board[cell + size.x].visited) neighbors.Add(cell + size.x);
        if (cell % size.x < size.x - 1 && !board[cell + 1].visited) neighbors.Add(cell + 1);
        if (cell % size.x > 0 && !board[cell - 1].visited) neighbors.Add(cell - 1);
        return neighbors;
    }
}

