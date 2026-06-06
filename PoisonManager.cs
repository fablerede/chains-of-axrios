using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Attach to any enemy GameObject. Manages all active Poison Counters.
/// Called by ToxicStrikeAbility via the target's NetworkEntityStub.
/// </summary>
public class PoisonManager : MonoBehaviour
{
    private List<PoisonCounter> activeCounters = new List<PoisonCounter>();
    private ParticleSystem poisonParticle;
    private EnemyEntity enemy;

    private const float TICK_INTERVAL = 1f;
    private bool ticking = false;

    private void Awake()
    {
        enemy = GetComponent<EnemyEntity>();
        BuildParticleEffect();
    }

    public void AddStacks(float stacks, float staminaSave)
    {
        var counter = new PoisonCounter();
        bool triggered = counter.TryTrigger(stacks, staminaSave);

        if (triggered)
        {
            activeCounters.Add(counter);
            PlayHitEffect();
            ChatWindowUI.Spell($"{enemy.EnemyName} is poisoned! ({stacks:F0} stacks vs {staminaSave:F0} save)");
            if (!ticking)
                StartCoroutine(PoisonTick());
        }
        else
        {
            ChatWindowUI.Combat($"{enemy.EnemyName} resisted the poison. ({stacks:F0} / {staminaSave:F0} needed)");
        }
    }

    private IEnumerator PoisonTick()
    {
        ticking = true;
        while (activeCounters.Count > 0)
        {
            yield return new WaitForSeconds(TICK_INTERVAL);
            if (enemy == null || enemy.IsDead) { activeCounters.Clear(); break; }

            for (int i = activeCounters.Count - 1; i >= 0; i--)
            {
                enemy.TakeDamage(1f);
                bool stillActive = activeCounters[i].Tick();
                if (!stillActive)
                {
                    activeCounters.RemoveAt(i);
                    ChatWindowUI.Spell($"A poison counter on {enemy.EnemyName} fades.");
                }
            }
        }
        ticking = false;
    }

    private void BuildParticleEffect()
    {
        var go = new GameObject("PoisonHitFX");
        go.transform.SetParent(transform, false);
        go.transform.localPosition = Vector3.up * 1f;

        poisonParticle = go.AddComponent<ParticleSystem>();

        // Stop immediately before configuring — prevents "duration while playing" error
        poisonParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = poisonParticle.main;
        main.duration = 0.6f;
        main.loop = false;
        main.startLifetime = 0.5f;
        main.startSpeed = 2f;
        main.startSize = 0.15f;
        main.startColor = new Color(0.2f, 0.9f, 0.2f, 0.85f);
        main.maxParticles = 30;
        main.playOnAwake = false;

        var emission = poisonParticle.emission;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 20) });

        var shape = poisonParticle.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;

        var rend = go.GetComponent<ParticleSystemRenderer>();
        rend.material = new Material(Shader.Find("Particles/Standard Unlit"));
        rend.material.color = new Color(0.2f, 0.9f, 0.2f, 1f);
    }

    private void PlayHitEffect()
    {
        if (poisonParticle != null)
        {
            poisonParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            poisonParticle.Play();
        }
    }

    public int ActiveCounterCount => activeCounters.Count;
}