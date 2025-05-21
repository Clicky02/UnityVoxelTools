public class NPVoxRayCastHit
{
    public NPVoxRayCastHit(bool isHit, VoxCoord coord)
    {
        this.isHit = isHit;
        this.coord = coord;
    }

    private bool isHit;
    public bool IsHit
    {
        get
        {
            return isHit;
        }
    }

    private VoxCoord coord;
    public VoxCoord Coord
    {
        get
        {
            return coord;
        }
    }
}
