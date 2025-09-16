using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public interface IHurtBox
{
    public float _damage { get; set; }
    public float _knockbackForce { get; set; }
    public Vector3 _knockbackDirection { get; set; }
}
