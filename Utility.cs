using Unity.Barracuda;
using UnityEngine;

namespace mj.gist.tracking {

    static class ObjectUtil {
        public static void Destroy(Object o) {
            if (o == null) return;
            if (Application.isPlaying)
                Object.Destroy(o);
            else
                Object.DestroyImmediate(o);
        }
    }

    static class ColorUtil {
        public static bool IsLinear
          => QualitySettings.activeColorSpace == ColorSpace.Linear;
    }

    static class IWorkerExtensions {
        //
        // Retrieves an output tensor from a NN worker and returns it as a
        // temporary render texture. The caller must release it using
        // RenderTexture.ReleaseTemporary.
        //
        public static RenderTexture
          CopyOutputToTempRT(this IWorker worker, string name, int w, int h, RenderTextureFormat format = RenderTextureFormat.RFloat) {
            var shape = new TensorShape(1, h, w, 1);
            var rt = RenderTexture.GetTemporary(w, h, 0, format);
            using (var tensor = worker.PeekOutput(name).Reshape(shape))
                tensor.ToRenderTexture(rt);
            return rt;
        }

        public static void CopyOutputToRT(this IWorker worker, string name, RenderTexture rt) {
            var shape = new TensorShape(1, rt.height, rt.width, 1);
            using (var tensor = worker.PeekOutput(name).Reshape(shape))
                tensor.ToRenderTexture(rt);
        }

        public static ComputeBuffer CopyOutputToBuffer(this IWorker worker, string name, int length) {
            var shape = new TensorShape(length);
            var tensor = worker.PeekOutput(name).Reshape(shape);
            var buffer = ((ComputeTensorData)tensor.data).buffer;
            tensor.Dispose();
            return buffer;
        }
    }
} // namespace Mlsd
