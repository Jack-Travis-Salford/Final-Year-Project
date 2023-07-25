using System;
using System.IO;
using TMPro;
using UnityEngine;

public class MapGeneratorHandler : MonoBehaviour
{
    private float CorridorWidth;
    private float DoorwayHeight;


    private float DoorwayWidth;
    private float EmptyRoomChance;
    private float ExitRoomChance;
    private float LrgChestRoomChance;
    private float MapPercentToBeRooms;
    private Vector2 MapSize;
    private float RoomHeight;
    private float[,] RoomSize;
    private float SmChestRoomChance;
    private float[] StraightChance;
    private float[] ThreeWayChance;
    private float TileSize;
    private float[] TurnChance;
    private float[] TwoWayChance;
    private float WallDepth;

    private WorldBuilder worldBuilder;
    [field: SerializeField] public TMP_InputField IFseed { private set; get; }
    [field: SerializeField] public TMP_InputField IFmapSizeX { private set; get; }
    [field: SerializeField] public TMP_InputField IFmapSizeZ { private set; get; }
    [field: SerializeField] public TMP_InputField IFtileSize { private set; get; }
    [field: SerializeField] public TMP_InputField IFroomSizeMinX { private set; get; }
    [field: SerializeField] public TMP_InputField IFroomSizeMinZ { private set; get; }
    [field: SerializeField] public TMP_InputField IFroomSizeMaxX { private set; get; }
    [field: SerializeField] public TMP_InputField IFroomSizeMaxZ { private set; get; }
    [field: SerializeField] public TMP_InputField IFcorridorWidth { private set; get; }
    [field: SerializeField] public TMP_InputField IFmapPercentToBeRooms { private set; get; }
    [field: SerializeField] public TMP_InputField IFroomHeight { private set; get; }
    [field: SerializeField] public TMP_InputField IFwallDepth { private set; get; }
    [field: SerializeField] public TMP_InputField IFdoorwayWidth { private set; get; }
    [field: SerializeField] public TMP_InputField IFdoorwayHeight { private set; get; }
    [field: SerializeField] public TMP_InputField IFemptyRoomChance { private set; get; }
    [field: SerializeField] public TMP_InputField IFexitRoomChance { private set; get; }
    [field: SerializeField] public TMP_InputField IFsm_chest_room { private set; get; }
    [field: SerializeField] public TMP_InputField IFlrg_chest_room { private set; get; }
    [field: SerializeField] public TMP_InputField IFstraight_min { private set; get; }
    [field: SerializeField] public TMP_InputField IFstraight_max { private set; get; }
    [field: SerializeField] public TMP_InputField IFturn_min { private set; get; }
    [field: SerializeField] public TMP_InputField IFturn_max { private set; get; }
    [field: SerializeField] public TMP_InputField IFtwo_way_min { private set; get; }
    [field: SerializeField] public TMP_InputField IFtwo_way_max { private set; get; }
    [field: SerializeField] public TMP_InputField IFthree_way_min { private set; get; }

    [field: SerializeField] public TMP_InputField IFthree_way_max { private set; get; }

