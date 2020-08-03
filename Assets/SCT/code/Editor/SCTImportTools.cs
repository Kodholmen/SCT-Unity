using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEditor;
using UnityEngine;

namespace sct
{
    public class SCTImportTools
    {
        static void ReadHeader(BinaryReader sr, SpatialCameraAsset cameraAsset)
        {
            cameraAsset.version = sr.ReadInt32();
            cameraAsset.frameCount = sr.ReadInt32();
            cameraAsset.deviceOrientation = sr.ReadInt32();
            if (cameraAsset.version > 202001)
            {
                cameraAsset.horizontalFOV = sr.ReadSingle();
                cameraAsset.verticalFOV = sr.ReadSingle();
            }
            else
            {
                // Default iPad Pro
                cameraAsset.horizontalFOV = 62.2f;
                cameraAsset.verticalFOV = 48.9f;
            }

            if (cameraAsset.version > 202002)
            {
                cameraAsset.focalLengthX = sr.ReadSingle();
                cameraAsset.focalLengthY = sr.ReadSingle();
            }
            else
            {
                // Default iPad Pro
                cameraAsset.focalLengthX= 1592.0f;
                cameraAsset.focalLengthY = 1592.0f;
            }
        }

        static void ReadSkeletonDefinition(BinaryReader sr, SpatialSkeletonAsset skeletonAsset)
        {
            // Read skeleton definition
            int jointCount = sr.ReadInt32();
            skeletonAsset.jointNames = new string[jointCount];
            for (int i = 0; i < jointCount; ++i)
            {
                string name = SpatialUtils.readString(sr);
                skeletonAsset.jointNames[i] = name;
            }

            int parentCount = sr.ReadInt32();
            skeletonAsset.parents = new int[parentCount];
            for (int i = 0; i < parentCount; ++i)
            {
                int parent = sr.ReadInt32();
                skeletonAsset.parents[i] = parent;
            }

            skeletonAsset.refSkeleton = new BoneData[jointCount];
            for (int b = 0; b < jointCount; ++b)
            {
                skeletonAsset.refSkeleton[b] = new BoneData();
                SpatialUtils.readBoneData(sr, skeletonAsset.refSkeleton[b], skeletonAsset.jointNames[b]);
            }
        }

        [MenuItem("SCT/Import Spatial Camera")]
        static void ImportSpatialCamera()
        {
            string[] filters = { "Replay Files", "dat" };
            string fileName = EditorUtility.OpenFilePanelWithFilters(
                "Load Replay",
                ".",
                filters);

            if (fileName.Length == 0)
                return;

            SpatialCameraAsset cameraAsset = SpatialCameraAsset.CreateInstance<SpatialCameraAsset>();
            cameraAsset.captureType = SpatialCameraAsset.CaptureType.Camera;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader sr = new BinaryReader(fs))
                    {
                        ReadHeader(sr, cameraAsset);
                        cameraAsset.frameData = sr.ReadBytes((int)(fs.Length - fs.Position));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Error parsing {0} with exception: {1}", fileName, e);
            }


            // Save Asset

            string savePath = EditorUtility.SaveFilePanelInProject("Save Asset", "Camera.asset", "asset", "Please enter a file name to save the asset to");

            if (savePath.Length == 0)
                return;

            AssetDatabase.CreateAsset(cameraAsset, savePath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = cameraAsset;
        }



        [MenuItem("SCT/Import Spatial Skeleton")]
        static void ImportSpatialSkeleton()
        {
            string[] filters = { "Replay Files", "dat" };
            string fileName = EditorUtility.OpenFilePanelWithFilters(
                "Load Replay",
                ".",
                filters);

            if (fileName.Length == 0)
                return;

            SpatialSkeletonAsset skeletonAsset = SpatialCameraAsset.CreateInstance<SpatialSkeletonAsset>();
            skeletonAsset.captureType = SpatialCameraAsset.CaptureType.Skeleton;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader sr = new BinaryReader(fs))
                    {
                        ReadHeader(sr, skeletonAsset);
                        ReadSkeletonDefinition(sr, skeletonAsset);
                        skeletonAsset.frameData = sr.ReadBytes((int)(fs.Length - fs.Position));
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogErrorFormat("Error parsing {0} with exception: {1}", fileName, e);
            }


            // Save Asset

            string savePath = EditorUtility.SaveFilePanelInProject("Save Asset", "Camera.asset", "asset", "Please enter a file name to save the asset to");

            if (savePath.Length == 0)
                return;

            AssetDatabase.CreateAsset(skeletonAsset, savePath);
            AssetDatabase.SaveAssets();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = skeletonAsset;
        }
    }
}
