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
    /// Instancia e adiciona um token ao inventário do jogador, se houver espaço.
    /// </summary>
    /// <returns>Retorna o novo Token instanciado se adicionou com sucesso, null se o inventário está cheio.</returns>
    public Token AdicionarToken(Token tokenPrefab)
    {
        if (tokensJogador.Count >= limiteEspaco)
        {
            Debug.Log("Inventário cheio! Não é possível adicionar mais tokens.");
            return null;
        }
        Token novoToken = Instantiate(tokenPrefab);
        tokensJogador.Add(novoToken);
        return novoToken;
    }

    /// <summary>
    /// Retorna a lista dos tokens do jogador.
    /// </summary>
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