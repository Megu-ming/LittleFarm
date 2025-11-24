using UnityEngine;

public class SetPlayerStateBehaviour : StateMachineBehaviour
{
    public bool updateOnState;
    public bool updateOnStateMachine;

    // OnStateEnter is called before OnStateEnter is called on any state inside this state machine
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (updateOnState)
        {
            var player = animator.GetComponent<Player>();
            if (player)
            {
                player.SetState(PlayerState.Acting);
            }
        }
    }

    // OnStateUpdate is called before OnStateUpdate is called on any state inside this state machine
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //}

    // OnStateExit is called before OnStateExit is called on any state inside this state machine
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (updateOnState)
        {
            var player = animator.GetComponent<Player>();
            if (player)
            {
                player.SetState(PlayerState.Idle);
            }
        }
    }

    //OnStateMachineEnter is called when entering a state machine via its Entry Node
    override public void OnStateMachineEnter(Animator animator, int stateMachinePathHash)
    {
        if(updateOnStateMachine)
        {
            var player = animator.GetComponent<Player>();
            if(player)
            {
                player.SetState(PlayerState.Acting);
            }
        }
    }

    // OnStateMachineExit is called when exiting a state machine via its Exit Node
    override public void OnStateMachineExit(Animator animator, int stateMachinePathHash)
    {
        if (updateOnStateMachine)
        {
            var player = animator.GetComponent<Player>();
            if (player)
            {
                player.SetState(PlayerState.Idle);
            }
        }
    }
}
