using System.Collections.Generic;
using Script.Rooms.RoomObjects;
using UnityEngine;

namespace Script.Rooms
{
    public class LrgChestRoom : RoomOptions
    {
        public LrgChestRoom()
        {
            objectsToAdd = new List<RoomObject>();
            RoomObject torches = new Torch(Random.Range(2, 5));
            objectsToAdd.Add(torches);
            RoomObject chests = new Chest(Random.Range(2, 5));
            objectsToAdd.Add(chests);
        }
    }
}