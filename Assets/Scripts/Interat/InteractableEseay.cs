using UnityEngine;
using UnityEngine.Events;

public class InteractableEseay : MonoBehaviour , IInteractable
{
    public UnityEvent OnInteract;

    public GameObject _gameObjectInteract => this.gameObject;

    public string _message => throw new System.NotImplementedException();

    UnityEvent IInteractable._OnInteract => OnInteract;

    public void Interact()
    {
        OnInteract?.Invoke();
    }
}