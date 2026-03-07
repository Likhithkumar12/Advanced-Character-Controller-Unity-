public class RisingState : IState {
    readonly PlayerController controller;

    public RisingState(PlayerController controller) {
        this.controller = controller;
    }

    public void OnEnter() {
        controller.OnGroundContactLost();
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