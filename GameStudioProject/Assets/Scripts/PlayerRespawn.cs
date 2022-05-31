using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerRespawn : MonoBehaviour
{
    private Transform currentCheckpoint;

    private static PlayerRespawn respawn;

    private PlayerRespawn()
    {
        if (respawn == null)
        {
            respawn = this;
        }
    }

    public static PlayerRespawn GetInstance()
    {
        return respawn;
    }

    public void Respawn()
    {
        if(currentCheckpoint != null)
        {
            transform.position = currentCheckpoint.position;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        PlayerController.GetInstance().GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;
        PlayerController.GetInstance().GetComponent<Collider2D>().enabled = true;
        PermanentUI.GetInstance().Respawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "Confiner" || collision.tag == "Checkpoint")
        {
            if (collision.tag == "Checkpoint")
            {
                currentCheckpoint = collision.transform;
            }
        }
    }
}
