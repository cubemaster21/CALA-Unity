using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingObjectSync : MonoBehaviour
{

    public GameObject texLabel;
    private Rigidbody2D rb;
    public GameObject poofPrefab;
    public string answer;

    // Use this for initialization
    void Start()
    {

        rb = gameObject.GetComponent<Rigidbody2D>();

        rb.AddForce(new Vector2(0, -60));

    }

    // Update is called once per frame
    void Update()
    {
        texLabel.transform.position = Camera.main.WorldToScreenPoint(gameObject.transform.position);
    }
    private void OnDestroy()
    {
        Instantiate(poofPrefab, gameObject.transform.position, Quaternion.identity);
    }
}