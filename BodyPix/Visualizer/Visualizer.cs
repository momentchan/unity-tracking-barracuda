using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;

namespace mj.gist.tracking.bodyPix {
    public class Visualizer : MonoBehaviour {
        [SerializeField] private RawImage rawUI = null;
        [SerializeField] private RawImage maskUI = null;
        [SerializeField] private Shader shader;
        [SerializeField] private KeyCode debugKey = KeyCode.D;

        private bool drawDebug = false;

        private Material material;
        private RenderTexture mask;
        private BodyPoseProvider provider;
        private ImageSource source = null;

        void Start() {
            provider = GetComponent<BodyPoseProvider>();
            source = GetComponent<ImageSource>();

            var reso = source.OutputResolution;
            mask = new RenderTexture(reso.x, reso.y, 0);

            material = new Material(shader);
        }

        private void LateUpdate() {
            if (Input.GetKeyDown(debugKey)) {
                drawDebug = !drawDebug;
                rawUI.enabled = drawDebug;
                maskUI.enabled = drawDebug;
            }

            rawUI.texture = source.Texture;
            maskUI.texture = mask;
            Graphics.Blit(provider.MaskTexture, mask, material, 0);
        }

        protected void OnCameraRender(ScriptableRenderContext context, Camera[] cameras) {
            if (!drawDebug) return;

            material.SetBuffer("_Keypoints", provider.KeypointBuffer);

            var ratio = new Vector2(rawUI.rectTransform.sizeDelta.x / Screen.width, rawUI.rectTransform.sizeDelta.y / Screen.height);
            material.SetVector("_CanvasRatio", ratio);
            material.SetVector("_CanvasOffset", new Vector2(1 - ratio.x, 1 - ratio.y));

            material.SetPass(1);
            Graphics.DrawProceduralNow(MeshTopology.Triangles, 6, Body.KeypointCount);

            material.SetPass(2);
            Graphics.DrawProceduralNow(MeshTopology.Lines, 2, 12);
        }

        private void OnEnable() {
            if (GraphicsSettings.renderPipelineAsset != null)
                RenderPipelineManager.endFrameRendering += OnCameraRender;
        }

        private void OnDisable() {
            if (GraphicsSettings.renderPipelineAsset != null)
                RenderPipelineManager.endFrameRendering -= OnCameraRender;
        }

        private void OnDestroy() {
            Destroy(material);
            Destroy(mask);
        }
    }
}