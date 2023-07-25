using System.Collections.Generic;
using Script.Rooms.RoomObjects;
using UnityEngine;

namespace Script.Rooms
{
    public class SmChestRoom : RoomOptions
    {
        public SmChestRoom()
        {
            objectsToAdd = new List<RoomObject>();
            RoomObject torches = new Torch(Random.Range(1, 5));
            objectsToAdd.Add(torches);
            RoomObject chests = new Chest(1);
            objectsToAdd.Add(chests);
        }
    }
}