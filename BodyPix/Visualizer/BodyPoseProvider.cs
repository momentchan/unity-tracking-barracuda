using System.Collections.Generic;
using UnityEngine;

namespace mj.gist.tracking.bodyPix {
    public class BodyPoseProvider : SingletonMonoBehaviour<BodyPoseProvider> {
        [SerializeField] private ResourceSet resources = null;
        [SerializeField] private Vector2Int resolution = new Vector2Int(512, 384);

        public GraphicsBuffer KeypointBuffer => detector.KeypointBuffer;
        public RenderTexture MaskTexture => detector.MaskTexture;
        public IEnumerable<Keypoint> Keypoints => detector.Keypoints;

        private ImageSource source = null;
        private BodyDetector detector;

        void Start() {
            source = GetComponent<ImageSource>();
            detector = new BodyDetector(resources, resolution.x, resolution.y);
        }

        void LateUpdate() {
            detector.ProcessImage(source.Texture);
        }

        void OnDestroy() {
            detector.Dispose();
        }
    }
}