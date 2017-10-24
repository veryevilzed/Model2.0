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


        public void m4(params object[] args)
        {
            mResult = (int) args[0] + (int) args[1];
            throw new ArgumentNullException();
        }
        

        [Test]
        public void TestParams()
        {
            AsyncEventManager aem = new AsyncEventManager();
            mResult = 0;
            aem.AddAction(m4);
            try
            {
                aem.Invoke("m4", 5, 7);
            }
            catch (System.Exception e)
            {
                Console.WriteLine("E:{0}",e.Message);
            }
            Assert.AreEqual(mResult, 12);
            
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
        
        [Test]
        public void TestLinkedField()
        {
            AsyncEventManager aem = new AsyncEventManager();
            LinkedField<string, int> lf = new LinkedField<string, int>("M4", value => string.Format("$ {0} $", value) );
            lf.Link(aem);
            
            aem.Invoke("OnM4Changed", 15);
            
            Assert.AreEqual(lf.Value, "$ 15 $");   
        }
        
    }
}