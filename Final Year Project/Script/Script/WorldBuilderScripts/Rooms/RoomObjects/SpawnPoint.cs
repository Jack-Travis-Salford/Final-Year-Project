namespace Script.Rooms.RoomObjects
{
    public class SpawnPoint : RoomObject
    {
        public SpawnPoint()
        {
            ObjectRef = WorldObjects.SPAWN_POINT;
            WantedQuantity = 1;
            IsWallMounted = false;
            IsPlacedAgainstWall = false;
        }
    }
}