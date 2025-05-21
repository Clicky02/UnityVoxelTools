// using UnityEngine;
// using System;

// public class NPVoxMeshData
// {
//     public NPVoxCoord voxCoord = NPVoxCoord.INVALID;

//     public bool hasLeft = false;
//     public bool hasRight = false;
//     public bool hasUp = false;
//     public bool hasDown = false;
//     public bool hasForward = false;
//     public bool hasBack = false;

//     public bool isHidden = true;

//     public bool includeLeft = false;
//     public bool includeRight = false;
//     public bool includeUp = false;
//     public bool includeDown = false;
//     public bool includeBack = false;
//     public bool includeForward = false;

//     public int numVertices = 0;
//     public int vertexIndexOffsetBegin = 0;

//     public Vector3 voxelCenter = Vector3.zero;
//     public int[] vertexIndexOffsets = new int[8];
//     public Vector3[] vertexPositionOffsets = new Vector3[8];

//     public NPVoxFaces loop = null;
//     public NPVoxFaces include = null;
//     public NPVoxFaces cutout = null;

//     public NPVoxToUnity voxToUnity = null;

//     public NPVoxNormalMode normalMode = NPVoxNormalMode.SMOOTH;

//     public NPVoxMeshData()
//     {
//     }
// }

// public class NPVoxMeshGenerator
// {
//     public static void CreateMeshForShape(
//         NPVoxModel model,
//         NPVoxShape shape,
//         Mesh mesh,
//         Vector3 cubeSize,
//         int NormalVarianceSeed = 0,
//         NPVoxOptimization optimization = NPVoxOptimization.OFF,
//         NPVoxNormalMode NormalMode = NPVoxNormalMode.SMOOTH,
//         int BloodColorIndex = 0,
//         NPVoxFaces loop = null,
//         NPVoxFaces cutout = null,
//         NPVoxFaces include = null,
//         NPVoxNormalProcessorList normalProcessors = null
//     )
//     {
//         var vertices = new Vector3[shape.NumVoxels * 8];
//         var triangles = new int[shape.NumVoxels * 36];

//         var normals = new Vector3[shape.NumVoxels * 8];
//         var tangents = new Vector4[shape.NumVoxels * 8];
//         var colors = new Color[shape.NumVoxels * 8];

//         var tmp = new NPVoxMeshData[shape.NumVoxels];

//         int currentVertexIndex = 0;
//         var currentTriangleIndex = 0;

//         UnityEngine.Random.InitState(NormalVarianceSeed);

//         loop ??= new NPVoxFaces();
//         include ??= new NPVoxFaces(1, 1, 1, 1, 1, 1);

//         NPVoxBox voxelsToInclude = shape.BoundingBox;
//         Vector3 cutoutOffset = Vector3.zero;
//         if (cutout != null)
//         {
//             Vector3 originalCenter = voxelsToInclude.SaveCenter;
//             voxelsToInclude.Left = (sbyte)Mathf.Abs(cutout.Left);
//             voxelsToInclude.Down = (sbyte)Mathf.Abs(cutout.Down);
//             voxelsToInclude.Back = (sbyte)Mathf.Abs(cutout.Back);
//             voxelsToInclude.Right = (sbyte)(voxelsToInclude.Right - (sbyte)Mathf.Abs(cutout.Right));
//             voxelsToInclude.Up = (sbyte)(voxelsToInclude.Up - (sbyte)Mathf.Abs(cutout.Up));
//             voxelsToInclude.Forward = (sbyte)(voxelsToInclude.Forward - (sbyte)Mathf.Abs(cutout.Forward));
//             cutoutOffset = Vector3.Scale(originalCenter - voxelsToInclude.SaveCenter, cubeSize);
//         }

//         NPVoxToUnity npVoxToUnity = new NPVoxToUnity(model, cubeSize);
//         Vector3 size = new Vector3(
//             voxelsToInclude.Size.X * cubeSize.x,
//             voxelsToInclude.Size.Y * cubeSize.y,
//             voxelsToInclude.Size.Z * cubeSize.z
//         );

//         NPVoxBox voxelNormalNeighbours = new NPVoxBox(new NPVoxCoord(-1, -1, -1), new NPVoxCoord(1, 1, 1));

//         // Collect temporary data to use for model generation
//         int voxIndex = 0;
//         foreach (NPVoxCoord voxCoord in voxelsToInclude.Enumerate())
//         {
//             if (shape.HasVoxel(voxCoord))
//             {
//                 tmp[voxIndex] = new NPVoxMeshData();

