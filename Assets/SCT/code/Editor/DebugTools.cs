using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace sct
{
    public class FramePose
    {
        public List<BoneData> m_bones;

        public void init(int boneCount)
        {
            m_bones = new List<BoneData>(boneCount);
            for (int i = 0; i < boneCount; ++i)
            {
                m_bones.Add(new BoneData());
            }
        }
    }

    public static class DebugTools
    {
        [MenuItem("SCT/Visualize Camera")]
        static void VisualizeCamera()
        {
            string[] filters = { "Replay Files", "dat" };
            string fileName = EditorUtility.OpenFilePanelWithFilters(
                "Load Replay",
                ".",
                filters);

            if (fileName.Length == 0)
                return;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader sr = new BinaryReader(fs))
                    {
                        int version = sr.ReadInt32();
                        Debug.LogFormat("Version {0}", version);

                        int frameCount = sr.ReadInt32();
                        Debug.LogFormat("Read {0} frames", frameCount);

                        int deviceOrientation = sr.ReadInt32();
                        Debug.LogFormat("Device Orientation {0}", deviceOrientation);

                        float fovX = sr.ReadSingle();
                        float fovY = sr.ReadSingle();

                        Transform root = new GameObject("root").transform;
                        root.position = Vector3.zero;
                        root.rotation = Quaternion.identity;

                        for (int i = 0; i < Mathf.Max(30, frameCount); ++i)
                        {
                            Vector3 pos = Vector3.zero;
                            Vector3 rot = Vector3.zero;
                            //Quaternion rot = Quaternion.identity;

                            SpatialUtils.readCameraTransform(sr, ref pos, ref rot);
                            //                            Debug.LogFormat("Frame {0}, pos: {1}, {2}, {3}, rot: {4}, {5}, {6}, {7}", i, pos.x, pos.y, pos.z, rot.x, rot.y, rot.z, rot.w);

                            Transform t = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                            t.name = i.ToString();
                            t.localScale = Vector3.one * 0.01f;
                            t.parent = root;
                            t.position = pos;
                            t.rotation = Quaternion.identity;

                            t.Rotate(Vector3.forward, rot.z);
                            t.Rotate(Vector3.up, rot.y);
                            t.Rotate(Vector3.right, rot.x);


                            //                            t.eulerAngles = rot;
                            //                            t.rotation = rot;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Error parsing {0} with exception: {1}", fileName, e);
            }
        }

        [MenuItem("SCT/Visualize Frame")]
        static void VisualizeFrame()
        {
            string[] filters = { "Replay Files", "dat" };
            string fileName = EditorUtility.OpenFilePanelWithFilters(
                "Load Replay",
                ".",
                filters);

            if (fileName.Length == 0)
                return;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader sr = new BinaryReader(fs))
                    {
                        int version = sr.ReadInt32();
                        Debug.LogFormat("Version {0}", version);

                        int frameCount = sr.ReadInt32();
                        Debug.LogFormat("Read {0} frames", frameCount);

                        int deviceOrientation = sr.ReadInt32();
                        Debug.LogFormat("Device Orientation {0}", deviceOrientation);

                        float fovX = sr.ReadSingle();
                        float fovY = sr.ReadSingle();

                        // Read skeleton definition
                        int jointCount = sr.ReadInt32();
                        string[] jointNames = new string[jointCount];
                        for (int i = 0; i < jointCount; ++i)
                        {
                            string name = SpatialUtils.readString(sr);
                            jointNames[i] = name;
                        }

                        int parentCount = sr.ReadInt32();
                        int[] parents = new int[parentCount];
                        for (int i = 0; i < parentCount; ++i)
                        {
                            int parent = sr.ReadInt32();
                            parents[i] = parent;
                        }

                        Transform[] neutralSkeleton = SpatialUtils.createSkeleton(jointNames, parents);
                        neutralSkeleton[0].name = "netural_" + neutralSkeleton[0].name;
                        FramePose neutralPose = new FramePose();
                        neutralPose.init(jointCount);
                        // Read neutral skeleton transforms
                        for (int b = 0; b < jointCount; ++b)
                        {
                            SpatialUtils.readBoneData(sr, neutralPose.m_bones[b], jointNames[b]);
                        }
                        visualizeFramePose(neutralPose, neutralSkeleton);


                        // Create skeleton structure
                        Transform[] skeleton = SpatialUtils.createSkeleton(jointNames, parents);
                        FramePose pose = new FramePose();
                        pose.init(jointCount);

                        // Create Camera Transform
                        Transform cameraTransform = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                        cameraTransform.name = "Camera Transform";
                        cameraTransform.localScale = Vector3.one * 0.01f;
                        cameraTransform.parent = null;

                        // PER FRAME DATA STARTS HERE

                        for (int f = 0; f < frameCount; ++f)
                        {
                            // Update skeleton and camera!
                            uint skeletonCount = sr.ReadUInt32();
                            for (int i = 0; i < skeletonCount; ++i)
                            {
                                for (int b = 0; b < jointCount; ++b)
                                    SpatialUtils.readBoneData(sr, pose.m_bones[b], jointNames[b]);

                                visualizeFramePose(pose, skeleton);
                            }

                            Vector3 pos = Vector3.zero;
                            Vector3 rot = Vector3.zero;
                            SpatialUtils.readCameraTransform(sr, ref pos, ref rot);

                            Transform t = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
                            t.name = f.ToString();
                            t.localScale = Vector3.one * 0.01f;
                            t.parent = cameraTransform;
                            t.position = pos;
                            t.rotation = Quaternion.identity;

                            t.Rotate(Vector3.forward, rot.z);
                            t.Rotate(Vector3.up, rot.y);
                            t.Rotate(Vector3.right, rot.x);
                            /*
                                                        cameraTransform.position = pos;
                                                        cameraTransform.rotation = Quaternion.identity;
                                                        cameraTransform.Rotate(Vector3.forward, rot.z);
                                                        cameraTransform.Rotate(Vector3.up, rot.y);
                                                        cameraTransform.Rotate(Vector3.right, rot.x);
                            */
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Error parsing {0} with exception: {1}", fileName, e);
            }
        }

        [MenuItem("SCT/Visualize Mesh")]
        static void VisualizeMesh()
        {
            string[] filters = { "Replay Files", "dat" };
            string fileName = EditorUtility.OpenFilePanelWithFilters(
                "Load Replay",
                ".",
                filters);

            if (fileName.Length == 0)
                return;

            try
            {
                using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                {
                    using (BinaryReader sr = new BinaryReader(fs))
                    {
                        Transform root = new GameObject("mesh").transform;

                        int partCount = sr.ReadInt32();
                        Debug.LogFormat("Mesh contains {0} parts", partCount);

                        for (int i = 0; i < partCount; ++i)
                        {
                            long vertCount = sr.ReadInt64();
                            Debug.LogFormat("Part has {0} vertices", vertCount);

                            Vector3[] vertices = new Vector3[vertCount];
                            for (int v = 0; v < vertCount; ++v)
                            {
                                Vector3 vert = Vector3.zero;
                                vert.x = sr.ReadSingle();
                                vert.y = sr.ReadSingle();
                                vert.z = sr.ReadSingle();

                                vertices[v] = vert;
                                //Debug.LogFormat("Vert_{0} x:{1}, y:{2}, z:{3}", v, vert.x, vert.y, vert.z);
                            }

                            long indicesCount = sr.ReadInt64();
                            Debug.LogFormat("Part has {0} indices", indicesCount);
                            int[] indices = new int[indicesCount];
                            for (int t = 0; t < indicesCount; ++t)
                            {
                                indices[t] = (int)sr.ReadUInt32();
                                //Debug.LogFormat("Index: {0}", indices[t]);
                            }

                            Mesh mesh = new Mesh();
                            mesh.vertices = vertices;
                            mesh.triangles = indices;
                            Transform part = new GameObject(i.ToString()).transform;
                            part.gameObject.AddComponent<MeshFilter>().mesh = mesh;
                            part.parent = root;
                            part.localPosition = Vector3.zero;

                            part.gameObject.AddComponent<MeshRenderer>();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogWarningFormat("Error parsing {0} with exception: {1}", fileName, e);
            }
        }

        static void visualizeFramePose(FramePose pose, Transform[] skeleton)
        {
            // Set pos/rot
            Transform root = skeleton[0];
            for (int i = 0; i < skeleton.Length; ++i)
            {
                skeleton[i].position = root.TransformPoint(pose.m_bones[i].pos);
                skeleton[i].rotation = root.rotation * pose.m_bones[i].rot;
            }
        }
    }
}
