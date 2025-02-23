using UnityEngine;

public abstract class ItemEffectObject : ScriptableObject
{
    public abstract void ApplyEffect();
}

[CreateAssetMenu(fileName = "AddScoreEffect", menuName = "Item Effects/Add Score effect")]
public class AddScoreEffect : ItemEffectObject
{
    public int scoreToAdd = 100;
    public override void ApplyEffect()
    {
       
    }
}


