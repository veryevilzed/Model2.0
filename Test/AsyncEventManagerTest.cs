using System;
using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class AsyncEventManagerTest
    {
        private int mResult = 0;
        private int mResult3 = 0;
        public void m(int a, int b)
        {
            mResult = a + b;
        }
        
        public void m2(long a, int b)
        {
            mResult = (int)a + b;
        }

        
        [Event("m2")]
        public void m3(long a, int b)
        {
            mResult3 = (int)a + b;
        }

        
        [Test]
        public void Test()
        {
            AsyncEventManager aem = new AsyncEventManager();
            aem.AddAction<int, int>(m);
            aem.AddAction<long, int>(m2);
            aem.AddAction<long, int>(m3);
            aem.Invoke("m", 5, 7);
            Assert.AreEqual(mResult, 12);
            aem.Invoke("m2", 10, 10);
            Assert.AreEqual(mResult, 20);
            Assert.AreEqual(mResult3, 20);
            aem.RemoveAction<long,int>(m2);
            aem.Invoke("m2", 50, 50);
            Assert.AreEqual(mResult, 20);
            Assert.AreEqual(mResult3, 100);
            
            
        }
        
    }
}