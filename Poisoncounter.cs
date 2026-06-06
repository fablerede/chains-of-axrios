/// <summary>
/// Represents a single Poison Counter applied to an enemy.
/// Stacks build up from Toxic Strike damage. Counter decays 1% per second.
/// Falls off when it drops to 50% of its trigger value.
/// Deals 1 damage per second while active.
/// </summary>
public class PoisonCounter
{
    public float Stacks { get; private set; }
    public float TriggerValue { get; private set; }
    public bool IsActive { get; private set; }

    private float fallOffThreshold;

    public bool TryTrigger(float stacks, float staminaSave)
    {
        Stacks = stacks;
        if (stacks >= staminaSave)
        {
            TriggerValue = stacks;
            fallOffThreshold = TriggerValue * 0.5f;
            IsActive = true;
            return true;
        }
        return false;
    }

    /// <summary>Call once per second. Returns false if counter has expired.</summary>
    public bool Tick()
    {
        if (!IsActive) return false;
        Stacks -= TriggerValue * 0.01f;
        if (Stacks <= fallOffThreshold)
        {
            IsActive = false;
            return false;
        }
        return true;
    }
}