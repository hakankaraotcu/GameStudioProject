using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleport : MonoBehaviour
{
    [SerializeField] private Transform targetArea;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if(other.gameObject.tag == "Player")
        {
            PlayerController.GetInstance().transform.position = targetArea.position;
            if (PlayerController.GetInstance().transform.position.x < targetArea.position.x)
            {
                PlayerController.GetInstance().transform.localScale = new Vector2(3, 3);
            }
            else
            {
                PlayerController.GetInstance().transform.localScale = new Vector2(-3, 3);
            }
        }
    }
}
