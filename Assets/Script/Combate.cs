using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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
    public TextMeshProUGUI textoHabilidadeBotao; // Novo para texto do botão de habilidade
    // NOVO: Botão e texto para deselar carta
    public Button botaoDeselarCarta;
    public TextMeshProUGUI textoDeselarCartaBotao;

    public GameObject painelDeDetalhes;
    public Button botaoFecharDetalhes;

    public TextMeshProUGUI textoDano;
    public TextMeshProUGUI textoVida;
    public TextMeshProUGUI textoRaridade;
    public TextMeshProUGUI textoNome;
    public TextMeshProUGUI textoManaCusto;
    public TextMeshProUGUI textoBuffInfo; // Para exibir informações de buff passivo
    public TextMeshProUGUI textoHabilidadeInfo; // Novo para exibir detalhes da habilidade ativa
    // NOVO: Texto para informações de carta selada
    public TextMeshProUGUI textoSeladoInfo;
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

        GerarTokensIniciais(8);

        botaoHabilidadeEspecial.onClick.AddListener(AtivarHabilidadeEspecial);
        botaoFecharDetalhes.onClick.AddListener(FecharDetalhes);
        // NOVO: Adiciona listener para o botão de deselar
        botaoDeselarCarta.onClick.AddListener(DeselarCartaSelecionada);
    }

    public void SelecionarTokenParaUI(GameObject token)
    {
        tokenAtualSelecionado = token;
        Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();

        if (tokenScript != null && tokenScript.PosicaoNoTab != Token.PosicaoTabuleiro.NaoNoTabuleiro)
        {
            MostrarDetalhesUI();

            // Lógica para mostrar o painel de ações
            bool isPlayerToken = tokenAtualSelecionado.CompareTag("Token Player");
            bool isPlayerTurn = turnManager.turnoAtual == TurnManager.Turno.Player;

            if (isPlayerToken && isPlayerTurn)
            {
                painelDeAcoes.SetActive(true);

                // Habilidade Especial
                if (tokenScript.tokenType == Token.TokenType.Dano && tokenScript.activeAbilityType != Token.ActiveAbilityType.None)
                {
                    botaoHabilidadeEspecial.gameObject.SetActive(true);
                    textoHabilidadeBotao.text = ObterNomeBotaoHabilidade(tokenScript.activeAbilityType);
                    // NOVO: Aplica redução de custo de habilidade
                    float custoReducaoBuff = turnManager.GetTotalAbilityCostReductionBuffPercentage(true);
                    int custoHabilidadeReal = Mathf.RoundToInt(tokenScript.abilityCost * (1 - (custoReducaoBuff / 100f)));
                    custoHabilidadeReal = Mathf.Max(0, custoHabilidadeReal); // Garante que o custo não seja negativo

                    botaoHabilidadeEspecial.interactable = (manaScript.manaPlayer >= custoHabilidadeReal && !tokenScript.abilityUsedThisTurn);
                }
                else
                {
                    botaoHabilidadeEspecial.gameObject.SetActive(false);
                }

                // NOVO: Botão de Deselar Carta (apenas para tokens Potencial selados)
                if (tokenScript.raridade == Token.Raridade.Potencial && tokenScript.isSealed)
                {
                    botaoDeselarCarta.gameObject.SetActive(true);
                    textoDeselarCartaBotao.text = $"Deselar ({tokenScript.divineManaCost} Mana Divina)";
                    botaoDeselarCarta.interactable = (manaScript.manaDivinaPlayer >= tokenScript.divineManaCost);
                }
                else
                {
                    botaoDeselarCarta.gameObject.SetActive(false);
                }

                // Se nenhum botão de ação estiver ativo, desativa o painel de ações
                if (!botaoHabilidadeEspecial.gameObject.activeSelf && !botaoDeselarCarta.gameObject.activeSelf)
                {
                    painelDeAcoes.SetActive(false);
                }
            }
            else
            {
                painelDeAcoes.SetActive(false);
            }
        }
        else
        {
            painelDeDetalhes.SetActive(false);
            painelDeAcoes.SetActive(false);
        }
    }

    private string ObterNomeBotaoHabilidade(Token.ActiveAbilityType type)
    {
        switch (type)
        {
            case Token.ActiveAbilityType.Damage: return "Usar Habilidade (Dano)";
            case Token.ActiveAbilityType.Buff: return "Usar Habilidade (Buff)";
            case Token.ActiveAbilityType.Summon: return "Invocar Carta";
            case Token.ActiveAbilityType.Copy: return "Copiar Habilidade"; // NOVO
            default: return "Habilidade";
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
            if (slotsScript.SlotEstaLivre(slot))
            {
                slotVazio = slot;
                break;
            }
        }

        if (slotVazio != null)
        {
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
                    tokenScript.gameObject.tag = tagDoDono;
                    // NOVO: Se a carta for Potencial, ela vem selada ao ser gerada
                    if (tokenScript.raridade == Token.Raridade.Potencial)
                    {
                        tokenScript.isSealed = true;
                        Debug.Log($"Token Potencial {tokenScript.nomeDoToken} gerado selado.");
                    }
                }
                if (tokenInstanciado.GetComponent<TokenDragDrop>() == null)
                {
                    tokenInstanciado.AddComponent<TokenDragDrop>();
                }
            }
        }
        else
        {
            Debug.LogWarning($"Não há slots vazios na mão do {(paraPlayer ? "Player" : "Oponente")} para gerar token inicial.");
        }
    }

    public GameObject EscolherTokenPorChance(float luckBuffPercentage)
    {
        List<Token> todosTokensScripts = todosOsTokens.Select(go => go.GetComponent<Token>()).ToList();

        Dictionary<Token.Raridade, float> chanceModificadores = new Dictionary<Token.Raridade, float>()
        {
            { Token.Raridade.Comum, -0.015f },
            { Token.Raridade.Incomum, -0.01f },
            { Token.Raridade.Raro, -0.005f },
            { Token.Raridade.Epico, 0.005f },
            { Token.Raridade.Lendario, 0.01f },
            { Token.Raridade.Mitico, 0.015f },
            { Token.Raridade.Alter, 0f },
            { Token.Raridade.Potencial, 0.02f } // NOVO: Chance base para Potencial
        };

        float totalAdjustedChance = 0f;
        List<(Token token, float adjustedChance)> adjustedChances = new List<(Token, float)>();

        foreach (Token token in todosTokensScripts)
        {
            float baseChance = token.chanceDeAparicao;
            float modifier = 0;
            if (chanceModificadores.ContainsKey(token.raridade))
            {
                modifier = chanceModificadores[token.raridade] * (luckBuffPercentage / 100f);
            }
            float adjustedChance = baseChance * (1 + modifier);

            adjustedChance = Mathf.Max(0f, adjustedChance);

            adjustedChances.Add((token, adjustedChance));
            totalAdjustedChance += adjustedChance;
        }

        if (totalAdjustedChance <= 0)
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

        return null;
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

                // Esconde todos por padrão
                textoDano.gameObject.SetActive(false);
                textoVida.gameObject.SetActive(false);
                textoBuffInfo.gameObject.SetActive(false);
                textoHabilidadeInfo.gameObject.SetActive(false);
                textoSeladoInfo.gameObject.SetActive(false); // NOVO

                // Exibir informações baseadas no tipo de token
                if (dados.tokenType == Token.TokenType.Dano)
                {
                    textoDano.gameObject.SetActive(true);
                    textoVida.gameObject.SetActive(true);
                    textoDano.text = $"Dano: {dados.danoBase}";
                    textoVida.text = $"Vida: {dados.vida}";

                    // Informações da habilidade ativa
                    if (dados.activeAbilityType != Token.ActiveAbilityType.None)
                    {
                        textoHabilidadeInfo.gameObject.SetActive(true);
                        string habilidadeDetalhes = $"Habilidade: {dados.activeAbilityType} (Custo: {dados.abilityCost} Mana)\n";
                        switch (dados.activeAbilityType)
                        {
                            case Token.ActiveAbilityType.Damage:
                                habilidadeDetalhes += $"  Dano: {dados.abilityDamage}";
                                break;
                            case Token.ActiveAbilityType.Buff:
                                habilidadeDetalhes += "  Buffs:\n";
                                foreach (var buff in dados.abilityBuffEffects)
                                {
                                    habilidadeDetalhes += $"    - {buff.buffType}: +{buff.percentage}%\n";
                                }
                                break;
                            case Token.ActiveAbilityType.Summon:
                                habilidadeDetalhes += $"  Invoca {dados.numCardsToSummon} cartas aleatórias.";
                                break;
                            case Token.ActiveAbilityType.Copy: // NOVO
                                habilidadeDetalhes = $"Habilidade: Copiar (Custo: {dados.abilityCost} Mana)";
                                if (dados.isAbilityCopied)
                                {
                                    habilidadeDetalhes += $"\n  (Copiado: {dados.activeAbilityType})";
                                }
                                else
                                {
                                    habilidadeDetalhes += "\n  Copia a habilidade ativa de um token inimigo.";
                                }
                                break;
                        }
                        // NOVO: Adiciona info da passiva se for Adaptacao
                        if (dados.passiveAbilityType == Token.PassiveAbilityType.Adaptacao)
                        {
                            habilidadeDetalhes += $"\nPassiva: Adaptação\n  Redução de dano cumulativa de +{dados.adaptacaoBaseReduction}% (Máx: {dados.adaptacaoMaxReduction}%)";
                        }

                        textoHabilidadeInfo.text = habilidadeDetalhes;
                    }
                    else if (dados.passiveAbilityType != Token.PassiveAbilityType.None)
                    {
                        // Exibe a passiva mesmo se não houver habilidade ativa
                        textoHabilidadeInfo.gameObject.SetActive(true);
                        string passivaDetalhes = "";
                        if (dados.passiveAbilityType == Token.PassiveAbilityType.Adaptacao)
                        {
                            passivaDetalhes = $"Passiva: Adaptação\n  Redução de dano cumulativa de +{dados.adaptacaoBaseReduction}% (Máx: {dados.adaptacaoMaxReduction}%)";
                        }
                        textoHabilidadeInfo.text = passivaDetalhes;
                    }
                }
                else if (dados.tokenType == Token.TokenType.Buff)
                {
                    textoBuffInfo.gameObject.SetActive(true);
                    textoBuffInfo.text = $"Buff Passivo: {dados.passiveBuffType} ({dados.passiveBuffPercentage}%)";
                }

                // NOVO: Informações de carta selada (Potencial)
                if (dados.raridade == Token.Raridade.Potencial)
                {
                    textoSeladoInfo.gameObject.SetActive(true);
                    if (dados.isSealed)
                    {
                        textoSeladoInfo.text = $"Estado: Selado\nBuff Selado: {dados.sealedBuffType} (+{dados.sealedBuffPercentage}%)\nCusto para Deselar: {dados.divineManaCost} Mana Divina";
                    }
                    else
                    {
                        textoSeladoInfo.text = "Estado: Deselado";
                    }
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
        painelDeAcoes.SetActive(false);
        tokenAtualSelecionado = null;
    }

    void AtivarHabilidadeEspecial()
    {
        if (tokenAtualSelecionado != null && tokenAtualSelecionado.CompareTag("Token Player"))
        {
            Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();
            if (tokenScript != null)
            {
                if (tokenScript.tokenType == Token.TokenType.Dano && tokenScript.activeAbilityType != Token.ActiveAbilityType.None)
                {
                    // Aplica redução de custo de habilidade
                    float custoReducaoBuff = turnManager.GetTotalAbilityCostReductionBuffPercentage(true);
                    int custoHabilidadeReal = Mathf.RoundToInt(tokenScript.abilityCost * (1 - (custoReducaoBuff / 100f)));
                    custoHabilidadeReal = Mathf.Max(0, custoHabilidadeReal);

                    if (manaScript.manaPlayer >= custoHabilidadeReal)
                    {
                        // --- NOVO: Lógica de Habilidade de Cópia ---
                        if (tokenScript.activeAbilityType == Token.ActiveAbilityType.Copy)
                        {
                            // Lógica de seleção de alvo: Seleciona o primeiro token inimigo com habilidade ativa (e não de cópia).
                            Token alvoParaCopia = slotsScript.GetTokensNoTabuleiro(false)
                                .Where(t => t.activeAbilityType != Token.ActiveAbilityType.None && t.activeAbilityType != Token.ActiveAbilityType.Copy && t.tokenType == Token.TokenType.Dano)
                                .FirstOrDefault();

                            if (alvoParaCopia != null)
                            {
                                if (manaScript.GastarManaPlayer(custoHabilidadeReal))
                                {
                                    tokenScript.CopiarHabilidade(alvoParaCopia);
                                    tokenScript.abilityUsedThisTurn = true; // A cópia em si é o uso da habilidade deste turno
                                    Debug.Log($"Habilidade {tokenScript.nomeDoToken} ativada: COPIAR. Habilidade copiada é {tokenScript.activeAbilityType} de {alvoParaCopia.nomeDoToken}.");
                                }
                                else
                                {
                                    Debug.Log("Mana insuficiente para usar a habilidade de cópia.");
                                }
                            }
                            else
                            {
                                Debug.Log("Não há alvo válido para copiar a habilidade.");
                            }
                        }
                        // --- FIM NOVO: Lógica de Habilidade de Cópia ---
                        else // Habilidades normais (Damage, Buff, Summon, ou a habilidade que foi copiada)
                        {
                            if (manaScript.GastarManaPlayer(custoHabilidadeReal))
                            {
                                tokenScript.abilityUsedThisTurn = true; // Marca a habilidade como usada
                                Debug.Log($"Habilidade de {tokenScript.nomeDoToken} ativada! (Tipo: {tokenScript.activeAbilityType}, Custo: {custoHabilidadeReal})");

                                // A lógica da habilidade em si é passada para o TurnManager
                                turnManager.HandleActiveAbility(tokenScript, tokenAtualSelecionado.transform.parent);
                            }
                            else
                            {
                                Debug.Log("Mana insuficiente para usar a habilidade especial.");
                            }
                        }

                        // Fechamento de UI deve ocorrer após a tentativa de uso
                        painelDeAcoes.SetActive(false);
                        FecharDetalhes();
                    }
                    else
                    {
                        Debug.Log("Mana insuficiente para usar a habilidade especial.");
                    }
                }
                else
                {
                    Debug.LogWarning("Este token não possui uma habilidade ativa ou é um token de buff passivo.");
                }
            }
        }
    }

    // NOVO: Método para deselar a carta selecionada
    void DeselarCartaSelecionada()
    {
        if (tokenAtualSelecionado != null && tokenAtualSelecionado.CompareTag("Token Player"))
        {
            Token tokenScript = tokenAtualSelecionado.GetComponent<Token>();
            if (tokenScript != null && tokenScript.raridade == Token.Raridade.Potencial && tokenScript.isSealed)
            {
                if (manaScript.GastarManaDivina(tokenScript.divineManaCost, true))
                {
                    tokenScript.Deselar(); // Chama o método Deselar no script Token
                    turnManager.RemoverTokenPotencialSelado(tokenScript); // Informa ao TurnManager para remover o buff
                    Debug.Log($"Carta {tokenScript.nomeDoToken} deselada com sucesso!");

                    painelDeAcoes.SetActive(false);
                    FecharDetalhes();
                }
                else
                {
                    Debug.Log("Mana Divina insuficiente para deselar esta carta.");
                }
            }
        }
    }
}