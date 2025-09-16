using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class InteractableBase : MonoBehaviour
{
    [Header("Interact Message Prompt")]
    public string message;
    public Vector3 offSetInteractText;
    
    [Header("On Interact")]
    public UnityEvent OnInteract;

    [Header("Hold Interact")]
    public bool hasDuration;
    public float duration = 0f;
    public bool isResetTimer;
    public UnityEvent OnFinished;

    [Header("System")]
    [HideInInspector] public UnityEvent<float> Progress;
    [HideInInspector] public float timer;
    private bool onProgress;
    private bool onFinishedCall;

    private void Update()
    {
        if (onProgress)
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                if (onFinishedCall) return;
                onFinishedCall = true;
                OnFinished?.Invoke();
                Progress?.Invoke(0f);
                EndInteract();
            }
            else
            {
                Progress?.Invoke(timer / duration);
            }
        }
    }
    public void Interact()
    {
        //interact
        OnInteract?.Invoke();
        //ani OnInteract

        if (hasDuration)
        {
            onProgress = true;
        }
    }

    public void InvokePlayCanMove()
    {
        //GameManager.instance.PlayeCanAction(true);
    }

    public void EndInteract()
    {
        onProgress = false;
        onFinishedCall = false;
        if (isResetTimer)
        {
            timer = 0f;
        }

        if (hasDuration)
        {
            AudioSource audioSource = gameObject.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                if(audioSource.loop) audioSource.Stop();
            }
        }
    }

    public void ReInteract()
    {
        gameObject.layer = 0;
        GetComponent<InteractableBase>().enabled = false;
    }

}
