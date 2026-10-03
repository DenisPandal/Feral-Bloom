using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingSaw : NavController
{
    protected override void Start()
    {
        base.Start();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {

        }
    }

}
