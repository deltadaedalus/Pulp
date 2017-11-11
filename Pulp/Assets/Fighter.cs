using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour {
    Animator animator;
    SpriteRenderer sprite;

    int dir;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        dir = 1;
	}
	
	// Update is called once per frame
	void Update () {
        FeedMachine();
    }

    void FeedMachine()
    {
        if (Input.GetAxis("Horizontal") != 0)
            if ((Input.GetAxis("Horizontal") > 0) == (dir == -1))
            {
                animator.SetTrigger("Back");
                animator.SetFloat("ForwardTime", 0f);
                Flip();
            }
            else
            {
                animator.SetTrigger("Forward");
                animator.SetFloat("ForwardTime", animator.GetFloat("ForwardTime") + Time.deltaTime);
            }
        else
            animator.SetFloat("ForwardTime", 0f);

        animator.SetBool("Crouch", Input.GetKey("down"));
        animator.SetBool("Up", Input.GetKey("up"));

        animator.SetFloat("Vertical", Input.GetAxis("Vertical"));
        animator.SetFloat("Horizontal", Input.GetAxis("Horizontal") * dir);

        if (Input.GetKeyDown("z"))
            animator.SetTrigger("Attack");
        if (Input.GetKeyDown("c"))
        {
            animator.SetTrigger("Block");
            animator.ResetTrigger("~Block");
        }
        if (Input.GetKeyUp("c"))
        {
            animator.SetTrigger("~Block");
            animator.ResetTrigger("Block");
        }
        if (Input.GetKeyDown("space"))
            animator.SetTrigger("Jump");
    }

    void Flip()
    {
        dir = -dir;
        sprite.flipX = (dir == -1);
    }
}
