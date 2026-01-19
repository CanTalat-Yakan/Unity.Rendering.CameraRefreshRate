using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEssentials
{
    [Serializable]
    public class FrameRateLimiterSettings
    {
        public int FrameRate = 120;
        public bool SendRenderRequest = false;
    }

    [RequireComponent(typeof(Camera))]
    public class CameraFrameRate : MonoBehaviour
    {
        public FrameRateLimiterSettings Settings;

        private Camera _camera;
        private double _nextRenderTime;

        public void Awake() =>
            _camera = GetComponent<Camera>();

        public void OnEnable()
        {
            GlobalRefreshRateLimiter.OnFrameLimiterTick += TryRender;

            _camera.enabled = false;
            _nextRenderTime = Time.timeAsDouble;
        }

        public void OnDisable()
        {
            GlobalRefreshRateLimiter.OnFrameLimiterTick -= TryRender;

            if (_camera != null)
                _camera.enabled = true;
        }

        public void SetTargetFrameRate(int frameRate) =>
            Settings.FrameRate = frameRate;

        public void TryRender()
        {
            if (Settings.FrameRate <= 0)
            {
                if (Settings.SendRenderRequest)
                    SendRenderRequest();
                else _camera.enabled = true;

                return;
            }

            _camera.enabled = false;
            double currentTime = Time.timeAsDouble;
            if (currentTime >= _nextRenderTime && Settings.FrameRate > 0)
            {
                if (Settings.SendRenderRequest) 
                    SendRenderRequest();
                else _camera.enabled = true;

                _nextRenderTime = currentTime + (1.0 / Settings.FrameRate);
            }
        }

        private void SendRenderRequest()
        {
            var request = new RenderPipeline.StandardRequest();

            if (RenderPipeline.SupportsRenderRequest(_camera, request))
            {
                request.destination = _camera.targetTexture;
                RenderPipeline.SubmitRenderRequest(_camera, request);
            }
        }
    }
}