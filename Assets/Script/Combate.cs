using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Certifique-se de que TMPro está sendo usado para TextMeshProUGUI
using System.Linq; // Para OrderBy

public class Combate : MonoBehaviour
{
    private Mana manaScript;
    private Slots slotsScript;
    private Caixa caixaScript;
    private TurnManager turnManager;

    [Header("Prefabs de Tokens Disponíveis")]
    public List<GameObject> todosOsTokens;

    [Header("Canvas UI")]
    public GameObject painelDeAcoes; // Para o botão de habilidade especial
    public Button botaoHabilidadeEspecial;
    public GameObject painelDeDetalhes;
    public Button botaoFecharDetalhes; // Agora o único botão no painel de detalhes

    public TextMeshProUGUI textoDano;
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoRaridade;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoManaCusto;
    public TextMeshProUGUI textoBuffInfo; // Novo para exibir informações de buff
    public Image imagemToken;

    [HideInInspector] public GameObject tokenAtualSelecionado;

    void Start()
    {
        manaScript = FindObjectOfType<Mana>();
        slotsScript = FindObjectOfType<Slots>();
        caixaScript = FindObjectOfType<Caixa>();
        turnManager = FindObjectOfType<TurnManager>();

        if (slotsScript == null || manaScript == null || caixaScript == null || turnManager == null)
        {
            Debug.LogError("Um dos scripts essenciais (Slots, Mana, Caixa ou TurnManager) não foi encontrado na cena.");
            enabled = false;
            return;
        }

        painelDeAcoes.SetActive(false);
        painelDeDetalhes.SetActive(false);

        // Inicializa 8 tokens para cada lado nas "mãos"
        GerarTokensIniciais(8);

        // Listener para o botão de habilidade especial
        botaoHabilidadeEspecial.onClick.AddListener(AtivarHabilidadeEspecial);
        botaoFecharDetalhes.onClick.AddListener(FecharDetalhes); // Certifica-se de que o listener está no lugar
    }

