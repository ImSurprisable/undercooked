using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : NetworkBehaviour, IKitchenObjectParent
{

    private const string INTERACT_ALTERNATE = "InteractAlternate";

    public static event EventHandler OnAnyPlayerSpawned;
    public static event EventHandler OnAnyObjectPickup;

    public static void ResetStaticData() {
        OnAnyPlayerSpawned = null;
    }

    public static PlayerController LocalInstance { get; private set; }

    public event EventHandler OnObjectPickup;
    public event EventHandler<OnSelectedCounterChangeEventArgs> OnSelectedCounterChange;
    public class OnSelectedCounterChangeEventArgs : EventArgs
    {
        public BaseCounter selectedCounter;
    }

    [SerializeField] private LayerMask countersLayerMask;
    [SerializeField] private Transform kitchenObjectHoldPoint;
    [SerializeField] private PlayerAnimator playerAnimator;

    [Space]

    [SerializeField] private float interactDistance;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    
    private Vector3 lastMoveDir;
    private BaseCounter selectedCounter;
    private KitchenObject kitchenObject;

    private bool isWalking;
    [SerializeField] bool canSwapItemsOnCounters;

    private Camera mainCamera;
    private Vector3 lastCursorWorldPosition = new Vector3(999f, 999f, 999f);




    private void Start()
    {
        mainCamera = Camera.main;
        
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
        GameInput.Instance.OnInteractAlternateAction += GameInput_OnInteractAlternateAction;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) {
            LocalInstance = this;
        }

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);
    }

    private void GameInput_OnInteractAction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying()) return;
        
        if (selectedCounter != null) {
            selectedCounter.Interact(this);

            playerAnimator.PlayInteractAnimation();
        }
    }
    private void GameInput_OnInteractAlternateAction(object sender, System.EventArgs e)
    {
        if (!GameManager.Instance.IsGamePlaying()) return;

        if (selectedCounter != null) {
            selectedCounter.InteractAlternate(this);

            playerAnimator.PlayInteractAnimation();
        }
        else
        {
            Debug.Log("The counter you are trying is either null or doesn't support that action.");
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        HandleMovement();
        HandleInteractions();
    }


    private void HandleInteractions()
    {
        if (GameInput.isUsingController)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, lastMoveDir, out hit, interactDistance, countersLayerMask))
            {
                if (hit.transform.TryGetComponent<BaseCounter>(out BaseCounter baseCounter)) {
                    if (baseCounter != selectedCounter) {
                        SetSelectedCounter(baseCounter);
                    }
                } 
                else 
                {
                    SetSelectedCounter(null);
                }
            } 
            else 
            {
                SetSelectedCounter(null);
            }
        }
        else
        {
            Vector3 mousePositionOnScreen = Input.mousePosition;

            Vector3 cursorWorldPosition = new Vector3();

            Ray ray = mainCamera.ScreenPointToRay(mousePositionOnScreen);
            if (Physics.Raycast(ray, out RaycastHit cursorHit))
            {
                if (cursorWorldPosition != lastCursorWorldPosition)
                {
                    cursorWorldPosition = cursorHit.point;
                    cursorWorldPosition.y = 0f;
                }
            }
            lastCursorWorldPosition = cursorWorldPosition;

            Vector3 finalLookDirection = cursorWorldPosition - transform.position;

            RaycastHit[] hits = Physics.RaycastAll(transform.position, finalLookDirection.normalized, interactDistance, countersLayerMask);
            if (hits.Length > 0) 
            {
                // Find the furthest baseCounter instance from the player that is in the hits[] array
                RaycastHit furthestHit = new RaycastHit();
                furthestHit.point = transform.position;

                foreach (RaycastHit hit in hits)
                {
                    if (hit.transform.GetComponent<BaseCounter>() != null)
                    {
                        if (Vector3.Distance(hit.point, transform.position) > Vector3.Distance(furthestHit.point, transform.position))
                        {
                            furthestHit = hit;
                        }
                    }
                }

                if (furthestHit.transform.TryGetComponent<BaseCounter>(out BaseCounter baseCounter))
                {
                    if (baseCounter != selectedCounter) {
                        SetSelectedCounter(baseCounter);
                    }
                }
                else 
                {
                    SetSelectedCounter(null);
                }
            }
            else
            {
                SetSelectedCounter(null);
            }
        }
    }

    private void HandleMovement()
    {
        Vector2 inputVector = GameInput.Instance.GetMovementVectorNormalized();

        Vector3 moveDir = new Vector3(inputVector.x, 0f, inputVector.y);

        if (moveDir != Vector3.zero) {
            lastMoveDir = moveDir;
        }

        float moveDistance = moveSpeed * Time.deltaTime;
        float playerRadius = .7f;
        float playerHeight = 2f;
        bool canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDir, moveDistance);

        if (!canMove)
        {
            Vector3 moveDirX = new Vector3(moveDir.x, 0, 0);
            canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirX, moveDistance);

            if (canMove) {
                moveDir = moveDirX;
            } else {
                Vector3 moveDirZ = new Vector3(0, 0, moveDir.z);
                canMove = !Physics.CapsuleCast(transform.position, transform.position + Vector3.up * playerHeight, playerRadius, moveDirZ, moveDistance);

                if (canMove) {
                    moveDir = moveDirZ;
                }
            }
        }

        if (canMove) {
            transform.position += moveDistance * moveDir;
        }

        isWalking = moveDir != Vector3.zero;

        float lerpSpeed = Mathf.Pow(0.001f, rotateSpeed * Time.deltaTime);
        transform.forward = Vector3.Slerp(transform.forward, lastMoveDir, rotateSpeed * Time.deltaTime);
    }

    private void SetSelectedCounter(BaseCounter selectedCounter)
    {
        this.selectedCounter = selectedCounter;

        OnSelectedCounterChange?.Invoke(this, new OnSelectedCounterChangeEventArgs {
            selectedCounter = selectedCounter
        });
    }


    public bool IsWalking()
    {
        return isWalking;
    }

    public bool CanSwapItemsOnCounters()
    {
        return canSwapItemsOnCounters;
    }




    public Transform GetKitchenObjectFollowTransform()
    {
        return kitchenObjectHoldPoint;
    }

    public void SetKitchenObject(KitchenObject kitchenObject, bool playSound = true)
    {
        this.kitchenObject = kitchenObject;

        if (kitchenObject != null && playSound) {
            OnObjectPickup?.Invoke(this, EventArgs.Empty);
            OnAnyObjectPickup?.Invoke(this, EventArgs.Empty);
        }
    }

    public KitchenObject GetKitchenObject()
    {
        return kitchenObject;
    }

    public void ClearKitchenObject()
    {
        kitchenObject = null;
    }

    public bool HasKitchenObject()
    {
        return kitchenObject != null;
    }

    public NetworkObject GetNetworkObject()
    {
        return NetworkObject;
    }
}
