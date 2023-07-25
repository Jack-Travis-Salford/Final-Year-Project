namespace Script.Rooms.RoomObjects
{
    public class RoomObject
    {
        public RoomObject(int objectRef, bool isWallMounted, bool isPlacedAgainstWall, int wantedQuantity)
        {
            ObjectRef = objectRef;
            IsWallMounted = isWallMounted;
            IsPlacedAgainstWall = isPlacedAgainstWall;
            WantedQuantity = wantedQuantity;
        }

        public RoomObject()
        {
        }

        public int ObjectRef { get; protected set; }
        public bool IsWallMounted { get; protected set; }
        public bool IsPlacedAgainstWall { get; protected set; }
        public int WantedQuantity { get; protected set; }
    }
}