    // Chamado pelo TokenDragDrop para selecionar um token (para detalhes ou habilidade)
    public void SelecionarTokenParaUI(GameObject token)
    {
        tokenAtualSelecionado = token;
        Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();

        // Só mostra painéis se o token estiver no tabuleiro
        if (tokenScript != null && tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
        {
            MostrarDetalhesUI(); // Isso ativa o painelDeDetalhes

            // Lógica para mostrar o botão de habilidade especial (apenas para o player no seu turno E se for um token de Dano)
            if (tokenAtualSelecionado.CompareTag("Token Player") && turnManager.turnoAtual == TurnManager.Turno.Player && tokenScript.tokenType == Token.TokenType.Dano)
            {
                painelDeAcoes.SetActive(true);
                // Habilita o botão apenas se tiver mana suficiente e a habilidade não estiver já ativada
                botaoHabilidadeEspecial.interactable = (manaScript.manaPlayer >= tokenScript.custoManaEspecial && !tokenScript.habilidadeAtivada);
            }
            else
            {
                painelDeAcoes.SetActive(false); // Esconde se não for para habilidade (oponente, turno errado, ou token de buff)
            }
        }
        else
        {
            // Se o token não está no tabuleiro, fecha ambos os painéis
            painelDeDetalhes.SetActive(false);
            painelDeAcoes.SetActive(false);
        }
    }


    void GerarTokensIniciais(int quantidade)
    {
        for (int i = 0; i < quantidade; i++)
        {
            GerarEColocarTokenNaMao(true); // Para o player
            GerarEColocarTokenNaMao(false); // Para o oponente
        }
    }

    void GerarEColocarTokenNaMao(bool paraPlayer)
    {
        List<Transform> slots = paraPlayer ? slotsScript.playerHandSlots : slotsScript.oponenteHandSlots;
        string tagDoDono = paraPlayer ? "Token Player" : "Token Oponente";
        Transform slotVazio = null;

        foreach (Transform slot in slots)
        {
            if (slotsScript.SlotEstaLivre(slot)) // Usa a nova verificação de slot livre
            {
                slotVazio = slot;
                break;
            }
        }

        if (slotVazio != null)
        {
            // Pega o buff de sorte do jogador ou oponente
            float luckBuff = turnManager.GetTotalLuckBuffPercentage(paraPlayer);
            GameObject tokenPrefab = EscolherTokenPorChance(luckBuff);
            if (tokenPrefab != null)
            {
                GameObject tokenInstanciado = Instantiate(tokenPrefab, slotVazio.position, Quaternion.identity);
                tokenInstanciado.transform.SetParent(slotVazio);
                tokenInstanciado.transform.localPosition = Vector3.zero;
                tokenInstanciado.tag = tagDoDono;

                Token tokenScript = tokenInstanciado.GetComponent<Token>();
                if (tokenScript != null)
                {
                    tokenScript.gameObject.tag = tagDoDono; // Garante a tag no script
                }
                // Adiciona o TokenDragDrop se não tiver (muito importante!)
                if (tokenInstanciado.GetComponent<TokenDragDrop>() == null)
                {
                    tokenInstanciado.AddComponent<TokenDragDrop>();
                }
            }
        } else {
            Debug.LogWarning($"Não há slots vazios na mão do {(paraPlayer ? "Player" : "Oponente")} para gerar token inicial.");
        }
    }

    public GameObject EscolherTokenPorChance(float luckBuffPercentage)
    {
        List<Token> todosTokensScripts = todosOsTokens.Select(go => go.GetComponent<Token>()).ToList();

        // Ajustar chances com base no buff de Sorte
        // Aumenta a chance de épicas, lendárias, míticas
        // Diminui a chance de comuns e raras
        // As porcentagens negativas aqui são para diminuir a chance, mas o totalChance não deve ser negativo
        Dictionary<Token.Raridade, float> chanceModificadores = new Dictionary<Token.Raridade, float>()
        {
            { Token.Raridade.Comum, -0.015f }, // -1.5%
            { Token.Raridade.Incomum, -0.01f }, // -1%
            { Token.Raridade.Raro, -0.005f }, // -0.5%
            { Token.Raridade.Epico, 0.005f }, // +0.5%
            { Token.Raridade.Lendario, 0.01f }, // +1%
            { Token.Raridade.Mitico, 0.015f }, // +1.5%
            { Token.Raridade.Alter, 0f }, // Sem alteração
            { Token.Raridade.Potencial, 0f } // Sem alteração
        };

        float totalAdjustedChance = 0f;
        List<(Token token, float adjustedChance)> adjustedChances = new List<(Token, float)>();

        foreach (Token token in todosTokensScripts)
        {
            float baseChance = token.chanceDeAparicao;
            float modifier = 0;
            if (chanceModificadores.ContainsKey(token.raridade))
            {
                // Aplica o modificador base multiplicado pela porcentagem de sorte
                modifier = chanceModificadores[token.raridade] * (luckBuffPercentage / 100f);
            }
            float adjustedChance = baseChance * (1 + modifier);
            
            // Garante que a chance não seja negativa
            adjustedChance = Mathf.Max(0f, adjustedChance);
            
            adjustedChances.Add((token, adjustedChance));
            totalAdjustedChance += adjustedChance;
        }

        if (totalAdjustedChance <= 0) // Fallback para evitar divisão por zero
        {
            Debug.LogWarning("Total de chances ajustadas é zero ou negativo. Gerando um token aleatório sem sorte.");
            return todosOsTokens[Random.Range(0, todosOsTokens.Count)];
        }

        float randomValue = Random.Range(0f, totalAdjustedChance);
        float acumulado = 0f;

        foreach (var item in adjustedChances)
        {
            acumulado += item.adjustedChance;
            if (randomValue <= acumulado)
                return item.token.gameObject;
        }
        
        return null; // Caso não encontre nenhum token (improvável se totalAdjustedChance > 0)
    }

    void MostrarDetalhesUI()
    {
        if (tokenAtualSelecionado != null)
        {
            Token dados = tokenAtualSelecionado.GetComponent<Token>();
            if (dados != null)
            {
                textoNome.text = dados.nomeDoToken;
                textoManaCusto.text = $"Custo: {dados.manaCusto}";
                textoRaridade.text = $"Raridade: {dados.raridade}";

                // Exibir informações baseadas no tipo de token
                if (dados.tokenType == Token.TokenType.Dano)
                {
                    textoDano.gameObject.SetActive(true);
                    textoVida.gameObject.SetActive(true);
                    textoBuffInfo.gameObject.SetActive(false); // Esconde info de buff
                    textoDano.text = $"Dano: {dados.danoBase}";
                    textoVida.text = $"Vida: {dados.vida}";
                }
                else if (dados.tokenType == Token.TokenType.Buff)
                {
                    textoDano.gameObject.SetActive(false); // Esconde dano
                    textoVida.gameObject.SetActive(false); // Esconde vida
                    textoBuffInfo.gameObject.SetActive(true); // Mostra info de buff
                    textoBuffInfo.text = $"Buff: {dados.buffType} ({dados.buffPercentage}%)";
                }

                SpriteRenderer spriteRenderer = tokenAtualSelecionado.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                    imagemToken.sprite = spriteRenderer.sprite;

                painelDeDetalhes.SetActive(true);
            }
            else
            {
                Debug.LogError("O token selecionado não possui o componente 'Token'.");
                painelDeDetalhes.SetActive(false);
            }
        }
        else
        {
            painelDeDetalhes.SetActive(false);
        }
    }

    public void FecharDetalhes()
    {
        painelDeDetalhes.SetActive(false);
        painelDeAcoes.SetActive(false); // Fecha o painel de ações junto
        tokenAtualSelecionado = null;
    }

    void AtivarHabilidadeEspecial()
    {
        if (tokenAtualSelecionado != null && tokenAtualSelecionado.CompareTag("Token Player"))
        {
            Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();
            if (tokenScript != null)
            {
                // Apenas tokens de Dano podem ter habilidade especial ativada manualmente
                if (tokenScript.tokenType == Token.TokenType.Dano)
                {
                    if (manaScript.GastarManaPlayer(tokenScript.custoManaEspecial))
                    {
                        tokenScript.habilidadeAtivada = true; // Apenas marca a flag
                        Debug.Log($"Habilidade especial de {tokenScript.nomeDoToken} ativada! O dano será aplicado no próximo ataque.");
                        painelDeAcoes.SetActive(false);
                        FecharDetalhes();
                    }
                    else
                    {
                        Debug.Log("Mana insuficiente para usar a habilidade especial.");
                    }
                } else {
                    Debug.LogWarning("Tokens de Buff não possuem habilidade especial ativável.");
                }
            }
        }
    }
}