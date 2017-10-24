using System;
using System.Reflection;
using System.Linq;

namespace TinyLima.Tools{
    public abstract class Model
    {
        private AsyncEventManager EventManager { get; }

        private void Change(IField field, object value, object before)
        {
            EventManager.InvokeAsync($"On{field.Name}Changed", value, before, this);
            EventManager.InvokeAsync("ModelChanged", field.Name, value, before, this);
        }
        
        public void ExecuteAsync(int count = int.MaxValue)
        {
            EventManager.ExecuteAsync(count);
        }

        ~Model()
        {
            EventManager.ClearAll(this);
        }
        
        protected Model()
        {
            EventManager = new AsyncEventManager();
            
            foreach (var fieldInfo in GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!fieldInfo.FieldType.GetInterfaces().Contains(typeof(IField))) continue;
                
                var changableObject = (IField) fieldInfo.GetValue(this);
                if (changableObject == null)
                {
                    changableObject = (IField) Activator.CreateInstance(fieldInfo.FieldType);
                    fieldInfo.SetValue(this, changableObject);
                }
                changableObject.OnChange = Change;
                changableObject.Name = fieldInfo.Name;
                
                if (fieldInfo.FieldType.GetInterfaces().Contains(typeof(ILinkedField)))
                {
                    var linkedObject = (ILinkedField)changableObject;
                    linkedObject.Link(EventManager);
                }
            }
            
            
            EventManager.Add(this);


            foreach (var fieldInfo in GetType()
                .GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                if (!fieldInfo.FieldType.GetInterfaces().Contains(typeof(IField))) continue;
                var changableObject = (IField) fieldInfo.GetValue(this);
                changableObject.Refresh();
            }

        }
        
    }
}