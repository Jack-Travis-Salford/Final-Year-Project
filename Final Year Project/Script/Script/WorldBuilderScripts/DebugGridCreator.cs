using UnityEngine;

public class DebugGridCreator : MonoBehaviour
{
    //Changeable parameters in editor to alter gridlines
    public bool displayGridPlane;
    public bool displayGrid3D;
    public int displayEveryGridPlane = 1;
    public int displayEveryGrid3D = 1;

    //Copied from GeneratorGlobalVals: Prevents unecessary calls to retrieve static values
    //Total tiles per plane (_sizeUnits / _tileDimension)
    private int[] _noTiles;

    private float _roomHeight;

    //Size of each tile
    private float _tileDimension;


    private void Start()
    {
        _noTiles = GeneratorGlobalVals.Instance.GetNoTiles();
        _tileDimension = GeneratorGlobalVals.Instance.GetTileDimension();
        _roomHeight = GeneratorGlobalVals.Instance.GetRoomHeight();
        displayGrid3D = false;
        displayGridPlane = false;
    }

    private void Update()
    {
        //Below values can be set whilst game is running in inspector, must not be <1
        displayEveryGridPlane = Mathf.Max(1, displayEveryGridPlane);
        displayEveryGrid3D = Mathf.Max(1, displayEveryGrid3D);
        DebugGridBuilder();
    }


    //Draws the gridlines, visible using Gizmos
    private void DebugGridBuilder()
    {
        if (!displayGridPlane) return;
        _noTiles = GeneratorGlobalVals.Instance.GetNoTiles();
        _tileDimension = GeneratorGlobalVals.Instance.GetTileDimension();
        //Create grid plane
        var length = _noTiles[1] * _tileDimension;
        for (var i = 0; i <= _noTiles[0]; i += displayEveryGridPlane)
        {
            var lineStart = new Vector3(i * _tileDimension, 0f, 0f);
            var lineEnd = new Vector3(i * _tileDimension, 0f, length);
            Debug.DrawLine(lineStart, lineEnd);
        }

        length = _noTiles[0] * _tileDimension;

        for (var i = 0; i <= _noTiles[1]; i += displayEveryGridPlane)
        {
            var lineStart = new Vector3(0f, 0f, i * _tileDimension);
            var lineEnd = new Vector3(length, 0f, i * _tileDimension);
            Debug.DrawLine(lineStart, lineEnd);
        }

        if (displayGrid3D)
        {
            //Create the 3D part of the grid
            length = _noTiles[1] * _tileDimension;
            for (var i = 1; i <= _noTiles[0]; i += displayEveryGrid3D)
            {
                var lineStart = new Vector3(i * _tileDimension, _roomHeight, 0f);
                var lineEnd = new Vector3(i * _tileDimension, _roomHeight, length);
                Debug.DrawLine(lineStart, lineEnd);
            }

            length = _noTiles[0] * _tileDimension;
            for (var i = 1; i <= _noTiles[1]; i += displayEveryGrid3D)
            {
                var lineStart = new Vector3(0f, _roomHeight, i * _tileDimension);
                var lineEnd = new Vector3(length, _roomHeight, i * _tileDimension);
                Debug.DrawLine(lineStart, lineEnd);
            }

            for (var i = 1; i <= _noTiles[0]; i += displayEveryGrid3D)
            for (var j = 1; j <= _noTiles[1]; j += displayEveryGrid3D)
            {
                var lineStart = new Vector3(i * _tileDimension, 0f, j * _tileDimension);
                var lineEnd = new Vector3(i * _tileDimension, _roomHeight, j * _tileDimension);
                Debug.DrawLine(lineStart, lineEnd);
            }
        }
    }
}