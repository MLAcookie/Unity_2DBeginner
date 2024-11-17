using UnityEngine;

public class HealthCollectible : MonoBehaviour 
{
    void OnTriggerEnter2D(Collider2D other)
    {
        var controller = other.GetComponent<RubyController>();

        if (controller is not null)
        {
            controller.ChangeHealth(1);
            Destroy(gameObject);
        }
    }
}
