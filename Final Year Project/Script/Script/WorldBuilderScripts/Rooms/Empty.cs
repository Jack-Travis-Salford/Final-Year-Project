using System.Collections.Generic;
using Script.Rooms.RoomObjects;
using UnityEngine;

namespace Script.Rooms
{
    public class Empty : RoomOptions
    {
        public Empty()
        {
            objectsToAdd = new List<RoomObject>();
            RoomObject torches = new Torch(Random.Range(1, 5));
            objectsToAdd.Add(torches);
        }
    }
}