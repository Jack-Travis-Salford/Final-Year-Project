using System.Collections.Generic;
using System.IO;
using Script;
using Script.Rooms;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.ProBuilder;

public class WorldBuilder : MonoBehaviour
{
    public bool showDebugLines;
    public bool showOnlyDebugLinesInUse;
    public bool showTileType;
    public bool showVisitedTiles;

    public bool showNotVisitedTiles;

    //Holds reference to empty objects, using for organisation in Hierarchy
    private GameObject[] _eastWallHolders;

    private Dictionary<double, GameObject> _floorInstances;

    //Array holding all info for grid. Row (0,x) & (x,0) are out of bounds
    private GridSegmentData[,] _gridData;
    private GameObject _mapHolder;
    private List<Room> _rooms;

    private GameObject[] _southWallHolders;

    //Holds a copy of each wall created of each width
    //Can be used to CreateInstance of wall, instead of creating a completely new object
    //Using ProBuilder API
    private Dictionary<double, GameObject> _wallInstances;

    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    public void Update()
    {
        DebugGridHelper();
    }

    public void CreateMap(StreamWriter sw)
    {
        sw.WriteLine("---Map Generation---");
        if (_mapHolder != null) Destroy(_mapHolder);
        Random.InitState(GeneratorGlobalVals.Instance.GetSeed());
        var startTime = Time.realtimeSinceStartup;
        var lastActionTime = startTime;
        float currentTime;
        InitialiseParameters();
        CreateWallHolders();
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Variable/Object holder preparation: " + (currentTime - lastActionTime));
        lastActionTime = currentTime;

        var roomGenerator = new RoomGenerator();
        roomGenerator.GenerateRooms(ref _gridData, ref _rooms);
        _rooms[0].isDestroyable = false;
        _rooms[1].isDestroyable = false;
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Rooms Generated: " + (currentTime - lastActionTime));
        lastActionTime = currentTime;
        sw.WriteLine("Total Rooms Generated: " + _rooms.Count);


        var doorGenerator = new DoorGenerator();
        doorGenerator.GenerateDoors(ref _gridData, ref _rooms);
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Doors Generated: " + (currentTime - lastActionTime));
        lastActionTime = currentTime;

        var totalRooms = _rooms.Count;
        RemoveDestroyedRooms();
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Destroy invalid rooms:" + (currentTime - lastActionTime));
        sw.WriteLine("Total destroyed rooms: " + (totalRooms - _rooms.Count));
        lastActionTime = currentTime;

        var corridorGenerator = new CorridorGenerator();
        corridorGenerator.GenerateCorridors(ref _gridData, ref _rooms);
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Corridor generation: " + (currentTime - lastActionTime));
        lastActionTime = currentTime;

        InterpretGridData();
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Grid to map interpreter: " + (currentTime - lastActionTime));
        lastActionTime = currentTime;

        DecorateRooms();
        currentTime = Time.realtimeSinceStartup;
        sw.WriteLine("Decorate rooms: " + (currentTime - lastActionTime));

        sw.WriteLine("---Generation Complete---");
        sw.WriteLine("Time taken:" + (currentTime - startTime));

        CheckIfRoomsAreConnected(_rooms[0].StartPos);
        var claimedTiles = 0;
        var accessibleTiles = 0;
        for (var i = 0; i < _gridData.GetLength(0); i++)
        for (var j = 0; j < _gridData.GetLength(1); j++)
            if (_gridData[i, j].RoomType != GeneratorGlobalVals.EMPTY)
            {
                claimedTiles++;
                if (_gridData[i, j].tileVisited) accessibleTiles++;
            }

        sw.WriteLine("Total Claimed Tiles: " + claimedTiles);
        sw.WriteLine("Tile accessible from spawn: " + accessibleTiles);
        sw.WriteLine("---End---");
    }

