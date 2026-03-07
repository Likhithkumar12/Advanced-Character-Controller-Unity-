
using UnityEngine;

    public class JumpState:IState
    {
        readonly PlayerController controller;
        public JumpState(PlayerController controller) {
            this.controller = controller;
        }
        public void OnEnter()
        {
            controller.OnGroundContactLost();
            controller.OnJumpStart();
            
        }

        public void OnExit()
        {

        }

        public void OnUpdate()
        {
           
        }

        public void OnFixedUpdate()
        {
            
        }
    }