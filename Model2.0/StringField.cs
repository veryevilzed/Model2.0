using System.Linq;

namespace TinyLima.Tools
{
    public class StringField : Field<string>
    {
        public void Add(string text) => Value += text;
        public void RemoveLast() => RemoveLast(1);
        public void RemoveLast(int count) => Value.Substring(0, Value.Length - System.Math.Min(count, Value.Length));
        public int Count => Value.Length;
        public void Trim() => Value.Trim();
        public StringField(string value) : base(value) {}
        public StringField() : base() {}
        
    }
}