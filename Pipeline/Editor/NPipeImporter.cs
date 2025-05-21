/*
 * NPipeline Importer 2016 By nilspferd.net
 */
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using System.IO;
using System.Collections.Generic;

public class NPipeImporter : AssetPostprocessor
{
    struct NPipeImportFile
    {
        public PipeContainer Container;

        /// <summary>
        /// ordered list of importables
        /// </summary>
        public IPipeImportable[] importables;

        /// <summary>
        /// path for this file
        /// </summary>
        public string Path;
    }
    private static Dictionary<string, NPipeImportFile> RetryFiles = new Dictionary<string, NPipeImportFile>();

    class NPipeImportFileComparer : IComparer<NPipeImportFile>
    {
        #region IComparer implementation
        public int Compare(NPipeImportFile x, NPipeImportFile y)
        {
            foreach (IPipeImportable xImportable in x.importables)
            {
                foreach (IPipeImportable yImportable in y.importables)
                {
                    if (Utils.IsPrevious(xImportable, yImportable, true))
                    {
                        return 1;
                    }
                    if (Utils.IsPrevious(yImportable, xImportable, true))
                    {
                        return -1;
                    }
                }
            }
            return 0;
        }
        #endregion
    }

    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        List<NPipeImportFile> listOfImportableAssets = new List<NPipeImportFile>();
        bool importedSomething = false;

        listOfImportableAssets.AddRange(RetryFiles.Values);
        RetryFiles.Clear();

        // gather list
        foreach (var asset in importedAssets)
        {
            if (!File.Exists(asset))
            {
                continue;
            }
            // string uuid = AssetDatabase.AssetPathToGUID(asset);
            string extension = Path.GetExtension(asset);

            if (extension == ".asset")
            {
                Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(asset);
                PipeContainer container = Utils.GetContainer(allAssets);
                if (container)
                {
                    container.OnImport();
                }

                IPipeImportable[] importables = Utils.GetImportables(allAssets);
                IPipeImportable[] ordered = Utils.OrderForImport(importables);

                if (ordered.Length > 0)
                {
                    NPipeImportFile file = new NPipeImportFile();
                    file.importables = ordered;
                    file.Path = asset;
                    file.Container = container;
                    listOfImportableAssets.Add(file);
                    importedSomething = true;

                }
            }
        }


        if (!importedSomething)
        {
            return;
        }

        if (importedSomething)
        {
            int lengthBefore = listOfImportableAssets.Count;
            listOfImportableAssets.Sort(new NPipeImportFileComparer());
            Assert.AreEqual(lengthBefore, listOfImportableAssets.Count);

            string plan = "--------======== NPipeline Import Plan ( " + listOfImportableAssets.Count + " files ) =======-------\n";
            int count = 1;
            foreach (NPipeImportFile importFile in listOfImportableAssets)
            {
                plan += string.Format(" - {0}. {1} ({2} Importables) \n", count++, importFile.Path, importFile.importables.Length);
            }
            Debug.Log(plan);

            foreach (NPipeImportFile importFile in listOfImportableAssets)
            {
                foreach (IPipeImportable importable in importFile.importables)
                {
                    Assert.IsNotNull(importable);
                    if (((UnityEngine.Object)importable))
                    {
                        try
                        {
                            importable.Import();
                            importedSomething = true;
                        }
                        catch (PipeException e)
                        {
                            Debug.LogWarning("Got Exception " + e.Message + " importing a pipe in " + importFile.Path + " (THIS IS ONLY A PROBLEM IF THERE IS NO SUCCESSFUL REPORT FOLLOWING) - ELSE RESTART UNITY");

                            NPipeImportFile file = RetryFiles.ContainsKey(importFile.Path) ? RetryFiles[importFile.Path] : new NPipeImportFile();
                            file.Path = importFile.Path;
                            file.Container = importFile.Container;
                            if (file.importables == null)
                            {
                                file.importables = new IPipeImportable[0];
                            }
                            ArrayUtility.Add(ref file.importables, importable);
                            RetryFiles[file.Path] = file;
                        }
                    }
                }
            }

            AssetDatabase.SaveAssets();
            //            AssetDatabase.Refresh();
        }
    }
    //    }
}
