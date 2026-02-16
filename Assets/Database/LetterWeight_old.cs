using System.Collections.Generic;
using UnityEngine;

public class LetterWeight_old : MonoBehaviour
{
    public static Dictionary<char, int> Weights = new Dictionary<char, int>
    {
        { 'A', 1 },
        { 'B', 3 },
        { 'C', 2 },
        { 'D', 2 },
        { 'E', 1 },
        { 'F', 4 },
        { 'G', 3 },
        { 'H', 4 },
        { 'I', 1 },
        { 'J', 8 },
        { 'K', 5 },
        { 'L', 2 },
        { 'M', 3 },
        { 'N', 1 },
        { 'O', 1 },
        { 'P', 3 },
        { 'Q', 8 },
        { 'R', 1 },
        { 'S', 1 },
        { 'T', 1 },
        { 'U', 2 },
        { 'V', 4 },
        { 'W', 10 },
        { 'X', 10 },
        { 'Y', 5 },
        { 'Z', 10 }
    };

    public static int GetLetterWeight(char letter)
    {
        letter = char.ToUpper(letter);
        if (Weights.TryGetValue(letter, out int weight))
            return weight;
        return 1; // Valeur par défaut
    }
}
