using FlaxEngine;
using System;

namespace Game
{

    public class PlayerController : Script
    {
        public enum Direction
        {
            Forward,
            Left,
            Right
        }

        public float MoveSpeed = 1.0f;
        public float StepDistance = 1.0f;
        public LayersMask TileLayer;

        private Vector3 _targetPosition;
        private bool _isMoving = false;
        private Direction _currentDirection = Direction.Forward;

        private TileManager _tileManager;
        private GameManager _gameManager;

        public event Action OnStepTaken;
        public event Action OnFallOffPath;

        public override void OnAwake()
        {
            _tileManager = Scene.FindScript<TileManager>(); //Actor -> Script
            if (_tileManager == null)
                Debug.LogError("PlayerController: TileManager not found!");

            _gameManager = Scene.FindScript<GameManager>();
            if (_gameManager == null)
                Debug.LogError("PlayerController: GameManager not found!");
        }

        public override void OnStart()
        {
            _targetPosition = Actor.Position;
        }

        public override void OnUpdate()
        {
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

            if (Input.GetKeyDown(KeyboardKeys.W) || Input.GetKeyDown(KeyboardKeys.ArrowUp))
            {
                MakeDirectionChoice(Direction.Forward);
            }
            else if (Input.GetKeyDown(KeyboardKeys.A) || Input.GetKeyDown(KeyboardKeys.ArrowLeft))
            {
                MakeDirectionChoice(Direction.Left);
            }
            else if (Input.GetKeyDown(KeyboardKeys.D) || Input.GetKeyDown(KeyboardKeys.ArrowRight))
            {
                MakeDirectionChoice(Direction.Right);
            }
        }

        private void MovePlayer()
        {
            if (_isMoving)
            {
                Actor.Position = Vector3.Lerp(Actor.Position, _targetPosition, MoveSpeed * Time.DeltaTime);

                if (Vector3.Distance(Actor.Position, _targetPosition) < 0.01f)
                {
                    Actor.Position = _targetPosition;
                    _isMoving = false;
                    OnStepTaken?.Invoke();

                    _tileManager?.GenerateNextTile(Actor.Position, _currentDirection);
                }
            }
        }

        public void MakeDirectionChoice(Direction direction)
        {
            if (_isMoving)
                return;

            _currentDirection = direction;
            MoveForward();
        }

        private void MoveForward()
        {
            Quaternion rotation = Actor.Orientation;

            switch (_currentDirection)
            {
                case Direction.Left:
                    rotation *= Quaternion.Euler(0, -90, 0);
                    break;
                case Direction.Right:
                    rotation *= Quaternion.Euler(0, 90, 0);
                    break;
            }

            Actor.Orientation = rotation;
            _targetPosition = Actor.Position + Actor.Transform.Forward * StepDistance;
            _isMoving = true;
        }

        private void CheckForFall()
        {
            if (_isMoving)
                return;

            if (!Physics.RayCast(Actor.Position + Vector3.Up * 0.1f, Vector3.Down, out RayCastHit hit, 0.2f, TileLayer))
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
