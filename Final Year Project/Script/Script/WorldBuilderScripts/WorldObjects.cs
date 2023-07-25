using System.Collections.Generic;
using UnityEngine;

public class WorldObjects : MonoBehaviour
{
    public const int TORCH = 0;
    public const int SPAWN_POINT = 2;

    public const int EXIT_POINT = 3;

    //CHEST01 = 5, CHEST02=6, CHEST03=7
    public const int CHEST = 4;
    public static WorldObjects Instance;

    public float emptyChance = 0.5f;
    public float smChestRoomChance = 0.3f;
    public float lrgChestRoomChance = 0.18f;
    public float exitChance = 0.02f;
    public GameObject torch;
    public GameObject chest01;
    public GameObject spawnPoint;

    public GameObject exitPoint;

    //Translates what the int value for roomType is in terms of English
    private Dictionary<int, GameObject> _roomObjects;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        CreateDictionary();
    }
    //Can be altered in code

    private void CreateDictionary()
    {
        _roomObjects = new Dictionary<int, GameObject>();
        _roomObjects.Add(TORCH, torch);
        _roomObjects.Add(CHEST, chest01);
        _roomObjects.Add(SPAWN_POINT, spawnPoint);
        _roomObjects.Add(EXIT_POINT, exitPoint);
    }

    public GameObject GetObject(int objectID)
    {
        if (_roomObjects.TryGetValue(objectID, out var gameObject)) return Instantiate(gameObject);
        return new GameObject();
    }
}