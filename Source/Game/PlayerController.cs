using FlaxEngine;
using System;

namespace Game
{
    public class PlayerController : Script
    {
        public float MoveSpeed = 300.0f; // cm per second (3 m/s)
        public float StepDistance = 100.0f; // 1 meter = 100 cm
        public float GroundY = 0f; // Adjust to match your tile height exactly
        public LayersMask TileLayer;
        
        private Vector3 _targetPosition;
        private bool _isMoving = false;

        private CharacterController _controller;
        private TileManager _tileManager;
        private GameManager _gameManager;

        public event Action OnStepTaken;
        public event Action OnFallOffPath;

        public override void OnAwake()
        {
            _controller = Actor.As<CharacterController>();
            if (_controller == null)
                Debug.LogError("PlayerController: CharacterController not found!");

            _tileManager = Scene.FindScript<TileManager>();
            if (_tileManager == null)
                Debug.LogError("PlayerController: TileManager not found!");

            _gameManager = Scene.FindScript<GameManager>();
            if (_gameManager == null)
                Debug.LogError("PlayerController: GameManager not found!");
        }

        public override void OnStart()
        {
            var pos = Actor.Position;
            Actor.Position = new Vector3(pos.X, GroundY, pos.Z);
            _targetPosition = Actor.Position;
        }

        public override void OnUpdate()
        {
            Debug.Log($"GameManager: {_gameManager}, State: {_gameManager.GetCurrentGameState()}");
            if (_gameManager != null && _gameManager.GetCurrentGameState() != GameManager.GameState.Playing)
                return;

            HandleInput();
            MovePlayer();
            CheckForFall();
        }

        private void HandleInput()
        {
            if (_isMoving)
                return;

            if (Input.GetKeyDown(KeyboardKeys.A) || Input.GetKeyDown(KeyboardKeys.ArrowLeft))
            {
                Rotate(-90);
            }
            else if (Input.GetKeyDown(KeyboardKeys.D) || Input.GetKeyDown(KeyboardKeys.ArrowRight))
            {
                Rotate(90);
            }
            else if (Input.GetKeyDown(KeyboardKeys.W) || Input.GetKeyDown(KeyboardKeys.ArrowUp))
            {
                Debug.Log("Moving Forward Input");
                StartMoveForward();
            }
        }

        private void Rotate(float angle)
        {
            Quaternion rotation = Quaternion.Euler(0, angle, 0);
            Actor.Orientation *= rotation;
        }

        private void StartMoveForward()
        {
            _targetPosition = Actor.Position + Actor.Transform.Forward * StepDistance;
            _isMoving = true;
        }

        private void MovePlayer()
        {
            if (!_isMoving)
                return;

            Vector3 currentPosition = Actor.Position;
            Vector3 toTarget = _targetPosition - currentPosition;

            float distanceToTarget = toTarget.Length;
            float moveStepDistance = MoveSpeed * Time.DeltaTime;
            Debug.Log("Distance: " + distanceToTarget + ", To Target: " + toTarget);
            const float epsilon = 0.01f;

            if (distanceToTarget <= moveStepDistance + epsilon)
            {
                _controller.Move(toTarget); // Snap to target
                _isMoving = false;

                // Snap Y for consistency
                Actor.Position = new Vector3(_targetPosition.X, GroundY, _targetPosition.Z);

                OnStepTaken?.Invoke();
                _tileManager?.GenerateNextTile(Actor.Position, 0);
            }
            else
            {
                Vector3 moveStep = toTarget.Normalized * moveStepDistance;
                _controller.Move(moveStep);
            }

        }


        private void CheckForFall()
        {
            if (_isMoving)
                return;

            if (!Physics.RayCast(Actor.Position + Vector3.Up * 10f, Vector3.Down, out RayCastHit hit, 200f, TileLayer))
            {
                OnFallOffPath?.Invoke();
                _gameManager?.EndGame(GameManager.EndReason.FellOffPath);
            }
        }

        public void TriggerFallOffPath()
        {
            OnFallOffPath?.Invoke();
            _gameManager?.EndGame(GameManager.EndReason.FellOffPath);
        }
    }
}
