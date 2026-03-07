using System;
using System.Numerics;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

using UnityEngine.Scripting;
using Vector3 = UnityEngine.Vector3;


public class PlayerController:MonoBehaviour
    {
        #region Fields
        [SerializeField,RequiredMember] InputReader _inputReader;
        [SerializeField] Transform cameraTransform;
        Transform _transform;
        PlayerMover _playerMover;
        CeilingDetector ceilingDetector;
        private CountDownTimer jumpTimer;

        private bool jumpInputIsLockedd, jumpkeywaspressed, jumpkeywasreleaded, jumpkeyisPressed;
        public float MovementSpeed = 1.0f;
        public float airControlRate = 2f;
        public float JumpSpeed = 10f;
        public float jumpduration = 0.5f;
        public float airFriction = 0.5f;
        public float GroundFriction = 100f;
        public float gravity = 30f;
        public float slideGravity = 5f;
        public float slopeLimit = 30f;
        public bool useLocalMomentum;
        [Header("Sprint Settings")]
        public float sprintMultiplier = 2f;
        public float sprintStaminaDrain = 10f;
        public float staminaRegenRate = 5f;
        public float maxStamina = 100f;

        private float currentStamina;
        private bool isSprinting;
        
        StateMachine _stateMachine;
        private Vector3 momentum, savedVelocity, savedMovementVelocity;
        private Animator _animator;
        public event Action<Vector3> OnJump= delegate { };
        public event Action<Vector3> OnLand = delegate { };
        

        #endregion

        private void Awake()
        {
            _transform = transform;
            _playerMover = GetComponent<PlayerMover>();
            ceilingDetector = GetComponent<CeilingDetector>();
            _animator=GetComponentInChildren<Animator>();
            jumpTimer = new CountDownTimer(jumpduration);
            currentStamina = maxStamina;
            SetupStateMachine();
        }
        

        private void Start()
        {
            _inputReader.EnablePlayerActions();
            _inputReader.Jump+= OnJumpInput;
            _inputReader.Run+= OnRunInput;
        }
        private void OnJumpInput(bool isbuttonPressed)
        {
            if (!jumpkeyisPressed && isbuttonPressed)
            {
                jumpkeywaspressed = true;
                
            }
            if (jumpkeyisPressed && !isbuttonPressed)
            {
                jumpkeywasreleaded = true;
                jumpInputIsLockedd = false;
            }
            jumpkeyisPressed = isbuttonPressed;
        }
        private void OnRunInput(bool isButtonPressed)
        {
            isSprinting = isButtonPressed && currentStamina > 0f;
        }

        private void Update()
        {
            _stateMachine.Update();
        }

        public Vector3 GetMomentum()=>useLocalMomentum? _transform.localToWorldMatrix*momentum : momentum;

        public bool IsSprinting
        {
            get => isSprinting;
            set => isSprinting = value;
        }

        void FixedUpdate()
        {
            _stateMachine.FixedUpdate();
            _playerMover.CheckForGround();
            HandleMomentum();
            HandleStamina();
            Vector3 velocity = _stateMachine.CurrentState is GroundedState ? CalculateMovementVelocity() : Vector3.zero;
            velocity+=(useLocalMomentum)? _transform.localToWorldMatrix*momentum : momentum;
            _playerMover.SetExtendSensorRange(IsGrounded());
            _playerMover.SetVelocity(velocity);
            savedVelocity = velocity;
            savedMovementVelocity = CalculateMovementVelocity();
            if (ceilingDetector != null) ceilingDetector.Reset();
            ResetJumpKeys();


        }
        void SetupStateMachine()
        {
            _stateMachine = new StateMachine();
            var grounded = new GroundedState(this);
            var sliding = new SlidingState(this); 
            var rising = new RisingState(this);
            var falling = new FallingState(this);
            var jumping = new JumpState(this);
            At(grounded, rising, ()=>IsRising());
            At(grounded, sliding, ()=>_playerMover.IsGrounded()&&isGroundTooSteep());
            At(grounded,falling,()=>!_playerMover.IsGrounded());
            At(grounded, jumping, () => (jumpkeyisPressed || jumpkeywaspressed) && !jumpInputIsLockedd);
            At(falling,rising,()=>IsRising());
            At(falling,grounded,()=>_playerMover.IsGrounded()&&!isGroundTooSteep());
            At(falling,sliding,()=>_playerMover.IsGrounded()&& isGroundTooSteep());
            At(sliding,rising,()=>IsRising());
            At(sliding,grounded,()=>_playerMover.IsGrounded()&&!isGroundTooSteep());
            At(sliding, falling, () => !_playerMover.IsGrounded());
            At(rising,grounded,()=>_playerMover.IsGrounded()&&!isGroundTooSteep());
            At(rising,sliding,()=>_playerMover.IsGrounded()&& isGroundTooSteep());
            At(rising,falling,()=>IsFalling());
            At(rising, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());
            At(jumping,rising,()=>jumpTimer.IsFinished||jumpkeywasreleaded);
            
            At(jumping, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());
            _stateMachine.SetState(falling);
            
        }
        void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
        void Any<T>(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
        bool IsRising() => GetDotProduct(GetMomentum(),_transform.up) > 0f;
        bool IsFalling() => GetDotProduct(GetMomentum(), _transform.up) < 0f;
        bool isGroundTooSteep() => Vector3.Angle(_playerMover.GroundNormal(), _transform.up) > slopeLimit;
        
        private  float GetDotProduct(Vector3 vector, Vector3 direction) => 
            Vector3.Dot(vector, direction.normalized);
        
        Vector3 CalculateMovementVelocity()
        {
            float speed = MovementSpeed;
    
            if (isSprinting && _stateMachine.CurrentState is GroundedState)
            {
                speed *= sprintMultiplier;
            }
    
            return CalculateMovementDirection() * speed;
        }

        Vector3 CalculateMovementDirection()
        {
            Vector3 direction =cameraTransform==null?_transform.right*_inputReader.Direction.x+_transform.forward*_inputReader.Direction.y:Vector3.ProjectOnPlane(cameraTransform.right, _transform.up).normalized*_inputReader.Direction.x+
                Vector3.ProjectOnPlane(cameraTransform.forward, _transform.up).normalized*_inputReader.Direction.y;
            return direction.magnitude>1f?direction.normalized:direction;
            
        }

        void HandleMomentum()
        {
            if(useLocalMomentum) momentum=_transform.localToWorldMatrix*momentum;
            var direction = _transform.up;
            direction.Normalize();
            Vector3 veticalMomentum = direction * Vector3.Dot(momentum, direction);
                
            Vector3 horizontalMomentum = momentum - veticalMomentum;
            veticalMomentum -= _transform.up * (gravity * Time.deltaTime);

            if (_stateMachine.CurrentState is GroundedState && Vector3.Dot(veticalMomentum, _transform.up) < 0f)
            {
                veticalMomentum = Vector3.zero;
            }

            if (!IsGrounded())
            {
                AdjustHorizontalMovementum(ref horizontalMomentum, CalculateMovementVelocity());
            }

            if (_stateMachine.CurrentState is SlidingState)
            {
                HandleSliding(ref horizontalMomentum);
            }
            float friction =_stateMachine.CurrentState is GroundedState ? GroundFriction : airFriction;
            horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);
            momentum = horizontalMomentum + veticalMomentum;
            if (_stateMachine.CurrentState is JumpState)
            {
                HandleJumping();
            }
            if (_stateMachine.CurrentState is SlidingState) {
                momentum = Vector3.ProjectOnPlane(momentum, _playerMover.GroundNormal());
                if (GetDotProduct(momentum, _transform.up) > 0f) {
                    momentum = RemoveDotVector(momentum, _transform.up);
                }
            
                Vector3 slideDirection = Vector3.ProjectOnPlane(-_transform.up, _playerMover.GroundNormal()).normalized;
                momentum += slideDirection * (slideGravity * Time.deltaTime);
            }
            
            if(useLocalMomentum) momentum=_transform.localToWorldMatrix*momentum;
            
        }
        private void HandleStamina()
        {
            if (isSprinting && _stateMachine.CurrentState is GroundedState && _inputReader.Direction.magnitude > 0.1f)
            {
                currentStamina -= sprintStaminaDrain * Time.fixedDeltaTime;
                currentStamina = Mathf.Max(0f, currentStamina);
                
                if (currentStamina <= 0f)
                {
                    isSprinting = false;
                }
            }
            else
            {
                currentStamina += staminaRegenRate * Time.fixedDeltaTime;
                currentStamina = Mathf.Min(maxStamina, currentStamina);
            }
        }
        

        private void AdjustHorizontalMovementum(ref Vector3 horizontalMomentum, Vector3 movementvelocity)
        {
            if (horizontalMomentum.magnitude > MovementSpeed)
            {
                if (Vector3.Dot(movementvelocity, horizontalMomentum.normalized) > 0f)
                {
                    movementvelocity= movementvelocity- horizontalMomentum.normalized*Vector3.Dot(movementvelocity, horizontalMomentum.normalized);
                }
                horizontalMomentum += movementvelocity * (Time.deltaTime * airControlRate * 0.25f);
            }
            else
            {
                horizontalMomentum+= movementvelocity * (Time.deltaTime * airControlRate);
                horizontalMomentum=Vector3.ClampMagnitude(horizontalMomentum, MovementSpeed);
                    
            }
        }
        private void HandleSliding(ref Vector3 horizontalMomentum)
        {
            Vector3 pointDownVector = Vector3.ProjectOnPlane(_playerMover.GroundNormal(), _transform.up).normalized;
            Vector3 movementVelocity = CalculateMovementVelocity();
            pointDownVector.Normalize();
            movementVelocity=movementVelocity-pointDownVector*Vector3.Dot(movementVelocity,pointDownVector);
            horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
        }

        public void OnJumpStart()
        {
            OnJump.Invoke(momentum);
            if(useLocalMomentum)momentum=_transform.localToWorldMatrix*momentum;
            momentum += _transform.up * JumpSpeed;
            jumpTimer.Start();
            jumpInputIsLockedd = true;
            if(useLocalMomentum)momentum=_transform.localToWorldMatrix*momentum;
        }

        public void OnGroundContactLost()
        {
            if(useLocalMomentum)momentum=_transform.localToWorldMatrix*momentum;
            Vector3 velocity=GetMovementVelocity();
            if (velocity.magnitude >= 0f && momentum.sqrMagnitude > 0f)
            {
                Vector3 projectmomentum = Vector3.Project(momentum, velocity.normalized);
                float dot= GetDotProduct(projectmomentum.normalized, velocity.normalized);
                if (projectmomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) 
                    velocity = Vector3.zero;
                else if (dot > 0f)
                {
                    velocity-= projectmomentum;
                }
            }
            momentum+=velocity;
             if(useLocalMomentum)momentum=_transform.localToWorldMatrix*momentum;
        }
        
        public Vector3 GetMovementVelocity()=> savedMovementVelocity;

        public void OnGroundContactRegained()
        {
            Vector3 collisionvelocit = useLocalMomentum ? _transform.localToWorldMatrix * momentum : momentum;
            OnLand.Invoke(collisionvelocit);

        }

        private void HandleJumping()
        {
            momentum=RemoveDotVector(momentum, _transform.up);
            momentum+=_transform.up*JumpSpeed;
            
        }
        public static Vector3 RemoveDotVector(Vector3 vector, Vector3 direction) {
            direction.Normalize();
            return vector - direction * Vector3.Dot(vector, direction);
        }

        void ResetJumpKeys()
        {
            jumpkeywasreleaded = false;
            jumpkeywaspressed = false;
            
        }

        bool IsGrounded() => _stateMachine.CurrentState is GroundedState or SlidingState;
    }
