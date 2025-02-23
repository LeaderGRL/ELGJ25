using UnityEngine;

[CreateAssetMenu(fileName = "AddTimeEffect", menuName = "Item Effects/Add Time effect")]
public class AddTimeEffect : ItemEffectObject
{
    public float timeToAdd = 30f;
    public override void ApplyEffect()
    {
        Debug.Log("Adding time: " + timeToAdd);
        EffectManager.GetInstance().ApplyTimerEffect(timeToAdd);
    }
}