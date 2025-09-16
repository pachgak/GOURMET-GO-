using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public interface IInteractable
{
    public GameObject _gameObjectInteract { get; }
    public string _message { get; }
    public UnityEvent _OnInteract { get ; }
    public void Interact();
    //public void InteractReset();
}
