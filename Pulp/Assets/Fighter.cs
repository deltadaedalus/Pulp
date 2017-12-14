using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FighterState
{
    public string name;
    public float damage;
    public Vector2 knockback;
    public bool heavy;
}

//Base class for fighter, TODO: subclass to different characters
public class Fighter : MonoBehaviour {
    public int playerNum;
    [HideInInspector] public SpriteRenderer hurtSprite;
    [HideInInspector] public SpriteRenderer hitSprite;
    [HideInInspector] public SpriteRenderer blockSprite;
    public float maxHealth;

    public List<FighterState> states;
    [HideInInspector] public FighterState currentState;

    Animator animator;
    SpriteRenderer sprite;
    int jumps = 2;
    int prevStateHash;
    int stateHash;
    InputBuffer input;
    Rigidbody2D body;
    float speed;
    float jumpSpeed;
    bool canTurn = true;
    [HideInInspector] public int dir;
    [HideInInspector] public float health;


	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        body = gameObject.GetComponent<Rigidbody2D>();
        speed = 5;
        jumpSpeed = 10;
        hurtSprite = transform.Find("Hurt").gameObject.GetComponent<SpriteRenderer>();
        hitSprite = transform.Find("Hit").gameObject.GetComponent<SpriteRenderer>();
        blockSprite = transform.Find("Block").gameObject.GetComponent<SpriteRenderer>();
        dir = 1;
        jumps = 2;
        canTurn = true;
        input = new InputBuffer(playerNum.ToString());
    }
	
	// Update is called once per frame
	void Update () {
        CheckStateChange();
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
        animator.SetBool("Up", input.up.IsActive());
        animator.SetFloat("Horizontal", input.right.GetValue() * dir);

        if (input.attack.CountPress() > 0 && !animator.GetBool("Attack"))
        {
            input.attack.ConsumePress();
            animator.SetTrigger("Attack");
        }

        while (input.jump.CountPress() > 0 && (jumps == 0))
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
            if (input.right.GetValue() > 0)
            {
                animator.SetBool("Forward", true);
                animator.SetBool("Back", false);
            }
            else if (input.right.GetValue() < 0)
            {
                animator.SetBool("Back", true);
                animator.SetBool("Forward", false);
                Flip();
            }
            else
            {
                animator.SetBool("Forward", false);
                animator.SetBool("Back", false);
            }
        } else
        {
            if(input.right.GetValue() < 0)
            {
                animator.SetBool("Forward", true);
                animator.SetBool("Back", false);
            }
            else if (input.right.GetValue() > 0)
            {
                animator.SetBool("Back", true);
                animator.SetBool("Forward", false);
                Flip();
            }
            else
            {
                animator.SetBool("Forward", false);
                animator.SetBool("Back", false);
            }
        }


        //optional TODO if things are working weird: cause input triggers to expire by checking if input times are too long ago
    }

    void MoveBehaviour()
    {
        if ((stateHash == Animator.StringToHash("Walk")) || (stateHash == Animator.StringToHash("In_Air")) || (stateHash == Animator.StringToHash("J_N_Attack")) || (stateHash == Animator.StringToHash("J_U_Attack")) || (stateHash == Animator.StringToHash("J_F_Attack")) || (stateHash == Animator.StringToHash("J_B_Attack")) || (stateHash == Animator.StringToHash("J_D_Attack")))
        {
            if(animator.GetBool("Grounded") != true)
            {
                if(input.down.IsActive())
                {
                    if (body.velocity.y > -1)
                        body.velocity = new Vector2(body.velocity.x, 0);
                        body.AddForce(new Vector2(0, -1), ForceMode2D.Impulse);
                }
            }
            if (input.right.GetValue() > 0.5)
            {
                if (body.velocity.x < 5)
                {
                    body.AddForce(new Vector2(speed, 0), 0);
                }
            }
            else if (input.right.GetValue() < -0.5)
            {
                if (body.velocity.x > -5)
                {
                    body.AddForce(new Vector2(0 - (speed), 0), 0);
                }
            }
            else if ((input.right.GetValue() > -0.5) && (input.right.GetValue() < 0.5))
            {
                if (body.velocity.x > 0)
                {
                    body.AddForce(new Vector2(0 - speed/2, 0), 0);
                }
                else if (body.velocity.x < 0)
                {
                    body.AddForce(new Vector2(speed / 2, 0), 0);
                }
            }
        }
        if (stateHash == Animator.StringToHash("Run"))
        {
            if (input.right.GetValue() > 0.5)
            {
                if (body.velocity.x < 10)
                {
                    body.AddForce(new Vector2(speed / 4, 0), 0);
                }
            }
            else if (input.right.GetValue() < -0.5)
            {
                if (body.velocity.x > -10)
                {
                    body.AddForce(new Vector2(0 - (speed / 4), 0), 0);
                }
            }
            else if ((input.right.GetValue() > -0.5) && (input.right.GetValue() < 0.5))
            {
                if (body.velocity.x > 0)
                {
                    body.AddForce(new Vector2(0 - (speed), 0), 0);
                }
                else if (body.velocity.x < 0)
                {
                    body.AddForce(new Vector2(speed, 0), 0);
                }
            }
        }
        
        //TODO: Refine movement and add a more natural drag
    }

    //Handles changs from state to state in the animator.  
    void StateChange(int prev, int next)
    {
        FighterState lastState = currentState;
        currentState = states.Find(x => Animator.StringToHash(x.name) == next);
        if (currentState == null)
        {
            Debug.Log("Undefined state!");
            return;
        }

        if (currentState.name == "Jump")
        {
            canTurn = false;
            if (jumps != 0)
            {
                jumps--;
                body.velocity = new Vector2(body.velocity.x, 0);
                body.AddForce(new Vector2(0, jumpSpeed), ForceMode2D.Impulse);
            }
        }
        if (currentState.name == "In_Air")
        {
            canTurn = false;
            if (jumps == 2)
            {
                jumps = 1;
            }
        }
        if (currentState.name == "Landed")
        {
            canTurn = true;
            jumps = 2;
        }
    }

    void Flip()
    {
        if (canTurn)
        {
            dir = -dir;
            sprite.flipX = (dir == -1);
            hitSprite.flipX = (dir == -1);
            hurtSprite.flipX = (dir == -1);
            blockSprite.flipX = (dir == -1);
        }
    }

    void Damage(float dmg)
    {
        health -= dmg;
        if (health <= 0)
        {
            health = 0;
            //TODO: death animations?
        }
    }

    public void HurtBehaviour(Fighter hitFighter)
    {
        if (currentState == null)
        {
            Debug.Log("Undefined state!");
            return;
        }
            

        if (currentState.heavy)
            hitFighter.animator.SetTrigger("HardHit");
        else
            hitFighter.animator.SetTrigger("Hit");

        hitFighter.Damage(currentState.damage);
        Vector2 knock = currentState.knockback;
        knock.x *= dir;
        hitFighter.body.AddForce(knock, ForceMode2D.Impulse);


        if (hitFighter.dir == dir) //Probably temporary?
            hitFighter.Flip();

        //TODO: other attacks?
    }

    public void HitBehaviour(Fighter hurtFighter)
    {
        
    }

    public void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Platform")
        {
            animator.SetBool("Grounded", true);
        }

        if (col.gameObject.tag == "Fighter")
        {
            jumps++;
        }
    }

    public void OnTriggerExit2D(Collider2D col)
    {
        if (col.gameObject.tag == "Platform")
        {
            animator.SetBool("Grounded", false);
        }
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
    float buffer = 0.1f;    //Amount of time in seconds during which an input exists before auto-consume

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
