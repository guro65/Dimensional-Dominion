using System.Collections.Generic;
using UnityEngine;

public class Baralho : MonoBehaviour
{
    public List<GameObject> cartas = new List<GameObject>(); // Lista de cartas disponíveis
    public List<Token> tokens = new List<Token>(); // Lista de tokens disponíveis
    public List<GameObject> deckPlayer = new List<GameObject>(); // Deck do jogador
    public List<GameObject> deckOponente = new List<GameObject>(); // Deck do oponente
    public Player player; // Referência ao jogador
    public Player oponente; // Referência ao oponente
    public float offsetX; // Offset para posicionar cartas
    public GameObject cartaSorteada; // Carta sorteada
    public int limitePlayer = 5; // Limite de cartas do player
    public int limiteOponente = 5; // Limite de cartas do oponente
    public RarityManager rarityManager;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<Player>();
        rarityManager = GetComponent<RarityManager>();
        DeckInicialOponente();
        DeckInicialPlayer();

    }

    public List<GameObject> DeckInicial(int limite, string tipo)
    {
        if (tipo == "Player")
        {
            return deckPlayer;
        }
        else if (tipo == "Oponente")
        {
            return deckOponente;
        }

        return null;
    }

    public void DeckInicialPlayer()
    {
        Vector3 posCarta = player.LocalDeck();
        Vector3 offset = new Vector3(offsetX, 0, 0);
        for (int i = 0; i < limitePlayer; i++)
        {
            cartaSorteada = rarityManager.GetCardByRarity(); // Obtém a carta pela raridade
            if (cartaSorteada != null)
            {
                cartaSorteada.tag = "Carta Player";
                Instantiate(cartaSorteada, posCarta += offset, Quaternion.identity);
            }
        }
    }

    public void DeckInicialOponente()
    {
        Vector3 posCarta = oponente.LocalDeck();
        Vector3 offset = new Vector3(offsetX, 0, 0);
        for (int i = 0; i < limiteOponente; i++)
        {
            cartaSorteada = rarityManager.GetCardByRarity(); // Obtém a carta pela raridade
            if (cartaSorteada != null)
            {
                cartaSorteada.tag = "Carta Oponente";
                Instantiate(cartaSorteada, posCarta += offset, Quaternion.identity);
            }
        }
    }

    public void AdicionarTokenAoJogador()
    {
        if (tokens.Count > 0)
        {
            Token tokenSorteado = tokens[Random.Range(0, tokens.Count)];
            player.AddToken(tokenSorteado); // Adiciona token ao jogador
        }
    }
}