using UnityEngine;

[CreateAssetMenu(fileName = "AddHealthEffect", menuName = "Item Effects/Add Health effect")]
public class AddHealthEffect : ItemEffectObject
{
    public int healthToAdd = 1;
    public override void ApplyEffect()
    {
        Debug.Log("Adding health: " + healthToAdd); 
        EffectManager.GetInstance().healthController.AddHealth(healthToAdd);
    }
}