    private void DecorateRooms()
    {
        var objectHolder = new GameObject();
        objectHolder.name = "Room Objects";
        objectHolder.transform.parent = _mapHolder.transform;
        _rooms[0].Decorate(new Spawn(), objectHolder);
        _rooms[1].Decorate(new Exit(), objectHolder);
        var roomOptionDecider = new RoomOptionDecider();
        for (var x = 2; x < _rooms.Count; x++)
        {
            var chosenOption = roomOptionDecider.DecideRoom();
            _rooms[x].Decorate(chosenOption, objectHolder);
        }
    }

    private void CheckIfRoomsAreConnected(int[] startingPoint)
    {
        var toVisit = new Stack<int[]>();
        toVisit.Push(new[] { startingPoint[0], startingPoint[1] });
        while (toVisit.Count > 0)
        {
            var tilePos = toVisit.Pop();
            if (_gridData[tilePos[0], tilePos[1]].tileVisited) continue;

            _gridData[tilePos[0], tilePos[1]].tileVisited = true;
            //Check can visit north
            if (tilePos[0] > 0)
                if (_gridData[tilePos[0] - 1, tilePos[1]].RoomType != GeneratorGlobalVals.EMPTY &&
                    !_gridData[tilePos[0] - 1, tilePos[1]].tileVisited)
                    if (!_gridData[tilePos[0] - 1, tilePos[1]].SouthWall ||
                        _gridData[tilePos[0] - 1, tilePos[1]].SouthWallIsDoorway)
                    {
                        int[] newTilePos = { tilePos[0] - 1, tilePos[1] };
                        toVisit.Push(newTilePos);
                    }

            //Check can visit south
            if (tilePos[0] < _gridData.GetLength(0) - 1)
                if (_gridData[tilePos[0] + 1, tilePos[1]].RoomType != GeneratorGlobalVals.EMPTY &&
                    !_gridData[tilePos[0] + 1, tilePos[1]].tileVisited)
                    if (!_gridData[tilePos[0], tilePos[1]].SouthWall ||
                        _gridData[tilePos[0], tilePos[1]].SouthWallIsDoorway)
                    {
                        int[] newTilePos = { tilePos[0] + 1, tilePos[1] };
                        toVisit.Push(newTilePos);
                    }

            //Check can visit west
            if (tilePos[1] > 0)
                if (_gridData[tilePos[0], tilePos[1] - 1].RoomType != GeneratorGlobalVals.EMPTY &&
                    !_gridData[tilePos[0], tilePos[1] - 1].tileVisited)
                    if (!_gridData[tilePos[0], tilePos[1] - 1].EastWall ||
                        _gridData[tilePos[0], tilePos[1] - 1].EastWallIsDoorway)
                    {
                        int[] newTilePos = { tilePos[0], tilePos[1] - 1 };
                        toVisit.Push(newTilePos);
                    }

            //Check can visit east
            if (tilePos[1] < _gridData.GetLength(1) - 1)
                if (_gridData[tilePos[0], tilePos[1] + 1].RoomType != GeneratorGlobalVals.EMPTY &&
                    !_gridData[tilePos[0], tilePos[1] + 1].tileVisited)
                    if (!_gridData[tilePos[0], tilePos[1]].EastWall ||
                        _gridData[tilePos[0], tilePos[1]].EastWallIsDoorway)
                    {
                        int[] newTilePos = { tilePos[0], tilePos[1] + 1 };
                        toVisit.Push(newTilePos);
                    }
        }
    }

    private void InitialiseParameters()
    {
        showOnlyDebugLinesInUse = false;
        showDebugLines = false;
        showTileType = false;
        showVisitedTiles = false;
        showNotVisitedTiles = false;
        _rooms = null;
        _rooms = new List<Room>();
        _floorInstances = new Dictionary<double, GameObject>();
        var totalTiles = GeneratorGlobalVals.Instance.GetNoTiles();
        var totalTilesX = totalTiles[0];
        var totalTilesY = totalTiles[1];
        _gridData = null;
        _gridData = new GridSegmentData[totalTilesX, totalTilesY];
        for (var i = 0; i < _gridData.GetLength(0); i++)
        for (var j = 0; j < _gridData.GetLength(1); j++)
            _gridData[i, j] = new GridSegmentData();

        _wallInstances = new Dictionary<double, GameObject>();
    }

