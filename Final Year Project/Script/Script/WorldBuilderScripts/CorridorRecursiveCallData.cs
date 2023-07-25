namespace Script
{
    public class CorridorRecursiveCallData
    {
        public int CorridorWidth;
        public int[] NewCorridorPos;
        public CorridorPathingDecider PathingDecider;
        public int PathingOption;

        public CorridorRecursiveCallData(int[] newCorridorPos, int corridorWidth,
            CorridorPathingDecider pathingDecider, int pathingOption)
        {
            NewCorridorPos = newCorridorPos.Clone() as int[];
            CorridorWidth = corridorWidth;
            PathingDecider = pathingDecider;
            PathingOption = pathingOption;
        }
    }
}