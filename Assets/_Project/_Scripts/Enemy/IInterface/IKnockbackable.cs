using System.Collections;
using UnityEngine;


public interface IKnockbackable
{
    public float _knockbackMultiplier { get; set; }
    public bool _canKnockback { get; set; }

    void GetKnockedBack(Vector3 direction, float force,float time);
    /*
    {
        if (!canKnockback) return;
    
        if (KnockbackCoroutine != null) StopCoroutine(KnockbackCoroutine);
        KnockbackCoroutine = StartCoroutine(ApplyKnockback(direction, force));
    }
    */

    //IEnumerator ApplyKnockback(Vector3 direction, float force);
    //ตัวอย่าง
    /*
    private IEnumerator ApplyKnockback(Vector3 direction, float force)
    {
        Debug.Log($"ApplyKnockback : {direction} | {force}");

        yield return null;
        agent.enabled = false;
        rb.useGravity = true;
        rb.isKinematic = false;

        rb.AddForce(direction * force, ForceMode.Impulse);

        yield return new WaitForFixedUpdate();
        float knockbackTime = Time.time;
        yield return new WaitUntil(
            () => rb.linearVelocity.magnitude < StillThreshold || Time.time > knockbackTime + MaxKnockbackTime
        );
        yield return new WaitForSeconds(0.25f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        agent.Warp(transform.position);
        agent.enabled = true;

        yield return null;


        //กลับไป stest เดิน
        //if (Player != null)
        //{
        //    KnockbackCoroutine = StartCoroutine(ChasePlayer(Player));
        //}
        //else
        //{
        //    KnockbackCoroutine = StartCoroutine(Roam());
        //}
    }
    */
}

////Nope
//[System.Serializable]
//public class KnockbackableStat
//{
//    public bool canKnockback = true;
//    private Coroutine KnockbackCoroutine;
//    [Range(0.001f, 0.1f)][SerializeField] private float StillThreshold = 0.05f;
//    [SerializeField] private float MaxKnockbackTime = 0.5f;
//}
