using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TurnManager : MonoBehaviour
{
    public enum Turno
    {
        Player,
        Oponente
    }

    public Turno turnoAtual = Turno.Player;

    [Header("UI de Turno")]
    public Button botaoPassarTurno;
    public TextMeshProUGUI textoTurno;

    [Header("Configurações da IA do Oponente")]
    [Range(0, 100)] public int chanceOponenteJogarCarta = 70;
    [Range(0, 100)] public int chanceOponenteUsarHabilidade = 50;
    [Range(0, 100)] public int chanceOponenteComprarCarta = 30;
    public float minDelayOponenteAcao = 0.5f;
    public float maxDelayOponenteAcao = 1.5f;


    private Slots slotsScript;
    private Mana manaScript;
    private Caixa caixaScript;
    private Combate combateScript;

    private List<GameObject> tokensJogadosNesteTurnoPlayer = new List<GameObject>();
    private List<GameObject> tokensJogadosNesteTurnoOponente = new List<GameObject>();

    // --- Sistema de Buffs ---
    private List<Token> playerActivePassiveBuffTokens = new List<Token>(); // Tokens com TokenType.Buff
    private List<Token> oponenteActivePassiveBuffTokens = new List<Token>();

    private List<Token.BuffEffect> playerTemporaryBuffs = new List<Token.BuffEffect>(); // Buffs ativos por habilidade (duram 1 turno)
    private List<Token.BuffEffect> oponenteTemporaryBuffs = new List<Token.BuffEffect>();

    // NOVO: Buffs de cartas Potencial seladas
    private List<Token> playerSealedBuffTokens = new List<Token>();
    private List<Token> oponenteSealedBuffTokens = new List<Token>();

    // --- Sistema de Invocação ---
    // Tuple: (Token que ativou a habilidade, Prefab da carta a ser invocada, É Player?, Slot do ativador)
    private List<(Token caster, GameObject prefabToSummon, bool isPlayer, Transform originalCasterSlot)> pendingSummons = new List<(Token, GameObject, bool, Transform)>();

    void Start()
    {
        slotsScript = FindObjectOfType<Slots>();
        manaScript = FindObjectOfType<Mana>();
        caixaScript = FindObjectOfType<Caixa>();
        combateScript = FindObjectOfType<Combate>();

        if (slotsScript == null || manaScript == null || caixaScript == null || combateScript == null)
        {
            Debug.LogError("Um script essencial (Slots, Mana, Caixa ou Combate) não foi encontrado para TurnManager.");
            enabled = false;
            return;
        }

        if (botaoPassarTurno != null)
        {
            botaoPassarTurno.onClick.AddListener(PassarTurno);
        }
        AtualizarTextoTurno();
    }

    public void AdicionarTokenJogado(GameObject tokenGO)
    {
        Token token = tokenGO.GetComponent<Token>();
        if (token == null) return;

        bool isPlayer = tokenGO.CompareTag("Token Player");

        if (isPlayer)
        {
            if (!tokensJogadosNesteTurnoPlayer.Contains(tokenGO))
                tokensJogadosNesteTurnoPlayer.Add(tokenGO);

            // Adiciona aos buffs passivos ativos se for um token de buff passivo
            if (token.tokenType == Token.TokenType.Buff && !playerActivePassiveBuffTokens.Contains(token))
            {
                playerActivePassiveBuffTokens.Add(token);
                Debug.Log($"Buff Passivo {token.passiveBuffType} ({token.passiveBuffPercentage}%) do Player ativado: {token.nomeDoToken}");
            }
            // NOVO: Adiciona aos buffs selados se for um token Potencial e estiver selado
            if (token.raridade == Token.Raridade.Potencial && token.isSealed && !playerSealedBuffTokens.Contains(token))
            {
                playerSealedBuffTokens.Add(token);
                Debug.Log($"Token Potencial {token.nomeDoToken} selado do Player ativado: {token.sealedBuffType} ({token.sealedBuffPercentage}%)");
            }
        }
        else // Oponente
        {
            if (!tokensJogadosNesteTurnoOponente.Contains(tokenGO))
                tokensJogadosNesteTurnoOponente.Add(tokenGO);

            if (token.tokenType == Token.TokenType.Buff && !oponenteActivePassiveBuffTokens.Contains(token))
            {
                oponenteActivePassiveBuffTokens.Add(token);
                Debug.Log($"Buff Passivo {token.passiveBuffType} ({token.passiveBuffPercentage}%) do Oponente ativado: {token.nomeDoToken}");
            }
            // NOVO: Adiciona aos buffs selados se for um token Potencial e estiver selado
            if (token.raridade == Token.Raridade.Potencial && token.isSealed && !oponenteSealedBuffTokens.Contains(token))
            {
                oponenteSealedBuffTokens.Add(token);
                Debug.Log($"Token Potencial {token.nomeDoToken} selado do Oponente ativado: {token.sealedBuffType} ({token.sealedBuffPercentage}%)");
            }
        }
    }

    public void RemoverTokenDerrotado(GameObject tokenGO)
    {
        tokensJogadosNesteTurnoPlayer.Remove(tokenGO);
        tokensJogadosNesteTurnoOponente.Remove(tokenGO);

        Token token = tokenGO.GetComponent<Token>();
        if (token != null)
        {
            bool isPlayer = tokenGO.CompareTag("Token Player");
            // Remove dos buffs passivos ativos se for um token de buff passivo
            if (token.tokenType == Token.TokenType.Buff)
            {
                if (isPlayer && playerActivePassiveBuffTokens.Remove(token))
                {
                    Debug.Log($"Buff Passivo {token.passiveBuffType} ({token.passiveBuffPercentage}%) do Player desativado: {token.nomeDoToken}");
                }
                else if (!isPlayer && oponenteActivePassiveBuffTokens.Remove(token))
                {
                    Debug.Log($"Buff Passivo {token.passiveBuffType} ({token.passiveBuffPercentage}%) do Oponente desativado: {token.nomeDoToken}");
                }
            }
            // NOVO: Remove dos buffs selados se for um token Potencial
            if (token.raridade == Token.Raridade.Potencial)
            {
                if (isPlayer && playerSealedBuffTokens.Remove(token))
                {
                    Debug.Log($"Token Potencial {token.nomeDoToken} selado do Player desativado (derrotado).");
                }
                else if (!isPlayer && oponenteSealedBuffTokens.Remove(token))
                {
                    Debug.Log($"Token Potencial {token.nomeDoToken} selado do Oponente desativado (derrotado).");
                }
            }
        }
    }

    // NOVO: Remove um token Potencial da lista de buffs selados (quando deselado)
    public void RemoverTokenPotencialSelado(Token token)
    {
        bool isPlayer = token.CompareTag("Token Player");
        if (isPlayer)
        {
            if (playerSealedBuffTokens.Remove(token))
            {
                Debug.Log($"Token Potencial {token.nomeDoToken} deselado do Player. Buff removido.");
            }
        }
        else
        {
            if (oponenteSealedBuffTokens.Remove(token))
            {
                Debug.Log($"Token Potencial {token.nomeDoToken} deselado do Oponente. Buff removido.");
            }
        }
    }

    // --- Funções para obter o total de buffs ativos (Passivos + Temporários + Selados) ---
    public float GetTotalLuckBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> activePassiveBuffs = isPlayer ? playerActivePassiveBuffTokens : oponenteActivePassiveBuffTokens;
        foreach (Token buffToken in activePassiveBuffs)
        {
            if (buffToken.passiveBuffType == Token.BuffType.Sorte)
            {
                total += buffToken.passiveBuffPercentage;
            }
        }
        // Buffs de sorte de habilidade ativa não são somados aqui, pois sorte afeta a geração, que ocorre antes do turno de combate.
        return total;
    }

    public float GetTotalStrengthBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> activePassiveBuffs = isPlayer ? playerActivePassiveBuffTokens : oponenteActivePassiveBuffTokens;
        foreach (Token buffToken in activePassiveBuffs)
        {
            if (buffToken.passiveBuffType == Token.BuffType.Forca)
            {
                total += buffToken.passiveBuffPercentage;
            }
        }
        // Adiciona buffs temporários (de habilidade ativa)
        List<Token.BuffEffect> temporaryBuffs = isPlayer ? playerTemporaryBuffs : oponenteTemporaryBuffs;
        foreach (var buffEffect in temporaryBuffs)
        {
            if (buffEffect.buffType == Token.BuffType.Forca)
            {
                total += buffEffect.percentage;
            }
        }
        return total;
    }

    public float GetTotalEnergyBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> activePassiveBuffs = isPlayer ? playerActivePassiveBuffTokens : oponenteActivePassiveBuffTokens;
        foreach (Token buffToken in activePassiveBuffs)
        {
            if (buffToken.passiveBuffType == Token.BuffType.Energia)
            {
                total += buffToken.passiveBuffPercentage;
            }
        }
        // Adiciona buffs temporários (de habilidade ativa)
        List<Token.BuffEffect> temporaryBuffs = isPlayer ? playerTemporaryBuffs : oponenteTemporaryBuffs;
        foreach (var buffEffect in temporaryBuffs)
        {
            if (buffEffect.buffType == Token.BuffType.Energia)
            {
                total += buffEffect.percentage;
            }
        }
        return total;
    }

    // NOVO: Buff de Redução de Custo de Habilidade
    public float GetTotalAbilityCostReductionBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> sealedBuffs = isPlayer ? playerSealedBuffTokens : oponenteSealedBuffTokens;
        foreach (Token token in sealedBuffs)
        {
            if (token.isSealed && token.sealedBuffType == Token.BuffType.ReducaoCustoHabilidade)
            {
                total += token.sealedBuffPercentage;
            }
        }
        return total;
    }

    // NOVO: Buff de Aumento de Dano Geral
    public float GetTotalDamageBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> sealedBuffs = isPlayer ? playerSealedBuffTokens : oponenteSealedBuffTokens;
        foreach (Token token in sealedBuffs)
        {
            if (token.isSealed && token.sealedBuffType == Token.BuffType.AumentoDanoGeral)
            {
                total += token.sealedBuffPercentage;
            }
        }
        return total;
    }

    // NOVO: Buff de Aumento de Vida Geral (ainda não usado na lógica de combate, mas disponível)
    public float GetTotalHealthBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> sealedBuffs = isPlayer ? playerSealedBuffTokens : oponenteSealedBuffTokens;
        foreach (Token token in sealedBuffs)
        {
            if (token.isSealed && token.sealedBuffType == Token.BuffType.AumentoVidaGeral)
            {
                total += token.sealedBuffPercentage;
            }
        }
        return total;
    }

    // --- Fim das Funções de Buff ---

    public void PassarTurno()
    {
        Debug.Log($"Fim do turno do {turnoAtual}.");
        combateScript.FecharDetalhes(); // Fecha qualquer painel de UI aberto

        // Fase 1: Executar Habilidades Ativas que afetam o combate (Dano, Buffs)
        ExecutarHabilidadesAtivasDeCombate();

        // Fase 2: Executar Ataques Normais
        ExecutarAtaquesDeTurno();

        // Fase 3: Executar Habilidades de Invocação
        ExecutarHabilidadesDeInvocacao();

        // Fase 4: Limpar buffs temporários e flags de habilidade
        LimparEstadoDoTurno();

        TrocarTurno();
        Debug.Log($"Início do turno do {turnoAtual}.");
    }

    void ExecutarHabilidadesAtivasDeCombate()
    {
        bool isPlayerTurn = (turnoAtual == Turno.Player);
        List<Token> tokensNoTabuleiro = slotsScript.GetTokensNoTabuleiro(isPlayerTurn);

        foreach (Token token in tokensNoTabuleiro)
        {
            // NOVO: Aplica redução de custo de habilidade
            float custoReducaoBuff = GetTotalAbilityCostReductionBuffPercentage(isPlayerTurn);
            int custoHabilidadeReal = Mathf.RoundToInt(token.abilityCost * (1 - (custoReducaoBuff / 100f)));
            custoHabilidadeReal = Mathf.Max(0, custoHabilidadeReal); // Garante que o custo não seja negativo

            // NOVO: Habilidade 'Copy' não é executada aqui, apenas as habilidades copiadas ou originais de dano/buff
            if (token.activeAbilityType == Token.ActiveAbilityType.Copy) continue;

            if (token.CompareTag("Token Player") == isPlayerTurn && token.abilityUsedThisTurn)
            {
                if (token.activeAbilityType == Token.ActiveAbilityType.Damage)
                {
                    // Lógica para habilidade de Dano (precisa de um alvo)
                    Token alvo = null;
                    List<Token> oponentesNoTab = slotsScript.GetTokensNoTabuleiro(!isPlayerTurn);
                    if (oponentesNoTab.Any())
                    {
                        // Exemplo: Ataque o primeiro oponente que encontrar
                        oponentesNoTab = oponentesNoTab.OrderBy(t => t.PosicaoNoTab == Token.PosicaoTabuleiro.Frente ? 0 : 1).ThenBy(t => Vector3.Distance(token.transform.position, t.transform.position)).ToList();
                        alvo = oponentesNoTab.FirstOrDefault(t => t.estaVivo);
                    }

                    if (alvo != null)
                    {
                        float forcaBuffPercent = GetTotalStrengthBuffPercentage(isPlayerTurn);
                        token.AplicarDanoHabilidade(alvo, forcaBuffPercent);
                    }
                    else
                    {
                        Debug.LogWarning($"Habilidade de dano de {token.nomeDoToken} não encontrou um alvo.");
                    }
                }
                else if (token.activeAbilityType == Token.ActiveAbilityType.Buff)
                {
                    // Aplica buffs temporários
                    List<Token.BuffEffect> targetBuffs = isPlayerTurn ? playerTemporaryBuffs : oponenteTemporaryBuffs;
                    foreach (var buffEffect in token.abilityBuffEffects)
                    {
                        targetBuffs.Add(buffEffect);
                        Debug.Log($"Buff Temporário {buffEffect.buffType} (+{buffEffect.percentage}%) aplicado por {token.nomeDoToken}");
                    }
                }
            }
        }
    }

    void ExecutarAtaquesDeTurno()
    {
        bool atacantesSaoPlayer = (turnoAtual == Turno.Player);
        List<Token> atacantes = slotsScript.GetTokensNoTabuleiro(atacantesSaoPlayer);

        float forcaBuffPercent = GetTotalStrengthBuffPercentage(atacantesSaoPlayer);

        atacantes = atacantes.OrderBy(t => t.PosicaoNoTab == Token.PosicaoTabuleiro.Tras ? 1 : 0)
                               .ThenBy(t => t.transform.position.x)
                               .ToList();

        Debug.Log($"Total de atacantes {(atacantesSaoPlayer ? "Player" : "Oponente")}: {atacantes.Count}");

        foreach (Token atacante in atacantes)
        {
            if (atacante.tokenType == Token.TokenType.Buff)
            { // Tokens de Buff Passivo não atacam
                continue;
            }
            // Tokens Potencial selados não atacam
            if (atacante.raridade == Token.Raridade.Potencial && atacante.isSealed)
            {
                Debug.Log($"Token Potencial {atacante.nomeDoToken} está selado, pulando ataque.");
                continue;
            }

            if (!atacante.estaVivo)
            {
                Debug.Log($"Atacante {atacante.nomeDoToken} não está vivo, pulando ataque.");
                continue;
            }

            // NOVO: Reseta a redução cumulativa da passiva Adaptação
            if (atacante.passiveAbilityType == Token.PassiveAbilityType.Adaptacao)
            {
                atacante.currentAdaptacaoReduction = 0f;
            }

            Token alvo = null;
            Transform slotAtacante = atacante.transform.parent;
            if (slotAtacante == null)
            {
                Debug.LogWarning($"Atacante {atacante.nomeDoToken} não tem pai (slot). Ignorando ataque.");
                continue;
            }

            // Lógica de alvo (primeiro na frente, depois atrás na mesma coluna)
            Transform slotAlvoFrente = slotsScript.GetSlotCorrespondenteNaColuna(slotAtacante, !atacantesSaoPlayer, Token.PosicaoTabuleiro.Frente);
            if (slotAlvoFrente != null && slotAlvoFrente.childCount > 0)
            {
                Token tokenNaFrente = slotAlvoFrente.GetComponentInChildren<Token>();
                if (tokenNaFrente != null && tokenNaFrente.estaVivo)
                {
                    alvo = tokenNaFrente;
                }
            }

            if (alvo == null)
            {
                bool frenteBloqueada = false;
                if (slotAlvoFrente != null && slotAlvoFrente.childCount > 0)
                {
                    Token tokenNaFrenteCheck = slotAlvoFrente.GetComponentInChildren<Token>();
                    if (tokenNaFrenteCheck != null && tokenNaFrenteCheck.estaVivo)
                    {
                        frenteBloqueada = true;
                    }
                }

                if (!frenteBloqueada)
                {
                    Transform slotAlvoTras = slotsScript.GetSlotCorrespondenteNaColuna(slotAtacante, !atacantesSaoPlayer, Token.PosicaoTabuleiro.Tras);
                    if (slotAlvoTras != null && slotAlvoTras.childCount > 0)
                    {
                        Token tokenNaTras = slotAlvoTras.GetComponentInChildren<Token>();
                        if (tokenNaTras != null && tokenNaTras.estaVivo)
                        {
                            alvo = tokenNaTras;
                        }
                    }
                }
            }

            if (alvo != null && alvo.estaVivo)
            {
                atacante.Atacar(alvo, forcaBuffPercent); // Passa o buff de força
            }
            else
            {
                Debug.Log($"{atacante.nomeDoToken} não encontrou um alvo válido ou vivo para atacar neste turno.");
            }
        }
    }

    void ExecutarHabilidadesDeInvocacao()
    {
        foreach (var summonRequest in pendingSummons.ToList()) // ToList() para permitir modificação da lista original
        {
            Transform spawnSlot = slotsScript.FindAdjacentEmptySlot(summonRequest.originalCasterSlot, summonRequest.isPlayer);
            if (spawnSlot != null)
            {
                GameObject newCard = Instantiate(summonRequest.prefabToSummon, spawnSlot.position, Quaternion.identity);
                newCard.transform.SetParent(spawnSlot);
                newCard.transform.localPosition = Vector3.zero;
                newCard.tag = summonRequest.isPlayer ? "Token Player" : "Token Oponente";

                Token newTokenScript = newCard.GetComponent<Token>();
                if (newTokenScript != null)
                {
                    newTokenScript.gameObject.tag = newCard.tag;
                    newTokenScript.PosicaoNoTab = slotsScript.GetPosicaoNoTabuleiro(spawnSlot, summonRequest.isPlayer);
                    // NOVO: Se a carta invocada for Potencial, ela vem selada
                    if (newTokenScript.raridade == Token.Raridade.Potencial)
                    {
                        newTokenScript.isSealed = true;
                        Debug.Log($"Token Potencial {newTokenScript.nomeDoToken} invocado selado.");
                    }
                }
                if (newCard.GetComponent<TokenDragDrop>() == null)
                {
                    newCard.AddComponent<TokenDragDrop>();
                }
                Debug.Log($"Carta {newCard.name} invocada por {summonRequest.caster.name} no slot {spawnSlot.name}.");
            }
            else
            {
                Debug.LogWarning($"Não foi possível invocar carta para {summonRequest.caster.name}. Nenhum slot vazio disponível adjacente.");
            }
            // Remove a requisição após tentar invocar
            pendingSummons.Remove(summonRequest);
        }
    }

    void LimparEstadoDoTurno()
    {
        // Limpa a lista de tokens jogados
        tokensJogadosNesteTurnoPlayer.Clear();
        tokensJogadosNesteTurnoOponente.Clear();

        // Limpa buffs temporários
        playerTemporaryBuffs.Clear();
        oponenteTemporaryBuffs.Clear();

        // Reseta a flag de habilidade usada e NOVO: Restaura a habilidade copiada
        List<Token> todosOsTokens = slotsScript.GetTokensNoTabuleiro(true);
        todosOsTokens.AddRange(slotsScript.GetTokensNoTabuleiro(false));

        foreach (Token token in todosOsTokens)
        {
            // NOVO: Se o token usou a habilidade e era uma cópia, restaura a original
            if (token.abilityUsedThisTurn && token.isAbilityCopied)
            {
                token.RestaurarHabilidadeOriginal();
            }

            token.abilityUsedThisTurn = false;
        }
    }

    void TrocarTurno()
    {
        if (turnoAtual == Turno.Player)
        {
            turnoAtual = Turno.Oponente;
            if (botaoPassarTurno != null) botaoPassarTurno.interactable = false;
            float delay = Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao);
            Invoke("OponenteFazAcao", delay);
        }
        else
        {
            turnoAtual = Turno.Player;
            if (botaoPassarTurno != null) botaoPassarTurno.interactable = true;
        }
        AtualizarTextoTurno();
    }

    void AtualizarTextoTurno()
    {
        if (textoTurno != null) textoTurno.text = $"Turno: {turnoAtual}";
    }

    // --- Nova função para que Combate.cs possa registrar a habilidade ---
    public void HandleActiveAbility(Token tokenScript, Transform casterSlot)
    {
        bool isPlayer = tokenScript.CompareTag("Token Player");

        switch (tokenScript.activeAbilityType)
        {
            case Token.ActiveAbilityType.Damage:
                // Dano direto será aplicado na fase de habilidades de combate
                break;
            case Token.ActiveAbilityType.Buff:
                // Buffs temporários serão aplicados na fase de habilidades de combate
                break;
            case Token.ActiveAbilityType.Summon:
                // Invocações são agendadas para a fase pós-combate
                for (int i = 0; i < tokenScript.numCardsToSummon; i++)
                {
                    if (tokenScript.summonableCards.Any())
                    {
                        GameObject cardToSummon = tokenScript.summonableCards[Random.Range(0, tokenScript.summonableCards.Count)];
                        pendingSummons.Add((tokenScript, cardToSummon, isPlayer, casterSlot));
                    }
                    else
                    {
                        Debug.LogWarning($"Carta de invocação {tokenScript.name} não tem cartas para invocar na lista 'Summonable Cards'.");
                    }
                }
                break;
            case Token.ActiveAbilityType.Copy:
                // A lógica de cópia é executada imediatamente em Combate.cs ou na IA, não precisa de agendamento.
                break;
            case Token.ActiveAbilityType.None:
            default:
                Debug.LogWarning($"Token {tokenScript.name} com habilidade ativa tipo 'None' ou desconhecido tentando ser ativada.");
                break;
        }
    }

    // Ações do Oponente mais aleatórias e com prioridade
    void OponenteFazAcao()
    {
        if (turnoAtual != Turno.Oponente) return;

        List<System.Action> acoesDisponiveis = new List<System.Action>();

        // Tenta jogar carta
        if (Random.Range(0, 100) < chanceOponenteJogarCarta)
        {
            acoesDisponiveis.Add(() => TentarJogarCartaOponente());
        }

        // Tenta usar habilidade (apenas para tokens de DANO com habilidade não usada)
        if (Random.Range(0, 100) < chanceOponenteUsarHabilidade)
        {
            acoesDisponiveis.Add(() => TentarUsarHabilidadeOponente());
        }

        // Tenta comprar carta
        if (Random.Range(0, 100) < chanceOponenteComprarCarta)
        {
            acoesDisponiveis.Add(() => TentarComprarCartaOponente());
        }

        // NOVO: Tenta deselar carta (se houver alguma selada e com mana divina suficiente)
        List<Token> oponenteSealedTokens = slotsScript.GetTokensNoTabuleiro(false)
                                                .Where(t => t.raridade == Token.Raridade.Potencial && t.isSealed && manaScript.manaDivinaOponente >= t.divineManaCost)
                                                .ToList();
        if (oponenteSealedTokens.Any())
        {
            acoesDisponiveis.Add(() => TentarDeselarCartaOponente(oponenteSealedTokens));
        }


        if (acoesDisponiveis.Count == 0)
        {
            acoesDisponiveis.Add(() => PassarTurnoAposDelay());
        }
        else
        {
            acoesDisponiveis = acoesDisponiveis.OrderBy(x => Random.value).ToList();
        }

        bool agiu = false;
        foreach (var acao in acoesDisponiveis)
        {
            try
            {
                acao.Invoke();
                agiu = true;
                break;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erro ao tentar executar ação do oponente: {e.Message}");
            }
        }

        if (!agiu)
        {
            PassarTurnoAposDelay();
        }
    }

    void TentarJogarCartaOponente()
    {
        List<Transform> oponenteHandSlotsComTokens = slotsScript.oponenteHandSlots.Where(s => s.childCount > 0).ToList();
        if (oponenteHandSlotsComTokens.Count > 0)
        {
            oponenteHandSlotsComTokens = oponenteHandSlotsComTokens.OrderBy(s => s.GetComponentInChildren<Token>().manaCusto).ToList();
            int numToConsider = Mathf.Min(oponenteHandSlotsComTokens.Count, 3);

            for (int i = 0; i < numToConsider; i++)
            {
                Transform chosenHandSlot = oponenteHandSlotsComTokens[i];
                Token token = chosenHandSlot.GetComponentInChildren<Token>();

                if (token != null && manaScript.manaOponente >= token.manaCusto)
                {
                    Transform slotDeDestino = slotsScript.GetPrimeiroSlotVazioFrente(false);
                    if (slotDeDestino == null)
                    {
                        slotDeDestino = slotsScript.GetPrimeiroSlotVazioTras(false);
                    }

                    if (slotDeDestino != null)
                    {
                        if (manaScript.GastarManaOponente(token.manaCusto))
                        {
                            token.transform.SetParent(slotDeDestino);
                            token.transform.localPosition = Vector3.zero;
                            token.PosicaoNoTab = slotsScript.GetPosicaoNoTabuleiro(slotDeDestino, false);
                            token.GetComponent<SpriteRenderer>().sortingOrder = 1;
                            // NOVO: Se a carta for Potencial, ela vem selada ao ser jogada
                            if (token.raridade == Token.Raridade.Potencial)
                            {
                                token.isSealed = true;
                                Debug.Log($"Oponente jogou Token Potencial {token.nomeDoToken} selado para o tabuleiro.");
                            }
                            AdicionarTokenJogado(token.gameObject);
                            Debug.Log($"Oponente jogou {token.nomeDoToken} para o tabuleiro.");
                            Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
                            return;
                        }
                    }
                }
            }
        }
        Debug.Log("Oponente não conseguiu jogar carta. Tentando próxima ação.");
        Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
    }

    void TentarUsarHabilidadeOponente()
    {
        // NOVO: Aplica redução de custo de habilidade para oponente
        float custoReducaoBuff = GetTotalAbilityCostReductionBuffPercentage(false);

        // Filtra apenas tokens de DANO que estão vivos, não usaram habilidade e têm mana
        List<Token> oponenteTokensNoTab = slotsScript.GetTokensNoTabuleiro(false)
                                             .Where(t => t.tokenType == Token.TokenType.Dano && t.activeAbilityType != Token.ActiveAbilityType.None && !t.abilityUsedThisTurn)
                                             .ToList();

        // NOVO: Prioriza o uso da habilidade de cópia (se disponível e houver um alvo decente)
        Token tokenParaCopia = oponenteTokensNoTab.FirstOrDefault(t => t.activeAbilityType == Token.ActiveAbilityType.Copy);
        if (tokenParaCopia != null)
        {
            // Lógica de AI para Copy: Tenta copiar uma habilidade de dano/buff/summon de um token do jogador
            Token alvoParaCopia = slotsScript.GetTokensNoTabuleiro(true)
                .Where(t => t.activeAbilityType != Token.ActiveAbilityType.None && t.activeAbilityType != Token.ActiveAbilityType.Copy && t.tokenType == Token.TokenType.Dano)
                .FirstOrDefault();

            if (alvoParaCopia != null)
            {
                int custoHabilidadeReal = Mathf.RoundToInt(tokenParaCopia.abilityCost * (1 - (custoReducaoBuff / 100f)));
                custoHabilidadeReal = Mathf.Max(0, custoHabilidadeReal);

                if (manaScript.GastarManaOponente(custoHabilidadeReal))
                {
                    tokenParaCopia.CopiarHabilidade(alvoParaCopia);
                    tokenParaCopia.abilityUsedThisTurn = true;
                    Debug.Log($"Oponente ativou habilidade COPIA com {tokenParaCopia.nomeDoToken}, copiando {alvoParaCopia.nomeDoToken}.");
                    Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
                    return;
                }
            }
            // Se não conseguiu copiar, remove o token de cópia para considerar outras habilidades
            oponenteTokensNoTab.Remove(tokenParaCopia);
        }

        // Executa habilidades normais (Damage, Buff, Summon) ou a habilidade copiada
        if (oponenteTokensNoTab.Any())
        {
            Token tokenParaHabilidade = oponenteTokensNoTab[Random.Range(0, oponenteTokensNoTab.Count)];

            int custoHabilidadeReal = Mathf.RoundToInt(tokenParaHabilidade.abilityCost * (1 - (custoReducaoBuff / 100f)));
            custoHabilidadeReal = Mathf.Max(0, custoHabilidadeReal);

            if (manaScript.GastarManaOponente(custoHabilidadeReal))
            {
                tokenParaHabilidade.abilityUsedThisTurn = true;
                Debug.Log($"Oponente ativou habilidade especial de {tokenParaHabilidade.nomeDoToken} (Tipo: {tokenParaHabilidade.activeAbilityType}, Custo: {custoHabilidadeReal}).");
                HandleActiveAbility(tokenParaHabilidade, tokenParaHabilidade.transform.parent); // Informa ao TurnManager

                Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
                return;
            }
        }
        Debug.Log("Oponente não conseguiu usar habilidade. Tentando próxima ação.");
        Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
    }

    void TentarComprarCartaOponente()
    {
        if (slotsScript.OponenteHandSlotDisponivel() && manaScript.manaOponente >= caixaScript.precoCompra)
        {
            caixaScript.OponenteTentarComprarToken();
            Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
            return;
        }
        Debug.Log("Oponente não conseguiu comprar carta. Passando turno.");
        PassarTurnoAposDelay();
    }

    // NOVO: Tentar deselar carta para oponente
    void TentarDeselarCartaOponente(List<Token> oponenteSealedTokens)
    {
        if (oponenteSealedTokens.Any())
        {
            Token tokenParaDeselar = oponenteSealedTokens[Random.Range(0, oponenteSealedTokens.Count)];
            if (manaScript.GastarManaDivina(tokenParaDeselar.divineManaCost, false))
            {
                tokenParaDeselar.Deselar(); // Chama o método Deselar no script Token
                RemoverTokenPotencialSelado(tokenParaDeselar); // Remove da lista de buffs selados
                Debug.Log($"Oponente deselou {tokenParaDeselar.nomeDoToken} por {tokenParaDeselar.divineManaCost} Mana Divina.");
                Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
                return;
            }
        }
        Debug.Log("Oponente não conseguiu deselar carta. Tentando próxima ação.");
        Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
    }

    void PassarTurnoAposDelay()
    {
        Invoke("PassarTurno", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
    }
}