    private void RemoveDestroyedRooms()
    {
        for (var i = _rooms.Count - 1; i >= 0; i--)
            if (_rooms[i].isDestroyed)
                _rooms.RemoveAt(i);
    }

    /*
     * Interprets the compete array of GridSegmentData, generating objects where required
     */
    private void InterpretGridData()
    {
        //WALLS
        var wallMat = GeneratorGlobalVals.Instance.wallMat;
        var tileDimension = GeneratorGlobalVals.Instance.GetTileDimension();
        var wallDepth = GeneratorGlobalVals.Instance.GetWallDepth();
        var floorDepth = GeneratorGlobalVals.Instance.GetFloorDepth();
        var wallHeight = GeneratorGlobalVals.Instance.GetWallHeight();
        var wallYPos = wallHeight / 2 + floorDepth;
        //Draw south walls
        for (var x = 0; x < _gridData.GetLength(0); x++)
        for (var z = 0; z < _gridData.GetLength(1); z++)
        {
            if (!_gridData[x, z].SouthWall) continue;
            if (!_gridData[x, z].SouthWallIsDoorway)
            {
                var wallStart = z;
                var wallEnd = z + 1;
                var wallContinue = true;
                while (wallContinue && wallEnd < _gridData.GetLength(1))
                    if (_gridData[x, wallEnd].SouthWall && !_gridData[x, wallEnd].SouthWallIsDoorway)
                        wallEnd++;
                    else
                        wallContinue = false;
                var dimensions = new Vector3((wallEnd - wallStart) * tileDimension + wallDepth, wallHeight, wallDepth);
                var southWall = GetWall(dimensions);
                southWall.transform.position = new Vector3((x + 1) * tileDimension, wallYPos,
                    (wallEnd + wallStart) * (tileDimension / 2));
                southWall.transform.rotation = new Quaternion(0f, -90f, 0f, 90f);
                southWall.GetComponent<MeshRenderer>().material = wallMat;
                southWall.AddComponent<MeshCollider>();
                southWall.name = "SouthWall_" + x + "," + z + "-" + x + "," + (wallEnd - 1);
                southWall.transform.parent = _southWallHolders[x].transform;
                southWall.gameObject.isStatic = true;
                z = wallEnd - 1;
            }
            else
            {
                if (wallHeight <= GeneratorGlobalVals.Instance.GetDoorwayHeight()) continue;
                var wallStart = z;
                var wallEnd = z + 1;
                var wallContinue = true;
                while (wallContinue && wallEnd < _gridData.GetLength(1))
                    if (_gridData[x, wallEnd].SouthWallIsDoorway)
                        wallEnd++;
                    else
                        wallContinue = false;
                var dimensions = new Vector3((wallEnd - wallStart) * tileDimension + wallDepth,
                    wallHeight - GeneratorGlobalVals.Instance.GetDoorwayHeight(), wallDepth);
                var southWall = GetWall(dimensions);
                southWall.transform.position = new Vector3((x + 1) * tileDimension,
                    wallYPos + 0.5f * GeneratorGlobalVals.Instance.GetDoorwayHeight(),
                    (wallEnd + wallStart) * (tileDimension / 2));
                southWall.transform.rotation = new Quaternion(0f, -90f, 0f, 90f);
                southWall.GetComponent<MeshRenderer>().material = wallMat;
                southWall.AddComponent<MeshCollider>();
                southWall.name = "SouthDoor_" + x + "," + z + "-" + x + "," + (wallEnd - 1);
                southWall.transform.parent = _southWallHolders[x].transform;
                southWall.gameObject.isStatic = true;
                z = wallEnd - 1;
            }
        }

        //Draw east walls  array[x,z]
        for (var z = 0; z < _gridData.GetLength(1); z++)
        for (var x = 0; x < _gridData.GetLength(0); x++)
        {
            if (!_gridData[x, z].EastWall) continue;
            if (!_gridData[x, z].EastWallIsDoorway)
            {
                var wallStart = x;
                var wallEnd = x + 1;
                var wallContinue = true;
                while (wallContinue && wallEnd < _gridData.GetLength(0))
                    if (_gridData[wallEnd, z].EastWall && !_gridData[wallEnd, z].EastWallIsDoorway)
                    {
                        var validWallMerge = !(_gridData[wallEnd - 1, z].SouthWall &&
                                               !_gridData[wallEnd - 1, z].SouthWallIsDoorway);
                        if (validWallMerge && z < _gridData.GetLength(1) - 1)
                            validWallMerge = !(_gridData[wallEnd - 1, z + 1].SouthWall &&
                                               !_gridData[wallEnd - 1, z + 1].SouthWallIsDoorway);

                        if (validWallMerge)
                            wallEnd++;
                        else
                            wallContinue = false;
                    }
                    else
                    {
                        wallContinue = false;
                    }

                var dimensions = new Vector3((wallEnd - wallStart) * tileDimension - wallDepth, wallHeight, wallDepth);
                var eastWall = GetWall(dimensions);
                eastWall.transform.position = new Vector3((wallEnd + wallStart) * (tileDimension / 2), wallYPos,
                    (z + 1) * tileDimension);
                eastWall.GetComponent<MeshRenderer>().material = wallMat;
                eastWall.AddComponent<MeshCollider>();
                eastWall.name = "EastWall_" + +x + "," + z + "-" + (wallEnd - 1) + "," + z;
                eastWall.transform.parent = _eastWallHolders[z].transform;
                eastWall.gameObject.isStatic = true;
                x = wallEnd - 1;
            }
            else
            {
                //Draw Doorway
                if (wallHeight <= GeneratorGlobalVals.Instance.GetDoorwayHeight()) continue;
                var wallStart = x;
                var wallEnd = x + 1;
                var wallContinue = true;
                while (wallContinue && wallEnd < _gridData.GetLength(0))
                    if (_gridData[wallEnd, z].EastWallIsDoorway)
                    {
                        var validWallMerge = !(_gridData[wallEnd - 1, z].SouthWall &&
                                               !_gridData[wallEnd - 1, z].SouthWallIsDoorway);
                        if (validWallMerge && z < _gridData.GetLength(1) - 1)
                            validWallMerge = !(_gridData[wallEnd - 1, z + 1].SouthWall &&
                                               !_gridData[wallEnd - 1, z + 1].SouthWallIsDoorway);

                        if (validWallMerge)
                            wallEnd++;
                        else
                            wallContinue = false;
                    }
                    else
                    {
                        wallContinue = false;
                    }

                var dimensions = new Vector3((wallEnd - wallStart) * tileDimension + wallDepth,
                    wallHeight - GeneratorGlobalVals.Instance.GetDoorwayHeight(), wallDepth);
                var eastWall = GetWall(dimensions);
                eastWall.transform.position = new Vector3((wallEnd + wallStart) * (tileDimension / 2),
                    wallYPos + 0.5f * GeneratorGlobalVals.Instance.GetDoorwayHeight(), (z + 1) * tileDimension);
                eastWall.GetComponent<MeshRenderer>().material = wallMat;
                eastWall.AddComponent<MeshCollider>();
                eastWall.name = "EastDoorway" + +x + "," + z + "-" + (wallEnd - 1) + "," + z;
                eastWall.transform.parent = _eastWallHolders[z].transform;
                eastWall.gameObject.isStatic = true;
                x = wallEnd - 1;
            }
        }

        //FLOOR/CEILING
        var floor = ShapeGenerator.GeneratePlane(PivotLocation.FirstCorner, _gridData.GetLength(1) * tileDimension,
            _gridData.GetLength(0) * tileDimension, 0, 0, Axis.Up);
        floor.GetComponent<MeshRenderer>().material = GeneratorGlobalVals.Instance.floorMat;
        floor.AddComponent<MeshCollider>();
        floor.transform.position = new Vector3(0f, floorDepth, 0f);
        ;
        floor.transform.parent = _mapHolder.transform;

        var ceiling = ShapeGenerator.GeneratePlane(PivotLocation.FirstCorner, _gridData.GetLength(0) * tileDimension,
            _gridData.GetLength(1) * tileDimension, 0, 0, Axis.Down);
        ceiling.GetComponent<MeshRenderer>().material = GeneratorGlobalVals.Instance.ceilingMat;
        ceiling.AddComponent<MeshCollider>();
        ceiling.transform.position = new Vector3(0f, GeneratorGlobalVals.Instance.GetRoomHeight(), 0f);
        ceiling.transform.parent = _mapHolder.transform;

        //Draw planes for rooms
        //Fill for corridors
        /*foreach (var room in _rooms)
        {
            //Vector3 dimensions = new Vector3(room.RoomSize[0],, room.RoomSize[1]);
            //ProBuilderMesh floor = ShapeGenerator.GeneratePlane(PivotLocation.FirstCorner, room.RoomSize[1], room.RoomSize[0], 0, 0, Axis.Up);
            GameObject floor = GetFloor(new Vector2(room.RoomSize[1], room.RoomSize[0]));
            floor.GetComponent<MeshRenderer>().material = wallMat;
            floor.AddComponent<MeshCollider>();
            floor.transform.position = new Vector3(room.StartPos[0], floorDepth, room.StartPos[1]);
            for (int i = room.StartPos[0]; i < room.StartPos[0] + room.RoomSize[0]; i++)
            {
                for (int j = room.StartPos[1]; j < room.StartPos[1] + room.RoomSize[1]; j++)
                {
                    _gridData[i, j].floorAdded = true;
                }
            }
        }
 
        for (int x = 1; x < _gridData.GetLength(0); x++)
        {
            for (int y = 1; y < _gridData.GetLength(1); y++)
            {
                if (_gridData[x,y].RoomType != GeneratorGlobalVals.EMPTY && !_gridData[x,y].floorAdded)
                {
                    bool isXDirectionValid =  (y+GeneratorGlobalVals.Instance.GetCorridorWidthTiles() < _gridData.GetLength(1));
                    bool isYDirectionValid = true;
                    int xWidth = 0;
                    int yWidth = 0;
                    //Check for corridor width tiles free in y direction, see how far you can path in x direction
                    for (int i = 1; i < GeneratorGlobalVals.Instance.GetCorridorWidthTiles() && isXDirectionValid; i++)
                    {
                        if (_gridData[x,y+i].RoomType == GeneratorGlobalVals.EMPTY || _gridData[x,y+i].floorAdded)
                        {
                            isXDirectionValid = false;
                        }
 
                        if (isXDirectionValid)
                        {
                            xWidth = 1;
                            bool isXDirectionSTILLValid =true;
                    
                            while (isXDirectionSTILLValid && ((x+xWidth) < _gridData.GetLength(0)))
                            {
                                for (int j = 0; j < GeneratorGlobalVals.Instance.GetCorridorWidthTiles() && isXDirectionSTILLValid; j++)
                                {
                                    if (_gridData[x+xWidth,y+j].RoomType == GeneratorGlobalVals.EMPTY || _gridData[x+xWidth,y+j].floorAdded)
                                    {
                                        isXDirectionSTILLValid = false;
                                    }
                                }
                                if (isXDirectionSTILLValid)
                                {
                                    xWidth++;
                                }
                            }
                            
                            GameObject floor = GetFloor(new Vector2(GeneratorGlobalVals.Instance.GetCorridorWidthTiles()*tileDimension, xWidth*tileDimension));
                            floor.GetComponent<MeshRenderer>().material = wallMat;
                            floor.AddComponent<MeshCollider>();
                            floor.transform.position = new Vector3(x*tileDimension, floorDepth, y*tileDimension);
                            for (int currentXPos = x; currentXPos <= (x+xWidth); currentXPos++)
                            {
                                for (int j = 0; j < GeneratorGlobalVals.Instance.GetCorridorWidthTiles(); j++)
                                {
                                    _gridData[currentXPos, y+j].floorAdded = true;
                                }
                            } 
                            
                        }
                    }
                    //Check for corridor width tiles free in x direction, see how far you can path in y direction
 
                    //If none valid, throw error
                }
            }
        }*/
    }

