using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class TestEventManager
    {
        
        public class EventObject
        {
            
            
            public int i = 0;

            public string eventField = "test";
            
            [Event("{eventField}")]
            public void OnExecute()
            {
                i++;
            }
        }
        
        
        [Test]
        public void TestSetString()
        {
            EventObject eo = new EventObject();
            AsyncEventManager aem = new AsyncEventManager();
            aem.Add(eo);
            Assert.AreEqual(eo.i, 0);
            aem.Invoke("test");
            Assert.AreEqual(eo.i, 1);
        }
    }
}