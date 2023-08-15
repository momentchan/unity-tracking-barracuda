using Unity.Barracuda;
using UnityEngine;

namespace mj.gist.tracking.body {

    [CreateAssetMenu(fileName = "NNCam", menuName = "ScriptableObjects/NNCam Resource")]
    public sealed class ResourceSet : ScriptableObject {
        public NNModel model;
        public int stride;
        public ComputeShader preprocess;
        public ComputeShader mask;
        public ComputeShader keypoints;
    }
}