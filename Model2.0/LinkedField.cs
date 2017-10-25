using System;
using System.Runtime.CompilerServices;

namespace TinyLima.Tools
{

    public delegate T DFormating<out T, in TS>(TS value);
    public delegate T DFormating<out T, in TSA, in TSB>(TSA valueA, TSB valueB);
    
    /// <summary>
    /// Поле которое устанавлиает связь с событием
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TS"></typeparam> 
    public class LinkedField<T, TS> : Field<T>, ILinkedField
    {
        public string TargetFieldName { get; }

        public DFormating<T, TS> Formating { get; }
        
        protected void OnChange(TS value)
        {
            try
            {
                if (Formating != null)
                    Value = Formating(value);
            }
            catch (Exception e)
            {
                LogCallback.Error(e);
            }
        }

        public void Link(AsyncEventManager asm)
        {
            asm.AddAction<TS>($"On{TargetFieldName}Changed", OnChange);
        }

        public void UnLink(AsyncEventManager asm)
        {
            asm.AddAction<TS>($"On{TargetFieldName}Changed", OnChange);
        }

        public LinkedField(string targetFieldName, DFormating<T, TS> formating)
        {
            TargetFieldName = targetFieldName;
            Formating = formating;
        }
    }
    
    
    public class LinkedField<T, TSA, TSB> : Field<T>, ILinkedField
    {
        public string[] TargetFieldNames { get; }

        public DFormating<T, TSA, TSB> Formating { get; }
        
        protected void OnChange(TSA valueA, TSB valueB)
        {
            try
            {
                if (Formating != null)
                    Value = Formating(valueA, valueB);
            }
            catch (Exception e)
            {
                LogCallback.Error(e);
            }
        }

        public void Link(AsyncEventManager asm)
        {
            foreach(string TargetFieldName in TargetFieldNames)
                asm.AddAction<TSA, TSB>($"On{TargetFieldName}Changed", OnChange);
        }

        public void UnLink(AsyncEventManager asm)
        {
            foreach(string TargetFieldName in TargetFieldNames)
                asm.AddAction<TSA, TSB>($"On{TargetFieldName}Changed", OnChange);
        }

        public LinkedField(string targetFieldNameA, string targetFieldNameB, DFormating<T, TSA, TSB> formating)
        {
            TargetFieldNames = new string[] {targetFieldNameA, targetFieldNameB};
            Formating = formating;
        }
    }
    
    public interface ILinkedField
    {
        void Link(AsyncEventManager asm);
        void UnLink(AsyncEventManager asm);
    }
}