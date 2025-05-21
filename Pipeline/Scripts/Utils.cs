#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Collections.Generic;

namespace UVT.Pipe
{
    public class Utils
    {
        public static T[] GetTypedFactories<T>(UnityEngine.Object[] objects) where T : class
        {
            List<T> result = new List<T>();
            foreach (UnityEngine.Object item in objects)
            {
                if (item is T)
                {
                    result.Add(item as T);
                }
            }
            return result.ToArray();
        }

        public static UnityEngine.Object[] GetUntypedFactories<T>(T[] objects) where T : class
        {
            List<UnityEngine.Object> result = new List<UnityEngine.Object>();
            foreach (T item in objects)
            {
                result.Add(item as UnityEngine.Object);
            }
            return result.ToArray();
        }

        public static void InvalidateAndReimportAll(UnityEngine.Object container)
        {
            IPipeImportable[] allImportables = GetByType<IPipeImportable>(container);
            InvalidateAll(allImportables);
            EditorUtility.SetDirty(container as UnityEngine.Object);
            AssetDatabase.SaveAssets();
        }
        public static void InvalidateAndReimportAll(UnityEngine.Object[] containers)
        {
            foreach (UnityEngine.Object container in containers)
            {
                InvalidateAndReimportAll(container);
            }
        }

        public static void InvalidateAll(IPipeImportable[] allImportables, bool deep = false)
        {
            foreach (IPipeImportable imp in allImportables)
            {
                if (imp != null && ((UnityEngine.Object)imp))
                {
                    imp.Invalidate(deep);
                }
            }
        }

        public static bool AreSourcesReady(IPipeImportable importable)
        {
            // TODO find a more viable way
            foreach (IPipeImportable source in EachSource(importable))
            {
                if (AreSourcesReady(source))
                {
                    return true;
                }
            }
            if (importable is NPVoxMagickaSource)
            {
                return true;
            }
            return false;
        }
        public static void InvalidateAndReimportAllDeep(UnityEngine.Object container)
        {
            IPipeImportable[] allImportables = FindOutputPipes(GetByType<IPipeImportable>(container));
            InvalidateAll(allImportables, true);
            EditorUtility.SetDirty(container as UnityEngine.Object);
            AssetDatabase.SaveAssets();
        }

        public static void InvalidateAndReimportAllDeep(UnityEngine.Object[] containers)
        {
            foreach (UnityEngine.Object container in containers)
            {
                InvalidateAndReimportAllDeep(container);
            }
        }

        public static void InvalidateAndReimportDeep(IPipeImportable output)
        {
            output.Invalidate(true);
            EditorUtility.SetDirty(output as UnityEngine.Object);
            AssetDatabase.SaveAssets();
        }

