using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class SpaceRoom : MissionTerminal {
    
    public GameObject Floor;
    public GameObject Door;
    public GameObject Wall;
    public GameObject Ramp;
    // There should be GridSizeOfEachTile*GridSizeOfEachTile of these in the same space as one Floor.
    public GameObject FloorTilePiece;
    public float MaxWeight;
    public int TileWidth;

    private Room[,] rooms;
    private List<GameObject> doors;
    private List<GameObject> walls;
    private List<GameObject> floors;
    private int Size;
    private int Connections;
    private readonly double ConnectionsDivisor = 2.5;
    private readonly double ExtraRoomsDivisor = 5;
    private readonly int GridSizeOfEachTile = 4;
    private readonly int NumModels = 1;
    // This really ought to be passed around, but I'm running out of time.
    private HashSet<Connection> connections;

   
    

    public override void Build(Vertex v)
    {
        connections = v.Connections;

        Size = v.Size;
        //////Debug.Log("In spaceroom build done with superclass call");
        //////Debug.Log(Size);
        Connections = (int)(Size * Size / ConnectionsDivisor);
        ////Debug.Log(Connections);
       
        Assert.IsTrue((Size * Size) - 1 > Connections);
        Assert.IsTrue(Connections >= 0);
        if (Size * Size - 1 > Connections && Connections >= 0)
        {
            this.doors = new List<GameObject>();
            this.floors = new List<GameObject>();
            this.walls = new List<GameObject>();
            rooms = new Room[Size, Size];
            int z = 0;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    rooms[i, j] = new Room();
                    rooms[i, j].Id = z;
                    z++;
                }
            }
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    if (i - 1 >= 0) rooms[i, j].Adj.Add(rooms[i - 1, j]);
                    if (i + 1 < Size) rooms[i, j].Adj.Add(rooms[i + 1, j]);
                    if (j - 1 >= 0) rooms[i, j].Adj.Add(rooms[i, j - 1]);
                    if (j + 1 < Size) rooms[i, j].Adj.Add(rooms[i, j + 1]);

                    rooms[i, j].Space.Add(new Vector2(i, j));
                }
            }
            for (int i = 0; i < Connections; i++)
            {
                MergeRoom(rooms);
            }


            // Want a set of rooms so I can regardless of size have an 
            // equal chance of picking each.
            HashSet<Room> hs = new HashSet<Room>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    hs.Add(rooms[i, j]);
                }
            }
            Room[] uniqueRoomsArray = new Room[hs.Count];
            hs.CopyTo(uniqueRoomsArray);
            Room middle = rooms[Size/2, Size/2];  

            //////Debug.Log("Begin is " + begin.Id);
            //////Debug.Log("End is " + end.Id);

            foreach (Room r in uniqueRoomsArray)
            {
                r.Weight = Random.Range(1, MaxWeight);
            }
            //DrawRooms(rooms, 0, 0, 10, Floor, Door);
            //PrintRooms(rooms);
            Dijkstra(hs, middle);
            //PrintRooms(rooms);
            //DrawRooms(rooms, 0, 0, 10, Floor, Door);


            //////Debug.Log("End to begin path is: ");
            HashSet<Room> path = new HashSet<Room>();
            foreach(Connection c in v.Connections)
            {
                ////Debug.Log("Attempting to get room: " + c.Point.x + " , " + c.Point.y);
                ////Debug.Log("basePosition is " + v.BasePosition.x + " , " + v.BasePosition.y);

                for (Room r = rooms[(int)(c.Point.x-v.BasePosition.x), (int)(c.Point.y-v.BasePosition.y)]; r != middle; r = r.Parent)
                {
                    //////Debug.Log(r.Id);
                    Assert.IsNotNull(r.Parent);
                    path.Add(r);
                }
            }
            
            path.Add(middle);

            // @TODO: add five rooms that are adjacent to existing rooms, rather than just five random rooms.
            // Also make sure that this is possible (there are that many not-in-path rooms) 
            // and check that those rooms added aren't already in path. 
            //for(int i = 0; i < ExtraRooms; i++)
            //{
            //    path.Add(rooms[Random.Range(0, rooms.GetLength(0)), Random.Range(0, rooms.GetLength(1))]);
            //}
            //////Debug.Log(begin.Id);


            //@TODO: Replace these random selections from sets with
            // using an iterator moved forward a random distance. This 
            // would be better and not waste as much computing time copying memory.

            Room[] roomsInDisplayedSet = new Room[path.Count];
            path.CopyTo(roomsInDisplayedSet);
            for (int i = 0; i < Size/ExtraRoomsDivisor; i++)
            {

                Room addedSelection;
                do
                {
                    Room random = roomsInDisplayedSet[Random.Range(0, roomsInDisplayedSet.Length)];
                    Assert.IsNotNull(random);
                    Room[] connectedRooms = new Room[random.Adj.Count];
                    random.Adj.CopyTo(connectedRooms);
                    Assert.IsTrue(connectedRooms.Length != 0);
                    //Change this to get a room not in the set if there is one. Is there a function
                    // that computes the intersection of two sets?
                    // If so, can use it here to get a room not in path but that is in random.Adj
                    addedSelection = connectedRooms[Random.Range(0, connectedRooms.Length)];
                    Assert.IsNotNull(addedSelection);
                } while (path.Contains(addedSelection));
                path.Add(addedSelection);
            }

            

            DrawRooms(rooms, v.BasePosition.x, v.BasePosition.y, TileWidth, Floor, Door, Wall, path);


        }
    }
    // Use this for initialization
    void Start () {
        ////Debug.Log("space start");
    }
	
	// Update is called once per frame
	void Update () {
		
	}
    private void Dijkstra(HashSet<Room> rooms, Room source)
    {


        List<Room> unvisited = new List<Room>();
        foreach (Room r in rooms)
        {
            unvisited.Add(r);
            r.Distance = Room.INF;
            r.Visited = false;
        }

        source.Distance = 0;
        Room curr = source;
        IComparer<Room> comp = new ByDistance();
        unvisited.Sort(comp);

        while (!(unvisited.Count == 0))
        {

            foreach (Room visit in curr.Adj)
            {
                if (!visit.Visited && (visit.Distance == Room.INF || visit.Distance > curr.Distance + curr.Weight))
                {

                    visit.Distance = curr.Distance + curr.Weight;
                    visit.Parent = curr;

                    // So this bit of code makes this not really dijkstra, because dijkstra runs in VlogV.
                    // I have to use an O(n) operation to decrease the distance key more or less
                    // because the C# standard library is deficient any class implemented using a heap
                    // with an API appropriate for use as a priority queue
                    // And I was having issues importing a library called PowerCollections into unity.
                    // So this runs in O(V^2).

                    // This is what would be the operation DecreaseKey() in a proper MinHeap.

                    // So now I'm sad and Edsger Dijkstra hates UCSC a little bit more.

                    //remove the now unsorted visit from the list
                    unvisited.Remove(visit);

                    // insert it back into the unvisited list, now sorted
                    int i;
                    for (i = 0; i < unvisited.Count; i++)
                    {
                        if (comp.Compare(unvisited[i], visit) > 0)
                        {
                            unvisited.Insert(i, visit);
                            break;
                        }
                    }
                    // visit is the largest distance thing in unvisited
                    if (i == unvisited.Count)
                    {
                        unvisited.Add(visit);
                    }


                }
            }

            curr.Visited = true;
            unvisited.Remove(curr);
            if (unvisited.Count > 0)
            {
                curr = unvisited[0];
            }

        }


    }



    // Works as above, but only creates rooms in the included HashSet
    // Also creates walls along space that isn't occupied by another room in the inclusions set.
    private void DrawRooms(Room[,] rooms, float baseX, float baseZ, float size, GameObject floor, GameObject door, GameObject wall, HashSet<Room> inclusions)
    {
        foreach (GameObject d in doors)
        {
            Destroy(d);
        }
        foreach (GameObject f in floors)
        {
            Destroy(f);
        }
        foreach(Room r in inclusions)
        {
            CreateFloors(r, floor, baseX, baseZ, size, inclusions);
            foreach (Room r1 in r.Adj)
            {
                if (r != null && inclusions.Contains(r))
                {
                    CreateDoor(r, r1, door, baseX, baseZ, size);
                }
            }
        }
             
        //create walls
        foreach (Room r in inclusions)
        {
            CreateWalls(r, wall, baseX, baseZ, 10, inclusions);
        }
    }

    private void MergeRoom(Room[,] rooms)
    {
        int randX = Random.Range(0, Size);
        int randY = Random.Range(0, Size);
        Room r = rooms[randX, randY];
        // Ensure there actually are some rooms to merge into this one
        if (r.Adj.Count > 0)
        {
            Room[] roomArray = new Room[r.Adj.Count];
            r.Adj.CopyTo(roomArray);
            Room rand = roomArray[Random.Range(0, r.Adj.Count)];

            foreach (Vector2 pt in rand.Space)
            {
                r.Space.Add(pt);
                rooms[(int)pt.x, (int)pt.y] = r;

            }
            foreach (Room pointsToRemovedRoom in rand.Adj)
            {
                if (pointsToRemovedRoom != r)
                {


                    pointsToRemovedRoom.Adj.Remove(rand);
                    pointsToRemovedRoom.Adj.Add(r);
                    r.Adj.Add(pointsToRemovedRoom);
                }
            }
            r.Adj.Remove(rand);
            int removedId = rand.Id;
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    Assert.AreNotEqual(removedId, rooms[i, j].Id);
                    foreach (Room room in rooms[i, j].Adj)
                    {
                        if (removedId == room.Id)
                        {
                            Assert.AreNotEqual(removedId, room.Id);

                        }
                    }
                }
            }
        }

    }
    private static void PrintRooms(Room[,] r)
    {
        for (int i = 0; i < r.GetLength(0); i++)
        {
            for (int j = 0; j < r.GetLength(1); j++)
            {
                string s = "";
                foreach (Room room in r[i, j].Adj)
                {
                    s += room.Id + ", ";
                }
                ////Debug.Log(i + ", " + j + ", id " + r[i, j].Id + " is adjacent to " + s);
                ////Debug.Log(i + ", " + j + ", id " + r[i, j].Id + " has parent " + (r[i, j].Parent == null ? "null" : r[i, j].Parent.Id.ToString()) + " and distance " + r[i, j].Distance);
            }
        }
    }



    // Create a wall at the given position. xoff and yoff denote what side of the designated tile of size 
    // should have the wall. In this world, rooms have four sides, so one of xoff and yoff must be 0 so a wall
    // can only be drawn on the +x, -x, +z, or -z side of a room.

    //Used as a helper function to CreateWalls that accepts a Room object.

    private void CreateWall(GameObject wall, float baseX, float baseZ, float x, float z, 
        float xoff, float zoff, float size)
    {
        Assert.IsTrue(xoff == 0 || zoff == 0);
        Assert.IsNotNull(wall);
        foreach (Connection c in connections)
        {
            //If this is a connection with another vertex don't put a wall here.
            if(c.Point.x - baseX == x && c.Point.y - baseZ == z &&
                ((c.Dir == Direction.Up && zoff == 1) ||
                (c.Dir == Direction.Down && zoff == -1) ||
                (c.Dir == Direction.Left && xoff == -1) ||
                (c.Dir == Direction.Right && xoff == 1)))
            {
                ////Debug.Log("Not putting wall! at position" + c.Point.x + " , " + c.Point.y + " in direction " + c.Dir);
                return;
            }
        }
        
        // i include the y value of the wall's position vector instead of 0 because it appears that passing in the gameobject wall does not incorporate
        // by default the gameobject's position added to the position I provide an instantiation of it - but it is on the other types of prefabs like Doors.
        // Hmmm.
        walls.Add(Instantiate(wall, new Vector3(baseX * size + (x + xoff / 2) * size, wall.transform.position.y, baseZ * size + (z + zoff / 2) * size),
                xoff == 0 ? Quaternion.identity : Quaternion.AngleAxis(90, new Vector3(0, 1, 0))).gameObject);
    }

    private void CreateWalls(Room r, GameObject wall, float baseX, float baseZ, float size, HashSet<Room> inclusions)
    {
        foreach (Vector2 pt in r.Space)
        {
            if (pt.x == 0)
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, -1, 0, size);
            }
            if (pt.y == 0)
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, 0, -1, size);
            }
            if (pt.x == rooms.GetLength(0) - 1)
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, 1, 0, size);
            }
            if (pt.y == rooms.GetLength(1) - 1)
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, 0, 1, size);
            }

            if (pt.x != 0 && !inclusions.Contains(rooms[(int)(pt.x - 1), (int)(pt.y)]))
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, -1, 0, size);
            }
            if (pt.y != 0 && !inclusions.Contains(rooms[(int)(pt.x), (int)(pt.y - 1)]))
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, 0, -1, size);
            }
            if (pt.x != rooms.GetLength(0) - 1 && !inclusions.Contains(rooms[(int)(pt.x + 1), (int)(pt.y)]))
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, 1, 0, size);
            }
            if (pt.y != rooms.GetLength(1) - 1 && !inclusions.Contains(rooms[(int)(pt.x), (int)(pt.y + 1)]))
            {
                CreateWall(wall, baseX, baseZ, pt.x, pt.y, 0, 1, size);
            }
        }
    }

    

    // Creates every floor tile for a given room.
    // Room r - the room to have its floor tiles created
    // GameObject floor - the floor tile prefab to be created. See CreateDoor for notes about this.
    // float baseX, baseZ - the beginning of a maze. Position at which the 0, 0 room's floor tile will go.
    // float size - the size of a floor tile. Used for positioning with relative indices in room objects.
    private void CreateFloors(Room r, GameObject floor, float baseX, float baseZ, float size, HashSet<Room> inclusions)
    {
        IEnumerator<Vector2> enumer = r.Space.GetEnumerator();
        enumer.MoveNext();
        Vector2 first = enumer.Current;

        float minX = first.x;
        float minY = first.y;

        foreach (Vector2 point in r.Space)
        {
            if (point.x < minX)
            {
                minX = point.x;
            }
            if (point.y < minY)
            {
                minY = point.y;
            }
        }


        List<Vector2> aspTiles = new List<Vector2>();
        List<Vector2> aspStartPoints = new List<Vector2>();

        foreach (Vector2 point in r.Space)
        {
            aspTiles.Add(new Vector2(point.x - minX, point.y - minY));
        }
        foreach (Room adj in r.Adj)
        {
            if (inclusions.Contains(adj))
            {
                aspStartPoints.AddRange(ConnectingPoints(r, adj, minX, minY));
            }
        }
        foreach (Vector2 pt in r.Space)
        {
            foreach (Connection c in connections)
            {
                if (c.Point.x - baseX == pt.x && c.Point.y - baseZ == pt.y)
                {
                    float x = 0;
                    float y = 0;
                    switch (c.Dir)
                    {
                        case Direction.Up:
                            x = (pt.x - minX) * GridSizeOfEachTile + (GridSizeOfEachTile / 2);
                            y = (pt.y - minY) * GridSizeOfEachTile + GridSizeOfEachTile - 1;
                            break;
                        case Direction.Down:
                            x = (pt.x - minX) * GridSizeOfEachTile + (GridSizeOfEachTile / 2);
                            y = (pt.y - minY) * GridSizeOfEachTile;
                            break;

                        case Direction.Left:
                            x = (pt.x - minX) * GridSizeOfEachTile;
                            y = (pt.y - minY) * GridSizeOfEachTile + (GridSizeOfEachTile / 2);
                            break;
                        case Direction.Right:
                            x = (pt.x - minX) * GridSizeOfEachTile + GridSizeOfEachTile - 1;
                            y = (pt.y - minY) * GridSizeOfEachTile + (GridSizeOfEachTile / 2);
                            break;
                    }
                    aspStartPoints.Add(new Vector2(x, y));
                }
            }
        }
        //Debug.Log("Creating floors for room " + r.Id);
        IEnumerator<Vector2> pointEnumer = r.Space.GetEnumerator();
        pointEnumer.MoveNext();
        //Debug.Log("Room " + r.Id + "contains a point " + pointEnumer.Current);
        //Debug.Log("room " + r.Id + " has " + r.Space.Count + " rooms");
        //Debug.Log("TIles");
        foreach (Vector2 v in aspTiles)
        {
            //Debug.Log(v);
        }
        //Debug.Log("start points");
        foreach (Vector2 v in aspStartPoints)
        {
            //Debug.Log(v);
        }
        GeneratedPath p = PathGeneration.RunPathGeneration(aspTiles, aspStartPoints, NumModels, GridSizeOfEachTile, aspTiles.Count*(2*GridSizeOfEachTile-1), Random.Range(0, 2));
        List<Vector2> points = p.Spaces;
        HashSet<Vector2> rampPoints = new HashSet<Vector2>(p.Ramps);
        
        //Debug.Log("Now laying tiles...");
        //Debug.Log("BaseX: " + baseX);
        //Debug.Log("BaseZ: " + baseZ);
        //Debug.Log("MinY: " + minY);
        //Debug.Log("MinX: " + minX);
        //Debug.Log("Size: " + size);

        foreach (Vector2 v in points)
        {
            if (!rampPoints.Contains(v))
            {
                float basePositionX = minX * size + baseX * size;
                float basePositionY = minY * size + baseZ * size;
                // magic numbers are BAD GABE WHY YOU DO THIS
                // currently they're magic for GridSize=4
                // They're magic because I don't actually know what their values are supposed to be.
                float offsetX = v.x * 2.5f - 5 * (1 - 1 / (float)GridSizeOfEachTile);
                float offsetY = v.y * 2.5f - 5 * (1 - 1 / (float)GridSizeOfEachTile);
                Vector3 pos = new Vector3(basePositionX + offsetX, 0, basePositionY + offsetY);

                Instantiate(FloorTilePiece, pos, Quaternion.identity);
            }
            
        }
        if(p.Ramps.Count > 0)
        {
            for(int i = 0; i < p.Ramps.Count; i += 3)
            {
                Vector2 avg = AveragePoint(p.Ramps, i, i+3);

                float basePositionX = minX * size + baseX * size;
                float basePositionY = minY * size + baseZ * size;
                // magic numbers are BAD GABE WHY YOU DO THIS
                // currently they're magic for GridSize=4
                // They're magic because I don't actually know what their values are supposed to be.
                float offsetX = avg.x * 2.5f - 5 * (1 - 1 / (float)GridSizeOfEachTile);
                float offsetY = avg.y * 2.5f - 5 * (1 - 1 / (float)GridSizeOfEachTile);
                Vector3 pos = new Vector3(basePositionX + offsetX, 0, basePositionY + offsetY);
                Instantiate(Ramp, pos, DirectionOfPoints(p.Ramps, i));
            }
            
        }


        //foreach (Vector2 point in r.Space)
        //{
        //    float x = point.x * size + baseX * size;
        //    float y = 0;
        //    float z = point.y * size + baseZ * size;
        //    floors.Add(Instantiate(floor, new Vector3(x, y, z), Quaternion.identity));
        //}
    }
    // Computes average point of a list from i to j.
    private Vector2 AveragePoint(List<Vector2> points, int i, int j)
    {
        Vector2 sum = Vector2.zero;
        for(int it = i; it < j; it++)
        {
            sum += points[it];
        }
        return sum / (j - i);
    }
    // returns the direction of a set of points. They should all be in a line along
    // the x or y axis. There should be at least 2 and they are used to assume the rest.
    private Quaternion DirectionOfPoints(List<Vector2> points, int i)
    {
        if(points[i].y != points[i+1].y)
        {
            return Quaternion.AngleAxis(90, new Vector3(0, 1, 0));
        }
        return Quaternion.identity;
    }

    // Selects a point in the GridSizeOfEachTilexGridSizeOfEachTile set of points in 
    // room r1 that is adjacent to r2.
    // Float minx and miny are for computing the normalized index of a space tile in r1 once an adjacent pair is found.
    private List<Vector2> ConnectingPoints(Room r1, Room r2, float minX, float minY)
    {
        List<Vector2> points = new List<Vector2>();
        foreach (Vector2 pt1 in r1.Space)
        {
            foreach (Vector2 pt2 in r2.Space)
            {
                if(Mathf.Abs(pt1.x-pt2.x) + Mathf.Abs(pt1.y - pt2.y) == 1)
                {
                    float x = 0;
                    float y = 0;
                    if(pt1.x == pt2.x)
                    {
                        x = (pt1.x - minX) * GridSizeOfEachTile + (GridSizeOfEachTile / 2);
                        if (pt1.y > pt2.y)
                        {
                            y = (pt1.y - minY) * GridSizeOfEachTile;
                        }
                        else // pt1.y < pt2.y
                        {
                            y = (pt1.y - minY) * GridSizeOfEachTile + GridSizeOfEachTile - 1;
                        }

                    }
                    if (pt1.y == pt2.y)
                    {
                        y = (pt1.y - minY) * GridSizeOfEachTile + (GridSizeOfEachTile / 2);
                        if (pt1.x > pt2.x)
                        {
                            x = (pt1.x - minX) * GridSizeOfEachTile;
                           
                        }
                        else // pt1.y < pt2.y
                        {
                            x = (pt1.x - minX) * GridSizeOfEachTile + GridSizeOfEachTile - 1;
                        }

                    }
                    points.Add(new Vector2(x, y));
                }
            }
        }
        return points;
    }

    // Creates a door prefab in the space between two rooms, r1 and r2.
    // Room r1, r2 - the rooms to place the door between. It should allow passage between the
    // floor tiles that make up these rooms.
    // GameObject door - the door prefab type to be created.
    // This function makes no assumptions about the door gameobject at all - so it could be interactive, or static, or it could even not be a door.
    // Use this freedom wisely. At time of writing door is just three transformed cubes positioned in an arch shape that allows free passage.
    // In the future I'd love to see if I can tile door objects to allow different appearing doors to be selected randomly. Perhaps this is accomplished
    // by creating a list of textures for a particular prefab? or a prefab for each door type, then random selection through a container of prefabs? We'll see.

    // Another comment about this is the reason I passed it as a paremter instead of just using the globally available Door public GameObject
    // This allwos me in the future to create different types of doors by passing this function different doors. Perhaps the way I accomplish my goal for
    // varied doors is just having a few Door prefabs and making them all public - then the caller of this function can pass a randoly selected GameObject for door.
    // This saves some refactoring later.

    // float baseX, baseZ - the origin of this particular maze. Rooms positions are recorded by index - this parameter allows us to vary what those indices mean
    // float size - the size of a floor tile. Room indices allow for positioning by their index * the size of a floor tile. So, a door will be positioned
    // halfway between baseX+size*point1.x and baseX+size*point2.x in both X and Z coordinates for any adjacent points in two rooms.
    private void CreateDoor(Room r1, Room r2, GameObject door, float baseX, float baseZ, float size)
    {
        foreach (Vector2 pt1 in r1.Space)
        {
            foreach (Vector2 pt2 in r2.Space)
            {
                // Look for any pair of points in these rooms space that 
                // is adjacent. Make a door there.


                // The door positioning is trivial once these points are found - put it halfway between them
                //But we must rotate the door based on in which direction the points are adjacent. 

                // The points are adjacent veritically (same horizontal position)
                if (pt1.x == pt2.x && Mathf.Abs(pt1.y - pt2.y) == 1)
                {
                    Vector2 absPoint1 = new Vector2(baseX * size + pt1.x * size, baseZ * size + pt1.y * size);
                    Vector2 absPoint2 = new Vector2(baseX * size + pt2.x * size, baseZ * size + pt2.y * size);
                    Vector3 doorPoint = new Vector2((absPoint1.x + absPoint2.x) / 2, (absPoint1.y + absPoint2.y) / 2);
                    doors.Add(Instantiate(door.GetComponent<Transform>(), new Vector3(doorPoint.x, 0, doorPoint.y), Quaternion.identity).gameObject);
                }
                // The points are vertically adjacent - in this case, we'll rotate the door PI/2 radians.
                else if (pt1.y == pt2.y && Mathf.Abs(pt1.x - pt2.x) == 1)
                {
                    Vector2 absPoint1 = new Vector2(baseX * size + pt1.x * size, baseZ * size + pt1.y * size);
                    Vector2 absPoint2 = new Vector2(baseX * size + pt2.x * size, baseZ * size + pt2.y * size);
                    Vector3 doorPoint = new Vector2((absPoint1.x + absPoint2.x) / 2, (absPoint1.y + absPoint2.y) / 2);
                    doors.Add(Instantiate(door.GetComponent<Transform>(), new Vector3(doorPoint.x, 0, doorPoint.y), Quaternion.AngleAxis(90, new Vector3(0, 1, 0))).gameObject);

                }
            }
        }
    }
}