    /*
     * Saves the original of all walls variations created.
     * Returns a clone of a wall of specified width, if one is present.
     * @dimensions - Width, Height, Depth
     */
    private GameObject GetWall(Vector3 dimensions)
    {
        float maxWallSize = GeneratorGlobalVals.MAX_WALL_SIZE_TILES;

        double key = dimensions.x + dimensions.y * maxWallSize + dimensions.z * maxWallSize * maxWallSize;
        if (_wallInstances.TryGetValue(key, out var value)) return Instantiate(value);
        var wall = ShapeGenerator.GenerateCube(PivotLocation.Center, dimensions);
        _wallInstances.Add(key, wall.gameObject);
        //Destroy(wall);
        Destroy(wall.gameObject);

        //return wall.gameObject;
        return Instantiate(_wallInstances.GetValueOrDefault(key));
    }

    /*
     * Saves the original of all floors variations created.
     * Returns a clone of a floor of specified width, if one is present.
     * @dimensions - Width, Height, Depth
     */
    private GameObject GetFloor(Vector2 dimensions)
    {
        float maxWallSize = GeneratorGlobalVals.MAX_WALL_SIZE_TILES;

        double key = dimensions.x + dimensions.y * maxWallSize;
        if (_wallInstances.TryGetValue(key, out var value)) return Instantiate(value);
        var floor = ShapeGenerator.GeneratePlane(PivotLocation.FirstCorner, dimensions.x, dimensions.y, 0, 0, Axis.Up);
        _wallInstances.Add(key, floor.gameObject);
        return floor.gameObject;
    }

