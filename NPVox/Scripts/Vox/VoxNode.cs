using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class VoxNode : ICloneable
{
    [SerializeField]
    private VoxTransform transform = VoxTransform.Default();
    [SerializeField]
    private VoxShape shape = null;
    [SerializeField]
    private List<VoxNode> children = new();


    public VoxTransform Transform => transform;

    public VoxShape Shape
    {
        get => shape;
        set => shape = value;
    }

    public List<VoxNode> Children
    {
        get => children;
        set => children = value;
    }


    public VoxNode() { }

    public VoxNode(VoxTransform transform)
    {
        this.transform = transform;
    }

    public VoxNode(VoxShape shape)
    {
        this.shape = shape;
    }

    public VoxNode(VoxShape shape, VoxTransform transform)
    {
        this.transform = transform;
        this.shape = shape;
    }

    public object Clone()
    {
        VoxNode clone = new((VoxShape)shape.Clone(), (VoxTransform)transform.Clone())
        {
            children = children.Select(x => (VoxNode)x.Clone()).ToList()
        };

        return clone;
    }

    public void SetTranslation(Vector3 translation)
    {
        transform.translation = translation;
    }

    public void SetRotation(Vector3 rotation)
    {
        transform.rotation = rotation;
    }
}
