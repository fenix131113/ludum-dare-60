using GameAssembly.Player.Data;
using GameAssembly.Player.Input;
using UnityEngine;
using VContainer;

namespace GameAssembly.Player
{
    public class PlayerCamera : MonoBehaviour
    {
        [SerializeField] private Camera cam;
        [SerializeField] private float xConstraint = 90;
        [SerializeField] private float maxZTilt = 15;
        [SerializeField] private float tiltStep = 0.5f;
        [SerializeField] private float sensitivity = 1f;

        [Inject] private PlayerInput _input;
        [Inject] private PlayerState _playerState;

        private Vector3 _startLocalPosition;
        private float _rotationX;
        private float _rotationY;
        private float _rotationZ;
        private float _currentTiltZ;
        private Vector2 _cameraVector;

        public float Sensitivity => sensitivity;

        public void SetSensitivity(float value) => sensitivity = Mathf.Max(0.01f, value);

        public void ResetCameraToStartPosition() => transform.localPosition = _startLocalPosition;
        
        public void SetRotation(Quaternion targetRotation)
        {
            var parent = cam.transform.parent;
            var localRotation = parent ? Quaternion.Inverse(parent.rotation) * targetRotation : targetRotation;
            var euler = localRotation.eulerAngles;
            
            _rotationX = Mathf.DeltaAngle(0, euler.x);
            _rotationY = Mathf.DeltaAngle(0, euler.y);
            _rotationZ = euler.z;
            
            _currentTiltZ = 0f;
            
            cam.transform.localRotation = localRotation;
        }
        
        public void ClearZRotation()
        {
            _currentTiltZ = 0;
            cam.transform.localRotation = Quaternion.Euler(_rotationX, _rotationY, _rotationZ);
        }

        private void Start()
        {
            _startLocalPosition = transform.localPosition;
            SetRotation(cam.transform.rotation);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            Bind();
        }

        private void Update()
        {
            if (_playerState.GetCurrentState() != PlayerInteractStates.FREE &&
                _playerState.GetCurrentState() != PlayerInteractStates.CAMERA_ONLY)
                return;

            _cameraVector = _input.ReadLookVector();

            _rotationX -= _cameraVector.y * sensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -xConstraint, xConstraint);

            _rotationY += _cameraVector.x * sensitivity;

            cam.transform.localRotation =
                Quaternion.Euler(_rotationX, _rotationY, _rotationZ + _currentTiltZ);

            if (!_playerState.IsInTwoD())
                CameraTilt();
        }

        private void OnDestroy()
        {
            Expose();
        }

        private void CameraTilt()
        {
            var moveVector = _input.ReadMovementVector();
            var targetTilt = -moveVector.x * maxZTilt;
            _currentTiltZ = Mathf.MoveTowards(_currentTiltZ, targetTilt, tiltStep * Time.deltaTime);

            cam.transform.localRotation = Quaternion.Euler(_rotationX, _rotationY, _rotationZ + _currentTiltZ);
        }

        private void OnPlayerDimensionChanged()
        {
            if(_playerState.IsInTwoD())
            {
                ClearZRotation();
                return;
            }
            
            ResetCameraToStartPosition();
        }

        private void Bind()
        {
            _playerState.OnDimensionChanged += OnPlayerDimensionChanged;
        }

        private void Expose()
        {
            _playerState.OnDimensionChanged -= OnPlayerDimensionChanged;   
        }
    }
}
