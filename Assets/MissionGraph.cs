using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;



public enum Direction { Up, Down, Left, Right };
public class Connection
{
    public Direction Dir;
    public UnityEngine.Vector2 Point;
    public Connection(Direction d, UnityEngine.Vector2 v)
    {
        Dir = d;
        Point = v;
    }
    public override int GetHashCode()
    {
        return Dir.GetHashCode() + Point.GetHashCode();
    }
}

// A vertex for a missiongraph. 
// Contains directed edges - but all edges point both forward and backward, so each
// Vertex is connected by two edges - a forward and a back edge.
// Also has a set of tags and a terminal flag for use in productions.

  

// I want to talk about the decision to make Name a string and tags a set of strings,
// rather than some kind of enum type - because that's something I considered for quite awhile.
// It would make the whole affair alot more type safe to make the name and tag types enums - 
// it's impossible to create an "invalid" grammar then, the compiler will stop you.

// I went with this approach because it's more scalable in a non-code sort of way - it allows that
// in the future I could extend this class to read its rules from some sort of dataset that is not written
// in C# code, such as a JSON file or some database table. This allows it to be more extendible to rulesets
// that are larger than the one I have here, whereas if I'd opted to use enums for type safety
// I lose that freedom in the future and have locked the rules I follow into code.
public class Vertex
{
    public bool IsTerminal;

    // The name of this symbol in the grammar.
    public string Name;
    
    public HashSet<Vertex> ForwardAdj;
    public HashSet<Vertex> BackAdj;
    public int Size;
    public UnityEngine.Vector2 BasePosition;
    public bool Visited;
    public HashSet<Connection> Connections;



    public int LockId;
    public UnityEngine.Color Color;
    

    public Vertex(bool terminal, string name, int size = 0)
    {
        Name = name;
        Size = size;
        IsTerminal = terminal;
        ForwardAdj = new HashSet<Vertex>();
        BackAdj = new HashSet<Vertex>();
        Connections = new HashSet<Connection>();
        Visited = false;
    }
}

// MissionGraph uses a graph structure to represent a series of metaphorical
// mission spaces in a level competion. Such spaces might include
// terminals keys, doors, and empty spaces, 
// and nonterminals Obstacles.
// This graph will have bidirectional referenced edges, but forward and
// backward edges will be distinguished.
// The start symbol is a "dungeon" nonterminal vertex.

public class MissionGraph: IEnumerable<Vertex>
{
    // The vertices in this MissionGraph. They are not identified in any particular way,
    // although references are maintained to the first 
    private HashSet<Vertex> vertices;

    // convenience sets for accessing the list of terminal and nonterminal
    // vertices in this graph grammar. 
    // terminals.UnionWith(nonterminals) is the same set as vertices
    private HashSet<Vertex> terminals;
    private HashSet<Vertex> nonterminals;

    private int obstacleBudget;
    private int sizeBudget;

    private Random rand;
    private int keyDoorCount = 0;

    // Creates a MissionGraph with a single vertex named dungeon. 
    // This dungeon node is replaced with some number of terminal rules in 
    // rewrite(). One may also add vertices manually using AddVertex
    // obstacleBudget is how many additional key rooms the MissionGraph may add.
    // sizeBudget is how much space it may allocate total among all of the spaces
    // it creates. The size of a space is the width of the possible network of rooms
    // So a size 5 space could possibly have 25 rooms.
    // Random seed is the random seed to be supplied to the random object that will
    // decide which production to apply when multiple possibilities exist given the
    // budgets.
    public MissionGraph(int obstacleBudget = 0, int sizeBudget = 10, int randomSeed = 0)
    {
        // Initialize
        this.obstacleBudget = obstacleBudget;
        this.sizeBudget = sizeBudget;
        this.vertices = new HashSet<Vertex>();
        this.terminals = new HashSet<Vertex>();
        this.nonterminals = new HashSet<Vertex>();

        // Add start symbol
        Vertex v = new Vertex(false, "dungeon");
        vertices.Add(v);
        nonterminals.Add(v);

        
        if(randomSeed == 0) //unspecified 
        {
            rand = new Random();
        }
        else
        {
            rand = new Random(randomSeed);
        }
        
    }
    // Iterates over the vertices in this MissionGraph.
    // This access method is preferred to allowing vertices to be public
    // because another class could modify vertices in an improper way
    // and not correspondingly modify terminals or nonterminals.
    public IEnumerator<Vertex> GetEnumerator()
    {
        foreach(Vertex v in vertices)
        {
            yield return v;
        }
    }

