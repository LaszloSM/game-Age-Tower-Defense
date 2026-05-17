using UnityEngine;

public class CloudParallax : MonoBehaviour
{
    [SerializeField] float scrollSpeed = 0.2f;
    [SerializeField] float resetX = 30f;
    [SerializeField] float startX = -30f;

    void Update()
    {
        transform.position += Vector3.right * scrollSpeed * Time.deltaTime;
        if (transform.position.x > resetX)
            transform.position = new Vector3(startX, transform.position.y, transform.position.z);
    }
}
