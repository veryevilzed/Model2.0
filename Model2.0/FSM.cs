using System;
using System.Collections.Generic;
using System.Linq;

namespace TinyLima.Tools
{
    /// <summary>
    /// Машина состояний
    /// </summary>
    public class FSM : IDisposable
    {
        private readonly Dictionary<string, IState> _states = new Dictionary<string, IState>();

        private IState currentState;
        
        public void Add(params IState[] states)
        {
            foreach (var state in states)
            {
                bool found = false;
                foreach (object attr in state.GetType().GetCustomAttributes(typeof(State), false))
                {
                    Add(state, ((State) attr).Name);
                    found = true;
                }
                if (!found)
                    LogCallback.Error("FSMState {0} doesn't have attribute State", state.GetType().Name);
            }
        }
        
        public void Replace(params IState[] states)
        {
            foreach (var state in states)
            {
                bool found = false;
                foreach (object attr in state.GetType().GetCustomAttributes(typeof(State), false))
                {
                    Replace(state, ((State) attr).Name);
                    found = true;
                }
                if (!found)
                    LogCallback.Error("FSMState {0} doesn't have attribute State", state.GetType().Name);
            }
        }

        public void Add(IState state, string stateName)
        {
            if (_states.ContainsKey(stateName))
            {
                LogCallback.Warn("Dublicate state with name {0}. Use method Replace", stateName);
                _states.Remove(stateName);
            }
            state.Parent = this;
            _states.Add(stateName, state);
        }

        public void Replace(IState state, string stateName)
        {
            if (_states.ContainsKey(stateName))
                _states.Remove(stateName);
            state.Parent = this;
            _states.Add(stateName, state);
        }

        public void Start(string stateName)
        {
            if (currentState == null)
                Change(stateName);
        }

        public void Change(string name)
        {
            currentState?.__ExitState();
            currentState = _states[name];
            currentState.__EnterState();
        }

        public void Update(float deltaTime) => currentState?.__UpdateState(deltaTime);
        public void Invoke(string eventName, params object[] args) => currentState?.__Invoke(eventName, args);
        
        public void Dispose()
        {
            foreach (var kv in _states)
                kv.Value.Dispose();
            _states.Clear();
        }
    }
}