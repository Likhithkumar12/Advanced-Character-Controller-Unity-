using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static PlayerInputActions;


[CreateAssetMenu(fileName = "InputReader", menuName = "InputReader")]
public class InputReader : ScriptableObject, IPlayerActions
{
    public event UnityAction<Vector2> Move=delegate { };
    public event UnityAction<Vector2,bool> Look=delegate { };
    public event UnityAction<bool> Jump=delegate { };
    
    public event UnityAction<bool> Run=delegate { };
    public event UnityAction EnableMouseControlCamera=delegate { };
    public event UnityAction DisableMouseControlCamera=delegate { };
    PlayerInputActions _playerInputActions;
    
    public Vector2 Direction => _playerInputActions.Player.Move.ReadValue<Vector2>();
    public Vector2 LookDirection => _playerInputActions.Player.Look.ReadValue<Vector2>();

    private void OnEnable()
    {
        if (_playerInputActions == null)
        {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.SetCallbacks(this);
        }
        _playerInputActions.Enable();
    }
    private void OnDisable() { 
        _playerInputActions.Disable();
    }
    public void EnablePlayerActions() {
        if (_playerInputActions == null) {
            _playerInputActions = new PlayerInputActions();
            _playerInputActions.Player.SetCallbacks(this);
        }
        _playerInputActions.Enable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
       Move.Invoke(context.ReadValue<Vector2>());
       
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        Look.Invoke(context.ReadValue<Vector2>(),IsDeviceMouse(context));
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        throw new System.NotImplementedException();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jump.Invoke(context.ReadValueAsButton());
    }

    public void OnRun(InputAction.CallbackContext context)
    {
        Run.Invoke(context.ReadValueAsButton());
    }

    public void OnMouseLook(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
                EnableMouseControlCamera.Invoke();
                break;
            case InputActionPhase.Canceled:
                DisableMouseControlCamera.Invoke();
                break;
        }
    }
    bool IsDeviceMouse(InputAction.CallbackContext context)=> context.control.device is Mouse;
}