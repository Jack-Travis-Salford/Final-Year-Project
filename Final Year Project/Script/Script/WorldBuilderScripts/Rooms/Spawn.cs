using System.Collections.Generic;
using Script.Rooms.RoomObjects;
using UnityEngine;

namespace Script.Rooms
{
    public class Spawn : RoomOptions
    {
        public Spawn()
        {
            objectsToAdd = new List<RoomObject>();
            RoomObject torches = new Torch(Random.Range(1, 8));
            objectsToAdd.Add(torches);
            RoomObject spawn = new SpawnPoint();
            objectsToAdd.Add(spawn);
        }
    }
}