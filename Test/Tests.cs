using System;
using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class Tests
    {
        public class MyModel : Model
        {
            public readonly Field<int> FieldA = new Field<int>(100);
            public readonly Field<long> FieldL = new Field<long>(100L);
            public readonly IntegerField FieldB = new IntegerField(100);
            public readonly Field<string> FieldC = new Field<string>();

            
        }
        
        
        [Test]
        public void Test1()
        {
            MyModel model = new MyModel();
            
            Assert.True(2 == 2L);

            
            Assert.AreEqual(model.FieldA.Value, 100);
            Assert.AreEqual(model.FieldB.Value, 100);

            Assert.True(model.FieldA.Equals(model.FieldB));
            Assert.True(model.FieldB.Equals(model.FieldA));

            Assert.True(model.FieldA.Value == model.FieldL.Value);
            Assert.True(model.FieldL.Value.Equals(model.FieldA.Value));
            

            Assert.True(model.FieldA.Equals(model.FieldL));
            Assert.True(model.FieldL.Equals(model.FieldA));
            
            Assert.False(model.FieldB.Equals(model.FieldC));
            Assert.False(model.FieldC.Equals(model.FieldA));

            
        }
    }
}