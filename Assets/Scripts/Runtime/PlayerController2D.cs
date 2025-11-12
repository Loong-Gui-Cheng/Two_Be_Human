using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using UnityEngine.Windows;

[RequireComponent(typeof(PlayerInput), typeof(BoxCollider2D), typeof(Animator))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Player Controls")]
    [SerializeField] private PlayerData playerData;
    [SerializeField, Range(1f, 5f)] private float speed = 3f;

    [Header("Tilemap")]
    [SerializeField] private Tilemap baseMap;
    [SerializeField] private List<Tilemap> colliderMap;

    // Player Components
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private int IsMovingHash;
    private int AXHash;
    private int AYHash;

    // Player Actions
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction interactAction;

    // Player States
    private bool isMenu;
    private bool isMoving;

    private IInteractable interactable;

    public static System.Action<Vector3> IsPlayerNear;
    public static System.Action InteractObject;

    private void OnEnable()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        IsMovingHash = Animator.StringToHash("IsMoving");
        AXHash = Animator.StringToHash("AX");
        AYHash = Animator.StringToHash("AY");

        playerInput = GetComponent<PlayerInput>();
        playerInput.actions.Enable();

        moveAction = playerInput.actions["Move"];
    }
    private void OnDisable() => playerInput.actions.Disable();

    private void Update()
    {
        if (playerInput == null)
        {
            Assert.IsNull(playerInput, "Player Input is not Initialised!");
            return;
        }

        if (!isMenu && moveAction.IsInProgress())
        {
            Move();
        }
        else
        {
            animator.SetBool(IsMovingHash, false);
        }


        if (interactable != null && interactAction.WasPressedThisFrame())
            InteractObject?.Invoke();
    }

    private void Move()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();
        float spdMultiplier = speed * Time.deltaTime;

        Vector2 moveDirection = new(input.x, input.y);
        if (CanMove(moveDirection))
        {
            if (moveDirection.x < 0f) { spriteRenderer.flipX = true; }
            else if (moveDirection.x > 0f) { spriteRenderer.flipX = false; }

            //animator.Play("Move");
            IsPlayerNear?.Invoke(transform.position);
        }

        animator.SetBool(IsMovingHash, true);
        animator.SetFloat(AXHash, input.x);
        animator.SetFloat(AYHash, input.y);
    }
    private bool CanMove(Vector2 direction)
    {
        if (isMoving) 
            return false;

        if (direction.x != 0f && direction.y != 0f)
            return false;

        Vector3Int gridPosition = baseMap.WorldToCell(transform.position + (Vector3)direction);
        if (!baseMap.HasTile(gridPosition)) return false;

        for (int i = 0; i < colliderMap.Count; i++)
        {
            if (colliderMap[i].HasTile(gridPosition))
                return false;
        }

        StartCoroutine(MoveRoutine(direction));
        return true;
    }
    private IEnumerator MoveRoutine(Vector3 direction)
    {
        isMoving = true;

        Vector3 finalPos = transform.position + direction;
        while ((finalPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalPos, speed * Time.deltaTime);
            yield return null;
        }

        isMoving = false;
        yield break;
    }
}

public interface IInteractable
{
    void OnInteract();
}