using Unity.Barracuda;
using UnityEngine;

namespace mj.gist.tracking.body
{
    public class SegementationFilter : System.IDisposable
    {
        public Texture MaskTexture => buffers.mask;
        public GraphicsBuffer KeyPoints => buffers.keypoints;
        public Vector3 GetKeyPoint(int id)
           => new Vector3(KeyPointCache[4 * id],
                          KeyPointCache[4 * id + 1],
                          KeyPointCache[4 * id + 2]);

        private int KeyPointCount = 17;
        private float[] KeyPointCache = new float[17 * 4];

        public SegementationFilter(ResourceSet resource, int w = 1920, int h = 1080)
        {
            this.resource = resource;

            config = new Config(resource, w, h);
            worker = ModelLoader.Load(resource.model).CreateWorker();
            buffers.preprocess = new ComputeBuffer(config.InputFootPrint, sizeof(float));
            buffers.segment = RTUtil.NewFloat(config.OutputWidth, config.OutputHeight);
            buffers.parts = RTUtil.NewFloat(config.OutputWidth * 24, config.OutputHeight);
            buffers.heatmaps = RTUtil.NewFloat(config.OutputWidth * KeyPointCount, config.OutputHeight);
            buffers.offsets = RTUtil.NewFloat(config.OutputWidth * KeyPointCount * 2, config.OutputHeight);
            buffers.mask = RTUtil.NewUAV(config.OutputWidth, config.OutputHeight);
            buffers.keypoints = new GraphicsBuffer(GraphicsBuffer.Target.Structured, KeyPointCount, sizeof(float) * 4);
        }

        private IWorker worker;
        private ResourceSet resource;
        private Config config;

        (ComputeBuffer preprocess,
         RenderTexture segment,
         RenderTexture parts,
         RenderTexture heatmaps,
         RenderTexture offsets,
         RenderTexture mask,
         GraphicsBuffer keypoints) buffers;

        public void ProcessImage(Texture sourceTex)
        {

            // Preprocess
            var pre = resource.preprocess;
            pre.SetInts("_InputSize", config.InputWidth, config.InputHeight);
            pre.SetTexture(0, "_Input", sourceTex);
            pre.SetBuffer(0, "_Output", buffers.preprocess);
            pre.DispatchThreads(0, config.InputWidth, config.InputHeight, 1);

            // NN worker invocation
            using (var tensor = new Tensor(config.InputShape, buffers.preprocess))
                worker.Execute(tensor);

            // NN output retrieval
            worker.CopyOutputToRT("segments", buffers.segment);
            worker.CopyOutputToRT("part_heatmaps", buffers.parts);
            worker.CopyOutputToRT("heatmaps", buffers.heatmaps);
            worker.CopyOutputToRT("short_offsets", buffers.offsets);

            var post1 = resource.mask;
            post1.SetTexture(0, "_Segments", buffers.segment);
            post1.SetTexture(0, "_Heatmaps", buffers.parts);
            post1.SetTexture(0, "_Output", buffers.mask);
            post1.SetInts("_InputSize", config.OutputWidth, config.OutputHeight);
            post1.DispatchThreads(0, config.OutputWidth, config.OutputHeight, 1);

            var post2 = resource.keypoints;
            post2.SetTexture(0, "_Heatmaps", buffers.heatmaps);
            post2.SetTexture(0, "_Offsets", buffers.offsets);
            post2.SetInts("_InputSize", config.OutputWidth, config.OutputHeight);
            post2.SetInt("_Stride", config.Stride);
            post2.SetBuffer(0, "_Keypoints", buffers.keypoints);
            post2.Dispatch(0, 1, 1, 1);

            buffers.keypoints.GetData(KeyPointCache);
            //Debug.Log($"{KeyPointCache[0]} {KeyPointCache[1]} {KeyPointCache[2]}");
        }

        public void Dispose()
        {
            worker?.Dispose();
            worker = null;

            buffers.preprocess?.Dispose();
            buffers.preprocess = null;

            ObjectUtil.Destroy(buffers.segment);
            buffers.segment = null;

            ObjectUtil.Destroy(buffers.parts);
            buffers.parts = null;

            ObjectUtil.Destroy(buffers.heatmaps);
            buffers.heatmaps = null;

            ObjectUtil.Destroy(buffers.segment);
            buffers.segment = null;

            ObjectUtil.Destroy(buffers.offsets);
            buffers.offsets = null;

            ObjectUtil.Destroy(buffers.mask);
            buffers.mask = null;

            buffers.keypoints?.Dispose();
            buffers.keypoints = null;
        }
    }
}