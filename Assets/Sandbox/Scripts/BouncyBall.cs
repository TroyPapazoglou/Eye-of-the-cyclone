using Mirror;
using UnityEngine;

public class BouncyBall : NetworkInteractable
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private float punchForce = 5f;
    [SerializeField] private LayerMask playerLayer;


    [SyncVar(hook = nameof(OnHeldByChanged))]
    private uint heldByNetId;

    public override bool CanPunch => true;

    private bool hasBeenThrown;

    private bool isHeld;

    [Server]
    public override void Interact(PlayerNetworkObject player)
    {
        PickUp(player);
    }

    [Server]
    public override void Punch(PlayerNetworkObject player)
    {
        if (rb == null)
            return;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
            return;

        Vector3 direction = (transform.position - gameplayPlayer.transform.position).normalized;

        rb.AddForce(direction * punchForce, ForceMode.Impulse);
    }

    public override bool CanBePickedUp()
    {
        return !isHeld && !hasBeenThrown;
    }

    public override void PickUp(PlayerNetworkObject player)
    {
        isHeld = true;
        heldByNetId = player.netId;

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        transform.SetParent(gameplayPlayer.BarrelHoldPoint, false);
        transform.SetLocalPositionAndRotation(
            Vector3.zero,
            Quaternion.identity
        );


    }

    [Server]
    public override void Throw(Vector3 direction)
    {
        heldByNetId = 0;
        transform.SetParent(null, true);

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        rb.linearVelocity = direction * throwForce;

        isHeld = false;
        hasBeenThrown = true;
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown)
            return;

        Vector3 impactPoint = collision.GetContact(0).point;

        Explode(impactPoint);
    }

    [Server]
    private void Explode(Vector3 position)
    {
        Collider[] hits = Physics.OverlapSphere(
            position,
            explosionRadius,
            playerLayer
        );

        foreach (Collider hit in hits)
        {
            GameplayPlayer player = hit.GetComponentInParent<GameplayPlayer>();

            if (player == null)
                continue;

            Vector3 direction = (player.transform.position - position).normalized;

            player.PlayerMovement.ApplyExplosionForce(
                direction,
                explosionForce
            );
        }

        hasBeenThrown = false;
    }  

    [Server]
    public void SetSpawnedState()
    {
        isHeld = false;
        hasBeenThrown = false;

        rb.isKinematic = false;
        rb.useGravity = true;
    }  

    private void OnHeldByChanged(uint oldNetId, uint newNetId)
    {
        if (newNetId == 0)
        {
            transform.SetParent(null);
            return;
        }

        TryAttachToPlayer(newNetId);
    }

    private void TryAttachToPlayer(uint playerNetId)
    {
        if (!NetworkClient.spawned.TryGetValue(playerNetId, out NetworkIdentity identity))
        {
            return;
        }

        if (!identity.TryGetComponent(out PlayerNetworkObject player))
        {
            return;
        }

        GameplayPlayer gameplayPlayer = player.Sandbox.GameplayPlayer;

        if (gameplayPlayer == null)
        {
            return;
        }

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        transform.SetParent(gameplayPlayer.BarrelHoldPoint, false);

        transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

  
}
