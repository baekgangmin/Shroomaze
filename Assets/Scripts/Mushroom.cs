using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Mushroom : MonoBehaviour
{
    public int colorIndex;

    void Reset()
    {
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectMushroom(colorIndex);
        }
        Destroy(gameObject);
    }
}
