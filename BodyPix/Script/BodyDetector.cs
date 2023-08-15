using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using mj.gist;

namespace mj.gist.tracking.bodyPix {

    public class BodyDetector : System.IDisposable {
        #region Public methods/properties

        public BodyDetector(ResourceSet resources, int width, int height)
          => AllocateObjects(resources, width, height);

        public void Dispose()
          => DeallocateObjects();

        public void ProcessImage(Texture sourceTexture)
          => RunModel(sourceTexture);

        public IEnumerable<Keypoint> Keypoints
          => _readCache.Cached;

        public RenderTexture MaskTexture
          => _buffers.mask;

        public GraphicsBuffer KeypointBuffer
          => _buffers.keypoints;

        #endregion

        #region Private objects

        ResourceSet _resources;
        Config _config;
        IWorker _worker;

        (ComputeBuffer preprocess,
         RenderTexture segment,
         RenderTexture parts,
         RenderTexture heatmaps,
         RenderTexture offsets,
         RenderTexture mask,
         GraphicsBuffer keypoints) _buffers;

        KeypointCache _readCache;

        void AllocateObjects(ResourceSet resources, int width, int height) {
            // NN model loading
            var model = ModelLoader.Load(resources.model);

            // Private object initialization
            _resources = resources;
            _config = new Config(model, _resources, width, height);
            _worker = model.CreateWorker();

            // Buffer allocation
            _buffers.preprocess = new ComputeBuffer
              (_config.InputFootprint, sizeof(float));

            _buffers.segment = RTUtil.NewFloat
              (_config.OutputWidth, _config.OutputHeight);

            _buffers.parts = RTUtil.NewFloat
              (_config.OutputWidth * 24, _config.OutputHeight);

            _buffers.heatmaps = RTUtil.NewFloat
              (_config.OutputWidth * Body.KeypointCount, _config.OutputHeight);

            _buffers.offsets = RTUtil.NewFloat
              (_config.OutputWidth * Body.KeypointCount * 2, _config.OutputHeight);

            _buffers.mask = RTUtil.NewArgbUav
              (_config.OutputWidth, _config.OutputHeight);

            _buffers.keypoints = new GraphicsBuffer
              (GraphicsBuffer.Target.Structured,
               Body.KeypointCount, sizeof(float) * 4);

            // Keypoint data read cache initialization
            _readCache = new KeypointCache(_buffers.keypoints);
        }

        void DeallocateObjects() {
            _worker?.Dispose();
            _worker = null;

            _buffers.preprocess?.Dispose();
            _buffers.preprocess = null;

            ObjectUtil.Destroy(_buffers.segment);
            _buffers.segment = null;

            ObjectUtil.Destroy(_buffers.parts);
            _buffers.parts = null;

            ObjectUtil.Destroy(_buffers.heatmaps);
            _buffers.heatmaps = null;

            ObjectUtil.Destroy(_buffers.offsets);
            _buffers.offsets = null;

            ObjectUtil.Destroy(_buffers.mask);
            _buffers.mask = null;

            _buffers.keypoints?.Dispose();
            _buffers.keypoints = null;
        }

        #endregion

        #region Main inference function

        void RunModel(Texture source) {
            // Preprocessing
            var pre = _resources.preprocess;
            pre.SetTexture(0, "Input", source);
            pre.SetBuffer(0, "Output", _buffers.preprocess);
            pre.SetInts("InputSize", _config.InputWidth, _config.InputHeight);
            pre.SetVector("ColorCoeffs", _config.PreprocessCoeffs);
            pre.SetBool("InputIsLinear", ColorUtil.IsLinear);
            pre.DispatchThreads(0, _config.InputWidth, _config.InputHeight, 1);

            // NN worker invocation
            using (var t = new Tensor(_config.InputShape, _buffers.preprocess))
                _worker.Execute(t);

            // NN output retrieval
            _worker.CopyOutputToRT("segments", _buffers.segment);
            _worker.CopyOutputToRT("part_heatmaps", _buffers.parts);
            _worker.CopyOutputToRT("heatmaps", _buffers.heatmaps);
            _worker.CopyOutputToRT("short_offsets", _buffers.offsets);

            // Postprocessing (mask)
            var post1 = _resources.mask;
            post1.SetTexture(0, "Segments", _buffers.segment);
            post1.SetTexture(0, "Heatmaps", _buffers.parts);
            post1.SetTexture(0, "Output", _buffers.mask);
            post1.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
            post1.DispatchThreads(0, _config.OutputWidth, _config.OutputHeight, 1);

            // Postprocessing (keypoints)
            var post2 = _resources.keypoints;
            post2.SetTexture(0, "Heatmaps", _buffers.heatmaps);
            post2.SetTexture(0, "Offsets", _buffers.offsets);
            post2.SetInts("InputSize", _config.OutputWidth, _config.OutputHeight);
            post2.SetInt("Stride", _config.Stride);
            post2.SetBuffer(0, "Keypoints", _buffers.keypoints);
            post2.Dispatch(0, 1, 1, 1);

            // Cache data invalidation
            _readCache.Invalidate();
        }

        #endregion
    }

} // namespace BodyPix