    // Start is called before the first frame update
    private void Start()
    {
        worldBuilder = GameObject.FindGameObjectWithTag("GlobalSettings").GetComponent<WorldBuilder>();
        MapSize = GeneratorGlobalVals.Instance.GetSizeUnits();
        IFmapSizeX.text = MapSize[0].ToString();
        IFmapSizeZ.text = MapSize[1].ToString();

        RoomSize = GeneratorGlobalVals.Instance.GetRoomSizeUnitsBounds();
        IFroomSizeMinX.text = RoomSize[0, 0].ToString();
        IFroomSizeMinZ.text = RoomSize[0, 1].ToString();
        IFroomSizeMaxX.text = RoomSize[1, 0].ToString();
        IFroomSizeMaxZ.text = RoomSize[1, 1].ToString();

        TileSize = GeneratorGlobalVals.Instance.GetTileDimension();
        IFtileSize.text = GeneratorGlobalVals.Instance.GetTileDimension().ToString();

        CorridorWidth = GeneratorGlobalVals.Instance.GetCorridorWidthUnits();
        IFcorridorWidth.text = CorridorWidth.ToString();

        MapPercentToBeRooms = GeneratorGlobalVals.Instance.GetMapPercentToBeRooms();
        IFmapPercentToBeRooms.text = MapPercentToBeRooms.ToString();

        RoomHeight = GeneratorGlobalVals.Instance.GetRoomHeight();
        IFroomHeight.text = RoomHeight.ToString();

        WallDepth = GeneratorGlobalVals.Instance.GetWallDepth();
        IFwallDepth.text = WallDepth.ToString();

        DoorwayWidth = GeneratorGlobalVals.Instance.GetDoorwayWidth();
        IFdoorwayWidth.text = DoorwayWidth.ToString();

        DoorwayHeight = GeneratorGlobalVals.Instance.GetDoorwayHeight();
        IFdoorwayHeight.text = DoorwayHeight.ToString();

        EmptyRoomChance = WorldObjects.Instance.emptyChance;
        IFemptyRoomChance.text = EmptyRoomChance.ToString();

        ExitRoomChance = WorldObjects.Instance.exitChance;
        IFexitRoomChance.text = ExitRoomChance.ToString();

        SmChestRoomChance = WorldObjects.Instance.smChestRoomChance;
        IFsm_chest_room.text = SmChestRoomChance.ToString();

        LrgChestRoomChance = WorldObjects.Instance.lrgChestRoomChance;
        IFlrg_chest_room.text = LrgChestRoomChance.ToString();

        StraightChance = GeneratorGlobalVals.Instance.GetStraightChance();
        IFstraight_min.text = StraightChance[0].ToString();
        IFstraight_max.text = StraightChance[1].ToString();

        TurnChance = GeneratorGlobalVals.Instance.GetCornerChance();
        IFturn_min.text = TurnChance[0].ToString();
        IFturn_max.text = TurnChance[1].ToString();

        TwoWayChance = GeneratorGlobalVals.Instance.GetTwoWayChance();
        IFtwo_way_min.text = TwoWayChance[0].ToString();
        IFtwo_way_max.text = TwoWayChance[1].ToString();

        ThreeWayChance = GeneratorGlobalVals.Instance.GetThreeWayChance();
        IFthree_way_min.text = ThreeWayChance[0].ToString();
        IFthree_way_max.text = ThreeWayChance[1].ToString();
    }

