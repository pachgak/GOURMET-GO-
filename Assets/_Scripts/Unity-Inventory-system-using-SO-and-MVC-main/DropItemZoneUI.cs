using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItemZoneUI : MonoBehaviour , IDropHandler
{
    public event Action OnItemDropped;
    public void OnDrop(PointerEventData eventData)
    {
        OnItemDropped?.Invoke();
    }
}
