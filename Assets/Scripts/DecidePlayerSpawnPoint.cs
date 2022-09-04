using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DecidePlayerSpawnPoint : MonoBehaviour
{
    public GameObject spawnPointObject;


    private string playerTag;


    // Start is called before the first frame update
    void Start()
    {
        playerTag = "Player";
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == playerTag)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.spawnPoint = new Vector2(spawnPointObject.gameObject.transform.position.x, spawnPointObject.gameObject.transform.position.y);
            }
            else
            {
                Debug.Log("error");
            }
        }
    }
}
