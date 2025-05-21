using UnityEngine;

abstract public class NPVoxCompositeProcessorBase<SOURCE_FACTORY, PRODUCT> : NPVoxProcessorBase<PRODUCT>, IPipeComposite where PRODUCT : Object where SOURCE_FACTORY : class, IPipeImportable
{
    [SerializeField, HideInInspector]
    private UnityEngine.Object input;

    override public IPipeImportable[] GetAllInputs()
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

    virtual public IPipeImportable Input
    {
        get
        {
            return input as IPipeImportable;
        }
        set
        {
            input = value as UnityEngine.Object;
        }
    }

#if UNITY_EDITOR
    override public bool DrawInspector(EditFlags flags)
    {
        bool changed = base.DrawInspector(flags);

        if ((flags & EditFlags.INPUT) == EditFlags.INPUT)
        {
            SOURCE_FACTORY newSource = Utils.DrawSourceSelector<SOURCE_FACTORY>("Input", input as SOURCE_FACTORY);
            if (newSource as NPVoxCompositeProcessorBase<SOURCE_FACTORY, PRODUCT> == this)
            {
                return false;
            }
            changed = newSource != Input || changed;
            Input = (IPipeImportable)newSource;
        }

        if ((flags & EditFlags.TOOLS) == EditFlags.TOOLS)
        {
            if (GUILayout.Button("Invalidate & Reimport Deep"))
            {
                Utils.InvalidateAndReimportDeep(this);
            }
        }

        return changed;
    }

    override public bool DrawMultiInstanceEditor(EditFlags flags, UnityEngine.Object[] objects)
    {
        bool changed = base.DrawMultiInstanceEditor(flags, objects);

        if ((flags & EditFlags.INPUT) == EditFlags.INPUT)
        {
            // input not supported when editing multiple instances
        }

        if ((flags & EditFlags.TOOLS) == EditFlags.TOOLS)
        {
            if (GUILayout.Button("Invalidate & Reimport Deep"))
            {
                foreach (UnityEngine.Object o in objects)
                {
                    Utils.InvalidateAndReimportDeep(o as IPipeImportable);
                }
            }
        }

        return changed;
    }
#endif
}