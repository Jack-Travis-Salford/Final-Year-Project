public class GridSegmentData
{
    public Room Room;
    public int RoomType;
    public bool tileVisited = false;

    public GridSegmentData()
    {
        EastWall = false;
        SouthWall = false;
        EastWallIsDoorway = false;
        SouthWallIsDoorway = false;
        RoomType = GeneratorGlobalVals.EMPTY;
    }

    public bool EastWall { get; private set; }
    public bool SouthWall { get; private set; }
    public bool EastWallIsDoorway { get; private set; }
    public bool SouthWallIsDoorway { get; private set; }

    public void SetEastWallIsDoorway(bool isDoorway)
    {
        if (EastWall) EastWallIsDoorway = isDoorway;
    }

    public void SetSouthWallIsDoorway(bool isDoorway)
    {
        if (SouthWall) SouthWallIsDoorway = isDoorway;
    }

    public void SetEastWall(bool isWall)
    {
        EastWall = isWall;
        if (!isWall) EastWallIsDoorway = false;
    }

    public void SetSouthWall(bool isWall)
    {
        SouthWall = isWall;
        if (!isWall) SouthWallIsDoorway = false;
    }
}