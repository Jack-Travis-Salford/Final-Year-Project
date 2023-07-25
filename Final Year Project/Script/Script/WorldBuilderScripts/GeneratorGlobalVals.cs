using System;
using System.Collections.Generic;
using UnityEngine;

public class GeneratorGlobalVals : MonoBehaviour
{
    //Int values for direction
    public const int NORTH = 0;
    public const int EAST = 1;
    public const int SOUTH = 2;

    public const int WEST = 3;

    //Int values for Room types
    public const int EMPTY = -1;
    public const int CORRIDOR = 0;

    public const int ROOM = 1;

    //Int values for corridor segment options
    public const int CORRIDOR_END = 0;
    public const int STRAIGHT = 1;
    public const int LEFT_TURN = 2;
    public const int RIGHT_TURN = 3;
    public const int LEFT_UP_SPLIT = 4;
    public const int RIGHT_UP_SPLIT = 5;
    public const int LEFT_RIGHT_SPLIT = 6;
    public const int LEFT_UP_RIGHT_SPLIT = 7;

    public static GeneratorGlobalVals Instance;

    //For providing a unique id to wall variations, to allow for cloning
    public static readonly int MAX_WALL_SIZE_TILES = 5000;

    //Wall Material
    public Material wallMat;

    public Material floorMat;

    public Material ceilingMat;
    private float[] _cornerChance;
    private float _corridorWidth;
    private int _corridorWidthTiles;

    private float _doorwayHeight;

    //The width and height of the doorway (section user walks though)
    private float _doorwayWidth;
    private int _doorwayWidthTiles;
    private int[] _endRoomSizeTilesBounds;
    private int[] _endRoomSizeUnitsBounds;

    private float _floorDepth;

    //targeted % of map to be filled with rooms
    private float _mapPercentToBeRooms;

    //Total tiles per plane (_sizeUnits / _tileDimension)
    private int[] _noTiles;
    private float _roomHeight;

    private int[,] _roomSizeTilesBounds;

    //How many grid segments a room can be. Size 2,2
    //[0,0] = minX [0,1] = maxX
    //[1,0] = minY [1,1] = maxY 
    private float[,] _roomSizeUnitsBounds;

    //Translates what the int value for roomType is in terms of English
    private Dictionary<string, int> _roomTypes;

    //Size of grid in Unity units
    private Vector2 _sizeUnits;

    private int[] _startRoomSizeTilesBounds;

    //Allows to specify exact size of start and end room
    //Every generation must have a start and end room
    private int[] _startRoomSizeUnitsBounds;


    //Corridors
    private float[] _straightChance;

    private float[] _threeWaySplitChance;

    //Size of each tile
    private float _tileDimension;
    private float[] _twoWaySplitChance;
    private float _wallDepth;
    private float _wallHeight;

