using UnityEngine;

    public interface IKnockbackable
    {
        void GetKnockedBack(Vector3 direction, float force);
    }