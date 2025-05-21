using UnityEngine;

public class NPVoxCoordUtil
{
    public static Vector3 ToVector(VoxCoord coord)
    {
        return new Vector3(coord.x, coord.y, coord.z);
    }

    public static VoxCoord ToCoord(Vector3 vector)
    {
        return new VoxCoord((sbyte)Mathf.Round(vector.x), (sbyte)Mathf.Round(vector.y), (sbyte)Mathf.Round(vector.z));
    }
}