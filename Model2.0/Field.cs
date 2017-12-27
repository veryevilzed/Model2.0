using System.Runtime.InteropServices.ComTypes;

namespace TinyLima.Tools
{

    
    public delegate void DFieldChange(IField field, object value, object before);
    public class Field<T> : IField
    {
        private T obj;
        public T Get()
        {
            return obj;
        }

        public T Get(T def)
        {
            return obj == null ? def : obj;
        }

        public object AsObject
        {
            get { return Get(); }
        }

        public string Name { get; set; }
        
        public DFieldChange OnChange { get; set; }
        
        public void Set(T item)
        {
            T before = obj;
            obj = item;
            if (OnChange != null) OnChange.Invoke(this, obj, before);
        }

        public void Refresh()
        {
            if (obj != null)
                if (OnChange != null) OnChange.Invoke(this, obj, obj);
        }

        public void SilentSet(T item)
        {
            obj = item;
        }
        
        public T Value
        {
            get { return Get(); }
            set { Set(value); }
        }

        public static bool operator ==(Field<T> a, Field<T> b)
        {
            if (ReferenceEquals(a, b))
                return true;
            return a.Equals(b);
        }

        public static bool operator !=(Field<T> a, Field<T> b)
        {
            return !(a == b);
        }


        public Field(T value)
        {
            obj = value;
        }

        public Field()
        {
            obj = default(T);
        }

        public override bool Equals(object obj)
        {
            
            if (obj == null || !typeof(IField).IsAssignableFrom(obj.GetType()))
                return false;

            return Equals((IField)obj);
        }

        protected bool Equals(IField other)
        {
            if (AsObject == null)
                return other.AsObject == null;
            
            if (other.AsObject == null)
                return AsObject == null;
            
            return AsObject.Equals(other.AsObject) ||
                   other.AsObject.Equals(AsObject) || AsObject.GetHashCode() == other.AsObject.GetHashCode();
        }

        public override int GetHashCode()
        {
            return obj.GetHashCode();
        }
        
        public string Format(string message)
        {
            return string.Format(message, Get());
        }
    }

    public interface IField
    {
        object AsObject { get; }
        string Name { get; set; }
        void Refresh();
        DFieldChange OnChange { get; set; }
    }
}