using UnityEngine;

public class ControllerVFX : MonoBehaviour
{
    public ParticleSystem _particleSystem;
    private bool _isPlaying = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(_particleSystem == null) _particleSystem = GetComponent<ParticleSystem>();
        _particleSystem.Play();
        _isPlaying = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (_isPlaying && !_particleSystem.IsAlive())
        {
            Destroy(gameObject);
        }
    }
}
