using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baralho : MonoBehaviour
{
    public List<GameObject> cartas = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<GameObject> DeckInicial(int limite)
    {
        List<GameObject> deck = new List<GameObject>();
        for(int i = 0; i < limite; i++)
        {
            deck.Add(cartas[Random.Range(0, cartas.Count)]);
        }

        return deck;
    }
}
