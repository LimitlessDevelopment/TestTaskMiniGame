using UnityEngine;
using System.Collections;
/// <summary>
/// Handles click interaction and flight animation for a board figure.
/// Sends itself to the ActionBar when clicked.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class Figure : MonoBehaviour
{
    [Tooltip("Combo indices: prefab, color, animal (x,y,z)")]
    public Vector3Int combo;

    [Tooltip("Is the figure currently active and clickable?")]
    public bool FigureIsActive = true;

    [Tooltip("Reference to the ActionBar that collects figures")]
    ActionBar bar;

    void Start()
    {
        // Find the ActionBar in the scene (assumes exactly one exists)
        bar = FindObjectsByType<ActionBar>(FindObjectsSortMode.None)[0];
    }
    /// <summary>
    /// Called when the figure is clicked or tapped.
    /// Begins its flight to the bar if active and bar is not locked.
    /// </summary>
    void OnMouseDown()
    {
        if (bar == null || bar.IsLocked || !FigureIsActive) return;

        // Disable physics simulation during flight
        GetComponent<Rigidbody2D>().simulated = false;

        // Notify ActionBar of player click
        bar.PlayerClick(transform.position, combo);

        // Start the flight animation coroutine
        StartCoroutine(FlyToBar(bar));
    }
    /// <summary>
    /// Animates the figure flying into its target slot in the ActionBar.
    /// Uses ease-in-out interpolation and a full 360° spin.
    /// </summary>
    IEnumerator FlyToBar(ActionBar bar)
    {
        // Lock the bar to prevent other inputs during flight
        bar.LockBar();

        Vector3 startPos = transform.position;
        Vector3 endPos = bar.GetNextSlotWorldPos(this);
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.identity;  // reset rotation on arrival

        float elapsed = 0f;
        float duration = 0.35f; // flight time

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Smooth ease-in-out for position
            float ease = Mathf.SmoothStep(0, 1, t);
            transform.position = Vector3.Lerp(startPos, endPos, ease);

            float spinAngle = Mathf.Lerp(0f, 360f, t);
            transform.rotation = startRot * Quaternion.Euler(0, 0, spinAngle);

            yield return null;
        }

        // Snap to final position and rotation
        transform.SetPositionAndRotation(endPos, endRot);

        bar.currentShapeInSlotPos = endPos;

        // Deliver this figure to the ActionBar
        bar.AcceptFigurine(this);

        // Unlock bar for the next input
        bar.UnlockBar();
    }
}
