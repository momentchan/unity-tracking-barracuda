using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace mj.gist.tracking.hands {
    public class HandMaskGenerator : MonoBehaviour {
        [SerializeField] private RenderTexture maskTex;
        [SerializeField] private Shader shader;

        private Material mat;

        void Start() {
            maskTex = RTUtil.NewFloat4(256, 256);
            mat = new Material(shader);
        }

        void Update() {
            mat.SetBuffer("_Detections", HandPosProvider.Instance.DetectionBuffer);
        }
    }
}