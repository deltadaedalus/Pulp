using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FighterColState
{
    public bool hurt_hit;
}

public class FighterManager : MonoBehaviour {
    public Fighter[] fighters;
    FighterColState[,] colStates;

	// Use this for initialization
	void Start () {
        colStates = new FighterColState[fighters.Length, fighters.Length];

    }
	
	// Update is called once per frame
	void Update () {
        DetectCollisions();
	}

    int t = 0;

    void DetectCollisions()
    {
        for (int a = 0; a < fighters.Length; ++a)
        {
            SpriteRenderer ahurt = fighters[a].hurtSprite;
            SpriteRenderer ahit = fighters[a].hitSprite;
            Vector2 aPos = new Vector2(fighters[a].transform.position.x, fighters[a].transform.position.y);

            for (int b = a+1; b < fighters.Length; ++b)
            {
                SpriteRenderer bhurt = fighters[b].hurtSprite;
                SpriteRenderer bhit = fighters[b].hitSprite;
                Vector2 bPos = new Vector2(fighters[b].transform.position.x, fighters[b].transform.position.y);

                //Check collisions
                bool AHurtB = ahurt.gameObject.activeInHierarchy && bhit.gameObject.activeInHierarchy && spriteCollision(ahurt.sprite, bhit.sprite, aPos, bPos);

                bool BHurtA = bhurt.gameObject.activeInHierarchy && ahit.gameObject.activeInHierarchy && spriteCollision(bhurt.sprite, ahit.sprite, aPos, bPos);

                //Fire hit events
                if (AHurtB && !colStates[a, b].hurt_hit)
                {
                    fighters[a].HurtBehaviour(fighters[b]);
                    fighters[b].HitBehaviour(fighters[a]);
                }
                if (BHurtA && !colStates[b, a].hurt_hit)
                {
                    fighters[b].HurtBehaviour(fighters[a]);
                    fighters[a].HitBehaviour(fighters[b]);
                }

                //Update contact states
                colStates[a, b].hurt_hit = AHurtB;
                colStates[b, a].hurt_hit = BHurtA;


                if (AHurtB)
                    Debug.Log("Smeesh");
            }
        }
    }

    Vector2[] translate(Vector2[] verts, Vector2 t)
    {
        Vector2[] nv = new Vector2[verts.Length];
        for (int i = 0; i < verts.Length; ++i)
        {
            nv[i] = verts[i] + t;
        }
        return nv;
    }

    bool spriteCollision(Sprite A, Sprite B, Vector2 aPos, Vector2 bPos)
    {
        for (int i = 0; i < A.GetPhysicsShapeCount(); ++i)
        {
            List<Vector2> AvertList = new List<Vector2>();
            A.GetPhysicsShape(i, AvertList);
            Vector2[] Averts = new Vector2[AvertList.Count];

            int a = 0;
            foreach(Vector2 v in AvertList)
            {
                Averts[a] = v + aPos - bPos;
                ++a;
            }

            for (int j = 0; j < B.GetPhysicsShapeCount(); ++j)
            {
                List<Vector2> Bverts = new List<Vector2>();
                B.GetPhysicsShape(j, Bverts);

                if (polyCollision(Averts, Bverts.ToArray()))
                    return true;
            }
        }

        return false;
    }

    bool polyCollision(Vector2[] I, Vector2[] J)
    {
        //Check if any edges cross
        for (int i = 0; i < I.Length; ++i)
        {
            int ix = (i + 1) % I.Length;
            float ai = I[ix].y - I[i].y;
            float bi = I[i].x - I[ix].x;
            float ci = ai * I[i].x + bi * I[i].y;

            Debug.DrawLine(new Vector3(I[i].x, I[i].y, 0), new Vector3(I[ix].x, I[ix].y), Color.cyan, 12f);

            for (int j = 0; j < J.Length; ++j)
            {
                int jx = (j + 1) % J.Length;
                float aj = J[jx].y - J[j].y;
                float bj = J[j].x - J[jx].x;
                float cj = aj * J[j].x + bj * J[j].y;

                float det = ai * bj - aj * bi;
                float x = (bj * ci - bi * cj) / det;
                float y = (ai * cj - aj * ci) / det;
                
                Debug.DrawLine(new Vector3(J[j].x, J[j].y, 0), new Vector3(J[jx].x, J[jx].y), Color.yellow, 12f);

                if (Mathf.Min(I[i].x, I[ix].x) <= x && x <= Mathf.Max(I[i].x, I[ix].x) && Mathf.Min(J[j].x, J[jx].x) <= x && x <= Mathf.Max(J[j].x, J[jx].x))
                {
                    Debug.Log(x + " " + y);
                    Debug.DrawLine(new Vector3(I[i].x, I[i].y, 0), new Vector3(I[ix].x, I[ix].y), Color.cyan, 12f);
                    Debug.DrawLine(new Vector3(J[j].x, J[j].y, 0), new Vector3(J[jx].x, J[jx].y), Color.yellow, 12f);
                    return true;
                }
            }
        }

        //check if J0 inside I
        Vector2 v = J[0];
        int count = 0;
        for (int i = 0; i < I.Length; ++i)
        {
            int ix = (i + 1) % I.Length;
            if (  ((I[i].y > v.y) == (I[ix].y < v.y))  &&  (v.y < Mathf.Lerp(I[i].x, I[ix].x, (v.y - I[i].y) / (I[ix].y - I[i].y)))  )
                ++count;
        }

        if (count % 2 == 1)
        { 
            Debug.DrawLine(new Vector3(v.x, v.y, 0), new Vector3(v.x, v.y-0.1f, 0), Color.cyan, 1f);
            return true;
        }

        //check if I0 inside J
        v = I[0];
        count = 0;
        for (int i = 0; i < J.Length; ++i)
        {
            int ix = (i + 1) % J.Length;
            if (((J[i].y > v.y) == (J[ix].y < v.y)) && (v.y < Mathf.Lerp(J[i].x, J[ix].x, (v.y - J[i].y) / (J[ix].y - J[i].y))))
                ++count;
        }

        if (count % 2 == 1)
        {
            Debug.DrawLine(new Vector3(v.x, v.y, 0), new Vector3(v.x, v.y - 0.1f, 0), Color.cyan, 1f);
            return true;
        }

        //All cases fail
        return false;
    }
}