//                 tmp[voxIndex].loop = loop;
//                 tmp[voxIndex].cutout = cutout;
//                 tmp[voxIndex].include = include;

//                 // Compute voxel center
//                 tmp[voxIndex].voxelCenter = npVoxToUnity.ToUnityPosition(voxCoord) + cutoutOffset;
//                 tmp[voxIndex].voxCoord = voxCoord;

//                 tmp[voxIndex].voxToUnity = npVoxToUnity;

//                 // Determine normal Mode
//                 tmp[voxIndex].normalMode = NormalMode;


//                 // do we have this side
//                 tmp[voxIndex].hasLeft = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.LEFT, loop));
//                 tmp[voxIndex].hasRight = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.RIGHT, loop));
//                 tmp[voxIndex].hasDown = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.DOWN, loop));
//                 tmp[voxIndex].hasUp = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.UP, loop));
//                 tmp[voxIndex].hasForward = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.FORWARD, loop));
//                 tmp[voxIndex].hasBack = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.BACK, loop));

//                 // do we actually want to include this side in our mesh
//                 // NOTE: cutout < 0 means we still render the mesh even though it is cutout
//                 //       cutout > 0 means we don't render the mesh when cutout
//                 tmp[voxIndex].includeLeft = (tmp[voxIndex].hasLeft || (cutout.Left < 0 && voxCoord.X == voxelsToInclude.Left)) && include.Left == 1;
//                 tmp[voxIndex].includeRight = (tmp[voxIndex].hasRight || (cutout.Right < 0 && voxCoord.X == voxelsToInclude.Right)) && include.Right == 1;
//                 tmp[voxIndex].includeUp = (tmp[voxIndex].hasUp || (cutout.Up < 0 && voxCoord.Y == voxelsToInclude.Up)) && include.Up == 1;
//                 tmp[voxIndex].includeDown = (tmp[voxIndex].hasDown || (cutout.Down < 0 && voxCoord.Y == voxelsToInclude.Down)) && include.Down == 1;
//                 tmp[voxIndex].includeBack = (tmp[voxIndex].hasBack || (cutout.Back < 0 && voxCoord.Z == voxelsToInclude.Back)) && include.Back == 1;
//                 tmp[voxIndex].includeForward = (tmp[voxIndex].hasForward || (cutout.Forward < 0 && voxCoord.Z == voxelsToInclude.Forward)) && include.Forward == 1;

//                 tmp[voxIndex].isHidden = !tmp[voxIndex].hasForward &&
//                                 !tmp[voxIndex].hasBack &&
//                                 !tmp[voxIndex].hasLeft &&
//                                 !tmp[voxIndex].hasRight &&
//                                 !tmp[voxIndex].hasUp &&
//                                 !tmp[voxIndex].hasDown;


//                 if (tmp[voxIndex].isHidden && optimization == NPVoxOptimization.PER_VOXEL)
//                 {
//                     continue;
//                 }

//                 if (tmp[voxIndex].isHidden && BloodColorIndex > 0)
//                 {
//                     shape.SetVoxel(voxCoord, (byte)BloodColorIndex); // WTF WTF WTF?!? we should not modify the MODEL in here !!!!           elfapo: AAAAHHH NOOOO!!!! :O    j.k. ;)
//                 }

//                 Color color = model.ColorAtIndex(shape.GetVoxel(voxCoord));

//                 // prepare cube vertices
//                 tmp[voxIndex].numVertices = 0;
//                 tmp[voxIndex].vertexIndexOffsetBegin = currentVertexIndex;

//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeBack || tmp[voxIndex].includeLeft || tmp[voxIndex].includeDown)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[0] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[0]] = new Vector3(-0.5f, -0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeBack || tmp[voxIndex].includeRight || tmp[voxIndex].includeDown)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[1] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[1]] = new Vector3(0.5f, -0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeBack || tmp[voxIndex].includeLeft || tmp[voxIndex].includeUp)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[2] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[2]] = new Vector3(-0.5f, 0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeBack || tmp[voxIndex].includeRight || tmp[voxIndex].includeUp)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[3] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[3]] = new Vector3(0.5f, 0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeForward || tmp[voxIndex].includeLeft || tmp[voxIndex].includeDown)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[4] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[4]] = new Vector3(-0.5f, -0.5f, 0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeForward || tmp[voxIndex].includeRight || tmp[voxIndex].includeDown)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[5] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[5]] = new Vector3(0.5f, -0.5f, 0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeForward || tmp[voxIndex].includeLeft || tmp[voxIndex].includeUp)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[6] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[6]] = new Vector3(-0.5f, 0.5f, 0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeForward || tmp[voxIndex].includeRight || tmp[voxIndex].includeUp)
//                 {
//                     tmp[voxIndex].vertexIndexOffsets[7] = tmp[voxIndex].numVertices++;
//                     tmp[voxIndex].vertexPositionOffsets[tmp[voxIndex].vertexIndexOffsets[7]] = new Vector3(0.5f, 0.5f, 0.5f);
//                 }


