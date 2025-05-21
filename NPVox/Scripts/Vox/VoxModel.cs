using System.Collections.Generic;
using UnityEngine;

public class VoxModel : ScriptableObject
{
    [SerializeField]
    private uint version = 0;
    [SerializeField]
    private VoxNode root;
    [SerializeField]
    protected Color32[] colortable;

    public VoxNode Root => root;
    public uint Version => version;
    public Color32[] Colortable
    {
        set => this.colortable = value;
        get => this.colortable;
    }


    public bool IsValid => this.colortable != null && root != null;


    public static VoxModel NewInstance()
    {
        VoxModel VoxModel = ScriptableObject.CreateInstance<VoxModel>();
        VoxModel.name = "Model";
        VoxModel.Initialize();
        return VoxModel;
    }

    public static VoxModel NewInvalidInstance(string message = "Created an invalid instance")
    {
        Debug.LogWarning(message);
        return NewInstance();
    }

    public virtual void Initialize()
    {
        this.colortable = null;
        this.root = null;
        this.version++;
    }

    public virtual void CopyOver(VoxModel source)
    {
        this.colortable = source.colortable != null ? (Color32[])source.colortable.Clone() : null;
        this.root = (VoxNode)source.root.Clone();
    }


#if UNITY_EDITOR
    public virtual Color32 ColorAtIndex(int idx)
#else
    public Color32 ColorAtIndex(int idx)
#endif
    {
        return this.colortable[idx];
    }

    public IEnumerator<VoxNode> GetEnumerator()
    {
        if (root != null)
        {
            foreach (var node in IterateNodes(root))
            {
                yield return node;
            }
        }
    }

    private IEnumerable<VoxNode> IterateNodes(VoxNode node)
    {
        yield return node;
        foreach (var child in node.Children)
        {
            foreach (var childNode in IterateNodes(child))
            {
                yield return childNode;
            }
        }
    }
}
