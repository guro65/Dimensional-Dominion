using System.Collections.Generic;
using UnityEngine;

public class RarityManager : MonoBehaviour
{
    [System.Serializable]
    public struct Rarity
    {
        public string name;
        public float percentage;
        public List<GameObject> cards;
    }

    public List<Rarity> rarities;

    public GameObject GetCardByRarity()
    {
        float randomValue = Random.value;
        float cumulativePercentage = 0;

        foreach (Rarity rarity in rarities)
        {
            cumulativePercentage += rarity.percentage;
            if (randomValue < cumulativePercentage)
            {
                if (rarity.cards.Count > 0)
                {
                    return rarity.cards[Random.Range(0, rarity.cards.Count)];
                }
                else
                {
                    Debug.LogWarning("A raridade " + rarity.name + " não possui cartas atribuídas.");
                    return null;
                }
            }
        }

        return null;
    }
}