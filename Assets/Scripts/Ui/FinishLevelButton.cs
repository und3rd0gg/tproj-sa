using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class FinishLevelButton : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }

    public event Action Clicked;
}