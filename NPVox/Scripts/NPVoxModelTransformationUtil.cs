using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPVoxModelTransformationUtil
{
    [System.Serializable]
    public enum ResolveConflictMethodType
    {
        NONE,
        CLOSEST,
        FILL_GAPS
    }

    // public static NPVoxCoord GetNearbyCoord(NPVoxModel model, Vector3 saveCoord, ResolveConflictMethodType resolveConflictMethod)
    // {
    //     NPVoxCoord favoriteCoord = NPVoxCoordUtil.ToCoord(saveCoord);
    //     if (model.HasVoxelFast(favoriteCoord))
    //     {
    //         NPVoxBox box = new NPVoxBox(favoriteCoord - NPVoxCoord.ONE, favoriteCoord + NPVoxCoord.ONE);
    //         favoriteCoord = NPVoxCoord.INVALID;
    //         float nearestDistance = 9999f;
    //         int favoriteEnclosingVoxelCount = -1;
    //         foreach (NPVoxCoord currentTestCoord in box.Enumerate())
    //         {
    //             if (model.IsInside(currentTestCoord) && !model.HasVoxelFast(currentTestCoord))
    //             {
    //                 if (resolveConflictMethod == ResolveConflictMethodType.CLOSEST)
    //                 {
    //                     float distance = Vector3.Distance(NPVoxCoordUtil.ToVector(currentTestCoord), saveCoord);
    //                     if (distance < nearestDistance)
    //                     {
    //                         nearestDistance = distance;
    //                         favoriteCoord = currentTestCoord;
    //                     }
    //                 }
    //                 else
    //                 {
    //                     int enclosingVoxelCount = 0;
    //                     NPVoxBox enclosingBoxCheck = new NPVoxBox(currentTestCoord - NPVoxCoord.ONE, currentTestCoord + NPVoxCoord.ONE);
    //                     foreach (NPVoxCoord enclosingTestCoord in enclosingBoxCheck.Enumerate())
    //                     {
    //                         if (model.IsInside(currentTestCoord) && model.HasVoxelFast(currentTestCoord))
    //                         {
    //                             enclosingVoxelCount++;
    //                         }
    //                     }

    //                     if (enclosingVoxelCount > favoriteEnclosingVoxelCount)
    //                     {
    //                         enclosingVoxelCount = favoriteEnclosingVoxelCount;
    //                         favoriteCoord = currentTestCoord;
    //                     }
    //                 }
    //             }
    //         }
    //     }
    //     return favoriteCoord;
    // }

    // public static NPVoxModel MatrixTransformSocket(NPVoxModel sourceModel, string socketName, Matrix4x4 matrix, Vector3 pivot, NPVoxModel reuse = null)
    // {
    //     NPVoxSocket socket = sourceModel.GetSocketByName(socketName);
    //     Vector3 pivotPoint = NPVoxCoordUtil.ToVector(socket.Anchor) + pivot;
    //     Matrix4x4 transformMatrix = (Matrix4x4.TRS(pivotPoint, Quaternion.identity, Vector3.one) * matrix) * Matrix4x4.TRS(-pivotPoint, Quaternion.identity, Vector3.one);

    //     return TransformSocket(sourceModel, socketName, transformMatrix, reuse);
    // }

    // public static NPVoxModel TransformSocket(NPVoxModel sourceModel, string socketName, Matrix4x4 transformMatrix, NPVoxModel reuse = null)
    // {
    //     NPVoxModel transformedModel = null;

    //     transformedModel = NPVoxModel.NewInstance(sourceModel, reuse);
    //     transformedModel.CopyOver(sourceModel);

    //     NPVoxSocket[] sockets = new NPVoxSocket[sourceModel.Sockets.Length];
    //     for (int i = 0; i < sockets.Length; i++)
    //     {
    //         NPVoxSocket socket = sourceModel.Sockets[i];
    //         if (socket.Name == socketName)
    //         {
    //             // transform anchor
    //             Vector3 saveOriginalAnchor = NPVoxCoordUtil.ToVector(socket.Anchor);
    //             Vector3 saveTargetAnchor = transformMatrix.MultiplyPoint(saveOriginalAnchor);
    //             socket.Anchor = sourceModel.Clamp(NPVoxCoordUtil.ToCoord(saveTargetAnchor));

    //             // transform Quaternion
    //             Quaternion originalRotation = Quaternion.Euler(socket.EulerAngles);
    //             Matrix4x4 rotated = (Matrix4x4.TRS(Vector3.zero, originalRotation, Vector3.one) * transformMatrix);
    //             socket.EulerAngles = Matrix4x4Util.GetRotation(rotated).eulerAngles;
    //         }
    //         sockets[i] = socket;
    //     }
    //     transformedModel.Sockets = sockets;

    //     return transformedModel;
    // }

    // public static NPVoxModel CreateWithNewSize(NPVoxModel source, NPVoxBox newBounds, NPVoxModel reuse = null)
    // {
    //     NPVoxCoord delta;
    //     NPVoxCoord newSize;
    //     CalculateResizeOffset(source.BoundingBox, newBounds, out delta, out newSize);

    //     NPVoxModel newModel = NPVoxModel.NewInstance(source, newSize, reuse);
    //     newModel.NumVoxels = source.NumVoxels;
    //     newModel.NumVoxelGroups = source.NumVoxelGroups;
    //     newModel.Colortable = source.Colortable != null ? (Color32[])source.Colortable.Clone() : null;
    //     newModel.Sockets = source.Sockets != null ? (NPVoxSocket[])source.Sockets.Clone() : null;

    //     if (newModel.Sockets != null)
    //     {
    //         for (int i = 0; i < newModel.Sockets.Length; i++)
    //         {
    //             newModel.Sockets[i].Anchor = newModel.Sockets[i].Anchor + delta;
    //         }
    //     }

    //     bool hasVoxelGroups = source.HasVoxelGroups();

    //     if (hasVoxelGroups)
    //     {
    //         newModel.InitVoxelGroups();
    //         newModel.NumVoxelGroups = source.NumVoxelGroups;
    //     }

    //     foreach (NPVoxCoord coord in source.EnumerateVoxels())
    //     {
    //         NPVoxCoord targetCoord = coord + delta;
    //         newModel.SetVoxel(targetCoord, source.GetVoxel(coord));
    //         if (hasVoxelGroups)
    //         {
    //             newModel.SetVoxelGroup(targetCoord, source.GetVoxelGroup(coord));
    //         }
    //     }

    //     //        newModel.InvalidateVoxelCache();

    //     return newModel;
    // }

    public static void CalculateResizeOffset(NPVoxBox parentBounds, NPVoxBox thisBounds, out VoxCoord delta, out VoxCoord size)
    {
        if (!thisBounds.Equals(parentBounds))
        {
            size = parentBounds.Size;
            bool isOverflow = false;

            sbyte deltaX = (sbyte)(Mathf.Max(parentBounds.Left - thisBounds.Left, thisBounds.Right - parentBounds.Right));
            if ((int)deltaX * 2 + (int)size.x > 126) // check for overflow
            {
                deltaX = (sbyte)((float)deltaX - Mathf.Ceil(((float)deltaX * 2f + (float)size.x) - 126) / 2f);
                isOverflow = true;
            }

            sbyte deltaY = (sbyte)(Mathf.Max(parentBounds.Down - thisBounds.Down, thisBounds.Up - parentBounds.Up));
            if ((int)deltaY * 2 + (int)size.y > 126) // check for overflow
            {
                deltaY = (sbyte)((float)deltaY - Mathf.Ceil(((float)deltaY * 2f + (float)size.y) - 126) / 2f);
                isOverflow = true;
            }

            sbyte deltaZ = (sbyte)(Mathf.Max(parentBounds.Back - thisBounds.Back, thisBounds.Forward - parentBounds.Forward));
            if ((int)deltaZ * 2 + (int)size.z > 126) // check for overflow
            {
                deltaZ = (sbyte)((float)deltaZ - Mathf.Ceil(((float)deltaZ * 2f + (float)size.z) - 126) / 2f);
                isOverflow = true;
            }

            delta = new VoxCoord(deltaX, deltaY, deltaZ);
            size = size + delta + delta;

            if (isOverflow)
            {
                Debug.LogWarning("Transformed Model is large, clamped to " + size);
            }
        }
        else
        {
            size = parentBounds.Size;
            delta = VoxCoord.ZERO;
        }
    }
}
