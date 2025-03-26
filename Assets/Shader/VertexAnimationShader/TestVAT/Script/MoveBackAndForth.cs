using UnityEngine;

public class MoveBackAndForth : MonoBehaviour
{
    [SerializeField] private float speed = 2f;  // Movement speed
    [SerializeField] private float distance = 5f;  // How far it moves from the starting position

    private float startX; // Starting position on the X-axis

    private void Start()
    {
        startX = transform.position.x; // Save the initial X position
    }

    private void Update()
    {
        float newX = startX + Mathf.PingPong(Time.time * speed, distance * 2) - distance;
        transform.position = new Vector3(newX, transform.position.y, transform.position.z);
    }
}
