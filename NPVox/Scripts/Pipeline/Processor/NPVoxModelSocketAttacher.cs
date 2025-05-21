using UnityEngine;
using System.Collections.Generic;

[PipeAppendableAttribute("Socket Attacher", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelSocketAttacher : NPVoxCompositeProcessorBase<NPVoxIModelFactory, VoxModel>, NPVoxIModelFactory
{
    public NPVoxSocket[] AddSockets = new NPVoxSocket[] { };

    override protected VoxModel CreateProduct(VoxModel reuse = null)
    {
        if (Input == null)
        {
            return VoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }
        else
        {
            VoxModel sourceModel = ((NPVoxIModelFactory)Input).GetProduct();

            VoxModel productModel = VoxModel.NewInstance(sourceModel, reuse);
            productModel.CopyOver(sourceModel);
            if (sourceModel.IsValid)
            {
                foreach (VoxCoord coord in sourceModel.BoundingBox.Enumerate())
                {
                    if (sourceModel.HasVoxel(coord))
                    {
                        productModel.SetVoxel(coord, sourceModel.GetVoxel(coord));
                    }
                }

                /// add all the sockets
                List<NPVoxSocket> productSockets = new List<NPVoxSocket>();
                productSockets.AddRange(productModel.Sockets);
                productSockets.AddRange(AddSockets);
                productModel.Sockets = productSockets.ToArray();
            }
            else
            {
                Debug.LogWarning("Couldn't create model: source was not valid [ TRY REIMPORTING ]");
            }

            return productModel;
        }
    }

    public override string GetTypeName()
    {
        return "Socket Attacher";
    }
}