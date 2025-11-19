using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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
    private InputAction menuAction;

    // Player States
    private bool IsInMenu;
    private bool IsMoving;

    private IInteractable interactable;

    public static System.Action<Vector3> IsPlayerNear;
    public static System.Action InteractObject;

    public static event System.Action<bool> OnToggleMenu;

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
        interactAction = playerInput.actions["Interact"];
        menuAction = playerInput.actions["Menu"];
    }
    private void OnDisable()
    {
        playerInput.actions.Disable();
    }


    private void Update()
    {
        if (playerInput == null)
        {
            Assert.IsNull(playerInput, "Player Input is not Initialised!");
            return;
        }

        MenuControl();
        MoveControl();
        InteractControl();
    }


    private void MenuControl()
    {
        if (menuAction.WasPressedThisFrame())
        {
            IsInMenu = !IsInMenu;
            OnToggleMenu?.Invoke(IsInMenu);
        }
    }


    private void MoveControl()
    {
        if (IsInMenu) return;
        if (moveAction.IsInProgress())
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
        else
        {
            animator.SetBool(IsMovingHash, false);
        }
    }
    private bool CanMove(Vector2 direction)
    {
        if (IsMoving)
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
        IsMoving = true;

        Vector3 finalPos = transform.position + direction;
        while ((finalPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, finalPos, speed * Time.deltaTime);
            yield return null;
        }

        IsMoving = false;
        yield break;
    }


    private void InteractControl()
    {
        if (interactable == null) return;

        if (interactAction.WasPressedThisFrame())
            InteractObject?.Invoke();
    }
}

public interface IInteractable
{
    void OnInteract();
}