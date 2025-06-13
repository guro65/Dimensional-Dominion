using System.Collections.Generic;
using UnityEngine;

public class Inventario : MonoBehaviour
{
    public static Inventario instance;

    [Header("Configurações de Inventário")]
    public int limiteEspaco = 10; // Defina o limite de espaço no Inspector

    private List<Token> tokensJogador = new List<Token>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Adiciona um token ao inventário do jogador, se houver espaço.
    /// </summary>
    /// <returns>Retorna true se adicionou com sucesso, false se o inventário está cheio.</returns>
    public bool AdicionarToken(Token token)
    {
        if (tokensJogador.Count >= limiteEspaco)
        {
            Debug.Log("Inventário cheio! Não é possível adicionar mais tokens.");
            return false;
        }
        tokensJogador.Add(token);
        return true;
    }

    public List<Token> ObterTokens()
    {
        return tokensJogador;
    }

    /// <summary>
    /// Verifica se há espaço suficiente para adicionar uma quantidade de tokens.
    /// </summary>
    public bool TemEspacoParaAdicionar(int quantidade)
    {
        return (tokensJogador.Count + quantidade) <= limiteEspaco;
    }

    /// <summary>
    /// Verifica se o inventário está cheio.
    /// </summary>
    public bool EstaCheio()
    {
        return tokensJogador.Count >= limiteEspaco;
    }
}