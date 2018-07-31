using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MyTests
{
    [TestClass]
    public class MissionGraphTest
    {
        [TestMethod]
        public void TestConstructedMissionGraphHasRootVertex()
        {
            MissionGraph g = new MissionGraph();
            System.Diagnostics.Trace.WriteLine("You can see output from statements like this in the test explorer --> this test --> output");
            Assert.IsNotNull(g);
            Assert.IsTrue(g.Count == 1);
            Assert.IsTrue(g.NonterminalCount == 1);
        }

        // Test the application of a single production on a graph.
        // It should produce the one rule for a dungeon - an entrance, a node, and an exit
        [TestMethod]
        public void TestMissionGraphWith0ObstaclesCreatesEntranceNodeExitInOneProduction()
        {
            MissionGraph g = new MissionGraph(0, 10, 1000);
            g.ApplyProduction();
            Assert.IsTrue(g.NonterminalCount == 1);
            Assert.IsTrue(g.TerminalCount == 2);
            bool entranceFound = false;
            bool exitFound = false;
            bool nodeFound = false;
            foreach (Vertex v in g)
            {
                // entrance points to node
                if (v.Name.Equals("entrance"))
                {
                    entranceFound = true;
                   

                    // nothing points into entrance
                    Assert.IsTrue(v.BackAdj.Count == 0);
                    Assert.IsTrue(v.ForwardAdj.Count == 1);

                    IEnumerator<Vertex> enumer = v.ForwardAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex second = enumer.Current;
                    // Second node is a node, and points back to entrance.
                    Assert.IsTrue(second.Name.Equals("node"));
                    Assert.IsTrue(second.BackAdj.Contains(v));
                }
                else if(v.Name.Equals("exit"))
                {
                    exitFound = true;

                    // exit points forward to nothing
                    Assert.IsTrue(v.ForwardAdj.Count == 0);
                    Assert.IsTrue(v.BackAdj.Count == 1);

                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex second = enumer.Current;
                    // Second node is a node, and points back to entrance.
                    Assert.IsTrue(second.Name.Equals("node"));
                    Assert.IsTrue(second.ForwardAdj.Contains(v));
                }
                else if(v.Name.Equals("node"))
                {
                    nodeFound = true;

                    // middle node points forward to one thing, and backward
                    // to another thing.
                    Assert.IsTrue(v.ForwardAdj.Count == 1);
                    Assert.IsTrue(v.BackAdj.Count == 1);


                    // first node is an entrance, and points back to node.
                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex first = enumer.Current;
                    
                    Assert.IsTrue(first.Name.Equals("entrance"));
                    Assert.IsTrue(first.ForwardAdj.Contains(v));

                    // Second node is a node, and points back to entrance.
                    IEnumerator<Vertex> enumer1 = v.ForwardAdj.GetEnumerator();
                    enumer1.MoveNext();
                    Vertex last = enumer1.Current;

                    Assert.IsTrue(last.Name.Equals("exit"));
                    Assert.IsTrue(last.BackAdj.Contains(v));

                }
                // There should only be these types of nodes in this particular missiongraph...
                else
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.IsTrue(nodeFound && exitFound && entranceFound);
        }

        // A missiongraph with an obstacle budget of 0 will do the same thing 
        // each time - form an entrance, a space, then an exit.

        // We test this functionality here.
        [TestMethod]
        public void TestMissionGraphWith0ObstaclesCreatesSimpleMaze()
        {
            
            MissionGraph g = new MissionGraph(0, 10, 1000);
            g.CompleteProduction();
            // modelled after the above test - we'll assert some things we know
            // about what a missiongraph with these parameters should yield.

            // it should have a three long maze - an entrance, a space, and an exit.
            // the only difference between the above with a single reduction performed
            // and this with as many reductions as possible performed should be
            // the change from the center node above to a space. Because the 
            // obstacle budget of this missiongraph is 0 it should generate no key/door puzzles.

            Assert.IsTrue(g.NonterminalCount == 0);
            Assert.IsTrue(g.TerminalCount == 3);
            bool entranceFound = false;
            bool exitFound = false;
            bool spaceFound = false;
            foreach (Vertex v in g)
            {
                Assert.IsTrue(v.IsTerminal);
                // entrance points to node
                if (v.Name.Equals("entrance"))
                {
                    entranceFound = true;
                    // nothing points into entrance
                    Assert.IsTrue(v.BackAdj.Count == 0);
                    Assert.IsTrue(v.ForwardAdj.Count == 1);

                    IEnumerator<Vertex> enumer = v.ForwardAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex second = enumer.Current;
                    // Second node is a node, and points back to entrance.
                    Assert.IsTrue(second.Name.Equals("space"));
                    Assert.IsTrue(second.BackAdj.Contains(v));
                }
                else if (v.Name.Equals("exit"))
                {
                    exitFound = true;

                    // exit points forward to nothing
                    Assert.IsTrue(v.ForwardAdj.Count == 0);
                    Assert.IsTrue(v.BackAdj.Count == 1);

                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex second = enumer.Current;
                    // Second node is a node, and points back to entrance.
                    Assert.IsTrue(second.Name.Equals("space"));
                    Assert.IsTrue(second.ForwardAdj.Contains(v));
                }
                else if (v.Name.Equals("space"))
                {
                    spaceFound = true;

                    // middle node points forward to one thing, and backward
                    // to another thing.
                    Assert.IsTrue(v.ForwardAdj.Count == 1);
                    Assert.IsTrue(v.BackAdj.Count == 1);


                    // first node is an entrance, and points back to node.
                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex first = enumer.Current;

                    Assert.IsTrue(first.Name.Equals("entrance"));
                    Assert.IsTrue(first.ForwardAdj.Contains(v));

                    // Second node is a node, and points back to entrance.
                    IEnumerator<Vertex> enumer1 = v.ForwardAdj.GetEnumerator();
                    enumer1.MoveNext();

                    Vertex last = enumer1.Current;
                    Assert.IsTrue(last.Name.Equals("exit"));
                    Assert.IsTrue(last.BackAdj.Contains(v));

                }
                // There should only be these types of nodes in this particular missiongraph...
                else
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.IsTrue(spaceFound && exitFound && entranceFound);
            
        }

        // A missiongraph with an obstacle budget of 1 will do the same thing
        // each time when expanded - form an entrance, a space, a door, a space, a key, and an exit.
        // We test for this functionality here.


        // Note this is the last test for which the MissionGraph will produce a deterministic result.
        // Once the obstacle budget is 2 or more there are multiple possible missiongraphs that can be
        // produced. So those tests all we'll check for is consistency in the structure
        // of the missiongraph produced.
        // Perhaps we can also do a BFS check for key/lock puzzles being solvable.
        [TestMethod]
        public void TestMissionGraphWith1ObstacleCreatesOneKeyLockPuzzle()
        {
            MissionGraph g = new MissionGraph(1, 10, 1000);
            g.CompleteProduction();
            //Modelled after the tests above. 
            
            Assert.IsTrue(g.NonterminalCount == 0);
            Assert.IsTrue(g.TerminalCount == 6);
            bool entranceFound = false;
            bool exitFound = false;
            int  spaceCount  = 0;
            bool keyFound = false;
            bool doorFound = false;
            foreach (Vertex v in g)
            {
                Assert.IsTrue(v.IsTerminal);
                Assert.AreNotEqual(v.Name, "node");
                // entrance points to node
                if (v.Name.Equals("entrance"))
                {
                    entranceFound = true;


                    // nothing points into entrance
                    Assert.IsTrue(v.BackAdj.Count == 0);
                    Assert.IsTrue(v.ForwardAdj.Count == 1);

                    IEnumerator<Vertex> enumer = v.ForwardAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex second = enumer.Current;
                    // Second node is a node, and points back to entrance.
                    Assert.IsTrue(second.Name.Equals("space"));
                    Assert.IsTrue(second.BackAdj.Contains(v));
                }
                else if (v.Name.Equals("exit"))
                {
                    exitFound = true;

                    // exit points forward to nothing
                    Assert.IsTrue(v.ForwardAdj.Count == 0);
                    Assert.IsTrue(v.BackAdj.Count == 1);

                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();

                    Vertex second = enumer.Current;
                    // Second node is a node, and points back to entrance.
                    System.Diagnostics.Trace.WriteLine(second.Name);
                    Assert.IsTrue(second.Name.Equals("door"));
                    Assert.IsTrue(second.ForwardAdj.Contains(v));
                }
                else if (v.Name.Equals("space"))
                {
                    spaceCount += 1;
                    // Don't know which space this is, 
                    // so we'll just check the structure for consistency.
                    // The assertions made by the other types of vertices 
                    // ensure that this graph has the correct structure for
                    // this obstacle budget.

                    foreach (Vertex forward in v.ForwardAdj)
                    {
                        Assert.IsTrue(forward.BackAdj.Contains(v));
                    }
                    foreach (Vertex backward in v.BackAdj)
                    {
                        Assert.IsTrue(backward.ForwardAdj.Contains(v));
                    }

                }
                else if (v.Name.Equals("key"))
                {
                    keyFound = true;
                    // The key in the 1 obstacle missiongraph has no edges out of it, only into it from a space.
                    Assert.AreEqual(v.ForwardAdj.Count, 0);
                    Assert.AreEqual(v.BackAdj.Count, 1);

                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex back = enumer.Current;
                    Assert.IsTrue(back.Name.Equals("space"));
                    Assert.IsTrue(back.ForwardAdj.Contains(v));

                }
                // the door in the 1 obstacle missiongraph has an edge into it from a space and an edge out of it
                // into the exit
                else if (v.Name.Equals("door"))
                {
                    doorFound = true;
                    // The key in the 1 obstacle missiongraph has no edges out of it, only into it from a space.
                    Assert.AreEqual(v.ForwardAdj.Count, 1);
                    Assert.AreEqual(v.BackAdj.Count, 1);

                    IEnumerator<Vertex> enumer = v.BackAdj.GetEnumerator();
                    enumer.MoveNext();
                    Vertex back = enumer.Current;
                    Assert.IsTrue(back.Name.Equals("space"));
                    Assert.IsTrue(back.ForwardAdj.Contains(v));

                    IEnumerator<Vertex> forwardEnum = v.ForwardAdj.GetEnumerator();
                    forwardEnum.MoveNext();
                    Vertex forward = forwardEnum.Current;
                    Assert.IsTrue(forward.Name.Equals("exit"));
                    Assert.IsTrue(forward.BackAdj.Contains(v));



                }
                // There should only be these types of nodes in this particular missiongraph...
                else
                {
                    Assert.IsTrue(false);
                }
            }
            Assert.AreEqual(spaceCount, 2);
            Assert.IsTrue(entranceFound);
            Assert.IsTrue(exitFound);
            Assert.IsTrue(keyFound);
            Assert.IsTrue(doorFound);

        }


        // Tests that a few different missiongraphs have consistent vertices - that is,
        // for each vertex, every vertex that it points to via ForwardAdj points back at it via
        // BackAdj and vice versa.

        [TestMethod]
        public void TestMissionGraphsWithLargeSizesHaveConsistentVertices()
        {
            
            MissionGraph g1 = new MissionGraph(0, 10, 1000);
            g1.CompleteProduction();
            checkConsistency(g1);

            MissionGraph g2 = new MissionGraph(1, 15, 1000);
            g2.CompleteProduction();
            checkConsistency(g2);

            MissionGraph g3 = new MissionGraph(2, 20, 1000);
            g3.CompleteProduction();
            checkConsistency(g3);

            MissionGraph g4 = new MissionGraph(3, 25, 1000);
            g4.CompleteProduction();
            checkConsistency(g4);

            MissionGraph g5 = new MissionGraph(4, 30, 1000);
            g5.CompleteProduction();
            checkConsistency(g5);



        }
        private void checkConsistency(MissionGraph g)
        {
            foreach(Vertex v in g)
            {
                foreach(Vertex forward in v.ForwardAdj)
                {
                    Assert.IsTrue(forward.BackAdj.Contains(v));
                }
                foreach (Vertex backward in v.BackAdj)
                {
                    Assert.IsTrue(backward.ForwardAdj.Contains(v));
                }

            }
        }

        [TestMethod]
        public void TestMissionGraphWithAnyBudgetHasNoNonterminalsWhenProductionCompletes()
        {
            MissionGraph g1 = new MissionGraph(0, 10, 1000);
            g1.CompleteProduction();
            Assert.IsTrue(g1.NonterminalCount == 0);

            MissionGraph g2 = new MissionGraph(1, 10, 1000);
            g2.CompleteProduction();
            Assert.IsTrue(g2.NonterminalCount == 0);

            MissionGraph g3 = new MissionGraph(2, 10, 1000);
            g3.CompleteProduction();
            Assert.IsTrue(g3.NonterminalCount == 0);

            MissionGraph g4 = new MissionGraph(5, 10, 1000);
            g4.CompleteProduction();
            Assert.IsTrue(g4.NonterminalCount == 0);

            MissionGraph g5 = new MissionGraph(15, 50, 1000);
            g5.CompleteProduction();
            Assert.IsTrue(g5.NonterminalCount == 0);

        }
        //Deprecated. Doing something different with size now - instead of size budget
        // now missiongraphs will just make each space the same size, and every other
        // type of thing one large.
        // Left here for posterity.


        //[TestMethod]
        //public void TestMissionGraphMeetsSizeBudget()
        //{
        //    MissionGraph g1 = new MissionGraph(0, 10, 1000);
        //    g1.CompleteProduction();
        //    int g1SizeSum = 0;
        //    foreach(Vertex v in g1)
        //    {
        //        g1SizeSum += v.Size;
        //    }
        //    Assert.AreEqual(10, g1SizeSum);

        //    MissionGraph g2 = new MissionGraph(1, 15, 1000);
        //    g2.CompleteProduction();
        //    int g2SizeSum = 0;
        //    foreach (Vertex v in g2)
        //    {
        //        g2SizeSum += v.Size;
        //    }
        //    Assert.AreEqual(15, g2SizeSum);

        //    MissionGraph g3 = new MissionGraph(2, 20, 1000);
        //    g3.CompleteProduction();
        //    int g3SizeSum = 0;
        //    foreach (Vertex v in g3)
        //    {
        //        g3SizeSum += v.Size;
        //    }
        //    Assert.AreEqual(20, g3SizeSum);


        //    MissionGraph g4 = new MissionGraph(1, 50, 1000);
        //    g4.CompleteProduction();
        //    int g4SizeSum = 0;
        //    foreach (Vertex v in g4)
        //    {
        //        g4SizeSum += v.Size;
        //    }
        //    Assert.AreEqual(50, g4SizeSum);

        //    MissionGraph g5 = new MissionGraph(10, 100, 1000);
        //    g5.CompleteProduction();
        //    int g5SizeSum = 0;
        //    foreach (Vertex v in g5)
        //    {
        //        g5SizeSum += v.Size;
        //    }
        //    Assert.AreEqual(100, g5SizeSum);

        //}
    }
}
