using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {
        public bool isInputMove = false;

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        [SerializeField]
        GameObject imageHandleMove, imageHandelLook;


        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            if (virtualMoveDirection != Vector2.zero)
                isInputMove = true;
            else
                isInputMove = false;
                
            starterAssetsInputs.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }

        public void ResetImageHandles()
        {
            imageHandleMove.transform.localPosition = Vector3.zero;
            imageHandelLook.transform.localPosition = Vector3.zero;
        }
        
    }

}
