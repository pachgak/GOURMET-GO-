using System.Buffers;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class RunVfxTest : MonoBehaviour
{
    public SpriteRenderer target;
    public ParticleSystem[] effects;

    public ParticleSystem owner;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        owner = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        if (target.enabled == true && !effects[0].isPlaying)
        {
            owner.Play();

            //foreach (ParticleSystem owner in effects)
            //{
            //    owner.Play();

            //}
        }

        if (target.enabled == false && effects[0].isPlaying)
        {
            //foreach (ParticleSystem owner in effects)
            //{
            //    owner.Stop();
            //}

            owner.Stop();
        }
    }
}
