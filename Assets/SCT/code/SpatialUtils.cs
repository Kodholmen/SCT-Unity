using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace sct
{
    public static class SpatialUtils
    {
        public static void readCameraTransform(BinaryReader sr, ref Vector3 pos, ref Vector3 rot)
        {
            pos.x = sr.ReadSingle();
            pos.y = sr.ReadSingle();
            pos.z = sr.ReadSingle() * -1.0f;

            rot.x = sr.ReadSingle();
            rot.y = sr.ReadSingle();
            rot.z = sr.ReadSingle();

            rot.x = Mathf.Rad2Deg * (-rot.x);
            rot.y = Mathf.Rad2Deg * (-rot.y);
            rot.z = Mathf.Rad2Deg * (rot.z);
        }

        public static void applyCameraTransform(Transform transform, Vector3 pos, Vector3 rot)
        {
            transform.localPosition = pos;
            transform.localRotation = Quaternion.identity;

            transform.Rotate(Vector3.forward, rot.z);
            transform.Rotate(Vector3.up, rot.y);
            transform.Rotate(Vector3.right, rot.x);
        }

        public static Matrix4x4 readMatrix(BinaryReader sr)
        {
            Vector4 c0;
            c0.x = sr.ReadSingle();
            c0.y = sr.ReadSingle();
            c0.z = sr.ReadSingle();
            c0.w = sr.ReadSingle();
            Vector4 c1;
            c1.x = sr.ReadSingle();
            c1.y = sr.ReadSingle();
            c1.z = sr.ReadSingle();
            c1.w = sr.ReadSingle();
            Vector4 c2;
            c2.x = sr.ReadSingle();
            c2.y = sr.ReadSingle();
            c2.z = sr.ReadSingle();
            c2.w = sr.ReadSingle();
            Vector4 c3;
            c3.x = sr.ReadSingle();
            c3.y = sr.ReadSingle();
            c3.z = sr.ReadSingle();
            c3.w = sr.ReadSingle();

            Matrix4x4 matrix = Matrix4x4.identity;
            matrix.SetColumn(0, c0);
            matrix.SetColumn(1, c1);
            matrix.SetColumn(2, c2);
            matrix.SetColumn(3, c3);

            return matrix;
        }

        public static string readString(BinaryReader sr)
        {
            int len = sr.ReadInt32();
            if (len == 0) return "";

            byte[] str = new byte[len];
            str = sr.ReadBytes((int)len);

            return System.Text.Encoding.Default.GetString(str);
        }

        public static void readBoneData(BinaryReader sr, BoneData bone, string jointName)
        {
            //            Debug.LogFormat("Reading joint {0}", jointName);

            Matrix4x4 matrix = readMatrix(sr);

            bone.pos.x = matrix[0, 3];
            bone.pos.y = matrix[1, 3];
            bone.pos.z = -matrix[2, 3];

            bone.rot = Quaternion.identity;//matrix.rotation;
        }

        public static Transform[] createSkeleton(string[] jointNames, int[] parents)
        {
            // Create skeleton
            Transform[] skeleton = new Transform[parents.Length];
            for (int i = 0; i < parents.Length; ++i)
            {
                skeleton[i] = createGameObjectForBone(jointNames[i]);
            }

            // Setup parent structure
            for (int i = 0; i < parents.Length; ++i)
            {
                int parent = parents[i];
                if (parent == -1)
                    continue;

                skeleton[i].parent = skeleton[parent];
            }

            return skeleton;
        }

        static Transform createGameObjectForBone(string name)
        {
            Transform bone = new GameObject().transform;
            bone.name = name;

            Transform ball = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
            ball.name = name + " visualizer";
            ball.parent = bone;
            ball.localScale = new Vector3(0.05f, 0.05f, 0.05f);

            return bone;
        }

    }
}
