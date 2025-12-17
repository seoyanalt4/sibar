using UnityEngine;

public class PlayerAir : MonoBehaviour
{
    PlayerInteract player;

    public int hitDamage = 10;

    void Awake()
    {
        player = GetComponentInParent<PlayerInteract>();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.GetComponent<Collider>().CompareTag("Monster"))
        {
            player.TakeDamage(hitDamage);
        }
    }

    /*void OnTriggerExit2D(Collider2D other)
    {
        if(other.compareTag("Monster"))
        {
            player.ReduceMobTouch();
        }
    }*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
