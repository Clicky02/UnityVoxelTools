using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct NPVoxNodeData
{
    public int Id;
    public string Name;
    public Mesh Mesh;
    public VoxTransform Transform;
    public List<NPVoxNodeData> Children;


    public NPVoxNodeData(int id, string name, Mesh mesh, VoxTransform transform)
    {
        this.Id = id;
        this.Name = name;
        this.Mesh = mesh;
        this.Transform = transform;
        Children = new List<NPVoxNodeData>();
    }
}

public class NPVoxObjectData : ScriptableObject
{
    public NPVoxNodeData root;
}