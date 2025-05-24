using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // потрібна нова система вводу

public class HelloWorldPlayer : NetworkBehaviour
{
    public float moveSpeed = 3f;

    private NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();

    void Update()
    {
        if (!IsOwner) return;

        HandleMovementInput();
    }

    private void HandleMovementInput()
    {
        Vector2 input = Vector2.zero;

        if (Keyboard.current.wKey.isPressed) input.y += 1;
        if (Keyboard.current.sKey.isPressed) input.y -= 1;
        if (Keyboard.current.aKey.isPressed) input.x -= 1;
        if (Keyboard.current.dKey.isPressed) input.x += 1;

        if (input.sqrMagnitude > 0.001f)
        {
            Vector3 direction = new Vector3(input.x, input.y); // X = вліво/вправо, Z = вперед/назад
            Vector3 newPosition = transform.position + direction.normalized * moveSpeed * Time.deltaTime;
            SubmitMovementServerRpc(newPosition);
        }
    }

    [Rpc(SendTo.Server)]
    private void SubmitMovementServerRpc(Vector3 newPosition)
    {
        transform.position = newPosition;
        Position.Value = newPosition;
    }

    public override void OnNetworkSpawn()
    {
        Position.OnValueChanged += OnPositionChanged;

        if (IsOwner)
        {
            SubmitMovementServerRpc(transform.position); // початкова sync
        }
    }

    public override void OnNetworkDespawn()
    {
        Position.OnValueChanged -= OnPositionChanged;
    }

    private void OnPositionChanged(Vector3 previous, Vector3 current)
    {
        if (!IsOwner)
        {
            transform.position = current;
        }
    }
}