    // Looks like IEnumerator<T> inherits from IEnumerator - so we must add this method
    // Thanks to https://stackoverflow.com/questions/18355773/custom-collection-cannot-implement-ienumerablet
    // for this information
    IEnumerator IEnumerable.GetEnumerator()
    {
        return this.GetEnumerator();
    }



    public int Count
    {
        get { return vertices.Count; }
    }
    public int TerminalCount
    {
        get { return terminals.Count; }
    }
    public int NonterminalCount
    {
        get { return nonterminals.Count; }
    }

    // Applies productions until no nonterminals remain.
    // Use this to produce a graph to be ready for use in creation of a Space.
    // Uses ApplyProduction() repeatedly. Separated for unit testing ease.
    public void CompleteProduction()
    {
        // There is no check for an infinitely recursive grammar, i.e.
        // node -> node with no budgetary restrictions.
        // This should be fine, with the grammar I'm currently working with.
        // If I should continue working on this perhaps some loop count limit
        // would be called for?
        while(nonterminals.Count != 0)
        {
            ApplyProduction();
        }
    }

    // Performs a reduction on this graph. 
    // The current grammar is as follows:
    // /nonterminal/ *terminal*
    // Start: a /dungeon/

    // A /dungeon/ is an *entrance* that points forward into a
    // /node/ that points forward into an *exit*

    // A /node/ is a *space*

    // A /node/ is a *space* that points into a *door* and into a /node/ that points into a *key*

    // A /node/ is a *space* that points into a *door* 
    // and into two /node/s that each point into a separate *key*

    // Allocates space size based on the size budget
    // Decides what node reduction to perform based on remaining obstacle budget. If there
    // is no more additional obstacle budget, no reduction will be performed except for a space.
    // If there are more obstacles that should be added, the plain space reduction will not be
    // performed on the last node.


