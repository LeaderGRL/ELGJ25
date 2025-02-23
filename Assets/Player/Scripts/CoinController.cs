using UnityEngine;

public class CoinController : MonoBehaviour
{
    public Player player;
    public CoinView coinView;

    private void OnEnable()
    {
        player.OnCoinChange += OnCoinChange;
    }

    private void OnDisable()
    {
        player.OnCoinChange -= OnCoinChange;
    }

    private void OnCoinChange(int coin)
    {
        coinView.UpdateCoin(coin);
    }

    public void AddCoins(int amount)
    {
        player.AddCoins(amount);
    }

    public void RemoveCoins(int amount)
    {
        player.RemoveCoins(amount);
    }

}
