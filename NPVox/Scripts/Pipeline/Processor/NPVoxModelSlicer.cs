using UnityEngine;

[PipeAppendableAttribute("Model Slicer", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelSlicer : NPVoxCompositeProcessorBase<NPVoxIModelFactory, VoxModel>, NPVoxIModelFactory
{
    public NPVoxBox slice;

    override protected VoxModel CreateProduct(VoxModel reuse = null)
    {
        if (Input == null)
        {
            return VoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }
        else
        {
            VoxModel model = ((NPVoxIModelFactory)Input).GetProduct();

            return CreateSlicedModel(model, reuse);
        }
    }

    private VoxModel CreateSlicedModel(VoxModel source, VoxModel reuse)
    {
        NPVoxBox targetBox = slice.Clone();
        NPVoxBox sourceBounds = source.BoundingBox;
        targetBox.Clamp(source.BoundingBox);

        VoxCoord origin = targetBox.LeftDownBack;
        VoxModel model = VoxModel.NewInstance(source, targetBox.Size, reuse);
        int numVoxels = 0;
        foreach (VoxCoord coord in targetBox.Enumerate())
        {
            if (source.HasVoxel(coord))
            {
                numVoxels++;
                model.SetVoxel(coord - origin, source.GetVoxel(coord));
            }
        }

        model.NumVoxels = numVoxels;
        model.Colortable = source.Colortable;

        return model;
    }


    public override string GetTypeName()
    {
        return "Model Slicer";
    }
}