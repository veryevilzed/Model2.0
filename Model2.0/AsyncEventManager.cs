﻿using System;
using System.Collections.Generic;
using System.Reflection;
using SmartFormat;

namespace Novolot.Tools
{

    public delegate void DEventMethod<in T>(T a);
    public delegate void DEventMethod(params object[] array);
    public delegate void DEventMethod<in A,in B>(A a, B b);
    public delegate void DEventMethod<in A,in B, in C>(A a, B b, C c);
    public delegate void DEventMethod<in A,in B, in C, in D>(A a, B b, C c, D d);
    public delegate void DEventMethod<in A,in B, in C, in D,in E>(A a, B b, C c, D d, E e);
    public delegate void DEventMethod<in A,in B, in C, in D,in E, in F>(A a, B b, C c, D d, E e, F f);
    
    /// <summary>
    /// Событийный менеджер для Модели данных
    /// </summary>
    public class AsyncEventManager : IDisposable
    {
        
        private static readonly ILog Log = new ILog();
        
        /// <summary>
        /// Класс хранения объекта и метода
        /// </summary>
        private class MethodInfoObject
        {
            public MethodInfo Method { get; set; }
            public object Target { get; set; }
            public string EventName { get; set; }
            
            public void Invoke(object[] args)
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
                    Log.Error(thr);
                    throw thr;
                }
            }

            public override string ToString()
            {
                return $"{Target.GetType().Name}.{Method.Name} for {EventName}";
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
            float Delay { get; set; }
        }

        public void Dispose()
        {
            _eventListeners.Clear();
            
        }
        
        /// <summary>
        /// Класс хранения вызова объектов
        /// </summary>
        private class MethodInvocationObject : IMethodInvocationObject
        {
            public MethodInfoObject EventObject { get; set; }
            public object[] Send { get; set; }
            public float Delay { get; set; }
            public void Invoke()
            {
                EventObject.Invoke(Send);
            }
        }

        Queue<IMethodInvocationObject> _asyncQueue = new Queue<IMethodInvocationObject>();
        public Queue<IMethodInvocationObject> AsyncQueue { get { return _asyncQueue;  } }
        readonly Dictionary<string, List<MethodInfoObject>> _eventListeners = new Dictionary<string, List<MethodInfoObject>>();
        

