using UnityEngine;

public class vAnimatorSetTrigger : StateMachineBehaviour
{
    public bool setOnEnter, setOnExit;
    public string trigger;
    //WILL EDIT:
    public bool useHardSetEnter, useHardSetExit;
    public bool hardSetEnterVal, hardSetExitVal;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (setOnEnter)
        {
            if (!useHardSetEnter)
                animator.SetTrigger(trigger);
            else            
                animator.SetBool(trigger, hardSetEnterVal);            
        }
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (setOnExit)
        {
            if (!useHardSetExit)
                animator.SetTrigger(trigger);
            else            
                animator.SetBool(trigger, hardSetExitVal);            
        }
    }
}
