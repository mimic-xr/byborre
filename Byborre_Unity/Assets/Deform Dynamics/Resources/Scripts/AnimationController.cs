using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationController : MonoBehaviour
{
    private const int IDLE = 0;
    private const int WALK = 1;
    private const int RUN = 2;
    private const int JUMP = 3;

    private Animator animator;
    private int current_state;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        current_state = animator.GetInteger("state");

        if (Input.GetKeyDown(KeyCode.W))
        {
            animator.SetInteger("state", WALK);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            animator.SetInteger("state", IDLE);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (current_state == IDLE)
            {
                animator.SetInteger("state", JUMP);
            }
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            if (current_state == JUMP)
            {
                animator.SetInteger("state", IDLE);
            }
        }

        if (Input.GetKey(KeyCode.A))
        {
            transform.Rotate(transform.up, -1.0f);
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.Rotate(transform.up, 1.0f);
        }
    }
}
