// using UnityEngine;
// using System.Collections;
// using System.Collections.Generic;

// #if UNITY_EDITOR
// using UnityEditor;
// #endif

// public class NPVoxMeshInstance : MonoBehaviour
// {
//     [SerializeField, HideInInspector]
//     private NPVoxObjectData voxObjectData;
//     [SerializeField, HideInInspector]
//     private Dictionary<int, GameObject> nodeMap;


//     [NPipeSelectorAttribute(typeof(NPVoxIMeshFactory))]
//     public UnityEngine.Object meshFactory;

//     public NPVoxIMeshFactory MeshFactory
//     {
//         get
//         {
//             return meshFactory as NPVoxIMeshFactory;
//         }
//         set
//         {
//             meshFactory = (UnityEngine.Object)value;
//         }
//     }

//     public void UpdateMesh()
//     {
//         this.VoxObjectData = MeshFactory.GetProduct();
//     }

//     public NPVoxObjectData VoxObjectData
//     {
//         get => voxObjectData;
//         set
//         {
//             voxObjectData = value;
//             ApplyObjectData();
//         }
//     }
//     public Vector3 VoxelSize
//     {
//         get
//         {
//             if (MeshFactory is NPVoxMeshOutput output)
//             {
//                 return output.VoxelSize;
//             }
//             else
//             {
//                 return Vector3.one * 0.1f;
//             }
//         }
//     }

//     public void Align(Transform transform = null)
//     {
//         if (transform == null)
//         {
//             transform = this.transform;
//         }
//         if (MeshFactory == null)
//         {
//             return;
//         }
//         transform.SetLocalPositionAndRotation(new Vector3(
//             Mathf.Round(transform.localPosition.x / (VoxelSize.x * 0.5f)) * VoxelSize.x * 0.5f,
//             Mathf.Round(transform.localPosition.y / (VoxelSize.y * 0.5f)) * VoxelSize.y * 0.5f,
//             Mathf.Round(transform.localPosition.z / (VoxelSize.z * 0.5f)) * VoxelSize.z * 0.5f
//         ), Quaternion.Euler(
//             Mathf.Round(transform.localRotation.eulerAngles.x / 15f) * 15f,
//             Mathf.Round(transform.localRotation.eulerAngles.y / 15f) * 15f,
//             Mathf.Round(transform.localRotation.eulerAngles.z / 15f) * 15f
//         ));
//     }


//     private void ApplyObjectData()
//     {
//         if (nodeMap == null)
//         {
//             nodeMap = new Dictionary<int, GameObject>();
//         }
//         else
//         {
//             foreach (var node in nodeMap)
//             {
//                 DestroyImmediate(node.Value);
//             }
//             nodeMap.Clear();
//         }

//         if (voxObjectData != null)
//         {
//             foreach (var node in voxObjectData.Nodes)
//             {
//                 GameObject go = new(node.Name);
//                 go.transform.SetParent(this.transform, false);
//                 go.transform.localPosition = Vector3.zero;
//                 go.transform.localRotation = Quaternion.identity;
//                 go.transform.localScale = Vector3.one;

//                 nodeMap.Add(node.ID, go);
//             }
//         }
//     }

//     private GameObject CreateObjectForNode(NPVoxNodeData node, GameObject parent = null)
//     {
//         nodeMap ??= new();

//         // Node already created
//         if (nodeMap.ContainsKey(node.Id)) { return; }

//         GameObject go = new GameObject(node.Name);

//         if (parent != null)
//         {
//             go.transform.SetParent(parent.transform, false);
//         }

//         go.transform.localPosition = node.Transform.translation;
//         go.transform.localRotation = Quaternion.Euler(node.Transform.rotation);
//         go.transform.localScale = node.Transform.scale;

//         if (node.Mesh != null)
//         {
//             MeshFilter meshFilter = go.AddComponent<MeshFilter>();
//             meshFilter.sharedMesh = node.Mesh;

//             MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
//             meshRenderer.sharedMaterial = node.Material;
//         }

//         foreach (var childNode in node.Children)
//         {
//             CreateObjectForNode(childNode, go);
//         }



//         nodeMap.Add(node.Id, go);
//     }

//     private void ClearObjectData()
//     {
//         if (nodeMap != null)
//         {
//             foreach (var node in nodeMap)
//             {
//                 DestroyImmediate(node.Value);
//             }
//             nodeMap.Clear();
//         }
//         voxObjectData = null;
//     }


// #if UNITY_EDITOR
//     void OnDrawGizmos()
//     {
//         //        if (Selection.activeGameObject != this.gameObject)
//         //        {
//         //            return;
//         //        }

//         NPVoxMeshOutput MeshOutput = MeshFactory as NPVoxMeshOutput;

//         if (MeshOutput)
//         {
//             NPVoxToUnity npVoxToUnity = MeshOutput.GetNPVoxToUnity();
//             VoxModel model = MeshOutput.GetVoxModel();
//             if (model)
//             {
//                 foreach (var node in model)
//                 {
//                     if (node.Shape is null) { continue; }

//                     foreach (NPVoxSocket socket in node.Shape.Sockets)
//                     {
//                         Vector3 anchorPos = npVoxToUnity.ToUnityPosition(socket.Anchor);
//                         Quaternion rotation = Quaternion.Euler(socket.EulerAngles);
//                         Vector3 anchorRight = npVoxToUnity.ToUnityDirection(rotation * Vector3.right);
//                         Vector3 anchorUp = npVoxToUnity.ToUnityDirection(rotation * Vector3.up);
//                         Vector3 anchorForward = npVoxToUnity.ToUnityDirection(rotation * Vector3.forward);

//                         Gizmos.color = new Color(0.5f, 1.0f, 0.1f, 0.75f);
//                         Gizmos.DrawCube(transform.position + anchorPos, Vector3.one * 0.4f);

//                         Gizmos.color = Color.red;
//                         Gizmos.DrawLine(transform.position + anchorPos, transform.position + anchorPos + anchorRight * 10.0f);
//                         Gizmos.color = Color.green;
//                         Gizmos.DrawLine(transform.position + anchorPos, transform.position + anchorPos + anchorUp * 10.0f);
//                         Gizmos.color = Color.blue;
//                         Gizmos.DrawLine(transform.position + anchorPos, transform.position + anchorPos + anchorForward * 10.0f);
//                     }
//                 }
//             }
//         }
//     }
// #endif
// }
