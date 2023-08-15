using System.Runtime.InteropServices;
using UnityEngine;

namespace mj.gist.tracking.bodyPix {
    public static class Body {
        public const int PartCount = 24;
        public const int KeypointCount = 17;

        public enum KeypointID {
            Nose,
            LeftEye, RightEye,
            LeftEar, RightEar,
            LeftShoulder, RightShoulder,
            LeftElbow, RightElbow,
            LeftWrist, RightWrist,
            LeftHip, RightHip,
            LeftKnee, RightKnee,
            LeftAnkle, RightAnkle
        }

        public enum PartID {
            LeftFace, RightFace,
            LeftUpperArmFront, LeftUpperArmBack,
            RightUpperArmFront, RightUpperArmBack,
            LeftLowerArmFront, LeftLowerArmBack,
            RightLowerArmFront, RightLowerArmBack,
            LeftHand, RightHand,
            TorsoFront, TorsoBack,
            LeftUpperLegFront, LeftUpperLegBack,
            RightUpperLegFront, RightUpperLegBack,
            LeftLowerLegFront, LeftLowerLegBack,
            RightLowerLegFront, RightLowerLegBack,
            LeftFeet, RightFeet
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Keypoint {
        public Vector2 Position;
        public float Score;
        public uint Padding;
    }

    sealed class KeypointCache {
        public KeypointCache(GraphicsBuffer source) => _source = source;
        public Keypoint[] Cached => Read();
        public void Invalidate() => _isCached = false;

        GraphicsBuffer _source;
        Keypoint[] _array = new Keypoint[Body.KeypointCount];
        bool _isCached;

        Keypoint[] Read() {
            if (_isCached) return _array;
            _source.GetData(_array, 0, 0, Body.KeypointCount);
            _isCached = true;
            return _array;
        }
    }

} // namespace BodyPix