        /// <summary>
        /// Выполнить набор заданий из очереди
        /// </summary>
        /// <param name="count">Количество</param>
        /// <param name="deltaTime">Время между обновлениями для отложенных Task</param>
        public void ExecuteAsync(int count = int.MaxValue, float deltaTime = 1f)
        {
            count = Math.Min(count, AsyncQueue.Count);
            for (var i = 0; i < count; i++)
            {
                try
                {
                    
                    IMethodInvocationObject invokationObject = AsyncQueue.Dequeue();
                    if (invokationObject.Delay <= 0)
                        invokationObject.Invoke();
                    else
                    {
                        invokationObject.Delay -= deltaTime;
                        if (invokationObject.Delay <= 0)
                            invokationObject.Invoke();
                        else
                            AsyncQueue.Enqueue(invokationObject);
                    }

                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }
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
                    if (eventName.Contains("{"))
                        eventName = Smart.Format(eventName, obj);
                    
                    if (!_eventListeners.ContainsKey(eventName))
                        _eventListeners.Add(eventName, new List<MethodInfoObject>());
                    var mio = new MethodInfoObject {Target = obj, Method = methodInfo };
                    if (!_eventListeners[eventName].Contains(mio))
                        _eventListeners[eventName].Add(mio);
                }
            }
        }

        private void Add(MethodInfoObject mio, string newEventName = "")
        {
            string eventName = newEventName != "" ? newEventName : mio.Method.Name;
            
            if (mio.Method.GetCustomAttributes(typeof(Event), true).Length > 0)
            {
                foreach (var attr in mio.Method.GetCustomAttributes(typeof(Event), true))
                {
                    if (attr.GetType() != typeof(Event)) continue;
                    var e = (Event) attr;
                    eventName = newEventName != "" ? newEventName : e.EventName ?? mio.Method.Name;
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

        private void Remove(MethodInfoObject mio, string newEventName="")
        {
            string eventName = newEventName != "" ? newEventName : mio.Method.Name;
            if (mio.Method.GetCustomAttributes(typeof(Event), true).Length > 0)
            {
                foreach (var attr in mio.Method.GetCustomAttributes(typeof(Event), true))
                {
                    if (attr.GetType() != typeof(Event)) continue;
                    var e = (Event) attr;
                    eventName = newEventName != "" ? newEventName : e.EventName ?? mio.Method.Name;
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
        
        public void AddAction(string eventName ,Action method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction(DEventMethod method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void AddAction(string eventName, DEventMethod method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<T>(DEventMethod<T> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void AddAction<T>(string eventName, DEventMethod<T> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B>(DEventMethod<A,B> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void AddAction<A,B>(string eventName, DEventMethod<A,B> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C>(DEventMethod<A,B,C> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void AddAction<A,B,C>(string eventName, DEventMethod<A,B,C> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C,D>(DEventMethod<A,B,C,D> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void AddAction<A,B,C,D>(string eventName, DEventMethod<A,B,C,D> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }


        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C,D,E>(DEventMethod<A,B,C,D,E> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void AddAction<A,B,C,D,E>(string eventName, DEventMethod<A,B,C,D,E> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        
        /// <summary>
        /// Добавить кастомный метод
        /// </summary>
        /// <param name="method">Слушатель</param>
        public void AddAction<A,B,C,D,E,F>(DEventMethod<A,B,C,D,E,F> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        
        public void AddAction<A,B,C,D,E,F>(string eventName, DEventMethod<A,B,C,D,E,F> method)
        {
            Add(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        #endregion
        
        #region RemoveAction
        
        public void RemoveAction(Action method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction(string eventName, Action method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        public void RemoveAction(DEventMethod method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction(string eventName, DEventMethod method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        public void RemoveAction<T>(DEventMethod<T> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction<T>(string eventName, DEventMethod<T> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        
        public void RemoveAction<A,B>(DEventMethod<A,B> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        
        public void RemoveAction<A,B>(string eventName, DEventMethod<A,B> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        public void RemoveAction<A,B,C>(DEventMethod<A,B,C> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction<A,B,C>(string eventName, DEventMethod<A,B,C> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        public void RemoveAction<A,B,C,D>(DEventMethod<A,B,C,D> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }

        public void RemoveAction<A,B,C,D>(string eventName, DEventMethod<A,B,C,D> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }

        
        public void RemoveAction<A,B,C,D,E>(DEventMethod<A,B,C,D,E> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction<A,B,C,D,E>(string eventName, DEventMethod<A,B,C,D,E> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
        }
        
        public void RemoveAction<A,B,C,D,E,F>(DEventMethod<A,B,C,D,E,F> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method});
        }
        
        public void RemoveAction<A,B,C,D,E,F>(string eventName, DEventMethod<A,B,C,D,E,F> method)
        {
            Remove(new MethodInfoObject {Target = method.Target, Method = method.Method}, eventName);
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
                    var eventName = e.EventName ?? methodInfo.Name ;
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
                    //TODO Params Objects
                    if (e.Method.GetParameters().Length == 1
                        && e.Method.GetParameters()[0].GetCustomAttributes(typeof(ParamArrayAttribute), false).Length > 0
                        && e.Method.GetParameters()[0].ParameterType == typeof(object[]))
                    {
                        e.Invoke(new object[] {args});
                        return;
                    }
                    
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


        private object[] ProcessSend(MethodInfoObject e, object[] args)
        {
            var send = new object[e.Method.GetParameters().Length];
            for (var i = 0; i < Math.Min(args.Length, e.Method.GetParameters().Length); i++)
            {
                if (e.Method.GetParameters()[i].ParameterType == args[i].GetType())
                    send[i] = args[i];
                else
                {
                    //конвертируемые типы
                    if (e.Method.GetParameters()[i].ParameterType == typeof(object))
                        send[i] = args[i];
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
                                send[i] = (float) args[i];
                            }
                            catch (InvalidCastException)
                            {
                            }
                    if (e.Method.GetParameters()[i].ParameterType == typeof(string))
                        send[i] = args[i] == null ? "" : args[i].ToString();
                }
            }
            return send;
        }
        
        /// <summary>
        /// Положить вызовы в коллекцию для последующего вызова
        /// </summary>
        /// <param name="eventName">Имя события</param>
        /// <param name="args">Аргументы</param>
        public void InvokeAsync(string eventName, params object[] args)
        {
            InvokeDelayAsync(0f, eventName, args);
        }


        /// <summary>
        /// Вызвать событие с задержкой
        /// </summary>
        /// <param name="delay">Задержка</param>
        /// <param name="eventName">Событие</param>
        /// <param name="args">Аргументы</param>
        public void InvokeDelayAsync(float delay, string eventName, params object[] args)
        {
            
            if (_eventListeners.ContainsKey(eventName))
                _eventListeners[eventName].ForEach(e =>
                    AsyncQueue.Enqueue(new MethodInvocationObject{
                            EventObject = e,
                            Send = ProcessSend(e, args),
                            Delay = delay
                    })
                );
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


        /// <summary>
        /// Обновить таймер для отложенных задач
        /// </summary>
        /// <param name="timeDeltaTime"></param>
        public void Update(float timeDeltaTime)
        {
            ExecuteAsync(deltaTime: timeDeltaTime);
        }
    }
    
    
    /// <summary>
    /// Пометить метод для вызова события
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited = true)]
    public class Event : Attribute
    {
        public string EventName { get; protected set; }
        
        public string[] Except { get; protected set; }
        
        /// <summary>
        /// Имя события будет имя метода маленькими буквами
        /// </summary>
        public Event() {}

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="eventName">Имя события</param>
        public Event(string eventName=null)
        {
            EventName = eventName;
        }
        
        
        public Event(string eventName=null, string[] except = null)
        {
            EventName = eventName;
            Except = except;
        }
    }
}