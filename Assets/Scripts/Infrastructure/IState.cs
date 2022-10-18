﻿using UnityEngine;

namespace Infrastructure
{
    public interface IState : IExitableState
    {
        void Enter();
    }
}