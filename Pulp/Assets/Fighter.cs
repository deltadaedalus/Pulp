using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fighter : MonoBehaviour {
    public int playerNum;
    public SpriteRenderer hurtSprite;
    public SpriteRenderer hitSprite;
    public SpriteRenderer blockSprite;

    Animator animator;
    SpriteRenderer sprite;
    bool canJump = true;
    int prevStateHash;
    int stateHash;
    InputBuffer input;

    int dir;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        hurtSprite = transform.Find("Hurt").gameObject.GetComponent<SpriteRenderer>();
        hitSprite = transform.Find("Hit").gameObject.GetComponent<SpriteRenderer>();
        blockSprite = transform.Find("Block").gameObject.GetComponent<SpriteRenderer>();
        dir = 1;
        canJump = true;

        input = new InputBuffer(playerNum.ToString());
    }
	
	// Update is called once per frame
	void Update () {
        prevStateHash = stateHash;
        stateHash = animator.GetCurrentAnimatorStateInfo(0).nameHash;

        if (stateHash != prevStateHash)
        {
            StateChange();
        }

        input.Check();
        FeedMachine();
        MoveBehaviour();
        UpdateColliders();
    }

    void FeedMachine()
    {
        //send inputs
        animator.SetBool("Block", input.block.IsActive());
        animator.SetBool("Down", input.down.IsActive());

        if (input.attack.CountPress() > 0 && !animator.GetBool("Attack"))
        {
            input.attack.ConsumePress();
            animator.SetTrigger("Attack");
        }

        while (input.jump.CountPress() > 0 && !canJump)
            input.jump.ConsumePress();
        if (input.jump.CountPress() > 0 && !animator.GetBool("Jump"))
        {
            input.jump.ConsumePress();
            animator.SetTrigger("Jump");
        }

        if (input.special.CountPress() > 0 && !animator.GetBool("Special"))
        {
            input.special.ConsumePress();
            animator.SetTrigger("Special");
        }
        

        if (dir == 1)
        {
            if (input.right.CountPress() > 0)
            {
                input.right.ConsumePress();
                animator.SetTrigger("Forward");
            }
            if (input.left.CountPress() > 0)
            {
                input.left.ConsumePress();
                animator.SetTrigger("Back");
                Flip();
            }
        } else
        {
            if (input.left.CountPress() > 0)
            {
                input.left.ConsumePress();
                animator.SetTrigger("Forward");
            }
            if (input.right.CountPress() > 0)
            {
                input.right.ConsumePress();
                animator.SetTrigger("Back");
                Flip();
            }
        }


        //expire inputs
        //if (dir == 1 && )


        /*Old
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
            */
    }

    void MoveBehaviour()
    {

    }

    void UpdateColliders()
    {

    }

    void StateChange()
    {

    }

    void Flip()
    {
        dir = -dir;
        sprite.flipX = (dir == -1);
        hitSprite.flipX = (dir == -1);
        hurtSprite.flipX = (dir == -1);
        blockSprite.flipX = (dir == -1);
    }
}

class InputBuffer
{
    public TrackedInput up;
    public TrackedInput down;
    public TrackedInput left;
    public TrackedInput right;
    public TrackedInput attack;
    public TrackedInput block;
    public TrackedInput jump;
    public TrackedInput special;

    public InputBuffer(string playerTag)
    {
        attack = new TrackedInput("Attack" + playerTag, 0.5f, false);
        block = new TrackedInput("Block" + playerTag, 0.5f, false);
        jump = new TrackedInput("Jump" + playerTag, 0.5f, false);
        special = new TrackedInput("Special" + playerTag, 0.5f, false);
        right = new TrackedInput("Horizontal" + playerTag, 0.01f, false);
        left = new TrackedInput("Horizontal" + playerTag, -0.01f, true);
        up = new TrackedInput("Vertical" + playerTag, 0.01f, false);
        down = new TrackedInput("Vertical" + playerTag, -0.01f, true);
    }

    public void Check()
    {
        attack.Check();
        block.Check();
        jump.Check();
        special.Check();
        up.Check();
        down.Check();
        left.Check();
        right.Check();
    }
}

class TrackedInput
{
    float buffer = 0.5f;    //Amount of time in seconds during which an input exists before auto-consume

    string axis;    //Input axis this input is tied to
    float cutoff;   //Threshold above which this input is considered active
    bool inverted;  //If true, input will be considered active below threshold
    
    float value;    //Current value of the axis
    bool active;    //Whether the input is currently considered active

    float lastPress;    //Time of last press
    float lastRelease;  //Time of last release

    Queue<float> presses;   //Queue of buffered inputs
    Queue<float> releases;  //Queue of buffered releases

    public TrackedInput(string axis, float cutoff, bool inverted)
    {
        this.axis = axis;
        this.cutoff = cutoff;
        this.inverted = inverted;

        presses = new Queue<float>();
        releases = new Queue<float>();
    }

    public void setBuffer(float b)
    {
        buffer = b;
    }

    public float getBuffer()
    {
        return buffer;
    }

    public void Check()
    {
        value = Input.GetAxis(axis);
        if (!inverted && value > cutoff || inverted && value < cutoff)
        {
            if (!active)
            {
                active = true;
                presses.Enqueue(Time.time);
            }
        } else
        {
            if (active)
            {
                active = false;
                releases.Enqueue(Time.time);
            }
        }

        while (presses.Count > 0 && presses.Peek() < Time.time - buffer)
        {
            presses.Dequeue();
        }
    }

    public int CountPress()
    {
        return presses.Count;
    }
    public int CountRelease()
    {
        return releases.Count;
    }

    public float PeekPress()
    {
        return presses.Peek();
    }
    public float PeekRelease()
    {
        return releases.Peek();
    }

    public float ConsumePress()
    {
        return presses.Dequeue();
    }
    public float ConsumeRelease()
    {
        return releases.Dequeue();
    }

    public bool IsActive()
    {
        return active;
    }

    public float GetValue()
    {
        return value;
    }
}
