using UnityEngine;

[CreateAssetMenu(fileName = "RevealSpecificLetterEffect", menuName = "Item Effects/Reveal specific Letter effect")]
public class RevealSpecificLetterEffect : ItemEffectObject
{
    //public char letterToReveal = 'a';

    public char GetRandomLetter()
    {
        return (char)Random.Range(65, 90);
    }
    public override void ApplyEffect()
    {
        Debug.Log("RevealSpecificLetterEffect");
        EffectManager.GetInstance().ApplySpecificLetterRevealEffect(GetRandomLetter());
    }
}