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
        private readonly Dictionary<string, Timing> timersDictionary;
        private readonly List<Timing> timers;
        
        public FSM Parent { get; set; }
        
        
        
        protected FSMState()
        {
            asm = new AsyncEventManager();
            enters = new List<MethodInfoObject>();
            exits = new List<MethodInfoObject>();
            updates = new List<MethodInfoObject>();
            timersDictionary = new Dictionary<string, Timing>();
            
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
                        case Loop loop:
                            timersDictionary.Add(loop.Name ?? methodInfo.Name, loop);
                            loop.Method = methodInfo;
                            loop.Target = this;
                            break;
                        case One one:
                            timersDictionary.Add(one.Name ?? methodInfo.Name, one);
                            one.Method = methodInfo;
                            one.Target = this;
                            break;           
                    }
                }
            }
            
            enters = enters.OrderBy(i => i.Priority).ToList();
            exits = exits.OrderBy(i => i.Priority).ToList();
            updates = updates.OrderBy(i => i.Priority).ToList();
            timers = timersDictionary.Select(i => i.Value).OrderBy(i => i.Priority).ToList();
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
            timers.ForEach(t => t.Reset());
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
            Console.WriteLine("Update timers:{0}", timers.Count);
            foreach (var t in timers)
                t.Update(deltaTime);
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

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class One : Timing
    {
        public One(float time) : this(time, null){ }
        
        public One(float time, string name) : this(time, name, 0){}
        
        public One(float time, string name, int priority) : base(time, 0, false){ 
            Name = name;
            Priority = priority;
        }
    }
    

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class Loop : Timing
    {
        public Loop(float time) : this(time, 0){ }
        
        public Loop(float time, float startTime) : this(time, startTime, null, 0){}
        
        public Loop(float time, float startTime, string name) : this(time, startTime, name, 0){}
        
        public Loop(float time, float startTime, string name, int priority) : base(time, startTime, false){ 
            Name = name;
            Priority = priority;
        }
    }

    
    /// <summary>
    /// Таймер
    /// </summary>
    public class Timing : Attribute
    {
        private float _currentTime = 0.0f;
        private float _resetTime = 0.0f;
        public bool Loop { get; }
        public bool Enable { get; protected set; }
        public object Target { get; set; }
        public MethodInfo Method { get; set; }
        public object[] Args = new object[0];
        public int Priority = 0;
        public string Name { get; set; }

        public void Reset()
        {
            _currentTime = _resetTime;
            Enable = true;
            Console.WriteLine("Timer reset!");
        }

        public void Stop()
        {
            Enable = false;
        }

        public void Update(float deltaTime)
        {
            if (!Enable)
                return;
            _currentTime -= deltaTime;
            if (_currentTime > 0)
                return;
            
            try
            {
                Console.WriteLine("M:{0} T:{1}", Method == null, Target == null);
                Method.Invoke(Target, Args);
            }
            catch (TargetInvocationException e)
            {
                Exception thr = e;
                while (thr.InnerException != null)
                    thr = thr.InnerException;
                LogCallback.Error(thr);
            }

            if (Loop)
                _currentTime += _resetTime;
            else
                Enable = false;
        }

        public Timing(float currentTime, float resetTime, bool loop)
        {
            _currentTime = currentTime;
            _resetTime = resetTime;
            Loop = loop;
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