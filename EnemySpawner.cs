using UnityEngine;
using FishNet.Object;
using FishNet;
using System.Collections;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public NetworkObject enemyPrefab;
        public Transform spawnPoint;
        [Tooltip("Y offset from spawn point. Use negative values to spawn underground (e.g. Ice Nimbral).")]
        public float spawnYOffset = 0f;
        [HideInInspector] public NetworkObject liveInstance;
    }

    [Header("Spawn Entries")]
    [SerializeField] private SpawnEntry[] entries;

    [Header("Respawn")]
    [SerializeField] private float respawnDelay = 30f;

    private void Start()
    {
        if (InstanceFinder.IsServerStarted)
            SpawnAll();
        else
            InstanceFinder.ServerManager.OnServerConnectionState += OnServerStarted;
    }

    private void OnServerStarted(FishNet.Transporting.ServerConnectionStateArgs args)
    {
        if (args.ConnectionState == FishNet.Transporting.LocalConnectionState.Started)
            SpawnAll();
    }

    private void SpawnAll()
    {
        foreach (var entry in entries)
            SpawnSingleEntry(entry);
    }

    private void SpawnSingleEntry(SpawnEntry entry)
    {
        if (entry.enemyPrefab == null || entry.spawnPoint == null) return;

        Vector3 spawnPos = entry.spawnPoint.position + Vector3.up * entry.spawnYOffset;

        NetworkObject enemy = Instantiate(
            entry.enemyPrefab,
            spawnPos,
            entry.spawnPoint.rotation);

        InstanceFinder.ServerManager.Spawn(enemy);
        entry.liveInstance = enemy;

        StartCoroutine(WatchForDeath(entry));
    }

    private IEnumerator WatchForDeath(SpawnEntry entry)
    {
        EnemyEntity entity = null;
        yield return new WaitUntil(() =>
        {
            if (entry.liveInstance == null) return true;
            entity = entry.liveInstance.GetComponent<EnemyEntity>();
            return entity != null;
        });

        yield return new WaitUntil(() =>
            entry.liveInstance == null || (entity != null && entity.IsDead));

        entry.liveInstance = null;

        yield return new WaitForSeconds(respawnDelay);

        if (InstanceFinder.IsServerStarted)
            SpawnSingleEntry(entry);
    }

    private void OnDestroy()
    {
        if (InstanceFinder.ServerManager != null)
            InstanceFinder.ServerManager.OnServerConnectionState -= OnServerStarted;
    }
}