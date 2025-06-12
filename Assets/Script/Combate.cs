using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Combate : MonoBehaviour
{
    private Mana manaScript;
    private Slots slotsScript;
    private Caixa caixaScript; // Referência ao script Caixa

    [Header("Prefabs de Tokens Disponíveis")]
    public List<GameObject> todosOsTokens;

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
    public TextMeshProUGUI textoManaCusto;
    public Button botaoFecharDetalhes;
    public Image imagemToken;

    private GameObject tokenSelecionado;

    [Header("Painel de Combate")]
    public GameObject painelCombate;
    public Button botaoAtacar;
    public Button botaoStatus;

    // Elementos para habilidade especial
    [Header("Especial")]
    public Button botaoEspecial; // Botão para usar habilidade especial
    public TextMeshProUGUI textoEspecialInfo; // Texto de info sobre custo e dano da habilidade

    private GameObject tokenPlayerEmDuelo;
    private GameObject tokenOponenteEmDuelo;
    private bool tokenPlayerAtacou = false;
    private float tempoParaCompraOponente = 5f; // Tempo entre as tentativas de compra do oponente
    private float contadorTempoCompraOponente = 0f;

    void Start()
    {
        manaScript = FindObjectOfType<Mana>();
        slotsScript = FindObjectOfType<Slots>();
        caixaScript = FindObjectOfType<Caixa>(); // Encontra o script Caixa

        if (slotsScript == null || manaScript == null || caixaScript == null)
        {
            Debug.LogError("Um dos scripts (Slots, Mana ou Caixa) não foi encontrado na cena.");
            enabled = false;
            return;
        }

        GerarTokensIniciais();

        painelDeAcoes.SetActive(false);
        painelDeDetalhes.SetActive(false);
        painelCombate.SetActive(false);

        botaoJogar.onClick.AddListener(JogarToken);
        botaoDetalhes.onClick.AddListener(MostrarDetalhes);
        botaoCancelar.onClick.AddListener(CancelarSelecao);

        // Inicializa o botão e texto do especial se existirem
        if (botaoEspecial != null)
            botaoEspecial.gameObject.SetActive(false);
        if (textoEspecialInfo != null)
            textoEspecialInfo.gameObject.SetActive(false);
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

            // HABILIDADE ESPECIAL - mostrar botão e configurar evento
            if (botaoEspecial != null && textoEspecialInfo != null && tokenPlayerEmDuelo != null)
            {
                Token tPlayer = tokenPlayerEmDuelo.GetComponent<Token>();
                botaoEspecial.gameObject.SetActive(true);
                textoEspecialInfo.gameObject.SetActive(true);
                textoEspecialInfo.text = $"Especial: Dano {tPlayer.danoEspecial}, Mana {tPlayer.custoManaEspecial}";
                botaoEspecial.onClick.RemoveAllListeners();
                botaoEspecial.onClick.AddListener(() => UsarHabilidadeEspecial());
                botaoEspecial.interactable = manaScript.manaPlayer >= tPlayer.custoManaEspecial;
            }
        }
        else
        {
            painelCombate.SetActive(false);

            // Esconde o botão e info do especial
            if (botaoEspecial != null)
                botaoEspecial.gameObject.SetActive(false);
            if (textoEspecialInfo != null)
                textoEspecialInfo.gameObject.SetActive(false);
        }

        // Lógica de compra automática do oponente
        contadorTempoCompraOponente += Time.deltaTime;
        if (contadorTempoCompraOponente >= tempoParaCompraOponente)
        {
            contadorTempoCompraOponente = 0f;
            if (caixaScript != null)
            {
                caixaScript.OponenteTentarComprarToken();
            }
        }
    }

    void GerarTokensIniciais()
    {
        int quantidadeParaCadaLado = Mathf.Min(5, slotsScript.playerSlots.Count); // Garante que não gere mais tokens que slots
        for (int i = 0; i < quantidadeParaCadaLado; i++)
        {
            GerarEColocarTokenInicial(true);
            GerarEColocarTokenInicial(false);
        }
    }

    void GerarEColocarTokenInicial(bool paraPlayer)
    {
        List<Transform> slots = paraPlayer ? slotsScript.playerSlots : slotsScript.oponenteSlots;
        string tagDoDono = paraPlayer ? "Token Player" : "Token Oponente";
        Transform slotVazio = null;

        // Tenta encontrar um slot vazio
        foreach (Transform slot in slots)
        {
            if (slot.childCount == 0)
            {
                slotVazio = slot;
                break;
            }
        }

        if (slotVazio != null)
        {
            GameObject tokenPrefab = EscolherTokenPorChance();
            if (tokenPrefab != null)
            {
                GameObject tokenInstanciado = Instantiate(tokenPrefab, slotVazio.position, Quaternion.identity);
                tokenInstanciado.transform.SetParent(slotVazio);
                tokenInstanciado.transform.localPosition = Vector3.zero;
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

    public GameObject EscolherTokenPorChance()
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
        if (tokenPlayerEmDuelo != null) return;

        tokenSelecionado = token;
        painelDeAcoes.SetActive(true);
    }

    void JogarToken()
    {
        if (tokenSelecionado != null)
        {
            Token dados = tokenSelecionado.GetComponent<Token>();
            int custo = dados.manaCusto;
            bool podeJogar = false;

            if (tokenSelecionado.CompareTag("Token Player"))
            {
                podeJogar = manaScript.GastarManaPlayer(custo);
                if (podeJogar)
                {
                    tokenSelecionado.transform.position = dueloPlayer.position;
                    tokenSelecionado.transform.SetParent(localDuelo);
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

        List<Transform> slotsOponente = slotsScript.oponenteSlots;
        List<GameObject> tokensDisponiveis = new List<GameObject>();

        foreach (Transform slot in slotsOponente)
        {
            if (slot.childCount > 0)
            {
                GameObject tokenNoSlot = slot.GetChild(0).gameObject;
                if (tokenNoSlot.CompareTag("Token Oponente") && tokenNoSlot.transform.position != dueloOponente.position)
                {
                    tokensDisponiveis.Add(tokenNoSlot);
                }
            }
        }

        tokensDisponiveis.Sort((a, b) => a.GetComponent<Token>().manaCusto.CompareTo(b.GetComponent<Token>().manaCusto));

        foreach (GameObject token in tokensDisponiveis)
        {
            Token dados = token.GetComponent<Token>();
            bool podeJogar = manaScript.GastarManaOponente(dados.manaCusto);
            if (podeJogar)
            {
                token.transform.position = dueloOponente.position;
                token.transform.SetParent(localDuelo);
                tokenOponenteEmDuelo = token;
                Debug.Log($"Oponente jogou o token: {dados.nomeDoToken} (Custo: {dados.manaCusto})");
                break;
            }
        }

        // Se não houver token para jogar, tenta comprar um
        if (tokenOponenteEmDuelo == null && caixaScript != null && slotsScript.OponenteSlotDisponivel())
        {
            caixaScript.OponenteTentarComprarToken();
        }
    }

    void MostrarDetalhes()
    {
        GameObject tokenParaMostrar = null;

        Collider2D[] colidersDuelo = Physics2D.OverlapCircleAll(dueloPlayer.position, 0.1f);
        foreach (Collider2D col in colidersDuelo)
        {
            if (col != null && col.CompareTag("Token Player"))
            {
                tokenParaMostrar = col.gameObject;
                break;
            }
        }

        if (tokenParaMostrar == null && tokenSelecionado != null && tokenSelecionado.CompareTag("Token Player"))
        {
            tokenParaMostrar = tokenSelecionado;
        }

        if (tokenParaMostrar != null)
        {
            Token dados = tokenParaMostrar.GetComponent<Token>();
            if (dados != null)
            {
                textoNome.text = dados.nomeDoToken;
                textoDano.text = $"Dano: {dados.dano}";
                textoVida.text = $"Vida: {dados.vida}";
                textoManaCusto.text = $"Mana: {dados.manaCusto}";
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
                painelDeDetalhes.SetActive(false);
            }
        }
        else
        {
            Debug.Log("Nenhum token válido do player para mostrar os detalhes.");
            painelDeDetalhes.SetActive(false);
        }
    }

    void MostrarStatusDuelo(GameObject token)
    {
        Token dados = token.GetComponent<Token>();

        textoNome.text = dados.nomeDoToken;
        textoDano.text = $"Dano: {dados.dano}";
        textoVida.text = $"Vida: {dados.vida}";
        textoManaCusto.text = $"Mana: {dados.manaCusto}";
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

        tOponente.ReceberDano(tPlayer.dano, tPlayer);
        Debug.Log($"Oponente perdeu {tPlayer.dano} de vida. Vida restante: {tOponente.vida}");

        tokenPlayerAtacou = true;

        if (!tOponente.estaVivo)
        {
            string tagVencedor = "Token Player";
            Token.Raridade raridadeTokenDerrotado = tOponente.raridade;
            Destroy(tokenOponenteEmDuelo);
            tokenOponenteEmDuelo = null;
            tokenPlayerAtacou = false;
            manaScript.AdicionarManaPorRaridade(raridadeTokenDerrotado, tagVencedor);

            Invoke("TentarNovoTokenOponente", 1.0f);
            return;
        }

        Invoke("ContraAtaqueOponente", 1.0f);
    }

    // Uso da habilidade especial do token do player
    void UsarHabilidadeEspecial()
    {
        if (tokenPlayerEmDuelo == null || tokenOponenteEmDuelo == null) return;
        Token tPlayer = tokenPlayerEmDuelo.GetComponent<Token>();
        Token tOponente = tokenOponenteEmDuelo.GetComponent<Token>();

        if (manaScript.manaPlayer < tPlayer.custoManaEspecial)
        {
            Debug.Log("Mana insuficiente para usar a habilidade especial.");
            return;
        }

        bool gastouMana = manaScript.GastarManaPlayer(tPlayer.custoManaEspecial);
        if (!gastouMana)
        {
            Debug.Log("Erro ao gastar a mana para habilidade especial.");
            return;
        }

        tOponente.ReceberDano(tPlayer.danoEspecial, tPlayer);
        Debug.Log($"Habilidade Especial: Oponente perdeu {tPlayer.danoEspecial} de vida. Vida restante: {tOponente.vida}");

        tokenPlayerAtacou = true;

        if (!tOponente.estaVivo)
        {
            string tagVencedor = "Token Player";
            Token.Raridade raridadeTokenDerrotado = tOponente.raridade;
            Destroy(tokenOponenteEmDuelo);
            tokenOponenteEmDuelo = null;
            tokenPlayerAtacou = false;
            manaScript.AdicionarManaPorRaridade(raridadeTokenDerrotado, tagVencedor);

            Invoke("TentarNovoTokenOponente", 1.0f);
            return;
        }

        Invoke("ContraAtaqueOponente", 1.0f);
    }

    // Uso da habilidade especial do oponente (aleatório)
    void OponenteUsarHabilidadeEspecial()
    {
        if (tokenPlayerEmDuelo == null || tokenOponenteEmDuelo == null) return;
        Token tOponente = tokenOponenteEmDuelo.GetComponent<Token>();
        Token tPlayer = tokenPlayerEmDuelo.GetComponent<Token>();

        if (manaScript.manaOponente < tOponente.custoManaEspecial)
            return;

        bool gastouMana = manaScript.GastarManaOponente(tOponente.custoManaEspecial);
        if (!gastouMana)
            return;

        tPlayer.ReceberDano(tOponente.danoEspecial, tOponente);
        Debug.Log($"Oponente usou habilidade especial! Player perdeu {tOponente.danoEspecial} de vida. Vida restante: {tPlayer.vida}");

        if (!tPlayer.estaVivo)
        {
            string tagVencedor = "Token Oponente";
            Token.Raridade raridadeTokenDerrotado = tPlayer.raridade;
            Destroy(tokenPlayerEmDuelo);
            tokenPlayerEmDuelo = null;
            manaScript.AdicionarManaPorRaridade(raridadeTokenDerrotado, tagVencedor);
        }
    }

    void ContraAtaqueOponente()
    {
        if (tokenPlayerAtacou && tokenPlayerEmDuelo != null && tokenOponenteEmDuelo != null)
        {
            Token tPlayer = tokenPlayerEmDuelo.GetComponent<Token>();
            Token tOponente = tokenOponenteEmDuelo.GetComponent<Token>();

            // Decide aleatoriamente se o oponente usará o ataque normal ou especial
            bool podeUsarEspecial = manaScript.manaOponente >= tOponente.custoManaEspecial;
            bool usarEspecial = podeUsarEspecial && Random.value < 0.5f; // 50% de chance

            if (usarEspecial)
            {
                OponenteUsarHabilidadeEspecial();
            }
            else
            {
                // Ataque normal
                tPlayer.ReceberDano(tOponente.dano, tOponente);
                Debug.Log($"Player perdeu {tOponente.dano} de vida. Vida restante: {tPlayer.vida}");

                if (!tPlayer.estaVivo)
                {
                    string tagVencedor = "Token Oponente";
                    Token.Raridade raridadeTokenDerrotado = tPlayer.raridade;
                    Destroy(tokenPlayerEmDuelo);
                    tokenPlayerEmDuelo = null;
                    manaScript.AdicionarManaPorRaridade(raridadeTokenDerrotado, tagVencedor);
                }
            }
            tokenPlayerAtacou = false;
        }
    }

    void TentarNovoTokenOponente()
    {
        if (tokenOponenteEmDuelo != null) return;

        List<Transform> slotsOponente = slotsScript.oponenteSlots;
        List<GameObject> tokensDisponiveis = new List<GameObject>();

        foreach (Transform slot in slotsOponente)
        {
            if (slot.childCount > 0)
            {
                GameObject tokenNoSlot = slot.GetChild(0).gameObject;
                if (tokenNoSlot.CompareTag("Token Oponente") && tokenNoSlot.transform.position != dueloOponente.position)
                {
                    tokensDisponiveis.Add(tokenNoSlot);
                }
            }
        }

        tokensDisponiveis.Sort((a, b) => a.GetComponent<Token>().manaCusto.CompareTo(b.GetComponent<Token>().manaCusto));

        foreach (GameObject token in tokensDisponiveis)
        {
            Token dados = token.GetComponent<Token>();
            bool podeJogar = manaScript.GastarManaOponente(dados.manaCusto);
            if (podeJogar)
            {
                token.transform.position = dueloOponente.position;
                token.transform.SetParent(localDuelo);
                tokenOponenteEmDuelo = token;
                Debug.Log($"Oponente jogou outro token: {dados.nomeDoToken} (Custo: {dados.manaCusto})");
                break;
            }
        }

        // Se não houver token para jogar, tenta comprar um
        if (tokenOponenteEmDuelo == null && caixaScript != null && slotsScript.OponenteSlotDisponivel())
        {
            caixaScript.OponenteTentarComprarToken();
        }
    }
}