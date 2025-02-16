using UnityEngine;

public abstract class ItemEffectObject : ScriptableObject
{
    public abstract void ApplyEffect();
}

[CreateAssetMenu(fileName = "AddTimeEffect", menuName = "Item Effects/Add Time effect")]
public class AddTimeEffect : ItemEffectObject
{
    public float timeToAdd = 30f;
    public override void ApplyEffect()
    {
        EffectManager.GetInstance().ApplyTimerEffect(timeToAdd);
    }
}

[CreateAssetMenu(fileName = "AddScoreEffect", menuName = "Item Effects/Add Score effect")]
public class AddScoreEffect : ItemEffectObject
{
    public int scoreToAdd = 100;
    public override void ApplyEffect()
    {
       
    }
}
