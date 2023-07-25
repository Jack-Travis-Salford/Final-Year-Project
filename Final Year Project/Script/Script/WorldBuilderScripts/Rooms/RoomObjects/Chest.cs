namespace Script.Rooms.RoomObjects
{
    public class Chest : RoomObject
    {
        public Chest(int qnty)
        {
            //ADD THE OBJECT
            ObjectRef = WorldObjects.CHEST;
            WantedQuantity = qnty;
            IsWallMounted = false;
            IsPlacedAgainstWall = true;
        }
    }
}