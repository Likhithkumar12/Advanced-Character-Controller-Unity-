
    public class GroundedState:IState
    {
        readonly PlayerController controller;

        public GroundedState(PlayerController controller) {
            this.controller = controller;
        }

        public void OnEnter() {
            controller.OnGroundContactRegained();
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
