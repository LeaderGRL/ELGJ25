using TMPro;
using UnityEngine;

public class CoinView : MonoBehaviour
{
    public TextMeshProUGUI coinText;

    public void UpdateCoin(int coin)
    {
        coinText.text = coin.ToString();
    }
}