    private int seed;
    public Vector3 _worldSpawnLocation { private set; get; }

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(this);
        SetChangeableParameters();
        SetNonChangeableParameters();
        CreateRoomTypeDictionary();
    }
    //Can be altered in code

    private void SetChangeableParameters()
    {
        _sizeUnits = new Vector2(150, 150);
        _tileDimension = 1f; //1f  (currently breaks build if changed
        _floorDepth = 0.001f; //0.1f 
        _wallDepth = 0.25f; //0.25f
        _doorwayWidth = 1.5f; //2f
        _doorwayHeight = 2.5f; //2.5f
        _corridorWidth = 3f; //3f
        _mapPercentToBeRooms = .1f; //.05f
        _roomHeight = 3f; //4f
        //{{minX,maxX},{minY,maxY}}
        _roomSizeUnitsBounds = new float[2, 2] { { 5, 15 }, { 5, 15 } }; //{5,15},{5,15}
        _startRoomSizeUnitsBounds = new[] { 5, 5 }; // {10,5}
        _endRoomSizeUnitsBounds = new[] { 5, 5 }; // {5,5}
        _straightChance = new[] { .3f, 1.5f };
        _cornerChance = new[] { .1f, .3f };
        _twoWaySplitChance = new[] { .005f, 0.03f };
        _threeWaySplitChance = new[] { .005f, 0.01f };
        seed = 0;
    }

    //Do not alter directly!
    public void SetNonChangeableParameters()
    {
        //Calc room sizes in grid units, not unity units
        _tileDimension = Mathf.Max(_tileDimension, _wallDepth);
        _roomSizeTilesBounds = new int[2, 2];
        _startRoomSizeTilesBounds = new int[2];
        _endRoomSizeTilesBounds = new int[2];
        _wallHeight = _roomHeight - _floorDepth;
        _roomSizeUnitsBounds[0, 0] = Math.Min(_roomSizeUnitsBounds[0, 0], _roomSizeUnitsBounds[1, 0]);
        _roomSizeUnitsBounds[0, 0] = Math.Max(1, _roomSizeUnitsBounds[0, 0]);
        _roomSizeUnitsBounds[1, 0] = Math.Max(_roomSizeUnitsBounds[0, 0], _roomSizeUnitsBounds[1, 0]);
        _roomSizeUnitsBounds[0, 1] = Math.Min(_roomSizeUnitsBounds[0, 1], _roomSizeUnitsBounds[1, 1]);
        _roomSizeUnitsBounds[0, 1] = Math.Max(1, _roomSizeUnitsBounds[0, 1]);
        _roomSizeUnitsBounds[1, 1] = Math.Max(_roomSizeUnitsBounds[0, 1], _roomSizeUnitsBounds[1, 1]);
        for (var i = 0; i < _roomSizeUnitsBounds.GetLength(0); i++)
        {
            var noOfTiles = _startRoomSizeUnitsBounds[i] / _tileDimension;
            _startRoomSizeTilesBounds[i] = Mathf.Max(1, Mathf.RoundToInt(noOfTiles));
            noOfTiles = _endRoomSizeUnitsBounds[i] / _tileDimension;
            _endRoomSizeTilesBounds[i] = Mathf.Max(1, Mathf.RoundToInt(noOfTiles));
            for (var j = 0; j < _roomSizeUnitsBounds.GetLength(1); j++)
            {
                noOfTiles = _roomSizeUnitsBounds[i, j] / _tileDimension;
                _roomSizeTilesBounds[i, j] = Mathf.Max(1, Mathf.RoundToInt(noOfTiles));
            }
        }

        //+1 To include North and West border
        var noTilesX = Mathf.FloorToInt(_sizeUnits.x / _tileDimension) + 1;
        var noTilesZ = Mathf.FloorToInt(_sizeUnits.y / _tileDimension) + 1;
        //Ensure start and end room can be created
        noTilesX = Math.Max(noTilesX, _startRoomSizeTilesBounds[0] + 2 * _endRoomSizeTilesBounds[0] + 1);
        noTilesZ = Math.Max(noTilesZ, _startRoomSizeTilesBounds[1] + 1);
        noTilesZ = Math.Max(noTilesZ, _endRoomSizeTilesBounds[1] + 1);
        _noTiles = new int[2] { noTilesX, noTilesZ };
        //Make sure the doorway can fit in the wall and aligns with grid
        _doorwayHeight = MathF.Min(_doorwayHeight, _roomHeight);
        _doorwayWidthTiles = Mathf.FloorToInt(_doorwayWidth / _tileDimension);
        _doorwayWidthTiles = Math.Max(1, _doorwayWidthTiles);
        _corridorWidthTiles = Mathf.FloorToInt(_corridorWidth / _tileDimension);
        _corridorWidthTiles = Math.Max(1, _corridorWidthTiles);
        if (_doorwayWidth % _tileDimension != 0)
        {
            _doorwayWidthTiles++;
            _doorwayWidth = _doorwayWidthTiles * _tileDimension;
        }
    }

    private void CreateRoomTypeDictionary()
    {
        _roomTypes = new Dictionary<string, int>();
        _roomTypes.Add("Empty", -1);
        _roomTypes.Add("Corridor", 0);
        _roomTypes.Add("Room", 1);
    }
    
    public int GetCorridorWidthTiles()
    {
        return _corridorWidthTiles;
    }

    public int[] GetNoTiles()
    {
        return _noTiles.Clone() as int[];
    }

    public int[] GetStartRoomSizeTilesBounds()
    {
        return _startRoomSizeTilesBounds.Clone() as int[];
    }

    public int[] GetEndRoomSizeTilesBounds()
    {
        return _endRoomSizeTilesBounds.Clone() as int[];
    }

    public float[,] GetRoomSizeUnitsBounds()
    {
        return _roomSizeUnitsBounds.Clone() as float[,];
    }

    public int[,] GetRoomSizeTilesBounds()
    {
        return _roomSizeTilesBounds.Clone() as int[,];
    }

    public Vector2 GetSizeUnits()
    {
        return _sizeUnits;
    }

    public float GetMapPercentToBeRooms()
    {
        return _mapPercentToBeRooms;
    }

    public float GetWallDepth()
    {
        return _wallDepth;
    }

    public float GetFloorDepth()
    {
        return _floorDepth;
    }

    public float GetDoorwayWidth()
    {
        return _doorwayWidth;
    }

    public float GetDoorwayHeight()
    {
        return _doorwayHeight;
    }

    public float GetWallHeight()
    {
        return _wallHeight;
    }

    public int GetDoorwayWidthTiles()
    {
        return _doorwayWidthTiles;
    }

    public float GetTileDimension()
    {
        return _tileDimension;
    }

    public float GetRoomHeight()
    {
        return _roomHeight;
    }

    public void SetWorldSpawn(Vector3 spawnLocation)
    {
        _worldSpawnLocation = spawnLocation;
    }

    public float GetCorridorWidthUnits()
    {
        return _corridorWidth;
    }

    public float[] GetStraightChance()
    {
        return _straightChance.Clone() as float[];
    }

    public float[] GetCornerChance()
    {
        return _cornerChance.Clone() as float[];
    }

    public float[] GetTwoWayChance()
    {
        return _twoWaySplitChance.Clone() as float[];
    }

    public float[] GetThreeWayChance()
    {
        return _threeWaySplitChance.Clone() as float[];
    }

    public void SetSeed(int seed)
    {
        this.seed = seed;
    }

    public void SetMapSize(Vector2 mapSize)
    {
        _sizeUnits = mapSize;
    }

    public void SetRoomSizeBounds(float[,] roomSizeBounds)
    {
        _roomSizeUnitsBounds = roomSizeBounds.Clone() as float[,];
    }

    public void SetTileSize(float tileSize)
    {
        _tileDimension = tileSize;
    }

    public void SetCorridorWidth(float corridorWidth)
    {
        _corridorWidth = corridorWidth;
    }

    public void SetMapPercentToBeRooms(float value)
    {
        _mapPercentToBeRooms = value;
    }

    public void SetRoomHeight(float roomHeight)
    {
        _roomHeight = roomHeight;
    }

    public void SetWallDepth(float wallDepth)
    {
        _wallDepth = wallDepth;
    }

    public void SetDoorwayWidth(float doorwayWidth)
    {
        _doorwayWidth = doorwayWidth;
    }

    public void SetDoorwayHeight(float doorwayHeight)
    {
        _doorwayHeight = doorwayHeight;
    }

    public void SetStraightChance(float[] straightChance)
    {
        _straightChance = straightChance.Clone() as float[];
    }

    public void SetCornerChance(float[] cornerChance)
    {
        _cornerChance = cornerChance.Clone() as float[];
    }

    public void SetTwoWayChance(float[] twoWayChance)
    {
        _twoWaySplitChance = twoWayChance.Clone() as float[];
    }

    public void SetThreeWayChance(float[] threeWayChance)
    {
        _threeWaySplitChance = threeWayChance.Clone() as float[];
    }

    public int GetSeed()
    {
        return seed;
    }
}