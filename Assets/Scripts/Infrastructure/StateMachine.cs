using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public abstract class StateMachine
    {
        private IExitableState _activeState;
        protected Dictionary<Type, IExitableState> States;

        public void Enter<TState>() where TState : class, IState
        {
            IState state = GetState<TState>();

            if (state == _activeState)
                return;

            _activeState?.Exit();
            _activeState = state;
            state.Enter();
        }

        public void Enter<TState, TPayload>(TPayload payload) where TState : class, IPayloadedState<TPayload>
        {
            _activeState?.Exit();
            IPayloadedState<TPayload> state = GetState<TState>();
            _activeState = state;
            state.Enter(payload);
        }

        private TState GetState<TState>() where TState : class, IExitableState
        {
            return States[typeof(TState)] as TState;
        }
    }
}