using UnityEngine;
/// <summary>
/// Plays a particle system at the given slot position whenever a slot is finished.
/// </summary>
public class ParticlesEnable : MonoBehaviour
{
    #region Inspector Fields
    [Tooltip("Prefab of a one-shot particle system for arrival effects")]
    public GameObject ParticlesPrefab;
    [Tooltip("Reference to the ActionBar to subscribe to slot-finish events")]
    public ActionBar Bar;
    #endregion
    #region Private Fields
    GameObject _particles;
    ParticleSystem _particlesSystem;
    #endregion
    #region Unity Callbacks
    void Start()
    {
        // Instantiate and cache the particle system
        _particles = Instantiate(ParticlesPrefab, Vector3.zero, Quaternion.identity);
        _particlesSystem = _particles.GetComponent<ParticleSystem>();

        // Subscribe to the ActionBar's finish event
        Bar.OnSlotFinished += PlayPartcls;

    }
    #endregion
    #region Public Methods
    /// <summary>
    /// Moves the particle system to the given position and plays it.
    /// </summary>
    public void PlayPartcls(Vector3 position)
    {
        if (_particlesSystem != null)
        {
            _particles.transform.SetPositionAndRotation(position, Quaternion.identity);
            _particlesSystem.Play();
        }
    }
    #endregion
}
