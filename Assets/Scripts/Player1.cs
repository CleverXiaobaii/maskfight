using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Player1 : MonoBehaviour
{
    Rigidbody2D rb;

    Vector3 pos;
    public int Hp;
    public int maskId;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        Hp = 3;
        maskId = 0;

    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log("Player1 Update");
        rb.velocity = Vector2.zero;
        Move();
    }

    void Move()
    {
        if (Input.GetKey(KeyCode.W))
        {
            rb.velocity = new Vector2(rb.velocity.x, 1f);
        }
        if(Input.GetKey(KeyCode.S))
        {
            rb.velocity = new Vector2(rb.velocity.x, -1f);
        }
        if(Input.GetKey(KeyCode.A))
        {
            rb.velocity = new Vector2(-1f, rb.velocity.y);
        }
        if(Input.GetKey(KeyCode.D))
        {
            rb.velocity = new Vector2(1f, rb.velocity.y);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player1 OnCollisionEnter2D");
        if (collision.gameObject.tag == "Mask")
        {
            Mask mask = collision.gameObject.GetComponent<Mask>();
            maskId = mask.MaskID;
            Destroy(collision.gameObject);
        }
    }
}
