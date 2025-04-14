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
    public TextMeshProUGUI textoManaCusto; // Mantendo o nome para a TextMeshPro
    public Button botaoFecharDetalhes;
    public Image imagemToken;

    private List<Transform> spawnsPlayer = new List<Transform>();
    private List<Transform> spawnsOponente = new List<Transform>();

    private GameObject tokenSelecionado;

    [Header("Quantidade de Tokens")]
    public int quantidadeParaCadaLado = 5;

    [Header("Painel de Combate")]
    public GameObject painelCombate;
    public Button botaoAtacar;
    public Button botaoStatus;

    private GameObject tokenPlayerEmDuelo;
    private GameObject tokenOponenteEmDuelo;
    private bool tokenPlayerAtacou = false;

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
        painelCombate.SetActive(false);

        botaoJogar.onClick.AddListener(JogarToken);
        botaoDetalhes.onClick.AddListener(MostrarDetalhes);
        botaoCancelar.onClick.AddListener(CancelarSelecao);
    }

    void Update()
    {
        if (tokenPlayerEmDuelo == null)
        {
            foreach (GameObject token in GameObject.FindGameObjectsWithTag("Token Player"))
            {
                if (token.transform.position == dueloPlayer.position)
                    tokenPlayerEmDuelo = token;
            }
        }

        if (tokenOponenteEmDuelo == null)
        {
            foreach (GameObject token in GameObject.FindGameObjectsWithTag("Token Oponente"))
            {
                if (token.transform.position == dueloOponente.position)
                    tokenOponenteEmDuelo = token;
            }
        }

        if (tokenPlayerEmDuelo != null && tokenOponenteEmDuelo != null)
        {
            painelCombate.SetActive(true);

            botaoAtacar.onClick.RemoveAllListeners();
            botaoAtacar.onClick.AddListener(() => AtacarToken());

            botaoStatus.onClick.RemoveAllListeners();
            botaoStatus.onClick.AddListener(() => MostrarStatusDuelo(tokenOponenteEmDuelo));
        }
        else
        {
            painelCombate.SetActive(false);
        }
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
        if (token.CompareTag("Token Oponente")) return;
        if (tokenPlayerEmDuelo != null) return; // Impede jogar outro token se já tiver um em duelo

        tokenSelecionado = token;
        painelDeAcoes.SetActive(true);
    }

    void JogarToken()
    {
        if (tokenSelecionado != null)
        {
            Token dados = tokenSelecionado.GetComponent<Token>();
            int custo = dados.manaCusto; // Usando o nome correto da variável
            bool podeJogar = false;

            if (tokenSelecionado.CompareTag("Token Player"))
            {
                podeJogar = manaScript.GastarManaPlayer(custo);
                if (podeJogar)
                {
                    tokenSelecionado.transform.position = dueloPlayer.position;
                    tokenPlayerEmDuelo = tokenSelecionado;
                }
            }

            if (podeJogar)
            {
                painelDeAcoes.SetActive(false);
                tokenSelecionado = null;
                Invoke("JogadaDoOponente", 1.0f);
            }
            else
            {
                Debug.Log("Mana insuficiente para jogar o token.");
            }
        }
    }

    void JogadaDoOponente()
    {
        if (tokenOponenteEmDuelo != null) return;

        GameObject[] tokensOponente = GameObject.FindGameObjectsWithTag("Token Oponente");

        // Ordenar os tokens do oponente por custo de mana (opcional, mas pode ser útil para IA)
        List<GameObject> listaTokensOponente = new List<GameObject>(tokensOponente);
        listaTokensOponente.Sort((a, b) => a.GetComponent<Token>().manaCusto.CompareTo(b.GetComponent<Token>().manaCusto));

        foreach (GameObject token in listaTokensOponente)
        {
            if (token.transform.position == dueloOponente.position)
                continue;

            Token dados = token.GetComponent<Token>();
            bool podeJogar = manaScript.GastarManaOponente(dados.manaCusto); // Usando o nome correto da variável
            if (podeJogar)
            {
                token.transform.position = dueloOponente.position;
                tokenOponenteEmDuelo = token;
                Debug.Log($"Oponente jogou o token: {dados.nomeDoToken} (Custo: {dados.manaCusto})");
                break;
            }
        }
    }

    void MostrarDetalhes()
    {
        GameObject tokenEmDuelo = null;

        // Verifica se há um token do player no local de duelo
        Collider2D[] coliders = Physics2D.OverlapCircleAll(dueloPlayer.position, 0.1f);
        foreach (Collider2D col in coliders)
        {
            if (col != null && col.CompareTag("Token Player"))
            {
                tokenEmDuelo = col.gameObject;
                break;
            }
        }

        GameObject tokenParaMostrar = null;

        if (tokenEmDuelo != null)
        {
            // Se houver um token do player no duelo, mostra ele
            tokenParaMostrar = tokenEmDuelo;
        }
        else if (tokenSelecionado != null && tokenSelecionado.CompareTag("Token Player"))
        {
            // Se não houver token em duelo, mas o jogador selecionou um token válido do player
            tokenParaMostrar = tokenSelecionado;
        }

        // Exibir os dados do token do player
        if (tokenParaMostrar != null)
        {
            Token dados = tokenParaMostrar.GetComponent<Token>();
            if (dados != null) // Adicionada verificação se o componente Token existe
            {
                textoNome.text = dados.nomeDoToken;
                textoDano.text = $"Dano: {dados.dano}";
                textoVida.text = $"Vida: {dados.vida}";
                textoManaCusto.text = $"Mana: {dados.manaCusto}"; // Correção aqui!
                textoRaridade.text = $"Raridade: {dados.raridade}";

                SpriteRenderer spriteRenderer = tokenParaMostrar.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    imagemToken.sprite = spriteRenderer.sprite;

                painelDeDetalhes.SetActive(true);

                botaoFecharDetalhes.onClick.RemoveAllListeners();
                botaoFecharDetalhes.onClick.AddListener(FecharDetalhes);
            }
            else
            {
                Debug.LogError("O token encontrado não possui o componente 'Token'.");
                painelDeDetalhes.SetActive(false); // Garante que o painel não fique aberto com dados inválidos
            }
        }
        else
        {
            Debug.Log("Nenhum token válido do player para mostrar os detalhes.");
            painelDeDetalhes.SetActive(false); // Garante que o painel seja fechado indevidamente
        }
    }

    void MostrarStatusDuelo(GameObject token)
    {
        Token dados = token.GetComponent<Token>();

        textoNome.text = dados.nomeDoToken;
        textoDano.text = $"Dano: {dados.dano}";
        textoVida.text = $"Vida: {dados.vida}";
        textoManaCusto.text = $"Mana: {dados.manaCusto}"; // Correção aqui!
        textoRaridade.text = $"Raridade: {dados.raridade}";

        SpriteRenderer spriteRenderer = token.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            imagemToken.sprite = spriteRenderer.sprite;

        painelDeDetalhes.SetActive(true);
        botaoFecharDetalhes.onClick.RemoveAllListeners();
        botaoFecharDetalhes.onClick.AddListener(FecharDetalhes);
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

    void AtacarToken()
    {
        if (tokenPlayerEmDuelo == null || tokenOponenteEmDuelo == null) return;

        Token tPlayer = tokenPlayerEmDuelo.GetComponent<Token>();
        Token tOponente = tokenOponenteEmDuelo.GetComponent<Token>();

        tOponente.ReceberDano(tPlayer.dano, tPlayer); // 'tPlayer' é o atacante
        Debug.Log($"Oponente perdeu {tPlayer.dano} de vida. Vida restante: {tOponente.vida}");

        tokenPlayerAtacou = true;

        if (!tOponente.estaVivo)
        {
            tokenOponenteEmDuelo = null;
            tokenPlayerAtacou = false;

            // Tentar jogar outro token do oponente
            Invoke("TentarNovoTokenOponente", 1.0f);
            return;
        }

        Invoke("ContraAtaqueOponente", 1.0f);
    }

    void ContraAtaqueOponente()
    {
        if (tokenPlayerAtacou && tokenPlayerEmDuelo != null && tokenOponenteEmDuelo != null)
        {
            Token tPlayer = tokenPlayerEmDuelo.GetComponent<Token>();
            Token tOponente = tokenOponenteEmDuelo.GetComponent<Token>();

            tPlayer.ReceberDano(tOponente.dano, tOponente); // 'tOponente' é o atacante
            Debug.Log($"Player perdeu {tOponente.dano} de vida. Vida restante: {tPlayer.vida}");

            if (!tPlayer.estaVivo)
            {
                tokenPlayerEmDuelo = null;
            }

            tokenPlayerAtacou = false;
        }
    }
    void TentarNovoTokenOponente()
    {
        if (tokenOponenteEmDuelo != null) return; // Já existe um token

        GameObject[] tokensOponente = GameObject.FindGameObjectsWithTag("Token Oponente");

        // Ordenar os tokens do oponente por custo de mana (opcional, mas pode ser útil para IA)
        List<GameObject> listaTokensOponente = new List<GameObject>(tokensOponente);
        listaTokensOponente.Sort((a, b) => a.GetComponent<Token>().manaCusto.CompareTo(b.GetComponent<Token>().manaCusto));

        foreach (GameObject token in listaTokensOponente)
        {
            if (token.transform.position == dueloOponente.position)
                continue;

            Token dados = token.GetComponent<Token>();
            bool podeJogar = manaScript.GastarManaOponente(dados.manaCusto); // Usando o nome correto da variável
            if (podeJogar)
            {
                token.transform.position = dueloOponente.position;
                tokenOponenteEmDuelo = token;
                Debug.Log($"Oponente jogou outro token: {dados.nomeDoToken} (Custo: {dados.manaCusto})");
                break;
            }
        }
    }
}