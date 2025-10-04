using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public interface IHurtBox
{
    public GameObject _ownerHit { get; set; }
    public float _damage { get; set; }
    public float _knockbackForce { get; set; }
    public Vector3 _knockbackDirection { get; set; }

    public LayerMask _targetLayer { get; set; }
    //public DamageType _damageType { get; set; }

    public void PerformAttack();

    //public enum DamageType
    //{
    //    NoneOwner, NoneTeam, AllEntity
    //}
}
