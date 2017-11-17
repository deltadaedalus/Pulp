using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Base class for fighter, TODO: subclass to different characters
public class Fighter : MonoBehaviour {
    public int playerNum;
    [HideInInspector] public SpriteRenderer hurtSprite;
    [HideInInspector] public SpriteRenderer hitSprite;
    [HideInInspector] public SpriteRenderer blockSprite;

    Animator animator;
    SpriteRenderer sprite;
    bool canJump = true;
    int prevStateHash;
    int stateHash;
    InputBuffer input;
    Rigidbody2D body;
    float speed;
    [HideInInspector] public int dir;

	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        body = gameObject.GetComponent<Rigidbody2D>();
        speed = 8.0f;
        hurtSprite = transform.Find("Hurt").gameObject.GetComponent<SpriteRenderer>();
        hitSprite = transform.Find("Hit").gameObject.GetComponent<SpriteRenderer>();
        blockSprite = transform.Find("Block").gameObject.GetComponent<SpriteRenderer>();
        dir = 1;
        canJump = true;

        input = new InputBuffer(playerNum.ToString());
    }
	
	// Update is called once per frame
	void Update () {
        CheckStateChange(); //TODO: Ash lib
        input.Check();
        FeedMachine();
        MoveBehaviour();
    }

    void CheckStateChange()
    {
        prevStateHash = stateHash;
        stateHash = animator.GetCurrentAnimatorStateInfo(0).shortNameHash;

        if (stateHash != prevStateHash)
        {
            StateChange(prevStateHash, stateHash);
        }
    }

    void FeedMachine()
    {
        //send inputs
        animator.SetBool("Block", input.block.IsActive());
        animator.SetBool("Down", input.down.IsActive());
        animator.SetFloat("Horizontal", input.right.GetValue() * dir);

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


        //optional TODO if things are working weird: cause input triggers to expire by checking if input times are too long ago
    }

    void MoveBehaviour()
    {
        if (stateHash == Animator.StringToHash("Walk"))
        {

            if (input.right.GetValue() > 0.99)
            {
                body.AddForce(new Vector2(speed/2, 0), 0);
            }
            else if (input.right.GetValue() < -0.5)
            {
                body.AddForce(new Vector2(0 - (speed/2), 0), 0);
            }
            else if ((input.right.GetValue() > -0.5) && (input.right.GetValue() < 0.5))
            {
                if (body.velocity.x > 0)
                {
                    body.AddForce(new Vector2(0 - speed/2, 0), 0);
                }
                else if (body.velocity.x < 0)
                {
                    body.AddForce(new Vector2(speed/2, 0), 0);
                }
            }
        }
        if (stateHash == Animator.StringToHash("Run"))
        {
            if (input.right.GetValue() > 0.5)
            {
                body.AddForce(new Vector2(speed, 0), 0);
            }
            else if (input.right.GetValue() < -0.5)
            {
                body.AddForce(new Vector2(0 - (speed), 0), 0);
            }
            else if ((input.right.GetValue() > -0.5) && (input.right.GetValue() < 0.5))
            {
                if (body.velocity.x > 0)
                {
                    body.AddForce(new Vector2(0 - speed, 0), 0);
                }
                else if (body.velocity.x < 0)
                {
                    body.AddForce(new Vector2(speed, 0), 0);
                }
            }
        }
        
        //TODO: apply physics pushes to attached rigidbody component based on inputs
    }

    //Handles changs from state to state in the animator.  Possible TODO: rejigger to work with Ash's library for doing exactly this
    void StateChange(int prev, int next)
    {
        if (next == Animator.StringToHash("Jump"))
        {
            //TODO: Apply vertical physics push to attached rigidbody
        }
    }

    void Flip()
    {
        dir = -dir;
        sprite.flipX = (dir == -1);
        hitSprite.flipX = (dir == -1);
        hurtSprite.flipX = (dir == -1);
        blockSprite.flipX = (dir == -1);
    }

    public void HurtBehaviour(Fighter hitFighter)
    {
        if (stateHash == Animator.StringToHash("H_Attack_1"))
        {
            hitFighter.animator.SetTrigger("Hit");
            //TODO: knockback
        } else if (stateHash == Animator.StringToHash("H_Attack_2"))
        {
            hitFighter.animator.SetTrigger("Hit");
            //TODO: knockback
        }
        else if (stateHash == Animator.StringToHash("H_Attack_3"))
        {
            hitFighter.animator.SetTrigger("HardHit");
            //TODO: knockback
        }

        if (hitFighter.dir == dir) //Probably temporary?
            hitFighter.Flip();

        //TODO: other attacks?
        //TODO later: Integrate Ash's library so we aren't doing a big if-else
    }

    public void HitBehaviour(Fighter hurtFighter)
    {
        //TODO: probably nothing for now
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
