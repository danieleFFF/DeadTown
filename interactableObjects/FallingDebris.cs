using UnityEngine;

public class FallingDebris : MonoBehaviour
{
    private Renderer _renderer;
    private Rigidbody _rb;
    private bool hasAppeared = false;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Only run logic if physics is active (falling)
        if (_rb != null && !_rb.isKinematic)
        {
            if (_renderer != null)
            {
                // If the object has been seen by the camera
                if (_renderer.isVisible)
                {
                    hasAppeared = true;
                }
                // If it was seen, but is no longer visible (e.g. fell through floor or out of view)
                else if (hasAppeared)
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
