using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    #region Public properties

    public ActionBar Bar;
    [Tooltip("Array of spawn points where figures appear")]
    public Transform[] SpawnPoints;
    [Space]
    [Tooltip("List of figure prefabs to spawn (each must have a Figure component)")]
    public GameObject[] FigurePrefabs;
    [Tooltip("Color options for figure outlines or shapes")]
    public UnityEngine.Color[] Colors;
    [Tooltip("Animal sprite options to overlay on figures")]
    public Sprite[] Animals;
    [Space]
    [Tooltip("Minimum figures per combination required for removal (default is 3)")]
    public int MinFiguresToCollect;
    [Tooltip("Number of unique prefab-color-animal combinations to generate")]
    public int CombinationsCount;
    [Tooltip("Total number of figures to spawn (auto-adjusted to a multiple of CombinationsCount)")]
    public int TotalObjectsCount;
    [Space]
    [Tooltip("Number of figures spawned in each burst")]
    public int BurstSize = 5;
    [Tooltip("Delay in seconds between spawn bursts")]
    public float BurstInterval = 0.5f;
    [Space]
    [Tooltip("Debug: current count of active figures on the board")]
    public int allCurrentObjectsCount;
    #endregion
    #region Private properties
    // List of all active figures on the scene
    List<Figure> _allCurrentObjects;
    // Generated list of unique combinations
    private List<Vector3Int> _combos;
    // Queue of combinations to spawn
    private List<Vector3Int> _spawnList;
    // Shuffled list of spawn points
    private List<Transform> _spawns;
    [HideInInspector]
    public GameObject _figuresParent; // Container GameObject for spawned figures

    /// <summary>Event invoked when all figures have finished spawning</summary>
    public delegate void DoneInstantiating();
    public event DoneInstantiating OnDoneInstantiating;
    #endregion
    #region Unity Methods
    void Start()
    {
        // Initialize tracking lists
        _allCurrentObjects = new List<Figure>();
        allCurrentObjectsCount = _allCurrentObjects.Count;
        // Link the ActionBar to this manager
        Bar.manager = this;

        PrepareCombinationsAndSpawnList();
        StartCoroutine(SpawnWithBursts());
    }
    void Update()
    {
        // Update debug counter each frame
        allCurrentObjectsCount = _allCurrentObjects.Count;
    }
    #endregion
    #region Main Methods
    /// <summary>
    /// Prepares all possible combinations and shuffles spawn points.
    /// </summary>
    void PrepareCombinationsAndSpawnList()
    {
        int F = FigurePrefabs.Length, C = Colors.Length, A = Animals.Length;
        int maxUnique = F * C * A;

        // Clamp the number of combos to valid range
        CombinationsCount = Mathf.Clamp(CombinationsCount, 1, maxUnique);

        // Build all possible prefab-color-animal triples
        var all = new List<Vector3Int>(maxUnique);
        for (int f = 0; f < F; f++)
            for (int c = 0; c < C; c++)
                for (int a = 0; a < A; a++)
                    all.Add(new Vector3Int(f, c, a));
        Shuffle(all);

        // Take the first CombinationsCount combos
        _combos = all.GetRange(0, CombinationsCount);

        // Ensure minimum total figures for removal logic
        int minTotal = CombinationsCount * MinFiguresToCollect;
        if (TotalObjectsCount < minTotal) TotalObjectsCount = minTotal;

        // Round up TotalObjectsCount to a multiple of CombinationsCount
        if (TotalObjectsCount % CombinationsCount != 0)
        {
            int rounded = ((TotalObjectsCount + CombinationsCount - 1) / CombinationsCount) * CombinationsCount;
            TotalObjectsCount = rounded;
        }

        // Build spawn list with equal repeats of each combo
        int perCombo = TotalObjectsCount / CombinationsCount;
        _spawnList = new List<Vector3Int>(TotalObjectsCount);
        foreach (var combo in _combos)
            for (int k = 0; k < perCombo; k++)
                _spawnList.Add(combo);

        // Shuffle combos and spawn points
        Shuffle(_spawnList);
        _spawns = new List<Transform>(SpawnPoints);
        Shuffle(_spawns);

        // Create a parent object to organize spawned figures
        _figuresParent = Instantiate(new GameObject("Figures"), Vector3.zero, Quaternion.identity);
    }
    /// <summary>
    /// Spawns figures in bursts according to BurstSize and BurstInterval.
    /// </summary>
    IEnumerator SpawnWithBursts()
    {
        int total = _spawnList.Count;
        int spawned = 0;
        int spCount = _spawns.Count;

        while (spawned < total)
        {
            // Spawn a burst of figures
            for (int i = 0; i < BurstSize && spawned < total; i++, spawned++)
            {
                var combo = _spawnList[spawned];
                var point = _spawns[spawned % spCount];

                // Instantiate figure prefab
                var obj = Instantiate(FigurePrefabs[combo.x], point.position, Quaternion.identity, _figuresParent.transform);

                // Color the inner sprite
                var td = obj.transform.Find("inner");
                td.GetComponent<SpriteRenderer>().color = Colors[combo.y];

                // Assign the animal sprite
                var tf = obj.transform.Find("Animal");
                tf.GetComponent<SpriteRenderer>().sprite = Animals[combo.z];

                // Register the figure and store its combo
                var fig = obj.GetComponent<Figure>();
                fig.combo = combo;
                _allCurrentObjects.Add(fig);
            }

            // Wait before next burst
            yield return new WaitForSeconds(BurstInterval);
        }

        // Notify that spawning is complete
        OnDoneInstantiating?.Invoke();
    }
    #endregion
    #region Public methods
    /// <summary>
    /// Finds the first active figure matching the given combo.
    /// </summary>
    public Figure FindObjectByCombo(Vector3Int combo)
    {
        for (int i = 0; i < _allCurrentObjects.Count; i++)
        {
            if (_allCurrentObjects[i].combo == combo) return _allCurrentObjects[i];
        }
        return null;
    }
    /// <summary>
    /// Picks the specified number of random figures from the active list.
    /// </summary>
    public List<Figure> PickRandomFigures(int amount)
    {
        List<Figure> final = new List<Figure>();
        for (int i = 0; i < amount; i++)
        {
            int rnd = UnityEngine.Random.Range(0, _allCurrentObjects.Count);
            final.Add(_allCurrentObjects[rnd]);
        }
        return final;
    }
    /// <summary>
    /// Removes a figure from the manager's tracking list.
    /// </summary>
    public void RemoveObject(Figure obj)
    {
        _allCurrentObjects.Remove(obj);
    }
    #endregion
    #region Utilities
    /// <summary>
    /// Shuffles a list in place using the Fisher–Yates algorithm.
    /// </summary>
    private static void Shuffle<T>(IList<T> list)
    {
        int n = list.Count;
        for (int i = 0; i < n - 1; i++)
        {
            int j = Random.Range(i, n);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
    #endregion
}
