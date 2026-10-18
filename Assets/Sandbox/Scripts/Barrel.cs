using Mirror;
using System.Collections;
using UnityEngine;

public class Barrel : NetworkInteractable
{
    [Header("References")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private GameObject explosionVFXPrefab;
    [SerializeField] private LayerMask playerLayer;

    [Header("Throwing")]
    [SerializeField] private float throwForce = 15f;
    [SerializeField] private float throwerIgnoreDuration = 0.3f;

    [Header("Explosion")]
    [SerializeField] private float explosionRadius = 4f;
    [SerializeField] private float explosionForce = 10f;
    [SerializeField] private int maxExplosionHits = 32;
    [SerializeField] private float explosionVFXLifetime = 2f;

    private Collider[] explosionHits;
    private Renderer[] barrelRenderers;

    private uint lastThrowerNetId;
    private double ignoreThrowerUntil;

    [SyncVar(hook = nameof(OnHeldByChanged))]
    private uint heldByNetId;

    private bool hasBeenThrown;
    private bool isHeld;

    private void Awake()
    {
        explosionHits = new Collider[maxExplosionHits];
        barrelRenderers = GetComponentsInChildren<Renderer>();
    }

    [Server]
    public override void Interact(PlayerNetworkObject player)
    {
        PickUp(player);
    }

    [Server]
    public override void Throw(Vector3 direction)
    {
        lastThrowerNetId = heldByNetId;
        ignoreThrowerUntil = NetworkTime.time + throwerIgnoreDuration;

        heldByNetId = 0;
        isHeld = false;

        transform.SetParent(null, true);

        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearVelocity = direction * throwForce;

        hasBeenThrown = true;
    }

    [ServerCallback]
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasBeenThrown || ShouldIgnoreThrower(collision.collider))
            return;

        Vector3 impactPoint = collision.GetContact(0).point;
        Explode(impactPoint);
    }

    [Server]
    private bool ShouldIgnoreThrower(Collider other)
    {
        if (NetworkTime.time >= ignoreThrowerUntil ||
            lastThrowerNetId == 0)
        {
            return false;
        }

        GameplayPlayer gameplayPlayer =
            other.GetComponentInParent<GameplayPlayer>();

        return gameplayPlayer != null &&
               gameplayPlayer.Player != null &&
               gameplayPlayer.Player.netId == lastThrowerNetId;
    }

    [Server]
    private void Explode(Vector3 position)
    {
        ApplyExplosionForce(position);

        RpcPlayExplosionVFX(position);

        // Hide the server's barrel immediately.
        HideBarrel();

        hasBeenThrown = false;

        StartCoroutine(DestroyBarrelAfterVFX());
    }

    [Server]
    private void ApplyExplosionForce(Vector3 position)
    {
        int hitCount = Physics.OverlapSphereNonAlloc(
            position,
            explosionRadius,
            explosionHits,
            playerLayer
        );

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = explosionHits[i];

            if (hit == null)
                continue;

            GameplayPlayer gameplayPlayer = hit.GetComponentInParent<GameplayPlayer>();

            if (gameplayPlayer == null || gameplayPlayer.Player == null)
            {
                explosionHits[i] = null;
                continue;
            }

            Vector3 direction = gameplayPlayer.transform.position - position;

            direction.y = 0f;

            if (direction.sqrMagnitude < 0.001f)
                direction = transform.forward;

            direction =(direction.normalized + Vector3.up * 0.5f).normalized;

            gameplayPlayer.Player.Sandbox.TargetApplyExplosionForce(
                gameplayPlayer.Player.connectionToClient,
                direction,
                explosionForce
            );

            explosionHits[i] = null;
        }
    }

    public override bool CanBePickedUp()
    {
        return !isHeld && !hasBeenThrown;
    }

    [Server]
    public void SetSpawnedState()
    {
        isHeld = false;
        hasBeenThrown = false;
        heldByNetId = 0;

        rb.isKinematic = false;
        rb.useGravity = true;

        ShowBarrel();
    }

    [Server]
    public override void PickUp(PlayerNetworkObject player)
    {
        if (player == null || player.Sandbox.GameplayPlayer == null)
            return;

        heldByNetId = player.netId;
        isHeld = true;

        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        AttachToPlayer(player.Sandbox.GameplayPlayer);
    }

    private void OnHeldByChanged(uint oldNetId, uint newNetId)
    {
        if (newNetId == 0)
        {
            transform.SetParent(null, true);
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
            return;

        AttachToPlayer(gameplayPlayer);
    }

    private void AttachToPlayer(GameplayPlayer gameplayPlayer)
    {
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.None;

        transform.SetParent(
            gameplayPlayer.BarrelHoldPoint,
            false
        );

        transform.SetLocalPositionAndRotation(
            Vector3.zero,
            Quaternion.identity
        );
    }

    private void HideBarrel()
    {
        foreach (Renderer rend in barrelRenderers)
        {
            rend.enabled = false;
        }
    }

    private void ShowBarrel()
    {
        foreach (Renderer rend in barrelRenderers)
        {
            rend.enabled = true;
        }
    }

    [Server]
    private IEnumerator DestroyBarrelAfterVFX()
    {
        yield return new WaitForSeconds(explosionVFXLifetime);

        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    private void RpcPlayExplosionVFX(Vector3 position)
    {        
        HideBarrel();

        if (explosionVFXPrefab == null)
        {
            Debug.LogError("[Barrel] VFX prefab is not assigned!");
            return;
        }

        GameObject explosionVFX = Instantiate(
            explosionVFXPrefab,
            position,
            Quaternion.identity
        );

        Destroy(explosionVFX, explosionVFXLifetime);
    }
}
