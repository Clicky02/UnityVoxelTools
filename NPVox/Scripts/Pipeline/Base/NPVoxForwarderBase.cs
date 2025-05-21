using UnityEngine;

abstract public class NPVoxForwarderBase<SOURCE_FACTORY, PRODUCT> : ScriptableObject, IPipeImportable, IPipeComposite, IPipeEditable where PRODUCT : Object where SOURCE_FACTORY : class, IPipeImportable
{
    [SerializeField]
    private string InstanceName = "";

    [SerializeField, HideInInspector]
    private UnityEngine.Object input;

#if UNITY_EDITOR
    private double lastInvalidatedAt = 0;
#endif

    public IPipeImportable[] GetAllInputs()
    {
        if (Input != null)
        {
            return new IPipeImportable[] { (IPipeImportable)Input };
        }
        else
        {
            return new IPipeImportable[] { };
        }
    }

    public IPipeImportable Input
    {
        get
        {
            return input as IPipeImportable;
        }
        set
        {
            input = (UnityEngine.Object)value;
        }
    }

    virtual public void Import()
    {
        GetProduct();
    }

    public bool IsTemplate()
    {
        return false;
    }

    public void Invalidate(bool includeInputs = false)
    {
        if (includeInputs)
        {
            if (Input != null)
            {
                Input.Invalidate(true);
            }
        }

#if UNITY_EDITOR
        lastInvalidatedAt = UnityEditor.EditorApplication.timeSinceStartup;
#endif
    }

    public virtual void Destroy()
    {
    }

    public void OnDestroy()
    {
        Destroy();
    }

    protected string GetFileType()
    {
        return "asset";
    }

    abstract public PRODUCT GetProduct();

    public abstract string GetTypeName();

    public string GetInstanceName()
    {
        return this.InstanceName;
    }

    public bool IsValid()
    {
        return false;
    }

    virtual public UnityEngine.Object Clone()
    {
        NPVoxForwarderBase<SOURCE_FACTORY, PRODUCT> copy = Object.Instantiate(this);
        return copy;
    }

    public virtual void IncludeSubAssets(string path) { }

#if UNITY_EDITOR
    virtual public bool DrawInspector(EditFlags flags)
    {
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(this);
        bool changed = editor.DrawDefaultInspector();
        if ((flags & EditFlags.INPUT) == EditFlags.INPUT)
        {
            SOURCE_FACTORY newSource = Utils.DrawSourceSelector<SOURCE_FACTORY>("Input", input as SOURCE_FACTORY);
            if (newSource as NPVoxForwarderBase<SOURCE_FACTORY, PRODUCT> == this)
            {
                return false;
            }
            changed = newSource != Input || changed;
            Input = (IPipeImportable)newSource;
        }

        // if((flags & NPVoxEditFlags.TOOLS) == NPVoxEditFlags.TOOLS)
        // {
        //     if(GUILayout.Button("Invalidate & Reimport Mesh Output Deep"))
        //     {
        //         NPVoxPipelineUtils.InvalidateAndReimportDeep( this );
        //     }
        // }

        return changed;
    }

    virtual public bool DrawMultiInstanceEditor(EditFlags flags, UnityEngine.Object[] objects)
    {
        UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(objects);
        bool changed = editor.DrawDefaultInspector();
        return changed;
    }

    public double GetLastInvalidatedTime()
    {
        return lastInvalidatedAt;
    }
#endif
}