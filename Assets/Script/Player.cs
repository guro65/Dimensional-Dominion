using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    private List<GameObject> deck = new List<GameObject>();
    public List<GameObject> deckNaTela = new List<GameObject>();
    public Baralho baralho;
    public int limite = 5;
    public GameObject localDeck;
    public bool minhaVez;
    public float offsetX;
    public float tempo;
    public string tipo;

    // Start is called before the first frame update
    void Start()
    {
        baralho = GameObject.Find("Baralho").GetComponent<Baralho>();
        deck = baralho.DeckInicial(limite, tipo);
        StartCoroutine("ColocarCartasNaMesa");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ColocarCartasNaMesa()
    {
        Vector3 posCarta = localDeck.transform.position;
        Vector3 offset = new Vector3(offsetX,0,0);
        foreach(GameObject carta in deck)
        {
            Instantiate(carta, posCarta += offset, Quaternion.identity);
            yield return new WaitForSeconds(tempo);
        }

        yield return new WaitForSeconds(1.0f);

        PegaCartasNaTela();
    }

    private void PegaCartasNaTela()
    {
        GameObject[] cartasNoJogo = new GameObject[limite];
        
        if(tipo == "Player")
        {
            cartasNoJogo = GameObject.FindGameObjectsWithTag("Carta Player");
        }
        else if(tipo == "Oponente")
        {
            cartasNoJogo = GameObject.FindGameObjectsWithTag("Carta Oponente");
        }

        foreach(GameObject carta in cartasNoJogo) 
        {
            deckNaTela.Add(carta);
        }
        
    }

    public void MinhaVez(bool vez)
    {
        minhaVez = vez;

        if(vez)
        {
            AtivaDeck();
        }
        else if(!vez)
        {
            DesativaDeck();
        }
    }

    private void AtivaDeck()
    {
        foreach(GameObject carta in deckNaTela)
        {
            carta.gameObject.GetComponent<Carta>().AtivaCarta();
        }
    }

    private void DesativaDeck()
    {
        foreach(GameObject carta in deckNaTela)
        {
            carta.gameObject.GetComponent<Carta>().DesativaCarta();
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
