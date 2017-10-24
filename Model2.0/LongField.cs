namespace TinyLima.Tools
{
    public class LongField : Field<long>
    {
        public void Inc() => Inc(1);
        public void Inc(long value) => Set(Get() + value);
        public void Inc(LongField value) => Set(Get() + value.Value);
        public void Dec() => Dec(1);
        public void Dec(long value) => Set(Get() - value);
        public void Dec(LongField value) => Set(Get() - value.Value);
        public LongField(long value) : base(value) {}
        
        
    }
}