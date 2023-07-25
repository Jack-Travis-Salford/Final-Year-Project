namespace Script.Rooms.RoomObjects
{
    public class ExitPoint : RoomObject
    {
        public ExitPoint()
        {
            ObjectRef = WorldObjects.EXIT_POINT;
            WantedQuantity = 1;
            IsWallMounted = false;
            IsPlacedAgainstWall = false;
        }
    }
}