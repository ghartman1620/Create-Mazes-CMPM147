using UnityEngine;
using NUnit.Framework;
using System.Collections;


public class MissionGraphTest {

    [TestCase]
    public void MissionGraphTestSimplePasses()
    {
        System.Console.WriteLine("Hello from unit test!");    
        MissionGraph G = new MissionGraph();
        Assert.IsTrue(1 == 0);  
    }

    // given for testing game functionality - i just want to unit test
    // non-game functionality code.

    //// A UnityTest behaves like a coroutine in PlayMode
    //// and allows you to yield null to skip a frame in EditMode
    //[UnityTest]
    //public IEnumerator MissionGraphTestWithEnumeratorPasses() {
    //    // Use the Assert class to test conditions.
    //    // yield to skip a frame
    //    yield return null;
    //}
}
