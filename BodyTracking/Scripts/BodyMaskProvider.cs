using UnityEngine;

namespace mj.gist.tracking.body
{
    public class BodyMaskProvider : MonoBehaviour
    {
        public Texture MaskTexture => output;
        public Texture SourceTexture => source.Texture;
        public GraphicsBuffer Keypoints => filter.KeyPoints;
        public Vector3 GetKeypoint(int id) => filter.GetKeyPoint(id);

        [SerializeField] protected ResourceSet resource;
        [SerializeField] protected Shader shader;
        [SerializeField] private RenderTexture output;

        protected SegementationFilter filter;
        protected ImageSource source;
        private Material material;

        protected virtual void Start()
        {
            source = GetComponent<ImageSource>();
            filter = new SegementationFilter(resource, 512, 384);
            material = new Material(shader);
        }

        protected virtual void Update()
        {
            filter.ProcessImage(source.Texture);

            Graphics.SetRenderTarget(output);
            material.SetTexture("_BodyPixTexture", filter.MaskTexture);
            material.SetPass(0);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 3, 1);
        }

        protected virtual void OnDestroy()
        {
            filter?.Dispose();
            Destroy(material);
        }
    }
}