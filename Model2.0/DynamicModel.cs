﻿using System;
using System.Collections;
using System.Collections.Generic;
using SmartFormat;
using System.Linq;

namespace TinyLima.Tools
{
    /// <summary>
    /// Модель основанная на дикте с динамичискими полями
    /// </summary>
    public abstract class DynamicModel : IDictionary<string, object>, IDisposable
    {
        private readonly Dictionary<string, object> dataObject;
        private readonly AsyncEventManager aem;

        public AsyncEventManager EventManager => aem;

        public void Dispose()
        {
            aem.Dispose();
            dataObject.Clear();
        }
        
        public void Add(string key, object value) => Set(key, value);

        public bool ContainsKey(string key) => dataObject.ContainsKey(key);

        public bool Remove(string key)
        {
            if (dataObject.ContainsKey(key))
            {
                var before = dataObject[key];
                dataObject.Remove(key);
                SendEvents(key, null, before);
                return true;
            } 
            return false;
        }

        public bool TryGetValue(string key, out object value) => dataObject.TryGetValue(key, out value);

        public object this[string name]
        {
            get => dataObject[name];
            set => Set(name, value);
        }

        public ICollection<string> Keys => dataObject.Keys;
        public ICollection<object> Values => dataObject.Values;

        protected virtual void SendEvents(string name, object value, object before)
        {
            aem.Invoke(Smart.Format("On{0}Changed", name), value, before, this);
            aem.Invoke("ModelChanged", name, this);
        }
        
        protected virtual void SendEventsAsync(string name, object value, object before)
        {
            aem.InvokeAsync(Smart.Format("On{0}Changed", name), value, before, this);
            aem.InvokeAsync("ModelChanged", name, this);
        }
        
        public void Set(string name, object obj)
        {
            
            if (dataObject.ContainsKey(name))
            {
                if (dataObject[name] != obj)
                {
                    var before = dataObject[name]; 
                    dataObject[name] = obj;
                    SendEvents(name, obj, before);
                }
            }
            else
            {
                dataObject.Add(name, obj);
                SendEvents(name, obj, null);
            }
        }
        
        
        public void SetAsync(string name, object obj)
        {
            
            if (dataObject.ContainsKey(name))
            {
                if (dataObject[name] != obj)
                {
                    var before = dataObject[name]; 
                    dataObject[name] = obj;
                    SendEventsAsync(name, obj, before);
                }
            }
            else
            {
                dataObject.Add(name, obj);
                SendEventsAsync(name, obj, null);
            }
        }

        public int GetInt(string name, int def = default(int))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return Convert.ToInt32(dataObject[name]);
        }

        
        public bool GetBool(string name, bool def = default(bool))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return Convert.ToBoolean(dataObject[name]);
        }
        
        public double GetDouble(string name, double def = default(double))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return Convert.ToDouble(dataObject[name]);
        }
        
         
        public float GetFloat(string name, float def = default(float))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return (float)dataObject[name];
        }
        
        public long GetLong(string name, long def = default(long))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return Convert.ToInt64(dataObject[name]);
        }

        public string GetString(string name, string def = default(string))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null || dataObject[name].ToString() == "")
                return def;
            return Convert.ToString(dataObject[name]);
        }

        public T Get<T>(string name, T def = default(T))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return (T)dataObject[name];
        }
        
        public List<T> GetList<T>(string name, List<T> def = default(List<T>))
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
                return def;
            return (List<T>)dataObject[name];
        }
        
        public void Inc(string name, int value = 1)
        {
            if (!dataObject.ContainsKey(name) || dataObject[name] == null)
            {
                Set(name, value);
            }
            else
            {
                var o = dataObject[name];
                if (o is long)
                    Set(name, (long) o+ value);
                if (o is int)
                    Set(name, (int) o+ value);
            }
        }

        public void Dec(string name, int value = 1) => Inc(name, -value);
        
        public DynamicModel(): this( new Dictionary<string, object>()) {}
        public DynamicModel(Dictionary<string, object> construct)
        {
            dataObject = construct;
            aem = new AsyncEventManager();
            aem.Add(this);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator() => dataObject.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => dataObject.GetEnumerator();

        public void Add(KeyValuePair<string, object> item) => Set(item.Key, item.Value);

        public void Clear() => dataObject.Clear();

        public bool Contains(KeyValuePair<string, object> item) => dataObject.Contains(item);

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
        }

        public bool Remove(KeyValuePair<string, object> item) => Remove(item.Key);

        public int Count => dataObject.Count;
        public bool IsReadOnly => false;
    }
}