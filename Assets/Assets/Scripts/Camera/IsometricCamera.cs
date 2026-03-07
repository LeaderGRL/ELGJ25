using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Crossatro.Board
{
    /// <summary>
    /// Isometric camera with auto framing at startup, drag pan and scroll zoom.
    /// </summary>
    public class IsometricCamera: MonoBehaviour
    {
        // ============================================================
        // Configuration
        // ============================================================

        [Header("Camera reference")]
        [Tooltip("The camera has to be Orthographic")]
        [SerializeField] private Camera _camera;

        [Header("Isometric Angle")]
        [SerializeField] private float _pitchAngle = 45f;
        [SerializeField] private float _yawAngle = 45f;

        [SerializeField] private float _cameraDistance = 30f;

        [Header("Zoom")]
        [Tooltip("Minimum orthographic zoom")]
        [SerializeField] private float _minZoom = 3f;

        [Tooltip("Maximum orthographic zoom")]
        [SerializeField] private float _maxZoom = 20f;

        [Tooltip("Zoom speed per scoll step")]
        [SerializeField] private float _zoomSpeed = 1.5f;

        [Tooltip("Zoom smoothing")]
        [SerializeField] private float _zoomSmoothing = 8f;

        [Header("Pan")]
        [Tooltip("Pan speed multiplier")]
        [SerializeField] private float _panSpeed = 0.02f;

        [Tooltip("Pan smoothing")]
        [SerializeField] private float _panSmoothing = 10f;

        [Header("Framing")]
        [Tooltip("Extra padding around the grid when auto framing")]
        [SerializeField] private float _framingPadding = 2f;

        // ============================================================
        // Input
        // ============================================================

        [Header("Input Action")]
        [Tooltip("Button that enables panning")]
        [SerializeField] private InputActionReference _panButtonAction;

        [Tooltip("Pointer delta while panning")]
        [SerializeField] private InputActionReference _panDeltaAction;

        [Tooltip("Scroll input for zoom")]
        [SerializeField] private InputActionReference _zoomAction;

        // ============================================================
        // State
        // ============================================================

        // Target value for smooth interpolation
        private Vector3 _targetPosition;
        private float _targetZoom;

        // Pan state
        private bool _isPanning;
        private Vector2 _lastMousePosition;

        // Grid bounds for clamping
        private Bounds _gridBounds;
        private bool _hasBounds;

        // ============================================================
        // Lifecycle
        // ============================================================

        private void Awake()
        {
            if (_camera == null) 
                _camera = GetComponentInChildren<Camera>();

            if (_camera == null)
            {
                Debug.LogError("[IsometricCamera] No camera found!");
                return;
            }

            ApplyIsometricAngle();

            _targetPosition = transform.position;
            _targetZoom = _camera.orthographicSize;
        }

        private void OnEnable()
        {
            EnableAction(_panButtonAction);
            EnableAction(_panDeltaAction);
            EnableAction(_zoomAction);

            if (_panButtonAction != null)
            {
                _panButtonAction.action.started += OnPanButtonStarted;
                _panButtonAction.action.canceled += OnPanButtonCanceled;
            }

            if (_zoomAction != null)
            {
                _zoomAction.action.performed += OnZoomPerformed;
            }
        }

        private void OnDisable()
        {
            if (_panButtonAction != null )
            {
                _panButtonAction.action.started -= OnPanButtonStarted;
                _panButtonAction.action.canceled -= OnPanButtonCanceled;
            }

            if (_zoomAction != null)
                _zoomAction.action.performed -= OnZoomPerformed;
        }

        private void LateUpdate()
        {
            ProcessPan();
            ApplySmoothing();
        }

        // ============================================================
        // API
        // ============================================================

        /// <summary>
        /// Frame the camera to show the entire grid.
        /// </summary>
        /// <param name="gridCenter"></param>
        /// <param name="gridSize"></param>
        public void FrameGrid(Vector3 gridCenter, Vector2 gridSize)
        {
            // Store bounds for pan clamping
            _gridBounds = new Bounds(gridCenter, new Vector3(gridSize.x + _framingPadding * 2, 0, gridSize.y + _framingPadding * 2));
            _hasBounds = true;

            // Center on grid
            _targetPosition = new Vector3(gridCenter.x, 0, gridCenter.z);
            transform.position = _targetPosition;

            // Calculate orthographic size to fit the grid
            float aspect = _camera.aspect;
            float halfHeight = (gridSize.y + _framingPadding * 2) * 0.5f;
            float halfWidth = (gridSize.x + _framingPadding * 2) * 0.5f;

            // Account for isometric projection squishing
            float pitchFactor = Mathf.Cos(_pitchAngle * Mathf.Deg2Rad);
            halfHeight /= Mathf.Max(pitchFactor, 0.1f);

            // Fit whichever dimension is tighter
            float sizeForHeight = halfHeight;
            float sizeForWidth = halfWidth / Mathf.Max(aspect, 0.1f);
            float fitSize = Mathf.Max(sizeForHeight, sizeForWidth);

            _targetZoom = Mathf.Clamp(fitSize, _minZoom, _maxZoom);
            _camera.orthographicSize = _targetZoom;

            Debug.Log($"[IsometricCamera] Framed grid: center = {gridCenter}, size = {gridSize}, orthoSize = {_targetZoom}");
        }

        /// <summary>
        /// Frame the grid using positions from the board.
        /// </summary>
        /// <param name="tilePositions"></param>
        public void FrameGrid(HashSet<Vector2> tilePositions)
        {
            if (tilePositions ==  null || tilePositions.Count == 0)
            {
                Debug.LogWarning("[IsometricCamera] No tile positions to frame.");
                return;
            }

            float minX = float.MaxValue, maxX = float.MinValue;
            float minZ = float.MaxValue, maxZ = float.MinValue;

            foreach (Vector2 tilePosition in tilePositions)
            {
                Vector3 worldPos = Tile.GridToWorldPosition(tilePosition);
                minX = Mathf.Min(minX, worldPos.x);
                maxX = Mathf.Max(maxX, worldPos.x);
                minZ = Mathf.Min(minZ, worldPos.z);
                maxZ = Mathf.Max(maxZ, worldPos.z);
            }

            Vector3 center = new Vector3((minX + maxX) * 0.5f, 0, (minZ + maxZ) * 0.5f);
            Vector2 size = new Vector2(maxX - minX + 1, maxZ - minZ + 1);

            FrameGrid(center, size);
        }

        /// <summary>
        /// Move the camera to looj at a specific wordl position.
        /// </summary>
        /// <param name="wordPosition"></param>
        public void LookAt(Vector3 wordPosition)
        {
            _targetPosition = new Vector3(wordPosition.x, 0, wordPosition.z);
        }

        /// <summary>
        /// Set zoom level.
        /// </summary>
        /// <param name="orthoSize"></param>
        public void SetZoom(float orthoSize)
        { 
            _targetZoom = Mathf.Clamp(orthoSize, _minZoom, _maxZoom);
        }

        // ============================================================
        // Isometric setup
        // ============================================================

        /// <summary>
        /// Position and rotate the camera to achieve the isometric view.
        /// </summary>
        private void ApplyIsometricAngle()
        {
            if (_camera ==  null) return;

            _camera.orthographic = true;
            
            // Calculate camera offset from rig pivot
            Quaternion rotation = Quaternion.Euler(_pitchAngle, _yawAngle, 0);
            Vector3 offset = rotation * (Vector3.back * _cameraDistance);

            _camera.transform.localPosition = offset;
            _camera.transform.localRotation = rotation;
        }

        // ============================================================
        // Pan
        // ============================================================

        private void OnPanButtonStarted(InputAction.CallbackContext ctx)
        {
            _isPanning = true;
        }

        private void OnPanButtonCanceled(InputAction.CallbackContext ctx)
        {
            _isPanning = false;
        }

        /// <summary>
        /// Read pan delta every frame while the pan button is held.
        /// </summary>
        private void ProcessPan()
        {
            if (!_isPanning) return;
            if (_panDeltaAction == null) return;

            Vector2 delta = _panDeltaAction.action.ReadValue<Vector2>();
            if (delta.sqrMagnitude < 0.01f) return;

            // Scale by ortho size so pan feels consistent at any zoom
            float scaleFactor = _camera.orthographicSize * _panSpeed;

            // Project camera axes onto XZ plane
            Vector3 right = _camera.transform.right;
            Vector3 up = _camera.transform.up;
            right.y = 0;
            right.Normalize();
            up.y = 0;
            up.Normalize();

            _targetPosition += (-right * delta.x - up * delta.y) * scaleFactor;

            // Calmp to grid bounds
            if (_hasBounds)
            {
                _targetPosition.x = Mathf.Clamp(_targetPosition.x, _gridBounds.min.x, _gridBounds.max.x);
                _targetPosition.z = Mathf.Clamp(_targetPosition.z, _gridBounds.min.z, _gridBounds.max.z);
            }
        }

        // ============================================================
        // Zoom
        // ============================================================

        private void OnZoomPerformed(InputAction.CallbackContext ctx)
        {
            Vector2 scroll = ctx.ReadValue<Vector2>();
            if (Mathf.Abs(scroll.y) < 0.01f) return;

            float zoomDelta = -Mathf.Sign(scroll.y) * _zoomSpeed;
            _targetZoom = Mathf.Clamp(_targetZoom + zoomDelta, _minZoom, _maxZoom);
        }

        // ============================================================
        // Smoothing
        // ============================================================

        private void ApplySmoothing()
        {
            if (_camera == null) return;

            transform.position = Vector3.Lerp(transform.position, _targetPosition, _panSmoothing * Time.deltaTime);
            _camera.orthographicSize = Mathf.Lerp(_camera.orthographicSize, _targetZoom, _zoomSmoothing * Time.deltaTime);
        }

        // ============================================================
        // Helper
        // ============================================================
        private static void EnableAction(InputActionReference actionRef)
        {
            if (actionRef != null && actionRef.action != null)
                actionRef.action.Enable();
        }

        // ============================================================
        // Editor preview
        // ============================================================

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_camera != null && !Application.isPlaying)
                ApplyIsometricAngle();
        }
#endif
    }
}