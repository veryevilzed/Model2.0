using System;

namespace TinyLima.Tools
{
    public class IntegerField : Field<int>
    {
        public void Inc() => Inc(1);
        public void Inc(int value) => Set(Get() + value);
        public void Inc(IntegerField value) => Set(Get() + value.Value);
        public void Dec() => Dec(1);
        public void Dec(int value) => Set(Get() - value);
        public void Dec(IntegerField value) => Set(Get() - value.Value);
        public IntegerField(int value) : base(value) {}
    }
}