    /**
     * Checks to see if tile is a destroyable room
     * If tile is not a room or
     */
    private bool IsRoomDestroyable(int[] pointInRoom)
    {
        return _gridData[pointInRoom[0], pointInRoom[1]].RoomType == 1 &&
               _gridData[pointInRoom[0], pointInRoom[1]].Room.isDestroyable;
    }
    /*
     * If room is invalid (doorway generation not possible), this function is called to remove said room
     */

/*
 * //Helps to ensure walls are being generated in the correct place.
 * ==For Walls==
 * Red line = Wall should be present
 * Black line = No wall should be present
 *
 *==Room Type indicators==
 * Diagonal Lines
 * Black - Invalid, something went wrong
 * Blue - A Room
 * Green - A Corridor
 */
    private void DebugGridHelper()
    {
        var tileDimension = GeneratorGlobalVals.Instance.GetTileDimension();
        var drawHeight = GeneratorGlobalVals.Instance.GetRoomHeight() + 0.5f;
        if (showDebugLines && _gridData != null)
            for (var x = 0; x < _gridData.GetLength(0); x++)
            for (var z = 0; z < _gridData.GetLength(1); z++)
            {
                var startPoint = new Vector3((x + 1) * tileDimension, drawHeight, (z + 1) * tileDimension);
                var endPoint = new Vector3(x * tileDimension, drawHeight, (z + 1) * tileDimension);
                if (_gridData[x, z].EastWall)
                {
                    if (!_gridData[x, z].EastWallIsDoorway)
                        Debug.DrawLine(startPoint, endPoint, Color.red);
                    else
                        Debug.DrawLine(startPoint, endPoint, Color.magenta);
                }
                else if (!showOnlyDebugLinesInUse)
                {
                    Debug.DrawLine(startPoint, endPoint, Color.black);
                }

                endPoint = new Vector3((x + 1) * tileDimension, drawHeight, z * tileDimension);
                if (_gridData[x, z].SouthWall)
                {
                    if (!_gridData[x, z].SouthWallIsDoorway)
                        Debug.DrawLine(startPoint, endPoint, Color.red);
                    else
                        Debug.DrawLine(startPoint, endPoint, Color.magenta);
                }
                else if (!showOnlyDebugLinesInUse)
                {
                    Debug.DrawLine(startPoint, endPoint, Color.black);
                }

                if (showTileType && _gridData[x, z].RoomType != -1)
                {
                    startPoint = new Vector3(x * tileDimension, drawHeight, z * tileDimension);
                    endPoint = new Vector3((x + 1) * tileDimension, drawHeight, (z + 1) * tileDimension);
                    switch (_gridData[x, z].RoomType)
                    {
                        case 0:
                            Debug.DrawLine(startPoint, endPoint, Color.green);
                            break;
                        case 1:
                            Debug.DrawLine(startPoint, endPoint, Color.blue);
                            break;
                        default:
                            Debug.DrawLine(startPoint, endPoint, Color.black);
                            break;
                    }
                }
            }

        if (showVisitedTiles || showNotVisitedTiles)
            for (var x = 0; x < _gridData.GetLength(0); x++)
            for (var z = 0; z < _gridData.GetLength(1); z++)
                if (_gridData[x, z].RoomType != GeneratorGlobalVals.EMPTY)
                {
                    var startPoint = new Vector3((x + 1) * tileDimension, drawHeight, z * tileDimension);
                    var endPoint = new Vector3(x * tileDimension, drawHeight, (z + 1) * tileDimension);
                    if (showVisitedTiles && _gridData[x, z].tileVisited)
                        Debug.DrawLine(startPoint, endPoint, Color.green);

                    if (showNotVisitedTiles && !_gridData[x, z].tileVisited)
                        Debug.DrawLine(startPoint, endPoint, Color.red);
                }
    }