//                 // add cube faces
//                 int i = currentTriangleIndex;

//                 // back
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeBack)
//                 {
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[0];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[2];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[1];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[2];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[3];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[1];
//                 }

//                 // Forward
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeForward)
//                 {
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[6];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[4];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[5];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[7];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[6];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[5];
//                 }

//                 // right
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeRight)
//                 {
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[1];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[3];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[5];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[3];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[7];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[5];
//                 }

//                 // left
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeLeft)
//                 {
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[0];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[4];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[2];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[2];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[4];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[6];
//                 }

//                 // up
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeUp)
//                 {
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[2];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[6];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[3];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[3];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[6];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[7];
//                 }

//                 // down
//                 if (optimization != NPVoxOptimization.PER_FACE || tmp[voxIndex].includeDown)
//                 {
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[0];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[1];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[4];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[1];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[5];
//                     triangles[i++] = tmp[voxIndex].vertexIndexOffsetBegin + tmp[voxIndex].vertexIndexOffsets[4];
//                 }


//                 // Tangents
//                 for (int t = 0; t < tmp[voxIndex].numVertices; t++)
//                 {
//                     // store voxel center for shader usage
//                     Vector4 tangent = new Vector4();
//                     tangent.x = tmp[voxIndex].voxelCenter.x;
//                     tangent.y = tmp[voxIndex].voxelCenter.y;
//                     tangent.z = tmp[voxIndex].voxelCenter.z;
//                     // encode model size
//                     tangent.w = ((voxelsToInclude.Size.X & 0x7F) << 14) | ((voxelsToInclude.Size.Y & 0x7F) << 7) | (voxelsToInclude.Size.Z & 0x7F);

//                     tangents[tmp[voxIndex].vertexIndexOffsetBegin + t] = tangent;
//                 }

//                 // UVs
//                 for (int t = 0; t < tmp[voxIndex].numVertices; t++)
//                 {
//                     colors[tmp[voxIndex].vertexIndexOffsetBegin + t] = color;
//                 }

//                 // translate & scale vertices to voxel position
//                 for (int t = 0; t < tmp[voxIndex].numVertices; t++)
//                 {
//                     vertices[tmp[voxIndex].vertexIndexOffsetBegin + t] = tmp[voxIndex].voxelCenter + Vector3.Scale(tmp[voxIndex].vertexPositionOffsets[t], cubeSize);
//                 }

//                 currentTriangleIndex = i;
//                 currentVertexIndex += tmp[voxIndex].numVertices;
//                 voxIndex++;
//             }
//         }

//         // elfapo: Remove invalid voxel information
//         Array.Resize(ref tmp, voxIndex);


//         ////////////////////////////////////// NORMAL STAGES ////////////////////////////////////////
//         // elfapo TODO: Test area 'Normal Processor' Move Normal Processor stages to Normal Processor Pipeline: 

//         //NPVoxNormalProcessor_Voxel generator = ScriptableObject.CreateInstance<NPVoxNormalProcessor_Voxel>();
//         //NPVoxNormalProcessor_Variance processor = ScriptableObject.CreateInstance<NPVoxNormalProcessor_Variance>();

//         //generator.InitOutputBuffer(normals);
//         //processor.InitOutputBuffer(normals);

//         //processor.NormalVariance = NormalVariance;
//         //processor.NormalVarianceSeed = NormalVarianceSeed;

//         //if (NormalModePerVoxelGroup != null && NormalModePerVoxelGroup.Length > 0)
//         //{
//         //    for (int i = 0; i < NormalModePerVoxelGroup.Length; i++)
//         //    {
//         //        generator.ClearVoxelGroupFilters();
//         //        generator.AddVoxelGroupFilter(i);
//         //        generator.NormalMode = NormalModePerVoxelGroup[i];
//         //        generator.Process(model, tmp, normals, normals);
//         //    }

//         //    processor.Process(model, tmp, normals, normals);
//         //}
//         //else
//         //{
//         //    generator.NormalMode = NormalMode;
//         //    generator.Process(model, tmp, normals, normals);
//         //    processor.Process(model, tmp, normals, normals);
//         //}


