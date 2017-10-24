namespace TinyLima.Tools
{
    public delegate void DFormatField(FormatedStringField field, string value); 
    public class FormatedStringField : StringField
    {
        public FormatedStringField(string value) : base(value)
        {
        }
    }
}