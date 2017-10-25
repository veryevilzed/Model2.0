using System;
using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class TestConvertation
    {
        public class MyModel : Model
        {
            public readonly IntegerField MyInt = new IntegerField(5);
            public readonly LongField MyLong = new LongField(14);
            public readonly StringField MyString = new StringField();

            [Event]
            public void OnMyIntChanged(int value)
            {
                Console.WriteLine("CallMI");
            }
            
            [Event]
            public void ModelChanged(string field, string value)
            {
                Console.WriteLine("1.Call:{0} = [{1}] = {2}", field, value, MyString.Value);
                if (field != "MyString")
                {
                    this.MyString.Set(string.Format("{0}={1}", field, value));
                }
                Console.WriteLine("2.Call:{0} = [{1}] = {2}", field, value, MyString.Value);
            }
            
        }
        
        [Test]
        public void testFieldToStringConvertation()
        {
            MyModel m = new MyModel();
            
            m.ExecuteAsync();
            m.MyInt.Value = 15;
            Assert.AreEqual(m.MyInt.Value, 15);
            m.MyString.Value = "Hello";
            m.ExecuteAsync();
            Assert.AreEqual(m.MyString.Value, "MyInt=15");
            m.MyLong.Value = 100;
            Assert.AreEqual(m.MyLong.Value, 100);
            m.ExecuteAsync();
            m.ExecuteAsync();
            Assert.AreEqual(m.MyString.Value, "MyLong=100");

        }
        
    }
}