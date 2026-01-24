using System;
using UnityEngine;
using UnityEngine.Playables;

// A behaviour that is attached to a playable
public interface IHitBox
{
    public GameObject _ownerHit { get; set; }
    public float _damage { get; set; }
    public Vector3 _knockbackDirection { get; set; }
    public float _knockbackForce { get; set; }
    public float _knockbackTime { get; set; }

    public LayerMask _targetLayer { get; set; }
    //public DamageType _damageType { get; set; }

    public Action<Collider[]> _OnAttackHit { get; set; }
    public Action _OnNoHit { get; set; }

    public void PerformAttack();

    public void SetUpHitBox(LayerMask targetLayer, GameObject ownerHit, float damage, Vector3 knockbackDirection, float knockbackForce, float knockbackTime)
    {
        _targetLayer = targetLayer;
        _ownerHit = ownerHit;
        _damage = damage;
        _knockbackDirection = knockbackDirection;
        _knockbackForce = knockbackForce;
        _knockbackTime = knockbackTime;

        _OnAttackHit = null;
        _OnNoHit = null;
    }
    //public enum DamageType
    //{
    //    NoneOwner, NoneTeam, AllEntity
    //}
}
