using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    public static Inventario instance;

    [Header("Tokens do jogador")]
    public List<Token> tokensJogador = new List<Token>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre cenas
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AdicionarToken(Token token)
    {
        tokensJogador.Add(token);
        Debug.Log($"Token adicionado ao inventário: {token.nomeDoToken}");
    }

    public List<Token> ObterTokens()
    {
        return tokensJogador;
    }

    public void LimparInventario()
    {
        tokensJogador.Clear();
    }
}