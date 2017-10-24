

using System;
using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class TestModel
    {
        public class MyModel : Model
        {
            public readonly IntegerField MyInt = new IntegerField(0);
            public readonly Field<DateTime> MyDateField = new Field<DateTime>(DateTime.Now);
            public readonly LinkedField<string, DateTime> myLinkedField =
                new LinkedField<string, DateTime>("MyDateField", v => v.ToString("dd.MM.yyyy HH:mm"));
        }
        
        [Test]
        public void TestFields()
        {
            MyModel m = new MyModel();
            m.MyDateField.Set(new DateTime(1980,02,01));
            m.ExecuteAsync();
            Assert.AreEqual(m.myLinkedField.Value, "01.02.1980 00:00");
        }
    }
}