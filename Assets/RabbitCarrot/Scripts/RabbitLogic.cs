using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RabbitLogic : MonoBehaviour {
    GameObject[] carrots;
    GameObject target;
    bool chomping = false;
    public float speed = 2;
    // Use this for initialization
    void Start () {
        carrots = GameObject.FindGameObjectsWithTag("carrot");
        target = pickTarget();
        Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        BoxCollider2D targetRB = target.GetComponent<BoxCollider2D>();
        float angle = Mathf.Atan2(targetRB.transform.position.y - rb.transform.position.y, targetRB.transform.position.x - rb.transform.position.x);
        Vector2 force = new Vector2(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed);
        gameObject.GetComponent<Rigidbody2D>().velocity = force;

        if(force.x < 0)
        {
            SpriteRenderer rend = gameObject.GetComponent<SpriteRenderer>();
            rend.flipX = true;
        }
    }
    void Awake()
    {
        
        //target = pickTarget();
        //Rigidbody2D rb = gameObject.GetComponent<Rigidbody2D>();
        //rb.AddForce(new Vector2(100, 100), ForceMode2D.Impulse);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == target)
        {
            Debug.Log("hit");
            gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            Animator anim = gameObject.GetComponent<Animator>();
            anim.Play("RabbitChomp");
            chomping = true;
        }
    }
    // Update is called once per frame
    void Update () {
		if(chomping && target != null)
        {
            CarrotStatus cs = target.GetComponent<CarrotStatus>();
            cs.timeLeft -= Time.deltaTime;


            if(cs.timeLeft <= 0)
            {
                pickTarget();
                if(target == null)
                {
                    //game over man
                }
            }
        }
	}
    private GameObject pickTarget()
    {
        chomping = false;
        bool targetPicked = false;
        int tries = 0;
        while (!targetPicked && tries < carrots.Length + 1)
        {
            int index = Random.Range(0, carrots.Length - 1);
            GameObject picked = carrots[index];
            CarrotStatus status = picked.GetComponent<CarrotStatus>();
            if(status.timeLeft != 0)
            {
                targetPicked = true;
                return picked;
            }
            tries++;
        }
        return null;
    }
}