    public void ApplyProduction()
    {
        // Select some nonterminal to replace. Any should suffice.

        IEnumerator<Vertex> enumer = nonterminals.GetEnumerator();
        enumer.MoveNext();
        Vertex nonterminal = enumer.Current;
        // Select a rule based on the name of this nonterminal

        // @TODO: Change the grammar to read from some non-code based format, like
        // JSON. This allows the structure of the grammar to be changed without messing
        // with the code.
        HashSet<Vertex> replacements = new HashSet<Vertex>();
        switch (nonterminal.Name)
        {
            // @TODO sizes
            case "dungeon":
                System.Diagnostics.Trace.WriteLine("dungeon replacement");
                Vertex entrance = new Vertex(true, "entrance");
                entrance.Size = 1;
                Vertex node = new Vertex(false, "node");
                node.Size = -1;
                Vertex exit = new Vertex(true, "exit");
                exit.Size = 1;
                entrance.ForwardAdj.Add(node);
                node.BackAdj.Add(entrance);
                node.ForwardAdj.Add(exit);
                exit.BackAdj.Add(node);

                replacements.Add(node);
                replacements.Add(entrance);
                replacements.Add(exit);

                this.Replace(nonterminal, replacements, entrance, exit);
                break;
        

            // This is a very hard-coded sort of way to do the parametric
            // reduction decision I want. 
            case "node":
                // If there's no remaining obstacle budget, we must
                // not add additional obstacles.
                int SpaceOption = 0;
                int OneObstacleOption = 1;
                int TwoObstacleOption = 2;
                int option;
                if(obstacleBudget == 0)
                {
                    option = SpaceOption;
                    
                }

                // If there are no other nonterminals, we must 
                // add additional obstacles.
                
                else if(nonterminals.Count == 1 && obstacleBudget == 1)
                {
                    option = 1;
                }
                else if(nonterminals.Count == 1 && obstacleBudget > 1)
                {
                    option = 1 + rand.Next() % 2;
                }
                // Otherwise, decide what to do at random.
                else {
                    option = rand.Next() % 3;
                }

                if (option == SpaceOption)
                {
                    System.Diagnostics.Trace.WriteLine("node to space replacement");
                    Vertex space = new Vertex(true, "space");
                    space.Size = sizeBudget;
                    replacements.Add(space);
                    Replace(nonterminal, replacements, space, space);
                }
                else if(option == OneObstacleOption)
                {
                    System.Diagnostics.Trace.WriteLine("One obstacle replacement");
                    Vertex space = new Vertex(true, "space");
                    space.Size = sizeBudget;
                    Vertex key = new Vertex(true, "key");
                    key.Size = 1;
                    int keyNumber = ++keyDoorCount;
                    key.LockId = keyNumber;
                    Vertex door = new Vertex(true, "door");
                    door.LockId = keyNumber;
                    door.Size = 1;
                    Vertex node1 = new Vertex(false, "node");
                    node1.Size = -1;

                    UnityEngine.Color c1 = UnityEngine.Random.ColorHSV();
                    key.Color = c1;
                    door.Color = c1;

                    space.ForwardAdj.Add(node1);
                    node1.BackAdj.Add(space);

                    space.ForwardAdj.Add(door);
                    door.BackAdj.Add(space);

                    node1.ForwardAdj.Add(key);
                    key.BackAdj.Add(node1);

                    replacements.Add(space);
                    replacements.Add(key);
                    replacements.Add(door);
                    replacements.Add(node1);
                    Replace(nonterminal, replacements, space, door);
                    obstacleBudget--;

                }
                else if(option == TwoObstacleOption)
                {
                    
                    System.Diagnostics.Trace.WriteLine("two obstacle replacement");
                    Vertex space = new Vertex(true, "space");
                    space.Size = sizeBudget;
                    Vertex key = new Vertex(true, "key");
                    key.Size = 1;
                    int key1Number = ++keyDoorCount;
                    key.LockId = key1Number;
                    Vertex key1 = new Vertex(true, "key");
                    int key2Number = ++keyDoorCount;
                    key1.LockId = key2Number;
                    key1.Size = 1;
                    Vertex door = new Vertex(true, "door");
                    door.LockId = key1Number;
                    door.Size = 1;
                    Vertex door1 = new Vertex(true, "door");
                    door1.LockId = key2Number;
                    door1.Size = 1;

                    UnityEngine.Color c1 = UnityEngine.Random.ColorHSV();
                    UnityEngine.Color c2 = UnityEngine.Random.ColorHSV();
                    key.Color = c1;
                    door.Color = c1;
                    key1.Color = c2;
                    door1.Color = c2;

                    Vertex node1 = new Vertex(false, "node");
                    node1.Size = -1;
                    Vertex node2 = new Vertex(false, "node");
                    node2.Size = -1;

                    space.ForwardAdj.Add(node1);
                    node1.BackAdj.Add(space);

                    space.ForwardAdj.Add(node2);
                    node2.BackAdj.Add(space);

                    space.ForwardAdj.Add(door);
                    door.BackAdj.Add(space);

                    door.ForwardAdj.Add(door1);
                    door1.BackAdj.Add(door);



                    node1.ForwardAdj.Add(key);
                    key.BackAdj.Add(node1);

                    node2.ForwardAdj.Add(key1);
                    key1.BackAdj.Add(node2);

                    replacements.Add(space);
                    replacements.Add(key);
                    replacements.Add(key1);
                    replacements.Add(door);
                    replacements.Add(door1);
                    replacements.Add(node1);
                    replacements.Add(node2);

                    Replace(nonterminal, replacements, space, door1);
                    obstacleBudget -= 2;

                }
                else
                {
                    throw new Exception("Should not reach here, invalid replacement option");
                }

                break;
            default:
                Debug.Assert(false); //this can not happen
                break;
        }
    }


    // Replaces a particular Vertex reference in the Mission Graph
    // With the given set of vertices and, among those, a reference
    // to the first and last among them. 
    // The edges going into the replaced vertex will now go into the first
    // vertex among the set of vertices. The edges going out of the replaced
    // vertex will go out of the last vertex.
    private void Replace(Vertex replaced, HashSet<Vertex> replacements, Vertex begin, Vertex end)
    {
        Debug.Assert(replaced != null);
        Debug.Assert(this.vertices.Contains(replaced));

        // I assume without asserting that replacements, begin, and end are all not in vertices.
        Debug.Assert(replacements.Contains(begin));
        Debug.Assert(replacements.Contains(end));

        // Add all the replacing vertices into this graph
        vertices.UnionWith(replacements);

        // Point all the existing vertices that point forward at replaced
        // to begin.
        begin.BackAdj = replaced.BackAdj;
        foreach(Vertex v in begin.BackAdj)
        {
            v.ForwardAdj.Remove(replaced);
            v.ForwardAdj.Add(begin);
        }
        // Point all the existing vertices that point backward at replaced
        // to end
        end.ForwardAdj = replaced.ForwardAdj;
        foreach (Vertex v in end.ForwardAdj)
        {
            v.BackAdj.Remove(replaced);
            v.BackAdj.Add(end);
        }


        vertices.Remove(replaced);
        if (replaced.IsTerminal)
        {
            terminals.Remove(replaced);
        }
        else
        {
            nonterminals.Remove(replaced);
        }
        foreach(Vertex v in replacements)
        {
            if (v.IsTerminal)
            {
                terminals.Add(v);
            }
            else
            {
                nonterminals.Add(v);
            }
        }
    }
}
