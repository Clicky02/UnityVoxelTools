using UnityEngine;

abstract public class NPVoxCompositeProcessorBase<SOURCE_FACTORY, PRODUCT> : NPVoxProcessorBase<PRODUCT>, NPipeIComposite where PRODUCT : Object where SOURCE_FACTORY : class, NPipeIImportable
{
    [SerializeField, HideInInspector]
    private UnityEngine.Object input;

    override public NPipeIImportable[] GetAllInputs()
    {
        if (Input != null)
        {
            return new NPipeIImportable[] { (NPipeIImportable)Input };
        }
        else
        {
            return new NPipeIImportable[] { };
        }
    }

    virtual public NPipeIImportable Input
    {
        get
        {
            return input as NPipeIImportable;
        }
        set
        {
            input = value as UnityEngine.Object;
        }
    }

#if UNITY_EDITOR
    override public bool DrawInspector(NPipeEditFlags flags)
    {
        bool changed = base.DrawInspector(flags);

        if ((flags & NPipeEditFlags.INPUT) == NPipeEditFlags.INPUT)
        {
            SOURCE_FACTORY newSource = NPipelineUtils.DrawSourceSelector<SOURCE_FACTORY>("Input", input as SOURCE_FACTORY);
            if (newSource as NPVoxCompositeProcessorBase<SOURCE_FACTORY, PRODUCT> == this)
            {
                return false;
            }
            changed = newSource != Input || changed;
            Input = (NPipeIImportable)newSource;
        }

        if ((flags & NPipeEditFlags.TOOLS) == NPipeEditFlags.TOOLS)
        {
            if (GUILayout.Button("Invalidate & Reimport Deep"))
            {
                NPipelineUtils.InvalidateAndReimportDeep(this);
            }
        }

        return changed;
    }

    override public bool DrawMultiInstanceEditor(NPipeEditFlags flags, UnityEngine.Object[] objects)
    {
        bool changed = base.DrawMultiInstanceEditor(flags, objects);

        if ((flags & NPipeEditFlags.INPUT) == NPipeEditFlags.INPUT)
        {
            // input not supported when editing multiple instances
        }

        if ((flags & NPipeEditFlags.TOOLS) == NPipeEditFlags.TOOLS)
        {
            if (GUILayout.Button("Invalidate & Reimport Deep"))
            {
                foreach (UnityEngine.Object o in objects)
                {
                    NPipelineUtils.InvalidateAndReimportDeep(o as NPipeIImportable);
                }
            }
        }

        return changed;
    }
#endif
}