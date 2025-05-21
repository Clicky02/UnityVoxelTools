using UnityEngine;

[PipeAppendable("Model Flipper", typeof(NPVoxIModelFactory), true, true)]
public class NPVoxModelFlipper : NPVoxCompositeProcessorBase<NPVoxIModelFactory, VoxModel>, NPVoxIModelFactory
{
    public VoxCoord XAxis = new VoxCoord(1, 0, 0);
    public VoxCoord YAxis = new VoxCoord(0, 1, 0);
    public VoxCoord ZAxis = new VoxCoord(0, 0, 1);

    override protected VoxModel CreateProduct(VoxModel reuse = null)
    {
        if (Input == null)
        {
            return VoxModel.NewInvalidInstance(reuse, "No Input Setup");
        }
        VoxModel model = ((NPVoxIModelFactory)Input).GetProduct();
        return CreateFlippedModel(model, XAxis, YAxis, ZAxis, reuse);
    }

    public static VoxModel CreateFlippedModel(VoxModel source, VoxCoord xFlip, VoxCoord yFlip, VoxCoord zFlip, VoxModel reuse = null)
    {

        sbyte sizeX = (sbyte)(xFlip.x * source.SizeX + xFlip.z * source.SizeZ + xFlip.y * source.SizeY);
        sbyte sizeY = (sbyte)(yFlip.x * source.SizeX + yFlip.z * source.SizeZ + yFlip.y * source.SizeY);
        sbyte sizeZ = (sbyte)(zFlip.x * source.SizeX + zFlip.z * source.SizeZ + zFlip.y * source.SizeY);
        VoxModel model = VoxModel.NewInstance(source, new VoxCoord(
            (sbyte)Mathf.Abs(sizeX),
            (sbyte)Mathf.Abs(sizeY),
            (sbyte)Mathf.Abs(sizeZ)
        ), reuse);
        sbyte xOffset = 0;
        if (sizeX < 0)
        {
            xOffset = (sbyte)((sizeX - 1));
        }
        sbyte yOffset = 0;
        if (sizeY < 0)
        {
            yOffset = (sbyte)((sizeY - 1));
        }
        sbyte zOffset = 0;
        if (sizeZ < 0)
        {
            zOffset = (sbyte)((sizeZ - 1));
        }

        NPVoxFaces allFaces = new NPVoxFaces();
        allFaces.Back = 1;
        allFaces.Forward = 1;
        allFaces.Left = 1;
        allFaces.Right = 1;
        allFaces.Up = 1;
        allFaces.Down = 1;

        bool hasVoxelGroups = source.HasVoxelGroups();
        if (hasVoxelGroups)
        {
            model.InitVoxelGroups();
        }
        model.NumVoxelGroups = source.NumVoxelGroups;
        model.Colortable = source.Colortable;
        model.NumVoxels = source.NumVoxels;

        // Transform Coordinates
        foreach (VoxCoord sourceCoord in source.Enumerate())
        {
            VoxCoord coord = new VoxCoord(
                (sbyte)(xOffset + (xFlip.x * sourceCoord.x + xFlip.z * sourceCoord.z + xFlip.y * sourceCoord.y)),
                (sbyte)(yOffset + (yFlip.x * sourceCoord.x + yFlip.z * sourceCoord.z + yFlip.y * sourceCoord.y)),
                (sbyte)(zOffset + (zFlip.x * sourceCoord.x + zFlip.z * sourceCoord.z + zFlip.y * sourceCoord.y))
            );
            coord = model.LoopCoord(coord, allFaces);

            model.SetVoxel(coord, source.GetVoxel(sourceCoord));
            if (hasVoxelGroups)
            {
                model.SetVoxelGroup(coord, source.GetVoxelGroup(sourceCoord));
            }
        }

        // Transform Sockets
        //        Matrix4x4 t = Matrix4x4.identity;
        //        if (xFlip.X != 0) t = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(xFlip.X, 1, 1));
        //        if (xFlip.Y != 0) t *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, xFlip.Y, 1));
        //        if (xFlip.Z != 0) t *= Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(1, 1, xFlip.Z));

        NPVoxSocket[] sockets = new NPVoxSocket[source.Sockets.Length];
        for (int i = 0; i < sockets.Length; i++)
        {
            // Anchor
            VoxCoord sourceCoord = source.Sockets[i].Anchor;
            VoxCoord anchorCoord = new VoxCoord(
                (sbyte)(xOffset + (xFlip.x * sourceCoord.x + xFlip.z * sourceCoord.z + xFlip.y * sourceCoord.y)),
                (sbyte)(yOffset + (yFlip.x * sourceCoord.x + yFlip.z * sourceCoord.z + yFlip.y * sourceCoord.y)),
                (sbyte)(zOffset + (zFlip.x * sourceCoord.x + zFlip.z * sourceCoord.z + zFlip.y * sourceCoord.y))
            );
            anchorCoord = model.LoopCoord(anchorCoord, allFaces);

            sockets[i].Name = source.Sockets[i].Name;
            sockets[i].Anchor = anchorCoord;

            // transform Quaternion
            Quaternion rotation = Quaternion.Euler(source.Sockets[i].EulerAngles);

            Vector3 anchorRight = rotation * Vector3.right;
            Vector3 anchorUp = rotation * Vector3.up;
            Vector3 anchorForward = rotation * Vector3.forward;

            Vector3 newRight = anchorRight;
            Vector3 newUp = anchorUp;
            Vector3 newForward = anchorForward;

            if (xFlip.x < 0)
            {
                newRight.Scale(new Vector3(-1f, 1f, 1f));
                newUp.Scale(new Vector3(-1f, 1f, 1f));
                newForward.Scale(new Vector3(-1f, 1f, 1f));
                Quaternion q = Quaternion.LookRotation(newForward, newUp);
                sockets[i].EulerAngles = q.eulerAngles;
            }
            // TODO: other mirrors not yet supported
        }
        model.Sockets = sockets;

        model.name = "zzz Flipped Model";
        return model;
    }

    override public string GetTypeName()
    {
        return "Model Flipper";
    }
}