using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/// <summary>
/// Manages the action bar UI: collects figures, stacks combos, triggers win/lose.
/// </summary>
public class ActionBar : MonoBehaviour
{
    #region Properties
    [Tooltip("Array of slot transforms where figures will be placed")]
    [SerializeField] Transform[] Slots;
    [Tooltip("UI panel to show on player win")]
    public GameObject WinScreen;
    [Tooltip("UI panel to show on player lose")]
    public GameObject LoseScreen;

    [HideInInspector]
    public Vector3 currentShapeInSlotPos;
    [HideInInspector]
    public BoardManager manager; // Assigned by BoardManager on Start
    public bool IsLocked { get; private set; }

    public void LockBar() => IsLocked = true;
    public void UnlockBar() => IsLocked = false;

    /// <summary>Called when a figure reaches its slot position (before stacking).</summary>
    public delegate void slotFinished(Vector3 pos);
    public event slotFinished OnSlotFinished;

    /// <summary>Called when a slot's figures finish (e.g. removed).</summary>
    public delegate void ShapeEnterSlot(Vector3 pos);
    public event ShapeEnterSlot OnShapeEnterSlot;

    /// <summary>Called when the player wins.</summary>
    public delegate void PlayerWin();
    public event PlayerWin OnPlayerWin;

    /// <summary>Called when the player loses.</summary>
    public delegate void PlayerLose();
    public event PlayerLose OnPlayerLose;

    /// <summary>Called when the player clicks a figure; provides its world position and combo.</summary>
    public delegate void PlayerClickOnShape(Vector3 pos, Vector3Int combo);
    public event PlayerClickOnShape OnPlayerClickOnShape;

    // Each slot holds a list (stack) of Figure instances.
    readonly List<List<Figure>> _stacks = new List<List<Figure>>();
    #endregion

    #region Unity Methods
    void Awake()
    {
        // Initialize stacks list for each slot
        for (int i = 0; i < Slots.Length; i++)
            _stacks.Add(new List<Figure>());
    }
    void LateUpdate()
    {
        // Win condition: no more figures on board
        if (manager.allCurrentObjectsCount == 0)
            Win();
    }
    #endregion
    #region Public Methods
    /// <summary>
    /// Returns world position for the next figure slot: either existing combo stack or first empty.
    /// </summary>
    public Vector3 GetNextSlotWorldPos(Figure fig)
    {
        int firstEmpty = -1;

        for (int i = 0; i < _stacks.Count; i++)
        {
            int cnt = _stacks[i].Count;
            // If matching stack exists and not full, return its slot
            if (cnt > 0 && cnt < 3 && _stacks[i][0].combo == fig.combo)
                return Slots[i].position;

            // Remember the first empty slot index
            if (cnt == 0 && firstEmpty == -1)
                firstEmpty = i;
        }
        // Use first empty, otherwise fallback to last slot
        int slotIndex = firstEmpty != -1? firstEmpty: Slots.Length - 1;

        return Slots[slotIndex].position;
    }
    /// <summary>
    /// Accepts a figure into the bar: stacks it, removes on full combo, or triggers lose.
    /// </summary>
    public void AcceptFigurine(Figure fig)
    {
        int targetSlot = -1;

        // Try to find a stack for this combo
        for (int i = 0; i < _stacks.Count; i++)
        {
            if (_stacks[i].Count > 0 &&
                _stacks[i][0].combo == fig.combo &&
                _stacks[i].Count < 3)
            {
                targetSlot = i;

                break;
            }
        }

        // Otherwise find an empty slot
        if (targetSlot == -1)
        {
            for (int i = 0; i < _stacks.Count; i++)
            {
                if (_stacks[i].Count == 0)
                {
                    targetSlot = i;
                    break;
                }
            }
        }
        // If still none, player loses
        if (targetSlot == -1)
        {
            Lose();
            return;
        }

        // Inform listeners of entry position
        OnShapeEnterSlot?.Invoke(currentShapeInSlotPos);

        // Add to stack
        _stacks[targetSlot].Add(fig);

        // If stack reached required count, finish and clear it
        if (_stacks[targetSlot].Count == manager.MinFiguresToCollect)
        {
            foreach (var f in _stacks[targetSlot])
            {
                SlotFinish(f);
            }
            _stacks[targetSlot].Clear();
            IsLocked = false;
        }
    }
    /// <summary>
    /// Called by Figure when clicked to notify bar.
    /// </summary>
    public void PlayerClick(Vector3 pos, Vector3Int combo)
    {
        OnPlayerClickOnShape?.Invoke(pos, combo);
    }
    /// <summary>
    /// Handles removal of a single figure and notifies finish.
    /// </summary>
    public void SlotFinish(Figure combo)
    {
        OnSlotFinished?.Invoke(currentShapeInSlotPos); // pass fig's slot position
        manager.RemoveObject(combo);
        Destroy(combo.gameObject);
    }
    /// <summary>
    /// Triggers win: shows WinScreen and pauses time.
    /// </summary>
    public void Win()
    {
        OnPlayerWin?.Invoke();
        WinScreen.SetActive(true);
        Time.timeScale = 0;
    }
    /// <summary>
    /// Triggers lose: cleans up figures, shows LoseScreen, pauses time.
    /// </summary>
    public void Lose()
    {
        OnPlayerLose?.Invoke();
        Destroy(manager._figuresParent);
        LoseScreen.SetActive(true);
        Time.timeScale = 0;
    }
    /// <summary>
    /// Restarts the current scene.
    /// </summary>
    public void RestartScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion
}