using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace sct
{
    [System.Serializable]
    public class BoneData
    {
        public Vector3 pos = Vector3.zero;
        public Quaternion rot = Quaternion.identity;
    }

    public class SpatialSkeletonAsset : SpatialCameraAsset
    {
        public string[] jointNames;
        public int[] parents;
        public BoneData[] refSkeleton;
    }
}
