using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace sct
{
    public class SpatialCameraPlayer : MonoBehaviour
    {
        public SpatialCameraAsset replayData;
        public bool loop = true;
        public Camera replayCamera;

        private BinaryReader sr;
        private int currFrame;

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
        }

        void Update()
        {
            Vector3 pos = Vector3.zero;
            Vector3 rot = Vector3.zero;

            SpatialUtils.readCameraTransform(sr, ref pos, ref rot);
            SpatialUtils.applyCameraTransform(transform, pos, rot);

            if (++currFrame >= replayData.frameCount)
            {
                currFrame = 0;
                sr.BaseStream.Seek(0, System.IO.SeekOrigin.Begin); 
                enabled = loop;
            }
        }
    }
}
