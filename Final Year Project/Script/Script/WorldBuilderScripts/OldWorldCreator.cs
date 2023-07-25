using UnityEngine;

public class OldWorldCreator : MonoBehaviour
{
    // Start is called before the first frame update
    private GameObject _floor;
    private float _floorWidth;
    private int _generatedRooms;
    private GameObject _mapHolder;
    private Vector2 _offset;
    private float _roomHeight;
    private GameObject _roomHolder;
    private Vector2 _roomSize;
    private GameObject _wallEast;
    private GameObject _wallNorth;
    private GameObject _wallSouth;
    private GameObject _wallWest;

    private float _wallWidth;

    [field: SerializeField] public Material MatWallN { get; private set; }
    [field: SerializeField] public Material MatWallE { get; private set; }
    [field: SerializeField] public Material MatWallS { get; private set; }
    [field: SerializeField] public Material MatWallW { get; private set; }

    private float wallHeight => _roomHeight - 2 * _floorWidth;

    private void Start()
    {
        _mapHolder = new GameObject
        {
            name = "Map",
            transform =
            {
                position = Vector3.zero
            },
            isStatic = true
        };

        _roomHolder = new GameObject
        {
            name = "Rooms",
            transform =
            {
                position = Vector3.zero
            },
            isStatic = true
        };
        _roomHolder.transform.SetParent(_mapHolder.transform);


        _wallWidth = 0.3f;
        _floorWidth = 0.1f;
        _roomHeight = 4f;
        _roomSize = new Vector2(1f, 1f); //length in x direction, length in z direction
        _offset = Vector2.zero;

        CreateRoom();


        _roomSize = new Vector2(15, 15);
        _offset = new Vector2(1f, 0f);
        CreateRoom();
    }

    private void CreateRoom()
    {
        var room = new GameObject
        {
            name = "Room_" + _generatedRooms,
            transform =
            {
                position = Vector3.zero
            },
            isStatic = true
        };
        room.transform.SetParent(_roomHolder.transform);
        CreateFloor(room.transform);
        CreateNorthWall(room.transform);
        CreateSouthWall(room.transform);
        CreateEastWall(room.transform);
        CreateWestWall(room.transform);

        _generatedRooms++;
    }

    private void CreateFloor(Transform parent)
    {
        _floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _floor.name = "Floor";
        _floor.transform.position =
            new Vector3(_roomSize.x / 2 + _offset.x, _floorWidth / 2, _roomSize.y / 2 + _offset.y);
        _floor.transform.localScale = new Vector3(_roomSize.x, _floorWidth, _roomSize.y);
        _floor.transform.SetParent(parent);
    }

    private void CreateNorthWall(Transform parent)
    {
        _wallNorth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wallNorth.name = "WallNorth";
        _wallNorth.transform.rotation = new Quaternion(0f, 90f, 0f, 90f);
        _wallNorth.transform.position = new Vector3(_roomSize.x + _offset.x - 0.5f * _wallWidth,
            wallHeight / 2 + _floorWidth, _roomSize.y / 2 + _offset.y);
        _wallNorth.transform.localScale = new Vector3(_roomSize.y, wallHeight, _wallWidth);
        _wallNorth.GetComponent<MeshRenderer>().material = MatWallN;
        _wallNorth.transform.SetParent(parent);
    }

    private void CreateSouthWall(Transform parent)
    {
        _wallSouth = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wallSouth.name = "WallSouth";
        _wallSouth.transform.rotation = new Quaternion(0f, 90f, 0f, 90f);
        _wallSouth.transform.position = new Vector3(_offset.x + 0.5f * _wallWidth, wallHeight / 2 + _floorWidth,
            _roomSize.y / 2 + _offset.y);
        _wallSouth.transform.localScale = new Vector3(_roomSize.y, wallHeight, _wallWidth);
        _wallSouth.GetComponent<MeshRenderer>().material = MatWallS;
        _wallSouth.transform.SetParent(parent);
    }

    private void CreateEastWall(Transform parent)
    {
        _wallEast = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wallEast.name = "WallWest";
        _wallEast.transform.position = new Vector3(_roomSize.x / 2 + _offset.x, wallHeight / 2 + _floorWidth,
            _offset.y + _wallWidth / 2);
        _wallEast.transform.localScale = new Vector3(_roomSize.x - 2 * _wallWidth, wallHeight, _wallWidth);
        _wallEast.GetComponent<MeshRenderer>().material = MatWallE;
        _wallEast.transform.SetParent(parent);
    }

    private void CreateWestWall(Transform parent)
    {
        _wallWest = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _wallWest.name = "WallEast";
        _wallWest.transform.position = new Vector3(_roomSize.x / 2 + _offset.x, wallHeight / 2 + _floorWidth,
            _roomSize.y + _offset.y - _wallWidth / 2);
        _wallWest.transform.localScale = new Vector3(_roomSize.x - 2 * _wallWidth, wallHeight, _wallWidth);
        _wallWest.GetComponent<MeshRenderer>().material = MatWallW;
        _wallWest.transform.SetParent(parent);
    }
}