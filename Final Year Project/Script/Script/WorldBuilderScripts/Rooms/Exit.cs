using System.Collections.Generic;
using Script.Rooms.RoomObjects;
using UnityEngine;

namespace Script.Rooms
{
    public class Exit : RoomOptions
    {
        public Exit()
        {
            objectsToAdd = new List<RoomObject>();
            RoomObject torches = new Torch(Random.Range(0, 5));
            objectsToAdd.Add(torches);
            RoomObject exit = new ExitPoint();
            objectsToAdd.Add(exit);
        }
    }
}