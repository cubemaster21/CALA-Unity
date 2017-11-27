using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarrotStatus : MonoBehaviour {
    public Sprite stage0, stage1, stage2, stage3, stage4;
    private SpriteRenderer sprite;
    private const float timeMax = 90;
    public float timeLeft = 90;
	// Use this for initialization
	void Start () {
        sprite = gameObject.GetComponent<SpriteRenderer>();
	}
	
	// Update is called once per frame
	void Update () {
        if (timeLeft == 0)
        {
            sprite.sprite = stage4;
        } else if (timeLeft <= timeMax * .25f)
        {
            sprite.sprite = stage3;
        } else if (timeLeft <= timeMax * .50f)
        {
            sprite.sprite = stage2;
        } else if (timeLeft <= timeMax * .75f)
        {
            sprite.sprite = stage1;
        } else
        {
            sprite.sprite = stage0;
        }
	}
    private Sprite load(string asset)
    {
        return Resources.Load(asset, typeof(Sprite)) as Sprite;
    }
}