//         //ScriptableObject.DestroyImmediate(generator);
//         //ScriptableObject.DestroyImmediate(processor);

//         if (normalProcessors != null)
//         {
//             normalProcessors.Run(model, tmp, normals, normals);
//         }

//         //////////////////////////////////////////////////////////////////////////////////////////////


//         // shrink arrays as needed
//         if (optimization != NPVoxOptimization.OFF)
//         {
//             Array.Resize(ref vertices, currentVertexIndex);
//             Array.Resize(ref normals, currentVertexIndex);
//             Array.Resize(ref tangents, currentVertexIndex);
//             Array.Resize(ref colors, currentVertexIndex);
//         }

//         mesh.vertices = vertices;
//         mesh.triangles = triangles;
//         mesh.normals = normals;
//         mesh.tangents = tangents;
//         mesh.colors = colors;
//         mesh.bounds = new Bounds(Vector3.zero, size);

//         if (NormalMode == NPVoxNormalMode.AUTO)
//         {
//             mesh.RecalculateNormals();
//         }
//     }

//     public static NPVoxMeshData[] GenerateVoxMeshData(
//         NPVoxModel model,
//         NPVoxShape shape,
//         Vector3 cubeSize,
//         NPVoxOptimization optimization = NPVoxOptimization.OFF,
//         NPVoxFaces loop = null,
//         NPVoxFaces cutout = null,
//         NPVoxFaces include = null)
//     {
//         var voxMeshData = new NPVoxMeshData[shape.NumVoxels];

//         int currentVertexIndex = 0;

//         if (loop == null)
//         {
//             loop = new NPVoxFaces();
//         }

//         if (include == null)
//         {
//             include = new NPVoxFaces(1, 1, 1, 1, 1, 1);
//         }

//         NPVoxBox voxelsToInclude = shape.BoundingBox;
//         Vector3 cutoutOffset = Vector3.zero;
//         if (cutout != null)
//         {
//             Vector3 originalCenter = voxelsToInclude.SaveCenter;
//             voxelsToInclude.Left = (sbyte)Mathf.Abs(cutout.Left);
//             voxelsToInclude.Down = (sbyte)Mathf.Abs(cutout.Down);
//             voxelsToInclude.Back = (sbyte)Mathf.Abs(cutout.Back);
//             voxelsToInclude.Right = (sbyte)(voxelsToInclude.Right - (sbyte)Mathf.Abs(cutout.Right));
//             voxelsToInclude.Up = (sbyte)(voxelsToInclude.Up - (sbyte)Mathf.Abs(cutout.Up));
//             voxelsToInclude.Forward = (sbyte)(voxelsToInclude.Forward - (sbyte)Mathf.Abs(cutout.Forward));
//             cutoutOffset = Vector3.Scale(originalCenter - voxelsToInclude.SaveCenter, cubeSize);
//         }

//         NPVoxToUnity npVoxToUnity = new NPVoxToUnity(model, cubeSize);
//         Vector3 size = new Vector3(
//             voxelsToInclude.Size.X * cubeSize.x,
//             voxelsToInclude.Size.Y * cubeSize.y,
//             voxelsToInclude.Size.Z * cubeSize.z
//         );

//         NPVoxBox voxelNormalNeighbours = new NPVoxBox(new NPVoxCoord(-1, -1, -1), new NPVoxCoord(1, 1, 1));

//         // Collect temporary data to use for model generation
//         int voxIndex = 0;
//         foreach (NPVoxCoord voxCoord in voxelsToInclude.Enumerate())
//         {
//             if (shape.HasVoxel(voxCoord))
//             {
//                 voxMeshData[voxIndex] = new NPVoxMeshData();

//                 voxMeshData[voxIndex].loop = loop;
//                 voxMeshData[voxIndex].cutout = cutout;
//                 voxMeshData[voxIndex].include = include;

//                 // Compute voxel center
//                 voxMeshData[voxIndex].voxelCenter = npVoxToUnity.ToUnityPosition(voxCoord) + cutoutOffset;
//                 voxMeshData[voxIndex].voxCoord = voxCoord;

//                 voxMeshData[voxIndex].voxToUnity = npVoxToUnity;

//                 // do we have this side
//                 voxMeshData[voxIndex].hasLeft = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.LEFT, loop));
//                 voxMeshData[voxIndex].hasRight = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.RIGHT, loop));
//                 voxMeshData[voxIndex].hasDown = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.DOWN, loop));
//                 voxMeshData[voxIndex].hasUp = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.UP, loop));
//                 voxMeshData[voxIndex].hasForward = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.FORWARD, loop));
//                 voxMeshData[voxIndex].hasBack = !shape.HasVoxel(shape.LoopCoord(voxCoord + NPVoxCoord.BACK, loop));

