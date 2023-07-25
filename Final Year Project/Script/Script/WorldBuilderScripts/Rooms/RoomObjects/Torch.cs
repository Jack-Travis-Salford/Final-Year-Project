namespace Script.Rooms.RoomObjects
{
    public class Torch : RoomObject
    {
        public Torch(int qnty)
        {
            ObjectRef = WorldObjects.TORCH;
            WantedQuantity = qnty;
            IsWallMounted = true;
            IsPlacedAgainstWall = false;
        }
    }
}