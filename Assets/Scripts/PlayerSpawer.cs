using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawer : MonoBehaviour
{
    [SerializeField] private GameObject spawnPointObject;

    private static readonly string playerTag = "Player";


    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == playerTag)
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.spawnPoint = spawnPointObject.transform.position;
            }
            else
            {
                Debug.Log("error");
            }
        }
    }
}
