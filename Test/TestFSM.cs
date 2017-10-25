using NUnit.Framework;
using TinyLima.Tools;

namespace Test
{
    [TestFixture]
    public class TestFSM
    {
        private static int testItem = 0;

        [State("init")]
        public class SInit : FSMState  
        {
            [Enter]
            public void Enter()
            {
                testItem *= 0;
            }

            [Enter(5)]
            public void Enter2()
            {
                testItem += 1;
            }
            
            [Exit(-10)]
            public void Exit()
            {
                testItem *= 0;
            }

            [Exit]
            public void Exit2()
            {
                testItem += 100;
            }
        }

        [State("ready")]
        public class SReady : FSMState
        {
            [Enter]
            public void Enter()
            {
                testItem = 500;
            }

            [One(0.5f)]
            void GoToInit()
            {
                Parent.Change("init");
            }             
            
            
        }


        [Test]
        public void FSMTest()
        {
            FSM fsm  = new FSM();
            fsm.Add(new SInit(), new SReady());
            fsm.Start("init");
            Assert.AreEqual(fsm.CurrentStateName, "init");
            fsm.Change("ready");
            Assert.AreEqual(fsm.CurrentStateName, "ready");
            Assert.AreEqual(testItem, 500);
            fsm.Update(20.0f);
            Assert.AreEqual(fsm.CurrentStateName, "init");
            Assert.AreEqual(testItem, 1);
        }

        [Test]
        public void StateTest()
        {
            testItem = 7;
            
            SInit sinit = new SInit();
            sinit.__EnterState();
            Assert.AreEqual(testItem, 1); 
            sinit.__ExitState();
            Assert.AreEqual(testItem, 100); 

            
        }
    }
}