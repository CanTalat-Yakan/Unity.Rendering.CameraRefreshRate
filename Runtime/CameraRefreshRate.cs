using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace UnityEssentials
{
    [Serializable]
    public class RefreshRateSettings
    {
        public int RefreshRate = 120;
        public bool SendRenderRequest = false;
    }

    [RequireComponent(typeof(Camera))]
    public class CameraRefreshRate : MonoBehaviour
    {
        public RefreshRateSettings Settings;

        private Camera _camera;
        private double _nextRenderTime;

        public void Awake() =>
            _camera = GetComponent<Camera>();

        public void OnEnable()
        {
            GlobalRefreshRate.OnTick += TryRender;

            _camera.enabled = false;
            _nextRenderTime = Time.timeAsDouble;
        }

        public void OnDisable()
        {
            GlobalRefreshRate.OnTick -= TryRender;

            if (_camera != null)
                _camera.enabled = true;
        }

        public void SetTarget(int refreshRate) =>
            Settings.RefreshRate = refreshRate;

        public void TryRender()
        {
            if (Settings.RefreshRate <= 0)
            {
                if (Settings.SendRenderRequest)
                    SendRenderRequest();
                else _camera.enabled = true;

                return;
            }

            _camera.enabled = false;
            double currentTime = Time.timeAsDouble;
            if (currentTime >= _nextRenderTime && Settings.RefreshRate > 0)
            {
                if (Settings.SendRenderRequest) 
                    SendRenderRequest();
                else _camera.enabled = true;

                _nextRenderTime = currentTime + (1.0 / Settings.RefreshRate);
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