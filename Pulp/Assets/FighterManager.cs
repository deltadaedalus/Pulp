using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FighterColState
{

}

public class FighterManager : MonoBehaviour {
    public Fighter[] fighters;
    public FighterColState[,] colStates;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        DetectCollisions();
	}

    void AddFighter()
    {

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

                if (ahurt.gameObject.activeInHierarchy && bhit.gameObject.activeInHierarchy
                    && polyCollision(translate(ahurt.sprite.vertices, aPos), translate(bhit.sprite.vertices, bPos)))
                    Debug.Log("A hurts B");
                if (bhurt.gameObject.activeInHierarchy && ahit.gameObject.activeInHierarchy
                    && polyCollision(translate(bhurt.sprite.vertices, bPos), translate(ahit.sprite.vertices, aPos)))
                    Debug.Log("B hurts A");
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

    bool polyCollision(Vector2[] I, Vector2[] J)
    {
        //Check if any edges cross
        for (int i = 0; i < I.Length; ++i)
        {
            int ix = (i + 1) % I.Length;
            float ai = I[ix].y - I[i].y;
            float bi = I[i].x = I[ix].x;
            float ci = ai * I[i].x + bi * I[i].y;

            for (int j = 0; j < J.Length; ++j)
            {
                int jx = (j + 1) % J.Length;
                float aj = J[jx].y - J[j].y;
                float bj = J[j].x = J[jx].x;
                float cj = aj * J[j].x + bj * J[j].y;

                float det = ai * bj - aj * bi;
                float x = (bj * ci - bi * cj) / det;
                float y = (ai * cj - aj * ci) / det;

                if ((x > I[i].x) == (x < I[ix].x))
                {
                    //Debug.Log(x + " " + y);
                    Debug.DrawLine(new Vector3(I[i].x, I[i].y, 0), new Vector3(I[ix].x, I[ix].y), Color.cyan, 1f);
                    Debug.DrawLine(new Vector3(J[j].x, J[j].y, 0), new Vector3(J[jx].x, J[jx].y), Color.yellow, 1f);
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
            //Debug.Log(v.x + " " + v.y);
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
            Debug.Log(v.x + " " + v.y);
            return true;
        }

        //All cases fail
        return false;
    }
}
