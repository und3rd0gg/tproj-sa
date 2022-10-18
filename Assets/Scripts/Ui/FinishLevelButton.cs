using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FinishLevelButton : MonoBehaviour, IPointerClickHandler
{
    public event Action Clicked;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
