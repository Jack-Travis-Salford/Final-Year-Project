public class Door
{
    public readonly int Offset;
    public readonly int Wall;
    public readonly int Weight;

    public Door(int offset, int wall)
    {
        Offset = offset;
        Wall = wall;
        Weight = 1;
    }
}