# Unity Essentials

This module is part of the Unity Essentials ecosystem and follows the same lightweight, editor-first approach.
Unity Essentials is a lightweight, modular set of editor utilities and helpers that streamline Unity development. It focuses on clean, dependency-free tools that work well together.

All utilities are under the `UnityEssentials` namespace.

```csharp
using UnityEssentials;
```

## Installation

Install the Unity Essentials entry package via Unity's Package Manager, then install modules from the Tools menu.

- Add the entry package (via Git URL)
    - Window → Package Manager
    - "+" → "Add package from git URL…"
    - Paste: `https://github.com/CanTalat-Yakan/UnityEssentials.git`

- Install or update Unity Essentials packages
    - Tools → Install & Update UnityEssentials
    - Install all or select individual modules; run again anytime to update

---

# Camera Frame Rate Limiter

> Quick overview: Per‑camera rendering frequency can be limited to a target FPS, using either normal camera rendering or SRP render requests, synchronized by a global frame‑limiter tick.

Per‑camera frame rate limiting is applied by gating when a camera renders. A global tick is listened to and, at scheduled times, either the camera is temporarily enabled to render a frame or a Scriptable Render Pipeline (SRP) render request is submitted. Between scheduled renders, the camera remains disabled. The approach helps reduce rendering load for secondary or expensive cameras.

![screenshot](Documentation/Screenshot.png)

## Features
- Target FPS per camera
  - An integer FPS target controls how often the camera is allowed to render
  - `SetTargetFrameRate(int)` can be called at runtime
- Two render paths
  - Normal path: the camera is enabled only on scheduled frames (disabled otherwise)
  - SRP path: a `RenderPipeline.StandardRequest` is submitted instead of toggling `enabled`
- Global synchronization
  - Rendering is attempted on a shared tick from a global limiter to keep scheduling consistent across cameras
- “Unlimited” mode
  - When target FPS is ≤ 0, the limiter is effectively bypassed (normal rendering or continuous requests)
- Lightweight and flexible
  - No allocations in the hot path and minimal state: next render time tracked per camera

## Requirements
- Unity 6000.0+ (per package manifest)
- A Camera component on the same GameObject (added by `[RequireComponent]`)
- Global frame limiter module present and active
  - This component subscribes to `GlobalRefreshRateLimiter.OnFrameLimiterTick` provided by the `Unity.Rendering.GlobalRefreshrateLimiter` package
  - Optionally, a global setting component (e.g., `SettingsGlobalFrameRateLimit`) can set the global target FPS
- For SRP render requests
  - A Scriptable Render Pipeline that supports `RenderPipeline.SubmitRenderRequest`
  - A valid `targetTexture` when using the request path is recommended (destination is set to the camera’s `targetTexture`)

## Usage
1) Add to a camera
   - Select the Camera GameObject and add `CameraFrameRateLimiter`
2) Configure target FPS
   - Set `Settings.FrameRate` to the desired limit (e.g., 30)
   - Set to `0` or less to bypass limiting for that camera
3) Choose render path
   - Normal rendering: leave `SendRenderRequest` unchecked (camera is enabled only at scheduled frames)
   - SRP request: enable `SendRenderRequest` (a render request is submitted on scheduled frames)
   - For SRP request mode, assign a `RenderTexture` to `Camera.targetTexture` if off‑screen rendering is desired
4) Global limiter
   - Ensure the `Unity.Rendering.GlobalRefreshrateLimiter` module is present so the global tick is raised
   - Optionally call `GlobalRefreshRateLimiter.SetTargetFrameRate(...)` elsewhere to define a global cadence

## How It Works
- Subscription
  - On enable, the component subscribes to a global tick (`GlobalRefreshRateLimiter.OnFrameLimiterTick`) and disables the camera
- Scheduling
  - A per‑camera `_nextRenderTime` is computed; when the current time exceeds it, a render is triggered and the next slot is scheduled as `now + 1/FrameRate`
- Render trigger
  - Normal path: the camera is briefly enabled so Unity renders it in that frame
  - SRP path: a `RenderPipeline.StandardRequest` is submitted if supported by the active SRP
- Unlimited
  - When `FrameRate <= 0`, the limiter defers to normal rendering (or continuously issues requests if SRP mode is selected)
- Cleanup
  - On disable, the component unsubscribes and re‑enables the camera for normal behavior

## Notes and Limitations
- Camera.enabled side‑effects: scripts that depend on `camera.enabled` may observe it toggling; use SRP request mode to avoid that
- SRP support: render requests require SRP support; when unsupported, the request is skipped
- Output surface: in SRP request mode, ensure a destination (e.g., `targetTexture`) is set to capture output
- Global dependency: without the global limiter module, no tick will be received and no scheduling can occur
- Time basis: scheduling uses `Time.timeAsDouble`; large time scale changes affect cadence accordingly

## Files in This Package
- `Runtime/CameraFrameRateLimiter.cs` – Per‑camera frame rate limiter (normal render vs SRP request)
- `Runtime/UnityEssentials.CameraFrameRateLimiter.asmdef` – Runtime assembly definition

## Tags
unity, camera, framerate, fps, limiter, render, performance, srp, render-request, offscreen
