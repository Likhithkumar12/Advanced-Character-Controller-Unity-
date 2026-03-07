public class SlidingState : IState {
    readonly PlayerController controller;

    public SlidingState(PlayerController controller) {
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