using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour {
    public Sprite p1Full;
    public Sprite p1Mid;
    public Sprite p1Low;
    public Sprite p1Victory;

    public Sprite p2Full;
    public Sprite p2Mid;
    public Sprite p2Low;
    public Sprite p2Victory;

    public Fighter p1;
    public Fighter p2;

    UnityEngine.UI.Text p1Text;
    UnityEngine.UI.Text p2Text;
    UnityEngine.UI.Image p1Image;
    UnityEngine.UI.Image p2Image;

    private void Start()
    {
        p1Image = transform.Find("Canvas").Find("P1Image").gameObject.GetComponent<UnityEngine.UI.Image>();
        p2Image = transform.Find("Canvas").Find("P2Image").gameObject.GetComponent<UnityEngine.UI.Image>();
        p1Text = transform.Find("Canvas").Find("P1Text").gameObject.GetComponent<UnityEngine.UI.Text>();
        p2Text = transform.Find("Canvas").Find("P2Text").gameObject.GetComponent<UnityEngine.UI.Text>();
    }

    private void Update()
    {
        p1Text.text = (int)p1.health + "%";
        p2Text.text = (int)p2.health + "%";

        if (p1.health <= 0)
        {
            p1Image.sprite = p1Low;
            p2Image.sprite = p2Victory;
        }
        else if (p2.health <= 0)
        {
            p1Image.sprite = p1Victory;
            p2Image.sprite = p2Low;
        }
        else
        {
            if (p1.health > 50)
                p1Image.sprite = p1Full;
            else if (p1.health > 20)
                p1Image.sprite = p1Mid;
            else
                p1Image.sprite = p1Low;

            if (p2.health > 50)
                p2Image.sprite = p2Full;
            else if (p2.health > 20)
                p2Image.sprite = p2Mid;
            else
                p2Image.sprite = p2Low;
        }
    }
}
