/*
MIT License

Copyright (c) 2020 Kodholmen

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace sct
{
    public class SpatialSkeletonPlayer : MonoBehaviour
    {
        public SpatialSkeletonAsset replayData;
        public bool loop = true;
        public Camera replayCamera;
        public bool showDebugSkeleton = false;

        private BinaryReader sr;
        private int currFrame;
        private BoneData[] skeleton;

        private Transform[] debugSkeleton;
        void Start()
        {
            if (replayData == null)
            {
                enabled = false;
                return;
            }

            Application.targetFrameRate = 60;

            sr = new BinaryReader(new MemoryStream(replayData.frameData));
            currFrame = 0;

            if (replayCamera != null)
                replayCamera.fieldOfView = replayData.verticalFOV;

            skeleton = new BoneData[replayData.refSkeleton.Length];
            for (int i = 0; i < skeleton.Length; ++i)
                skeleton[i] = new BoneData();

            if (showDebugSkeleton)
            {
                debugSkeleton = SpatialUtils.createSkeleton(replayData.jointNames, replayData.parents);                
            }
        }

        void readSkeleton()
        {
            int jointCount = replayData.refSkeleton.Length;
            uint skeletonCount = sr.ReadUInt32();
            for (int i = 0; i < skeletonCount; ++i)
            {
                for (int b = 0; b < jointCount; ++b)
                    SpatialUtils.readBoneData(sr, skeleton[b], replayData.jointNames[b]);

                if (showDebugSkeleton)
                {
                    Transform root = transform;
                    for (int b = 0; b < skeleton.Length; ++b)
                    {
                        debugSkeleton[b].position = root.TransformPoint(skeleton[0].pos + skeleton[b].pos);
                        debugSkeleton[b].rotation = root.rotation * skeleton[b].rot;
                    }
                }
            }
        }

        void readCamera()
        {
            Vector3 pos = Vector3.zero;
            Vector3 rot = Vector3.zero;

            SpatialUtils.readCameraTransform(sr, ref pos, ref rot);
            SpatialUtils.applyCameraTransform(replayCamera.transform, pos, rot);
        }

        void Update()
        {
            readSkeleton();
            readCamera();

            if (++currFrame >= replayData.frameCount)
            {
                currFrame = 0;
                sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin);
                enabled = loop;
            }
        }
    }
}