    /*
     * Organisational purposes: Organises created objects in Hierarchy
     */
    private void CreateWallHolders()
    {
        _mapHolder = new GameObject();
        _mapHolder.name = "Generated Map";
        var wallHolder = new GameObject
        {
            name = "walls",
            transform =
            {
                position = Vector3.zero,
                parent = _mapHolder.transform
            },
            isStatic = true
        };
        var southWalls = new GameObject
        {
            name = "southWalls",
            transform =
            {
                position = Vector3.zero,
                parent = wallHolder.transform
            },
            isStatic = true
        };
        var eastWalls = new GameObject
        {
            name = "eastWalls",
            transform =
            {
                position = Vector3.zero,
                parent = wallHolder.transform
            },
            isStatic = true
        };
        _southWallHolders = new GameObject[_gridData.GetLength(0)];
        for (var i = 0; i < _gridData.GetLength(0); i++)
        {
            //south
            var southWallsSubCat = new GameObject
            {
                name = "row_" + i,
                transform =
                {
                    position = Vector3.zero,
                    parent = southWalls.transform
                },
                isStatic = true
            };
            _southWallHolders[i] = southWallsSubCat;
        }

        _eastWallHolders = new GameObject[_gridData.GetLength(1)];
        for (var i = 0; i < _gridData.GetLength(1); i++)
        {
            //south
            var eastWallsSubCat = new GameObject
            {
                name = "row_" + i,
                transform =
                {
                    position = Vector3.zero,
                    parent = eastWalls.transform
                },
                isStatic = true
            };
            _eastWallHolders[i] = eastWallsSubCat;
        }
    }
}