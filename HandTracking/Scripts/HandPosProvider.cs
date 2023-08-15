using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static mj.gist.tracking.hands.PalmDetector;

namespace mj.gist.tracking.hands {
    public class HandPosProvider : SingletonMonoBehaviour<HandPosProvider> {
        [SerializeField] ResourceSet _resources = null;
        protected ImageSource source;

        PalmDetector _detector;

        GraphicsBuffer _boxDrawArgs;
        GraphicsBuffer _keyDrawArgs;

        public IEnumerable<Detection> Detections => _detector.Detections;
        public GraphicsBuffer DetectionBuffer => _detector.DetectionBuffer;
        public GraphicsBuffer BoxDrawArgs => _boxDrawArgs;
        public GraphicsBuffer KeyDrawArgs => _keyDrawArgs;

        void Start() {
            source = GetComponent<ImageSource>();
            _detector = new PalmDetector(_resources);

            _boxDrawArgs = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 4, sizeof(uint));
            _keyDrawArgs = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 4, sizeof(uint));
            _boxDrawArgs.SetData(new[] { 6, 0, 0, 0 });
            _keyDrawArgs.SetData(new[] { 24, 0, 0, 0 });
        }

        void OnDestroy() {
            _detector.Dispose();
            _boxDrawArgs.Dispose();
            _keyDrawArgs.Dispose();
        }

        void LateUpdate() {
            _detector.ProcessImage(source.Texture);
            _detector.SetIndirectDrawCount(_boxDrawArgs);
            _detector.SetIndirectDrawCount(_keyDrawArgs);
        }
    }
}