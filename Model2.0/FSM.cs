﻿using System;
using System.Collections.Generic;
using SmartFormat;

namespace Novolot.Tools
{
    /// <summary>
    /// Машина состояний
    /// </summary>
    public class FSM : IDisposable
    {
        public static bool ShowFsmEnterState { get; set; }
        public static bool ShowFsmExitState { get; set; }
        
        private readonly Dictionary<string, IState> _states = new Dictionary<string, IState>();

        private IState currentState;
        
        public string CurrentStateName { get; protected set; }
        private static readonly ILog Log = new ILog();// LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        /// <summary>
        /// Содержит состояние
        /// </summary>
        /// <param name="stateName"></param>
        /// <returns></returns>
        public bool ContainsState(string stateName)
        {
            return _states.ContainsKey(stateName);
        }
        
        /// <summary>
        /// Добавить новые состояния
        /// </summary>
        /// <param name="states"></param>
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
                    Log.Error(string.Format("FSMState {0} doesn't have attribute State", state.GetType().Name));
            }
        }
        
        /// <summary>
        /// Заменить состояния на новые
        /// </summary>
        /// <param name="states"></param>
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
                    Log.Error(string.Format("FSMState {0} doesn't have attribute State", state.GetType().Name));
            }
        }

        /// <summary>
        /// Добавить новое состояние
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stateName"></param>
        public void Add(IState state, string stateName)
        {
            if (_states.ContainsKey(stateName))
            {
                Log.Warn(string.Format("Dublicate state with name {0}. Use method Replace", stateName));
                _states.Remove(stateName);
            }
            state.Parent = this;
            _states.Add(stateName, state);
        }
        

        /// <summary>
        /// Заменить состояние на новое
        /// </summary>
        /// <param name="state"></param>
        /// <param name="stateName"></param>

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

        /// <summary>
        /// Перейти из одного состояния в другое
        /// </summary>
        /// <param name="name"></param>
        public void Change(string name)
        {
            if (ShowFsmExitState) Log.Debug(string.Format("--- EXIT {0} ---", CurrentStateName));
            if (currentState != null)
                currentState.__ExitState();
            
            if (!_states.ContainsKey(name))
                throw new Exception("Key " + name + " not found!");
            
            currentState = _states[name];
            CurrentStateName = name;
            if (ShowFsmEnterState) Log.Debug(string.Format("--- ENTER {0} ---", CurrentStateName));
            currentState.__EnterState(); 
        }

        /// <summary>
        /// Update loop
        /// </summary>
        /// <param name="deltaTime"></param>
        public void Update(float deltaTime)
        {
            if (currentState != null) currentState.__UpdateState(deltaTime);
        }

        /// <summary>
        /// События
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public void Invoke(string eventName, params object[] args)
        {
            if (currentState != null) currentState.__Invoke(eventName, args);
        }

        public void Dispose()
        {
            foreach (var kv in _states)
                kv.Value.Dispose();
            _states.Clear();
        }
    }
}