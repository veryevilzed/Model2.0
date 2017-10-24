using System;
using System.Collections.Generic;
using System.Reflection;

namespace TinyLima.Tools
{

    public delegate void DEventMethod<in T>(T a);
    public delegate void DEventMethod<in A,in B>(A a, B b);
    public delegate void DEventMethod<in A,in B, in C>(A a, B b, C c);
    public delegate void DEventMethod<in A,in B, in C, in D>(A a, B b, C c, D d);
    public delegate void DEventMethod<in A,in B, in C, in D,in E>(A a, B b, C c, D d, E e);
    public delegate void DEventMethod<in A,in B, in C, in D,in E, in F>(A a, B b, C c, D d, E e, F f);
    
    /// <summary>
    /// Событийный менеджер для Модели данных
    /// </summary>
    public class AsyncEventManager
    {
        /// <summary>
        /// Класс хранения объекта и метода
        /// </summary>
        private class MethodInfoObject
        {
            public MethodInfo Method { get; set; }
            public object Target { get; set; }
            
            public void Invoke(object[] args)
            {
                if (Target == null) return;
                try
                {
                    Method.Invoke(Target, args);
                }
                catch (Exception e)
                {
                    throw new Exception($"Invoke {Method.Name} on {Target.GetType().Name} Exception {e.Message}");
                }
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

        public interface IMethodInvocationObject
        {
            void Invoke();
        }
        
        /// <summary>
        /// Класс хранения вызова объектов
        /// </summary>
        private class MethodInvocationObject : IMethodInvocationObject
        {
            public MethodInfoObject EventObject { get; set; }
            public object[] Send { get; set; }
            public void Invoke()
            {
                EventObject.Invoke(Send);
            }
        }

        public Queue<IMethodInvocationObject> AsyncQueue { get; } = new Queue<IMethodInvocationObject>();
        readonly Dictionary<string, List<MethodInfoObject>> _eventListeners = new Dictionary<string, List<MethodInfoObject>>();


        /// <summary>
        /// Выполнить набор заданий из очереди
        /// </summary>
        /// <param name="count">Количество</param>
        public void ExecuteAsync(int count = int.MaxValue)
        {
            count = Math.Min(count, AsyncQueue.Count);
            for (var i = 0; i < count; i++)
                AsyncQueue.Dequeue().Invoke();
        }
        
        /// <summary>
        /// Добавить все подписанные методы объекта
        /// </summary>
        /// <param name="obj">Объект</param>
        public void Add(object obj)
        {
            foreach (var methodInfo in obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach(var attr in methodInfo.GetCustomAttributes(typeof(Event), true))
                {
                    if (attr.GetType() != typeof(Event)) continue;
                    var e = (Event) attr;
                    var eventName = e.EventName ?? methodInfo.Name;
                    if (!_eventListeners.ContainsKey(eventName))
                        _eventListeners.Add(eventName, new List<MethodInfoObject>());
                    var mio = new MethodInfoObject {Target = obj, Method = methodInfo };
                    if (!_eventListeners[eventName].Contains(mio))
                        _eventListeners[eventName].Add(mio);
                }
            }
        }

        private void Add(MethodInfoObject mio)
        {
            string eventName = mio.Method.Name;
            if (mio.Method.GetCustomAttributes(typeof(Event), true).Length > 0)
            {
                foreach (var attr in mio.Method.GetCustomAttributes(typeof(Event), true))
                {
                    if (attr.GetType() != typeof(Event)) continue;
                    var e = (Event) attr;
                    eventName = e.EventName ?? mio.Method.Name;
                    if (!_eventListeners.ContainsKey(eventName))
                        _eventListeners.Add(eventName, new List<MethodInfoObject>());
                    if (!_eventListeners[eventName].Contains(mio))
                        _eventListeners[eventName].Add(mio);
                }
            }
            else
            {
                if (!_eventListeners.ContainsKey(eventName))
                    _eventListeners.Add(eventName, new List<MethodInfoObject>());
                if (!_eventListeners[eventName].Contains(mio))
                    _eventListeners[eventName].Add(mio);
            }
        }

        private void Remove(MethodInfoObject mio)
        {
            string eventName = mio.Method.Name;
            if (mio.Method.GetCustomAttributes(typeof(Event), true).Length > 0)
            {
                foreach (var attr in mio.Method.GetCustomAttributes(typeof(Event), true))
                {
                    if (attr.GetType() != typeof(Event)) continue;
                    var e = (Event) attr;
                    eventName = e.EventName ?? mio.Method.Name;
                    if (_eventListeners.ContainsKey(eventName))
                        if (_eventListeners[eventName].Remove(mio))
                            if (_eventListeners[eventName].Count == 0)
                                _eventListeners.Remove(eventName);
                }
            }
            else
            {
                if (_eventListeners.ContainsKey(eventName))
                    if (_eventListeners[eventName].Remove(mio))
                        if (_eventListeners[eventName].Count == 0)
                            _eventListeners.Remove(eventName);
            }

        }
        
        #region AddAction
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction(Action method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<T>(DEventMethod<T> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B>(DEventMethod<A,B> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C>(DEventMethod<A,B,C> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C,D>(DEventMethod<A,B,C,D> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }


        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C,D,E>(DEventMethod<A,B,C,D,E> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C,D,E,F>(DEventMethod<A,B,C,D,E,F> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        #endregion
        
        #region RemoveAction
        
        public void RemoveAction(Action method)
        {
            Remove(method);
        }

        public void RemoveAction<T>(DEventMethod<T> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction<A,B>(DEventMethod<A,B> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void RemoveAction<A,B,C>(DEventMethod<A,B,C> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void RemoveAction<A,B,C,D>(DEventMethod<A,B,C,D> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void RemoveAction<A,B,C,D,E>(DEventMethod<A,B,C,D,E> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction<A,B,C,D,E,F>(DEventMethod<A,B,C,D,E,F> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        #endregion
        
        /// <summary>
        /// Удалить все подписанные события
        /// </summary>
        /// <param name="obj">Объект</param>
        public void Remove(object obj)
        {
            foreach (var methodInfo in obj.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                foreach (var attr in methodInfo.GetCustomAttributes(typeof(Event), true))
                {
                    if (attr.GetType() != typeof(Event)) continue;
                    var e = (Event) attr;
                    var eventName = e.EventName ?? methodInfo.Name;
                    var mio = new MethodInfoObject {Target = obj, Method = methodInfo };
                    if (_eventListeners.ContainsKey(eventName))
                        if (_eventListeners[eventName].Remove(mio))
                            if (_eventListeners[eventName].Count == 0)
                                _eventListeners.Remove(eventName);
                }
            }
        }

        /// <summary>
        /// Вызвать события
        /// </summary>
        /// <param name="eventName"></param>
        /// <param name="args"></param>
        public void Invoke(string eventName, params object[] args)
        {
            if (_eventListeners.ContainsKey(eventName))
                _eventListeners[eventName].ForEach(e =>
                {
                    var send = new object[e.Method.GetParameters().Length];
                    for (var i = 0; i < Math.Min(args.Length, e.Method.GetParameters().Length);i++)
                        if (e.Method.GetParameters()[i].ParameterType == args[i].GetType())
                            send[i] = args[i];
                        else
                        {
                            //конвертируемые типы
                            if (e.Method.GetParameters()[i].ParameterType == typeof(int))
                                if (args[i] is byte || args[i] is long || args[i] is double)
                                    send[i] = Convert.ToInt32(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(long))
                                if (args[i] is byte || args[i] is int || args[i] is double)
                                    send[i] = Convert.ToInt64(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(byte))
                                if (args[i] is int || args[i] is long || args[i] is double)
                                    send[i] = Convert.ToByte(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(double))
                                if (args[i] is byte || args[i] is long || args[i] is int || args[i] is float)
                                    send[i] = Convert.ToDouble(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(float))
                                if (args[i] is double)
                                    try
                                    {
                                        send[i] = (float)args[i];
                                    }
                                    catch (InvalidCastException){}
                            if (e.Method.GetParameters()[i].ParameterType == typeof(string))
                                send[i] = args[i] == null ? "" : args[i].ToString();
                        }
                    e.Invoke(send);
                });
        }

        /// <summary>
        /// Положить вызовы в коллекцию для последующего вызова
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <param name="args">Аргументы</param>
        public void InvokeAsync(string eventName, params object[] args)
        {
            if (_eventListeners.ContainsKey(eventName))
                _eventListeners[eventName].ForEach(e => 
                {
                    var send = new object[e.Method.GetParameters().Length];
                    for (var i = 0; i < Math.Min(args.Length, e.Method.GetParameters().Length);i++)
                        if (e.Method.GetParameters()[i].ParameterType == args[i].GetType())
                            send[i] = args[i];
                        else
                        {
                            //конвертируемые типы
                            if (e.Method.GetParameters()[i].ParameterType == typeof(int))
                                if (args[i] is byte || args[i] is long || args[i] is double)
                                    send[i] = Convert.ToInt32(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(long))
                                if (args[i] is byte || args[i] is int || args[i] is double)
                                    send[i] = Convert.ToInt64(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(byte))
                                if (args[i] is int || args[i] is long || args[i] is double)
                                    send[i] = Convert.ToByte(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(double))
                                if (args[i] is byte || args[i] is long || args[i] is int || args[i] is float)
                                    send[i] = Convert.ToDouble(args[i]);
                            if (e.Method.GetParameters()[i].ParameterType == typeof(float))
                                if (args[i] is double)
                                    try
                                    {
                                        send[i] = (float)args[i];
                                    }
                                    catch (InvalidCastException)
                                    {
                                    }
                            if (e.Method.GetParameters()[i].ParameterType == typeof(string))
                                send[i] = args[i] == null ? "" : args[i].ToString();
                        }
                    AsyncQueue.Enqueue(new MethodInvocationObject {EventObject = e, Send = send});
                });
        }
        
        /// <summary>
        /// Очистить все события
        /// </summary>
        /// <param name="obj"></param>
        public void ClearAll(object obj)
        {
            _eventListeners.Clear();
            AsyncQueue.Clear();
        }
    }
    
    
    /// <summary>
    /// Пометить метод для вызова события
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple=true) ]
    public class Event : System.Attribute
    {
        public string EventName { get; }
        
        /// <summary>
        /// Имя события будет имя метода маленькими буквами
        /// </summary>
        public Event(){}

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="eventName">Имя события</param>
        public Event(string eventName)
        {
            EventName = eventName;
        }
    }
}