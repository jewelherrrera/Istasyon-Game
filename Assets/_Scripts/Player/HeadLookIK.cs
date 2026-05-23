using UnityEngine;

public class HeadLookIK : MonoBehaviour
{
    private Animator anim;
    public Transform mainCamera;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (anim != null && mainCamera != null)
        {
            // Calculates a point 15 meters exactly in front of wherever the camera is looking
            Vector3 lookTarget = mainCamera.position + (mainCamera.forward * 15f);
            
            // Turns the head toward that target (Weight settings: overall, body, head, eyes, clamp)
            anim.SetLookAtWeight(1f, 0.1f, 0.8f, 1f, 0.5f); 
            anim.SetLookAtPosition(lookTarget);
        }
    }
}