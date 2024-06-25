using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Baralho : MonoBehaviour
{
    public List<GameObject> cartas = new List<GameObject>();
    public Player player;
    public Player oponente;
    public float offsetX;
    public float tempo;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<Player>();
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
            Vector3 posCarta = player.LocalDeck();
            Vector3 offset = new Vector3(offsetX,0,0);
            Instantiate(cartas[Random.Range(0, cartas.Count)], posCarta += offset, Quaternion.identity);
            deck.Add();
        }

        return deck;
    }

    IEnumerator ColocarCartasNaMesa()
    {
        Vector3 posCarta = player.LocalDeck();
        Vector3 offset = new Vector3(offsetX,0,0);
        foreach(GameObject carta in deck)
        {
            Instantiate(carta, posCarta += offset, Quaternion.identity);
            yield return new WaitForSeconds(tempo);
        }
    }
}
