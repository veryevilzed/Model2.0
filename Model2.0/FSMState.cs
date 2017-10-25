using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TinyLima.Tools
{
    /// <summary>
    /// Состояние
    /// </summary>
    public abstract class FSMState : IState
    {


        private AsyncEventManager asm;
        
        /// <summary>
        /// МетодШивокер
        /// </summary>
        private class MethodInfoObject
        {
            public MethodInfo Method { get; set; }
            public object Target { get; set; }
            public int Priority { get; set; }
            public void Invoke(params object[] args)
            {
                if (Target == null) return;
                try
                {
                    Method.Invoke(Target, args);
                }
                catch (Exception e)
                {
                    Exception thr = e;
                    while (thr.InnerException != null)
                        thr = thr.InnerException;
                    LogCallback.Error(thr);
                    throw thr;
                }
            }

            public override string ToString()
            {
                return $"{Target.GetType().Name}.{Method.Name}";
            }

            public override bool Equals(object obj)
            {
                if (obj == null || obj.GetType() != typeof(MethodInfoObject))
                    return false;
                return Equals((MethodInfoObject) obj);
            }

            protected bool Equals(MethodInfoObject other)
            {
                return Equals(Method, other.Method) && Equals(Target, other.Target);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Method != null ? Method.GetHashCode() : 0) * 397) ^ (Target != null ? Target.GetHashCode() : 0);
                }
            }
        }
        
        private readonly List<MethodInfoObject> enters;
        private readonly List<MethodInfoObject> exits;
        private readonly List<MethodInfoObject> updates;
        
        public FSM Parent { get; set; }
        
        
        
        protected FSMState()
        {
            asm = new AsyncEventManager();
            enters = new List<MethodInfoObject>();
            exits = new List<MethodInfoObject>();
            updates = new List<MethodInfoObject>();
            
            foreach (var methodInfo in GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var o in methodInfo.GetCustomAttributes(false))
                {
                    switch (o)
                    {
                        case Enter enter:
                            enters.Add(new MethodInfoObject {Method = methodInfo, Target = this, Priority = enter.Priority});
                            break;
                        case Exit exit:
                            exits.Add(new MethodInfoObject {Method = methodInfo, Target = this, Priority = exit.Priority});
                            break;
                        case Update update:
                            updates.Add(new MethodInfoObject {Method = methodInfo, Target = this, Priority = update.Priority});
                            break;
                    }
                }
            }

            enters = enters.OrderBy(i => i.Priority).ToList();
            exits = exits.OrderBy(i => i.Priority).ToList();
            updates = updates.OrderBy(i => i.Priority).ToList();
            asm.Add(this);
        }

        public void __Invoke(string eventName, object[] args) => asm.Invoke(eventName, args); 
        
        public void Dispose()
        {
            asm.Dispose();
            enters.Clear();
            exits.Clear();
            updates.Clear();
        }
        
        public void __EnterState()
        {
            foreach (var m in enters)
                m.Invoke();
        }
        
        public void __ExitState()
        {
            foreach (var m in exits)
                m.Invoke();
        }

        public void __UpdateState(float deltaTime)
        {
            foreach (var m in updates)
                m.Invoke(deltaTime);
        }

    }
    
    [AttributeUsage(AttributeTargets.Class) ]
    public class State : Attribute
    {
        public string Name { get; }
        
        public State(string name)
        {
            Name = name;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method) ]
    public class Enter : Attribute
    {
        public int Priority { get; }
        
        public Enter(){}

        public Enter(int priority)
        {
            Priority = priority;
        }
    }
    
    
    [AttributeUsage(AttributeTargets.Method) ]
    public class Exit : Attribute
    {
        public int Priority { get; }
        
        public Exit(){}

        public Exit(int priority)
        {
            Priority = priority;
        }
    }
    
    [AttributeUsage(AttributeTargets.Method) ]
    public class Update : Attribute
    {
        public int Priority { get; }
        
        public Update(){}

        public Update(int priority)
        {
            Priority = priority;
        }
    }


    public interface IState : IDisposable
    {
        void __EnterState();
        void __ExitState();
        void __UpdateState(float deltaTime);
        FSM Parent { get; set; }
        void __Invoke(string eventName, object[] args);
    }
    
}