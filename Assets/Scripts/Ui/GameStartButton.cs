using System;
using UnityEngine;

namespace Ui
{
    public class GameStartButton : MonoBehaviour
    {
        public event Action Clicked;

        public void OnPointerClick()
        {
            Clicked?.Invoke();
        }
    }
}