//                 // do we actually want to include this side in our mesh
//                 // NOTE: cutout < 0 means we still render the mesh even though it is cutout
//                 //       cutout > 0 means we don't render the mesh when cutout
//                 voxMeshData[voxIndex].includeLeft = (voxMeshData[voxIndex].hasLeft || (cutout.Left < 0 && voxCoord.X == voxelsToInclude.Left)) && include.Left == 1;
//                 voxMeshData[voxIndex].includeRight = (voxMeshData[voxIndex].hasRight || (cutout.Right < 0 && voxCoord.X == voxelsToInclude.Right)) && include.Right == 1;
//                 voxMeshData[voxIndex].includeUp = (voxMeshData[voxIndex].hasUp || (cutout.Up < 0 && voxCoord.Y == voxelsToInclude.Up)) && include.Up == 1;
//                 voxMeshData[voxIndex].includeDown = (voxMeshData[voxIndex].hasDown || (cutout.Down < 0 && voxCoord.Y == voxelsToInclude.Down)) && include.Down == 1;
//                 voxMeshData[voxIndex].includeBack = (voxMeshData[voxIndex].hasBack || (cutout.Back < 0 && voxCoord.Z == voxelsToInclude.Back)) && include.Back == 1;
//                 voxMeshData[voxIndex].includeForward = (voxMeshData[voxIndex].hasForward || (cutout.Forward < 0 && voxCoord.Z == voxelsToInclude.Forward)) && include.Forward == 1;

//                 voxMeshData[voxIndex].isHidden = !voxMeshData[voxIndex].hasForward &&
//                                 !voxMeshData[voxIndex].hasBack &&
//                                 !voxMeshData[voxIndex].hasLeft &&
//                                 !voxMeshData[voxIndex].hasRight &&
//                                 !voxMeshData[voxIndex].hasUp &&
//                                 !voxMeshData[voxIndex].hasDown;


//                 if (voxMeshData[voxIndex].isHidden && optimization == NPVoxOptimization.PER_VOXEL)
//                 {
//                     continue;
//                 }

//                 // prepare cube vertices
//                 voxMeshData[voxIndex].numVertices = 0;
//                 voxMeshData[voxIndex].vertexIndexOffsetBegin = currentVertexIndex;

//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeBack || voxMeshData[voxIndex].includeLeft || voxMeshData[voxIndex].includeDown)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[0] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[0]] = new Vector3(-0.5f, -0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeBack || voxMeshData[voxIndex].includeRight || voxMeshData[voxIndex].includeDown)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[1] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[1]] = new Vector3(0.5f, -0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeBack || voxMeshData[voxIndex].includeLeft || voxMeshData[voxIndex].includeUp)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[2] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[2]] = new Vector3(-0.5f, 0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeBack || voxMeshData[voxIndex].includeRight || voxMeshData[voxIndex].includeUp)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[3] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[3]] = new Vector3(0.5f, 0.5f, -0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeForward || voxMeshData[voxIndex].includeLeft || voxMeshData[voxIndex].includeDown)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[4] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[4]] = new Vector3(-0.5f, -0.5f, 0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeForward || voxMeshData[voxIndex].includeRight || voxMeshData[voxIndex].includeDown)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[5] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[5]] = new Vector3(0.5f, -0.5f, 0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeForward || voxMeshData[voxIndex].includeLeft || voxMeshData[voxIndex].includeUp)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[6] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[6]] = new Vector3(-0.5f, 0.5f, 0.5f);
//                 }
//                 if (optimization != NPVoxOptimization.PER_FACE || voxMeshData[voxIndex].includeForward || voxMeshData[voxIndex].includeRight || voxMeshData[voxIndex].includeUp)
//                 {
//                     voxMeshData[voxIndex].vertexIndexOffsets[7] = voxMeshData[voxIndex].numVertices++;
//                     voxMeshData[voxIndex].vertexPositionOffsets[voxMeshData[voxIndex].vertexIndexOffsets[7]] = new Vector3(0.5f, 0.5f, 0.5f);
//                 }

//                 currentVertexIndex += voxMeshData[voxIndex].numVertices;
//                 voxIndex++;
//             }
//         }

//         Array.Resize(ref voxMeshData, voxIndex);

//         return voxMeshData;
//     }
// }