    public void GenerateMap()
    {
        var fileName = "MapConfigs\\";
        if (!Directory.Exists(fileName)) Directory.CreateDirectory(fileName);
        fileName += DateTime.Now.ToString("dd-MM-yyyy hh-mm-ss") + ".txt";
        var sw = new StreamWriter(fileName);

        int seed;
        if (IFseed.text.Length == 0)
        {
            seed = (int)DateTime.Now.Ticks;
        }
        else
        {
            var wantedLength = Math.Min(10, IFseed.text.Length);
            seed = long.TryParse(IFseed.text.Substring(0, wantedLength), out var result)
                ? (int)result
                : (int)DateTime.Now.Ticks;
        }

        GeneratorGlobalVals.Instance.SetSeed(seed);
        sw.WriteLine("Seed: " + seed);

        var mapSize = new Vector2();
        if (IFmapSizeX.text.Length == 0)
        {
            mapSize[0] = MapSize.x;
            IFmapSizeX.text = MapSize.x.ToString();
        }
        else
            mapSize[0] = float.TryParse(IFmapSizeX.text, out var result) ? result : MapSize.x;

        if (IFmapSizeZ.text.Length == 0)
        {
            mapSize[1] = MapSize.y;
            IFmapSizeZ.text = MapSize.y.ToString();
        }
        else
            mapSize[1] = float.TryParse(IFmapSizeZ.text, out var result) ? result : MapSize.y;
        GeneratorGlobalVals.Instance.SetMapSize(mapSize);
        sw.WriteLine("Map Size: " + mapSize.x + "," + mapSize.y);

        var roomSize = new float[2, 2];
        if (IFroomSizeMinX.text.Length == 0)
        {
            roomSize[0, 0] = RoomSize[0, 0];
            IFroomSizeMinX.text = RoomSize[0, 0].ToString();
        }
        else
            roomSize[0, 0] = float.TryParse(IFroomSizeMinX.text, out var result) ? result : RoomSize[0, 0];

        if (IFroomSizeMinZ.text.Length == 0)
        {
            roomSize[0, 1] = RoomSize[0, 1];
            IFroomSizeMinZ.text = RoomSize[0, 1].ToString();
        }
        else
            roomSize[0, 1] = float.TryParse(IFroomSizeMinZ.text, out var result) ? result : RoomSize[0, 1];

        if (IFroomSizeMaxX.text.Length == 0)
        {
            roomSize[1, 0] = RoomSize[1, 0];
            IFroomSizeMaxX.text = RoomSize[1, 0].ToString();
        }
        else
            roomSize[1, 0] = float.TryParse(IFroomSizeMaxX.text, out var result) ? result : RoomSize[1, 0];

        if (IFroomSizeMaxZ.text.Length == 0)
        {
            roomSize[1, 1] = RoomSize[1, 1];
            IFroomSizeMaxZ.text = RoomSize[1, 1].ToString();
        }
        else
            roomSize[1, 1] = float.TryParse(IFroomSizeMaxZ.text, out var result) ? result : RoomSize[1, 1];
        GeneratorGlobalVals.Instance.SetRoomSizeBounds(roomSize);
        sw.WriteLine("Room Size Bounds: [" + roomSize[0, 0] + "," + roomSize[0, 1] + "],[" + roomSize[1, 0] + "," +
                     roomSize[1, 1] + "]");

        float tileSize;
        if (IFtileSize.text.Length == 0)
        {
            tileSize = TileSize;
            IFtileSize.text = TileSize.ToString();
        }
        else
            tileSize = float.TryParse(IFtileSize.text, out var result) ? result : TileSize;
        GeneratorGlobalVals.Instance.SetTileSize(tileSize);
        sw.WriteLine("Tile Size: " + tileSize);

        float corridorWidth;
        if (IFcorridorWidth.text.Length == 0)
        {
            corridorWidth = CorridorWidth;
            IFcorridorWidth.text = CorridorWidth.ToString();
        }
        else
            corridorWidth = float.TryParse(IFcorridorWidth.text, out var result) ? result : CorridorWidth;
        GeneratorGlobalVals.Instance.SetCorridorWidth(corridorWidth);
        sw.WriteLine("Corridor Width: " + corridorWidth);

        float mapPercentToBeRooms;
        if (IFmapPercentToBeRooms.text.Length == 0)
        {
            mapPercentToBeRooms = MapPercentToBeRooms;
            IFmapPercentToBeRooms.text = MapPercentToBeRooms.ToString();
        }
        else
            mapPercentToBeRooms = float.TryParse(IFmapPercentToBeRooms.text, out var result)
                ? result
                : MapPercentToBeRooms;
        GeneratorGlobalVals.Instance.SetMapPercentToBeRooms(mapPercentToBeRooms);
        sw.WriteLine("Map % to be rooms: " + mapPercentToBeRooms);

        float roomHeight;
        if (IFroomHeight.text.Length == 0)
        {
            roomHeight = RoomHeight;
            IFroomHeight.text = RoomHeight.ToString();
        }
        else
            roomHeight = float.TryParse(IFroomHeight.text, out var result) ? result : RoomHeight;
        GeneratorGlobalVals.Instance.SetRoomHeight(roomHeight);
        sw.WriteLine("Room Height: " + roomHeight);

        float wallDepth;
        if (IFwallDepth.text.Length == 0)
        {
            wallDepth = WallDepth;
            IFwallDepth.text = WallDepth.ToString();
        }
        else
            wallDepth = float.TryParse(IFwallDepth.text, out var result) ? result : WallDepth;
        GeneratorGlobalVals.Instance.SetWallDepth(wallDepth);
        sw.WriteLine("Wall Depth: " + wallDepth);

        float doorwayWidth;
        if (IFdoorwayWidth.text.Length == 0)
        {
            doorwayWidth = DoorwayWidth;
            IFdoorwayWidth.text = DoorwayWidth.ToString();
        }
        else
            doorwayWidth = float.TryParse(IFdoorwayWidth.text, out var result) ? result : DoorwayWidth;
        GeneratorGlobalVals.Instance.SetDoorwayWidth(doorwayWidth);
        sw.WriteLine("Doorway Width: " + doorwayWidth);

        float doorwayHeight;
        if (IFdoorwayHeight.text.Length == 0)
        {
            doorwayHeight = DoorwayHeight;
            IFdoorwayHeight.text = DoorwayHeight.ToString();
        }
        else
            doorwayHeight = float.TryParse(IFdoorwayHeight.text, out var result) ? result : DoorwayHeight;
        GeneratorGlobalVals.Instance.SetDoorwayHeight(doorwayHeight);
        sw.WriteLine("Doorway Height: " + doorwayHeight);

        float emptyRoomChance;
        if (IFemptyRoomChance.text.Length == 0)
        {
            emptyRoomChance = EmptyRoomChance;
            IFemptyRoomChance.text = EmptyRoomChance.ToString();
        }
        else
            emptyRoomChance = float.TryParse(IFemptyRoomChance.text, out var result) ? result : EmptyRoomChance;

        WorldObjects.Instance.emptyChance = emptyRoomChance;
        sw.WriteLine("Empty Room Chance: " + emptyRoomChance);

        float exitRoomChance;
        if (IFexitRoomChance.text.Length == 0)
        {
            exitRoomChance = ExitRoomChance;
            IFexitRoomChance.text = ExitRoomChance.ToString();
        }
        else
            exitRoomChance = float.TryParse(IFexitRoomChance.text, out var result) ? result : ExitRoomChance;
        WorldObjects.Instance.exitChance = exitRoomChance;
        sw.WriteLine("Exit room chance: " + exitRoomChance);

        float smChestRoomChance;
        if (IFsm_chest_room.text.Length == 0)
        {
            smChestRoomChance = SmChestRoomChance;
            IFsm_chest_room.text = SmChestRoomChance.ToString();
        }
        else
            smChestRoomChance = float.TryParse(IFsm_chest_room.text, out var result) ? result : SmChestRoomChance;
        WorldObjects.Instance.smChestRoomChance = smChestRoomChance;
        sw.WriteLine("Small Chest Room Chance: " + smChestRoomChance);

        float lrgChestRoomChance;
        if (IFlrg_chest_room.text.Length == 0)
        {
            lrgChestRoomChance = LrgChestRoomChance;
            IFlrg_chest_room.text = LrgChestRoomChance.ToString();
        }
        else
            lrgChestRoomChance = float.TryParse(IFlrg_chest_room.text, out var result) ? result : LrgChestRoomChance;
        WorldObjects.Instance.lrgChestRoomChance = lrgChestRoomChance;
        sw.WriteLine("Large Chest Room Chance " + lrgChestRoomChance);

        var straightChance = new float[2];
        if (IFstraight_min.text.Length == 0)
        {
            straightChance[0] = StraightChance[0];
            IFstraight_min.text = StraightChance[0].ToString();
        }
        else
            straightChance[0] = float.TryParse(IFstraight_min.text, out var result) ? result : StraightChance[0];

        if (IFstraight_max.text.Length == 0)
        {
            straightChance[1] = StraightChance[1];
            IFstraight_max.text = StraightChance[1].ToString();
        }
        else
            straightChance[1] = float.TryParse(IFstraight_max.text, out var result) ? result : StraightChance[1];
        GeneratorGlobalVals.Instance.SetStraightChance(straightChance);
        sw.WriteLine("Straight chance: [" + straightChance[0] + "," + straightChance[1] + "]");

        var cornerChance = new float[2];
        if (IFturn_min.text.Length == 0)
        {
            cornerChance[0] = TurnChance[0];
            IFturn_min.text = TurnChance[0].ToString();
        }
        else
            cornerChance[0] = float.TryParse(IFturn_min.text, out var result) ? result : TurnChance[0];

        if (IFturn_max.text.Length == 0)
        {
            cornerChance[1] = TurnChance[1];
            IFturn_max.text = TurnChance[1].ToString();
        }
        else
            cornerChance[1] = float.TryParse(IFturn_max.text, out var result) ? result : TurnChance[1];
        GeneratorGlobalVals.Instance.SetCornerChance(cornerChance);
        sw.WriteLine("Corner Chance: [" + cornerChance[0] + "," + cornerChance[1] + "]");

        var twoWayChance = new float[2];
        if (IFtwo_way_min.text.Length == 0)
        {
            twoWayChance[0] = TwoWayChance[0];
            IFtwo_way_min.text = TwoWayChance[0].ToString();
        }
        else
            twoWayChance[0] = float.TryParse(IFtwo_way_min.text, out var result) ? result : TwoWayChance[0];

        if (IFtwo_way_max.text.Length == 0)
        {
            twoWayChance[1] = TwoWayChance[1];
            IFtwo_way_max.text = TwoWayChance[1].ToString();
        }
        else
            twoWayChance[1] = float.TryParse(IFtwo_way_max.text, out var result) ? result : TwoWayChance[1];
        GeneratorGlobalVals.Instance.SetTwoWayChance(twoWayChance);
        sw.WriteLine("Two way chance: [" + twoWayChance[0] + "," + twoWayChance[1] + "]");

        var threeWayChance = new float[2];
        if (IFthree_way_min.text.Length == 0)
        {
            threeWayChance[0] = ThreeWayChance[0];
            IFthree_way_min.text = ThreeWayChance[0].ToString();
        }
        else
            threeWayChance[0] = float.TryParse(IFthree_way_min.text, out var result) ? result : ThreeWayChance[0];

        if (IFthree_way_max.text.Length == 0)
        {
            threeWayChance[1] = ThreeWayChance[1];
            IFthree_way_max.text = ThreeWayChance[1].ToString();
        }
        else
            threeWayChance[1] = float.TryParse(IFthree_way_max.text, out var result) ? result : ThreeWayChance[1];
        GeneratorGlobalVals.Instance.SetThreeWayChance(threeWayChance);
        sw.WriteLine("Three way chance: [" + threeWayChance[0] + "," + threeWayChance[1] + "]");
        GeneratorGlobalVals.Instance.SetNonChangeableParameters();
        worldBuilder.CreateMap(sw);
        sw.Close();
    }
}