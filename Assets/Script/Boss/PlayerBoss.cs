using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBoss : MonoBehaviour
{
    public List<GameObject> deckNaTela = new List<GameObject>();
    public BaralhoBoss baralho;
    public GameObject localDeck;
    public string tipo;
    public int cartasPlayer = 5; 
    public int cartasOponente = 5; 

    void Start()
    {
        baralho = GameObject.Find("Baralho").GetComponent<BaralhoBoss>();
        
        if (tipo == "Player")
        {
            baralho.DeckInicialPlayer();
        }
        else if (tipo == "Oponente")
        {
            baralho.DeckInicialOponente();
        }

        StartCoroutine("ColocarCartasNaMesa");
    }

    IEnumerator ColocarCartasNaMesa()
    {
        Vector3 posCarta = localDeck.transform.position;
        Vector3 offset = new Vector3(0.5f, 0, 0);
        foreach (GameObject carta in (tipo == "Player" ? baralho.deckPlayer : baralho.deckOponente))
        {
            Instantiate(carta, posCarta += offset, Quaternion.identity);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(1.0f);
        PegaCartasNaTela();
    }

    private void PegaCartasNaTela()
    {
        GameObject[] cartasNoJogo = GameObject.FindGameObjectsWithTag(tipo == "Player" ? "Carta Player" : "Carta Oponente");
        foreach (GameObject carta in cartasNoJogo)
        {
            deckNaTela.Add(carta);
        }
    }

    public Vector3 LocalDeck()
    {
        return localDeck.transform.position;
    }

    public List<GameObject> DeckNaTela()
    {
        return deckNaTela;
    }
}
