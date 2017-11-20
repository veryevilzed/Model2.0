using System;
using System.Collections.Generic;
using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class TestDynamicModel
    {
        class MyModel : DynamicModel
        {
            public int itemChangedCall;
            public int modelChangedCall;
            
            [Event]
            public void OnItem1Changed() => itemChangedCall += 1;
            
            [Event]
            public void ModelChanged() => modelChangedCall += 1; 
            
        }

        public int outsideModelChangedCall;

        [Event]
        public void ModelChanged(string field, object item)
        {
            outsideModelChangedCall++;
        }
        
        [Test]
        public void TestSetString()
        {
            MyModel model = new MyModel();
            model.EventManager.Add(this);
            String title = "MyNewTitle";
            model.SetString("Title", "title");
            Assert.AreEqual(model.GetString("Title"), "title");
            model.SetString("Title", title);
            Assert.AreEqual(model.GetString("Title"), "MyNewTitle");
        }
        
        
        [Test]
        public void TestItemsEvents()
        {
            MyModel model = new MyModel();
            model.EventManager.Add(this);    
            model.Set("Item1", 100);
            Assert.AreEqual(model.GetInt("Item1"), 100);
            Assert.AreEqual(model.itemChangedCall, 1);
            Assert.AreEqual(model.modelChangedCall, 1);
            model.Set("Item2", "world");
            Assert.AreEqual(model.GetString("Item2"), "world");
            Assert.AreEqual(model.itemChangedCall, 1);
            Assert.AreEqual(model.modelChangedCall, 2);
            Assert.AreEqual(outsideModelChangedCall, 2);


        }

        [Test]
        public void TestAsyncEvents()
        {
            MyModel model = new MyModel();
            model.Set("Item1", 100);
            Assert.AreEqual(model.GetInt("Item1"), 100);
            model.SetDelay(2.0f, "Item1", 200);
            Assert.AreEqual(model.GetInt("Item1"), 100);
            model.Update(1.0f);
            Assert.AreEqual(model.GetInt("Item1"), 100);
            model.Update(1.1f);
            Assert.AreEqual(model.GetInt("Item1"), 200);
            
        }
        
        
    }
    
}