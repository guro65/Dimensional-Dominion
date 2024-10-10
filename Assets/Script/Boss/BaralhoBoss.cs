using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaralhoBoss : MonoBehaviour
{
    public List<GameObject> cartas = new List<GameObject>();
    public List<GameObject> deckPlayer = new List<GameObject>();
    public List<GameObject> deckOponente = new List<GameObject>();
    public List<GameObject> cartasOponente = new List<GameObject>();
    public PlayerBoss player;
    public PlayerBoss oponente;
    public float offsetX;
    public int limitePlayer = 5; 
    public int limiteOponente = 5; 

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBoss>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<PlayerBoss>();
        DeckInicialPlayer();
        DeckInicialOponente();
    }

    public void DeckInicialPlayer()
    {
        Vector3 posCarta = player.LocalDeck();
        Vector3 offset = new Vector3(offsetX, 0, 0);
        for (int i = 0; i < limitePlayer; i++)
        {
            GameObject cartaSorteada = cartas[Random.Range(0, cartas.Count)];
            cartaSorteada.gameObject.tag = "Carta Player";
            Instantiate(cartaSorteada, posCarta += offset, Quaternion.identity);
        }
    }

    public void DeckInicialOponente()
    {
        Vector3 posCarta = oponente.LocalDeck();
        Vector3 offset = new Vector3(offsetX, 0, 0);
        for (int i = 0; i < limiteOponente; i++)
        {
            GameObject cartaSorteada = cartasOponente[Random.Range(0, cartasOponente.Count)];
            cartaSorteada.gameObject.tag = "Carta Oponente";
            Instantiate(cartaSorteada, posCarta += offset, Quaternion.identity);
        }
    }
}
