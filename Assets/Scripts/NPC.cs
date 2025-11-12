using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Dialog")]
    [SerializeField] private GameObject dialogBox;


    public static System.Action AlertPlayer;

    private void OnEnable()
    {
        PlayerController2D.IsPlayerNear += CheckPlayer;
    }
    private void OnDisable()
    {
        PlayerController2D.IsPlayerNear -= CheckPlayer;
    }
    public void CheckPlayer(Vector3 playerPos)
    {
        if (Vector2.Distance(transform.position, playerPos) <= 1f)
        {
            Vector3 direction = (transform.position - playerPos).normalized;
            if (direction.x != 0f && direction.y != 0f) return;

            Debug.Log("Player detected!");
        }
    }
}