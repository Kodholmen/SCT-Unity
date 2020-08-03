using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;


namespace sct
{
    public class SpatialCameraAsset : ScriptableObject
    {
        public enum CaptureType
        {
            Skeleton,
            Camera
        }

        public CaptureType captureType;
        public byte[] frameData;
        public int version;
        public int frameCount;
        public int deviceOrientation;
        public float horizontalFOV;
        public float verticalFOV;
        public float focalLengthX;
        public float focalLengthY;
    }
}

