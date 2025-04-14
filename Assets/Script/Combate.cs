using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Combate : MonoBehaviour
{
    private Mana manaScript;

    [Header("Prefabs de Tokens Disponíveis")]
    public List<GameObject> todosOsTokens;

    [Header("Locais de Spawn")]
    public Transform locaisPlayerPai;
    public Transform locaisOponentePai;

    [Header("Locais Especiais")]
    public Transform localDuelo;
    public Transform dueloPlayer;
    public Transform dueloOponente;

    [Header("Canvas UI")]
    public GameObject painelDeAcoes;
    public GameObject painelDeDetalhes;
    public Button botaoJogar;
    public Button botaoDetalhes;
    public Button botaoCancelar;

    public TextMeshProUGUI textoDano;
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoRaridade;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoMana;
    public Button botaoFecharDetalhes;
    public Image imagemToken;

    private List<Transform> spawnsPlayer = new List<Transform>();
    private List<Transform> spawnsOponente = new List<Transform>();

    private GameObject tokenSelecionado;


    [Header("Quantidade de Tokens")]
    public int quantidadeParaCadaLado = 5;

    void Start()
    {
        manaScript = FindObjectOfType<Mana>();

        foreach (Transform child in locaisPlayerPai)
            spawnsPlayer.Add(child);
        foreach (Transform child in locaisOponentePai)
            spawnsOponente.Add(child);

        GerarTokens(spawnsPlayer, "Token Player");
        GerarTokens(spawnsOponente, "Token Oponente");

        painelDeAcoes.SetActive(false);
        painelDeDetalhes.SetActive(false);

        botaoJogar.onClick.AddListener(JogarToken);
        botaoDetalhes.onClick.AddListener(MostrarDetalhes);
        botaoCancelar.onClick.AddListener(CancelarSelecao);
    }

    void GerarTokens(List<Transform> pontosDeSpawn, string tagDoDono)
    {
        for (int i = 0; i < Mathf.Min(quantidadeParaCadaLado, pontosDeSpawn.Count); i++)
        {
            GameObject tokenPrefab = EscolherTokenPorChance();
            if (tokenPrefab != null)
            {
                GameObject tokenInstanciado = Instantiate(tokenPrefab, pontosDeSpawn[i].position, Quaternion.identity);
                tokenInstanciado.tag = tagDoDono;

                tokenInstanciado.AddComponent<BoxCollider2D>();

                Token tokenScript = tokenInstanciado.GetComponent<Token>();
                if (tokenScript != null)
                {
                    tokenScript.gameObject.tag = tagDoDono;
                }
            }
        }
    }

    GameObject EscolherTokenPorChance()
    {
        float totalChance = 0f;
        foreach (GameObject token in todosOsTokens)
        {
            Token dados = token.GetComponent<Token>();
            totalChance += dados.chanceDeAparicao;
        }

        float randomValue = Random.Range(0f, totalChance);
        float acumulado = 0f;

        foreach (GameObject token in todosOsTokens)
        {
            Token dados = token.GetComponent<Token>();
            acumulado += dados.chanceDeAparicao;
            if (randomValue <= acumulado)
                return token;
        }

        return null;
    }

    public void SelecionarToken(GameObject token)
    {
        if (tokenSelecionado != null) return;

        // Impede selecionar tokens do oponente
        if (token.CompareTag("Token Oponente")) return;

        tokenSelecionado = token;
        painelDeAcoes.SetActive(true);
    }


    void JogarToken()
    {
        if (tokenSelecionado != null)
        {
            Token dados = tokenSelecionado.GetComponent<Token>();
            int custo = dados.mana;

            bool podeJogar = false;

            if (tokenSelecionado.CompareTag("Token Player"))
            {
                podeJogar = manaScript.GastarManaPlayer(custo);
                if (podeJogar)
                    tokenSelecionado.transform.position = dueloPlayer.position;
            }

            if (podeJogar)
            {
                painelDeAcoes.SetActive(false);
                tokenSelecionado = null;

                // Chama a IA após o jogador jogar
                Invoke("JogadaDoOponente", 1.0f); // pequeno delay para parecer mais natural
            }
            else
            {
                Debug.Log("Mana insuficiente para jogar o token.");
            }
        }
    }
    void JogadaDoOponente()
    {
        GameObject[] tokensOponente = GameObject.FindGameObjectsWithTag("Token Oponente");

        foreach (GameObject token in tokensOponente)
        {
            Token dados = token.GetComponent<Token>();
            if (token.transform.position != dueloOponente.position) // não jogar o mesmo token mais de uma vez
            {
                bool podeJogar = manaScript.GastarManaOponente(dados.mana);
                if (podeJogar)
                {
                    token.transform.position = dueloOponente.position;
                    Debug.Log($"Oponente jogou o token: {dados.nomeDoToken}");
                    break;
                }
            }
        }
    }

    void MostrarDetalhes()
    {
        if (tokenSelecionado != null)
        {
            Token dados = tokenSelecionado.GetComponent<Token>();

            textoNome.text = dados.nomeDoToken;
            textoDano.text = $"Dano: {dados.dano}";
            textoVida.text = $"Vida: {dados.vida}";
            textoMana.text = $"Mana: {dados.mana}";
            textoRaridade.text = $"Raridade: {dados.raridade}";

            SpriteRenderer spriteRenderer = tokenSelecionado.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
                imagemToken.sprite = spriteRenderer.sprite;

            painelDeDetalhes.SetActive(true);

            botaoFecharDetalhes.onClick.RemoveAllListeners();
            botaoFecharDetalhes.onClick.AddListener(FecharDetalhes);
        }
    }

    public void FecharDetalhes()
    {
        painelDeDetalhes.SetActive(false);
    }

    void CancelarSelecao()
    {
        painelDeAcoes.SetActive(false);
        painelDeDetalhes.SetActive(false);
        tokenSelecionado = null;
    }
}