        public static IPipeImportable[] GetImportables(PipeContainer container)
        {
            return GetImportables(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container)));
        }

        public static IPipeImportable[] GetImportables(string path)
        {
            return GetImportables(AssetDatabase.LoadAllAssetsAtPath(path));
        }

        public static IPipeImportable[] GetImportables(UnityEngine.Object[] objects)
        {
            return GetByType<IPipeImportable>(objects);
        }

        public static PipeContainer GetContainer(UnityEngine.Object[] objects)
        {
            PipeContainer[] container = GetByType<PipeContainer>(objects);
            if (container.Length > 0)
            {
                return container[0];
            }
            return null;
        }

        public static T[] GetByType<T>(UnityEngine.Object container) where T : class
        {
            return GetByType<T>(AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(container)));
        }

        public static T[] GetByType<T>(UnityEngine.Object[] objects) where T : class
        {
            List<T> result = new List<T>();
            foreach (UnityEngine.Object item in objects)
            {
                if (item is T)
                {
                    result.Add(item as T);
                }
            }
            return result.ToArray();
        }

        public static IPipeImportable[] OrderForImport(IPipeImportable[] importables)
        {
            List<IPipeImportable> result = new List<IPipeImportable>();
            IPipeImportable[] output = FindOutputPipes(importables);
            foreach (IPipeImportable item in output)
            {
                if (!item.IsTemplate())
                {
                    AddSourcesToList(item, result, importables);
                }
            }
            return result.ToArray();
        }

        /// <summary>
        /// Add Sources to List by checking the Input property ( thus also referencing other assets ).
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="outList">Out list.</param>
        private static void AddSourcesToList(IPipeImportable target, List<IPipeImportable> outList)
        {
            IPipeComposite composite = target as IPipeComposite;
            if (composite == null)
            {
                outList.Add(target);
                return;
            }
            foreach (IPipeImportable importable in EachSource(target))
            {
                AddSourcesToList(importable, outList);
            }
            outList.Add(target);
        }

        /// <summary>
        /// Add Sources to list that are contained in the sourceList. Removes previous finding ( to ensure only imported once )
        /// </summary>
        /// <param name="target">Target.</param>
        /// <param name="outList">Out list.</param>
        /// <param name="sourceList">Source list.</param>
        private static void AddSourcesToList(IPipeImportable target, List<IPipeImportable> outList, IPipeImportable[] sourceList)
        {
            IPipeComposite composite = target as IPipeComposite;
            if (composite == null)
            {
                if (outList.Contains(target))
                {
                    outList.Remove(target);
                }
                outList.Add(target);
                return;
            }
            foreach (IPipeImportable importable in EachSource(target, sourceList))
            {
                AddSourcesToList(importable, outList, sourceList);
            }

            if (outList.Contains(target))
            {
                outList.Remove(target);
            }
            outList.Add(target);
        }

        public static IPipeComposite[] FindNextPipes(IPipeImportable[] importables, IPipeImportable target)
        {
            List<IPipeComposite> nextPipes = new List<IPipeComposite>();
            foreach (var pipe in importables)
            {
                if (IsPrevious(pipe, target))
                {
                    nextPipes.Add(pipe as IPipeComposite);
                }
            }
            return nextPipes.ToArray();
        }

        public static T[] FindNextPipeOfType<T>(IPipeImportable[] importables, IPipeImportable target) where T : class
        {
            List<T> nextPipes = new List<T>();
            foreach (var pipe in importables)
            {
                if (IsPrevious(pipe, target) && pipe is T)
                {
                    nextPipes.Add(pipe as T);
                }
            }
            return nextPipes.ToArray();
        }

        public static T FindPreviousOfType<T>(IPipeImportable target) where T : class
        {
            foreach (IPipeImportable source in EachSource(target))
            {
                if (source is T)
                {
                    return (T)source;
                }
                T previous = FindPreviousOfType<T>(source);
                if (previous != null)
                {
                    return previous;
                }
            }
            return null;
        }

        public static T FindPrevious<T>(IPipeImportable target) where T : class
        {
            foreach (IPipeImportable source in EachSource(target))
            {
                if (source is T)
                {
                    return (T)source;
                }
                T previous = FindPreviousOfType<T>(source);
                if (previous != null)
                {
                    return previous;
                }
            }
            return null;
        }

        public static IPipeImportable[] FindOutputPipes(IPipeImportable[] importables)
        {
            List<IPipeImportable> output = new List<IPipeImportable>();
            foreach (var pipe in importables)
            {
                bool isOutputPipe = true;
                foreach (var pipe2 in importables)
                {
                    if (pipe != pipe2 && IsPrevious(pipe2, pipe))
                    {
                        isOutputPipe = false;
                        break;
                    }
                }
                if (isOutputPipe)
                {
                    output.Add(pipe);
                }
            }
            return output.ToArray();
        }

        /// <summary>
        /// Gets the matching pipes in all input containers that follow the same path (used for multiinstance editing)
        /// </summary>
        /// <returns>The matching pipes.</returns>
        /// <param name="InputContainers">Input containers.</param>
        /// <param name="lookup">Lookup.</param>
        public static IPipeImportable[] GetSimiliarPipes(UnityEngine.Object[] inputContainers, PipeContainer referenceContainer, IPipeImportable lookup, out string warningMessage)
        {
            // TODO: check for exact matchin structure, there may be cases where we have multiple pipes in a container
            warningMessage = "";

            List<IPipeImportable> result = new List<IPipeImportable>();

            bool bFoundMultiple = false;
            bool bNotFound = false;

            foreach (UnityEngine.Object container in inputContainers)
            {
                IPipeImportable[] instances2 = GetByType<IPipeImportable>(container);
                bool bFound = false;
                foreach (IPipeImportable item in instances2)
                {
                    if (item.GetType() == lookup.GetType())
                    {
                        result.Add(item);
                        if (bFound)
                        {
                            bFoundMultiple = true;
                        }
                        bFound = true;

                    }
                }

                if (!bFound)
                {
                    bNotFound = true;
                }
            }

            if (bFoundMultiple)
            {
                warningMessage += "Found Multiple Occurences in some containers. ";
            }

            if (bNotFound)
            {
                warningMessage += "Not foundt in some containers. ";
            }

            return result.ToArray();
        }

        public static bool IsPrevious(IPipeImportable target, IPipeImportable previous, bool recursive = false)
        {
            foreach (IPipeImportable importable in EachSource(target))
            {
                if (importable == previous)
                {
                    return true;
                }
                else if (recursive && IsPrevious(importable, previous, true))
                {
                    return true;
                }
            }
            return false;
        }

        public static IEnumerable<IPipeImportable> EachSource(IPipeImportable target)
        {
            IPipeComposite targetComposite = target as IPipeComposite;
            if (targetComposite != null)
            {
                IPipeImportable[] sources = targetComposite.GetAllInputs();
                foreach (IPipeImportable item in sources)
                {
                    yield return item;
                }
            }
        }

        public static IEnumerable<IPipeImportable> EachSource(IPipeImportable target, IPipeImportable[] importables)
        {
            IPipeComposite targetComposite = target as IPipeComposite;
            if (targetComposite != null)
            {
                IPipeImportable[] sources = targetComposite.GetAllInputs();
                foreach (IPipeImportable item in sources)
                {
                    if (ArrayUtility.Contains(importables, item))
                    {
                        yield return item;
                    }
                }
            }
        }


        public static PipeContainer ClonePipeContainer(PipeContainer container, string path)
        {
            PipeContainer newContainer = PipeContainer.CreateInstance<PipeContainer>();
            AssetDatabase.CreateAsset(newContainer, path);

            IPipeImportable[] templateImportables = Utils.GetImportables(container);
            foreach (IPipeImportable pipe in Utils.FindOutputPipes(templateImportables))
            {
                CloneRecursive(templateImportables, pipe, path);
            }
            return newContainer;
        }

        public static IPipeImportable CloneRecursive(IPipeImportable[] allImportables, IPipeImportable sourcePipe, string targetPath)
        {
            IPipeImportable clone = (IPipeImportable)sourcePipe.Clone();
            Utils.CreateAttachedPipe(targetPath, clone);

            if (clone is IPipeComposite)
            {
                IPipeImportable sourceOfSource = ((IPipeComposite)clone).Input;
                if (ArrayUtility.IndexOf(allImportables, sourceOfSource) > -1)
                {
                    ((IPipeComposite)clone).Input = (IPipeImportable)CloneRecursive(allImportables, sourceOfSource, targetPath);
                }
            }
            return clone;
        }

        public static T CreatePipeContainer<T>(string path) where T : PipeContainer
        {
            T pipeContainer = (T)PipeContainer.CreateInstance(typeof(T));
            // Undo.RegisterCreatedObjectUndo(pipeContainer, "Created a pipe Container");
            AssetDatabase.CreateAsset(pipeContainer, path);
            pipeContainer.OnAssetCreated();
            AssetDatabase.SaveAssets();
            return pipeContainer;
        }

        public static IPipeImportable CreateAttachedPipe(string path, System.Type type, IPipeImportable previous = null)
        {
            IPipeImportable instance = CreateAttachedPipe(path, UnityEngine.ScriptableObject.CreateInstance(type) as IPipeImportable);
            if (previous != null && (instance is IPipeComposite))
            {
                ((IPipeComposite)instance).Input = previous;
                EditorUtility.SetDirty(instance as UnityEngine.Object);
            }
            return instance;
        }

        public static IPipeImportable CreateSeparatedPipe(string path, System.Type type, IPipeImportable previous = null)
        {
            IPipeImportable instance = CreateSeparatedPipe(path, UnityEngine.ScriptableObject.CreateInstance(type) as IPipeImportable);
            if (previous != null && (instance is IPipeComposite))
            {
                ((IPipeComposite)instance).Input = previous;
                EditorUtility.SetDirty(instance as UnityEngine.Object);
            }
            return instance;
        }

        public static IPipeImportable CreateAttachedPipe(string path, IPipeImportable pipe)
        {
            UnityEngine.Object obj = pipe as UnityEngine.Object;
            obj.hideFlags = HideFlags.HideInHierarchy;
            IPipeImportable importable = obj as IPipeImportable;
            obj.name = importable.GetTypeName();
            AssetDatabase.AddObjectToAsset(obj, path);
            importable.IncludeSubAssets(path);
            UnityEditor.EditorUtility.SetDirty(pipe as UnityEngine.Object);
            // AssetDatabase.SaveAssets();
            // AssetDatabase.Refresh();
            // UnityEditor.Selection.activeObject = createdFactory;
            return importable;
        }


        public static IPipeImportable CreateSeparatedPipe(string originalPath, IPipeImportable pipe)
        {
            string path = originalPath.Substring(0, originalPath.Length - 6) + "_" + pipe.GetTypeName() + ".asset";
            CreatePipeContainer<PipeContainer>(path);
            return CreateAttachedPipe(path, pipe);
        }

        public static T DrawSourceSelector<T>(string label, T oldValue, IPipeImportable exclude = null) where T : class
        {
            UnityEngine.Object obj = oldValue as UnityEngine.Object;

            string path = AssetDatabase.GetAssetPath(obj);
            PipeContainer container = AssetDatabase.LoadAssetAtPath(path, typeof(PipeContainer)) as PipeContainer;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(label);
            container = (PipeContainer)EditorGUILayout.ObjectField(container, typeof(PipeContainer), false);
            path = AssetDatabase.GetAssetPath(container);
            T[] factories = GetTypedFactories<T>(container ? container.GetAllSelectableFactories() : new UnityEngine.Object[0] { });

            if (factories.Length != 1)
            {
                string[] options = new string[factories.Length];
                int i = 0;
                int selected = -1;
                for (int j = 0; j < factories.Length; j++)
                {
                    IPipeImportable fact = factories[j] as IPipeImportable;
                    if (fact == oldValue)
                    {
                        selected = i;
                    }
                    if (fact != exclude && !IsPrevious(fact, exclude, true))
                    {
                        options[i] = i + " " + container.GetDisplayName(fact);
                        factories[i] = factories[j];
                        i++;
                    }
                }
                Array.Resize(ref options, i);
                Array.Resize(ref factories, i);

                if (selected == -1 && options.Length > 0)
                {
                    selected = 0;
                }

                int newSelected = EditorGUILayout.Popup(selected, options);
                EditorGUILayout.EndHorizontal();

                if (newSelected == -1)
                {
                    return null;
                }
                return factories[newSelected];
            }
            else
            {
                if (factories[0] != exclude && !IsPrevious(factories[0] as IPipeImportable, exclude, true))
                {
                    GUILayout.Label(container.GetDisplayName(factories[0] as IPipeImportable));
                    EditorGUILayout.EndHorizontal();
                    return factories[0];
                }
                EditorGUILayout.EndHorizontal();
                return null;
            }
        }


        public static T DrawSourcePropertySelector<T>(GUIContent label, Rect position, T oldValue, IPipeImportable exclude = null) where T : class
        {
            UnityEngine.Object obj = oldValue as UnityEngine.Object;

            string path = AssetDatabase.GetAssetPath(obj);
            PipeContainer container = AssetDatabase.LoadAssetAtPath(path, typeof(PipeContainer)) as PipeContainer;

            // EditorGUILayout.BeginHorizontal();

            Rect containerPosition = new Rect(position.x, position.y, position.width / 4 * 3, position.height);
            Rect pipePosition = new Rect(position.x + position.width / 4 * 3, position.y, position.width / 4, position.height);

            container = (PipeContainer)EditorGUI.ObjectField(containerPosition, label, container, typeof(PipeContainer), false);
            path = AssetDatabase.GetAssetPath(container);
            T[] factories = GetTypedFactories<T>(container ? container.GetAllSelectableFactories() : new UnityEngine.Object[0] { });

            if (factories.Length != 1)
            {
                string[] options = new string[factories.Length];
                int i = 0;
                int selected = -1;
                for (int j = 0; j < factories.Length; j++)
                {
                    IPipeImportable fact = factories[j] as IPipeImportable;
                    if (fact == oldValue)
                    {
                        selected = i;
                    }
                    if (fact != exclude && !IsPrevious(fact, exclude, true))
                    {
                        options[i] = i + " " + container.GetDisplayName(fact);
                        factories[i] = factories[j];
                        i++;
                    }
                }
                Array.Resize(ref options, i);
                Array.Resize(ref factories, i);

                if (selected == -1 && options.Length > 0)
                {
                    selected = 0;
                }

                int newSelected = EditorGUI.Popup(pipePosition, selected, options);

                if (newSelected == -1)
                {
                    return null;
                }
                //return oldValue;
                return factories[newSelected];
            }
            else
            {

                if (factories[0] != exclude && !IsPrevious(factories[0] as IPipeImportable, exclude, true))
                {
                    EditorGUI.LabelField(pipePosition, container.GetDisplayName(factories[0] as IPipeImportable));
                    return factories[0];
                }

                return null;
            }
        }

        public static PipeContainer GetContainerForVoxPath(string asset)
        {
            string filename = Path.GetFileNameWithoutExtension(asset);
            string basename = Path.GetDirectoryName(asset);
            string pipelinePath = Path.Combine(Path.Combine(basename, "Pipeline/"), filename + ".asset");

            // Create or Load existing Pipeline
            PipeContainer pipeContainer = (PipeContainer)AssetDatabase.LoadAssetAtPath(pipelinePath, typeof(PipeContainer));
            return pipeContainer;
        }

        /// <summary>
        //	This makes it easy to create, name and place unique new ScriptableObject asset files.
        /// </summary>
        public static string GetCreateScriptableObjectAssetPath<T>(string name = null) where T : ScriptableObject
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }

            if (name == null)
            {
                name = "/New " + typeof(T).ToString();
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + name + ".asset");
            return assetPathAndName;
        }

        /// <summary>
        //This makes it easy to create, name and place unique new ScriptableObject asset files.
        // from http://wiki.unity3d.com/index.php?title=CreateScriptableObjectAsset
        /// </summary>
        public static void CreateScriptableObjectAsset<T>() where T : ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, GetCreateScriptableObjectAssetPath<T>());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = asset;
        }

        public static string GetPipelineDebugString(IPipeImportable element, bool withTimes = false)
        {
            string prefix = "";
            if (element is IPipeComposite)
            {
                prefix = GetPipelineDebugString(((IPipeComposite)element).Input, withTimes);
            }

            string cur = (!string.IsNullOrEmpty(element.GetInstanceName()) ? element.GetInstanceName() : element.GetTypeName());

            if (withTimes)
            {
                cur += " (" + (int)(EditorApplication.timeSinceStartup - element.GetLastInvalidatedTime()) + ") ";
            }

            return prefix + " / " + cur;
        }
    }


#endif
}