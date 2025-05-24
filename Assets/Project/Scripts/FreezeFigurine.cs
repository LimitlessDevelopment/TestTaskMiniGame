using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Controls a grayscale freeze/unfreeze effect on a random set of figures when a certain
/// number of figures have been removed from the board.
/// </summary>
public class FreezeFigurine : MonoBehaviour
{
    #region Properties
    [Tooltip("Reference to the BoardManager to read removal counts and pick figures")]
    public BoardManager Manager;
    [Tooltip("Material instance with a '_GrayAmount' property for grayscale effect")]
    public Material TargetMaterial;
    [Tooltip("Duration in seconds of the grayscale fade-in effect")]
    public float GrayScaleEffectDuration = 1.5f;
    [Tooltip("Number of figures removed at which to trigger unfreeze")]
    public int RemovedFiguresAmount;
    [Tooltip("How many figures to freeze when the effect starts")]
    public int FreezeFiguresAmount;

    // Cached list of currently frozen figures
    List<Figure> _figures;
    // Lock flag to avoid repeated unfreeze triggers
    bool _stop;
    #endregion
    #region Unity Methods
    void Start()
    {
        _figures = new List<Figure>();
        // Subscribe to spawn-complete event
        Manager.OnDoneInstantiating += Freeze;
        // Enable unfreeze checks after spawning
        Manager.OnDoneInstantiating += Stop;
        _stop = true;
    }
    private void FixedUpdate()
    {
        // Check if enough figures have been removed to unfreeze
        if (RemainFigures() >= RemovedFiguresAmount && !_stop)
        {
            UnFreeze();
            _stop = true;
        }
    }
    private void OnDestroy()
    {
        // Reset material on destruction
        TargetMaterial.SetFloat("_GrayAmount", 0);
    }
    #endregion
    #region Public Methods
    /// <summary>
    /// Freezes a random subset of figures: disables their click and applies grayscale material.
    /// </summary>
    public void Freeze()
    {
        if (Manager == null || TargetMaterial == null) return;

        // Pick random figures to freeze
        _figures = Manager.PickRandomFigures(FreezeFiguresAmount);

        // Apply the grayscale material to all renderers
        foreach (Figure f in _figures)
        {
            f.FigureIsActive = false;
            foreach (SpriteRenderer r in f.transform.GetComponentsInChildren<SpriteRenderer>())
            {
                r.material = TargetMaterial;
            }
        }
        // Start the fade-to-gray coroutine
        StartFreeze();
    }
    /// <summary>
    /// Allows unfreeze logic to run (called after spawn completes).
    /// </summary>
    public void Stop()
    {
        _stop = false;
    }
    /// <summary>
    /// Unfreezes all previously frozen figures: restores click and fades back to color.
    /// </summary>
    public void UnFreeze()
    {
        if (Manager == null) return;

        foreach (Figure f in _figures)
            f.FigureIsActive = true;

        // Start the fade-to-color coroutine
        StartUnfreeze(GrayScaleEffectDuration);
    }
    /// <summary>
    /// Returns number of figures removed so far (based on total vs current count).
    /// </summary>
    public int RemainFigures()
    {
        return Manager.TotalObjectsCount - Manager.allCurrentObjectsCount;
    }
    /// <summary>
    /// Gradually increases '_GrayAmount' on the material to 1 over duration.
    /// </summary>
    public void StartFreeze()
    {
        StopAllCoroutines();
        StartCoroutine(FreezeCoroutine());
    }
    public void StartUnfreeze(float unfreezeDuration)
    {
        StopAllCoroutines();
        StartCoroutine(UnfreezeCoroutine(unfreezeDuration));
    }
    #endregion
    #region Coroutines
    IEnumerator FreezeCoroutine()
    {
        // Optional delay before grayscale
        yield return new WaitForSeconds(5f);
        float t = 0;
        while (t < GrayScaleEffectDuration)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(t / GrayScaleEffectDuration);
            TargetMaterial.SetFloat("_GrayAmount", f);
            yield return null;
        }
        TargetMaterial.SetFloat("_GrayAmount", 1f);
    }
    /// <summary>
    /// Gradually decreases '_GrayAmount' back to 0 over duration.
    /// </summary>
    IEnumerator UnfreezeCoroutine(float dur)
    {
        float t = 0;
        float start = TargetMaterial.GetFloat("_GrayAmount");
        while (t < dur)
        {
            t += Time.deltaTime;
            float f = Mathf.Clamp01(1 - (t / dur) * (1 - start));
            TargetMaterial.SetFloat("_GrayAmount", f);
            yield return null;
        }
        TargetMaterial.SetFloat("_GrayAmount", 0f);
    }
    #endregion
}
