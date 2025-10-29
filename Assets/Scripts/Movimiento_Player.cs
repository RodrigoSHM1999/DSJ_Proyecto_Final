using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    // Start is called before the first frame update
    public float VelocidadMove = 5.0f;
    public float VelocidadRota =200.0f;
    private Animator anim;
    public float x, y;
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
        transform.Rotate(0, x * Time.deltaTime * VelocidadRota, 0);
        transform.Translate(0, 0, y * Time.deltaTime * VelocidadMove);
        //anim.SetFloat("VelX", x);
        //anim.SetFloat("VelY", y);

    }
}

