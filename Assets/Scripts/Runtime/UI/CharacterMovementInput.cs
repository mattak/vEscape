using UnityEngine;
using vEscape.InputActions;

namespace vEscape.UI
{
    public class CharacterMovementInput : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Animator _animator;
        [SerializeField] private Rigidbody _rigidbody;

        private float MAX_MOVE_SPEED = 4.0f;
        private float NORMAL_MOVE_SPEED = 2.0f;
        private float JUMP_VELOCITY = 20.0f;
        private float JUMP_DELAY = .10f;

        private PlayerInputAction _input;
        private float _moveSpeed = 2.0f;
        private float _previousSpeed = 0.0f;
        private Vector3 _moveDirection = Vector3.zero;
        private float _jumpTime = 0.0f;
        private bool _isJumping = false;

        private void Awake()
        {
            _input = new PlayerInputAction();
            _input.Enable();
            _input.Player.Jump.performed += context => OnJump();
        }

        private void Update()
        {
            // move direction
            _moveDirection = ReadMoveVelocity();

            // speed change
            if (_input.Player.Shift.IsPressed())
            {
                _moveSpeed += 0.1f;
                _moveSpeed = Mathf.Clamp(_moveSpeed, NORMAL_MOVE_SPEED, MAX_MOVE_SPEED);
            }
            else
            {
                _moveSpeed = Mathf.Lerp(_moveSpeed, NORMAL_MOVE_SPEED, 0.1f);
            }
        }

        private void FixedUpdate()
        {
            UpdateMove();
            UpdateJump();
        }

        private void OnJump()
        {
            if (_isJumping || IsJumpAnimating())
            {
                Debug.Log("Skip");
                return;
            }

            _animator.SetTrigger("Jump");
            _isJumping = true;
            _jumpTime = 0f;
            UpdateJump();
        }

        private Vector3 ReadMoveVelocity()
        {
            var axis = _input.Player.Move.ReadValue<Vector2>();
            return new Vector3(axis.x, 0, axis.y);
        }

        private void UpdateMove()
        {
            // 1. get vector from camera direction on x-z plane
            var cameraForward = Vector3.Scale(_camera.transform.forward, new Vector3(1, 0, 1)).normalized;

            // 2. determinate move direction
            var moveForward = cameraForward * _moveDirection.z + _camera.transform.right * _moveDirection.x;

            // 3. multiply speed if direction is enough
            if (Mathf.Abs(_moveDirection.x) + Mathf.Abs(_moveDirection.z) > 0.1f)
            {
                _rigidbody.velocity = moveForward * _moveSpeed + new Vector3(0, _rigidbody.velocity.y, 0);
            }

            // 4. send speed change to animator
            if (Mathf.Abs(_rigidbody.velocity.magnitude - _previousSpeed) > 0.1f)
            {
                var speed = _rigidbody.velocity.magnitude / MAX_MOVE_SPEED;
                _animator.SetFloat("Speed", speed);
            }

            _previousSpeed = _rigidbody.velocity.magnitude;

            // 5. change character orientation
            if (moveForward != Vector3.zero)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(moveForward), 0.1f);
            }
        }

        private void UpdateJump()
        {
            if (_isJumping)
            {
                _jumpTime += Time.fixedDeltaTime;
                if (_jumpTime >= JUMP_DELAY)
                {
                    _isJumping = false;
                    _jumpTime = 0.0f;
                    _rigidbody.AddForce(Vector3.up * JUMP_VELOCITY, ForceMode.Impulse);
                }
            }
        }

        private bool IsJumpAnimating()
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            var result = info.IsName("Jump");
            return result;
        }
    }
}