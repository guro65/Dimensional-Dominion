using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Certifique-se de que TMPro está sendo usado
using System.Linq; // Necessário para .OrderBy

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
    [Range(0, 100)] public int chanceOponenteJogarCarta = 70; // Chance % de tentar jogar uma carta
    [Range(0, 100)] public int chanceOponenteUsarHabilidade = 50; // Chance % de tentar usar habilidade
    [Range(0, 100)] public int chanceOponenteComprarCarta = 30; // Chance % de tentar comprar carta
    public float minDelayOponenteAcao = 0.5f;
    public float maxDelayOponenteAcao = 1.5f;

    private Slots slotsScript;
    private Mana manaScript;
    private Caixa caixaScript;
    private Combate combateScript; // Para fechar a UI de detalhes ao passar o turno

    private List<GameObject> tokensJogadosNesteTurnoPlayer = new List<GameObject>();
    private List<GameObject> tokensJogadosNesteTurnoOponente = new List<GameObject>();

    // --- Novas Adições para Sistema de Buff ---
    private List<Token> playerActiveBuffs = new List<Token>();
    private List<Token> oponenteActiveBuffs = new List<Token>();

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

        if (tokenGO.CompareTag("Token Player"))
        {
            if (!tokensJogadosNesteTurnoPlayer.Contains(tokenGO))
                tokensJogadosNesteTurnoPlayer.Add(tokenGO);
            
            // Adiciona aos buffs ativos se for um token de buff
            if (token.tokenType == Token.TokenType.Buff && !playerActiveBuffs.Contains(token))
            {
                playerActiveBuffs.Add(token);
                Debug.Log($"Buff {token.buffType} ({token.buffPercentage}%) do Player ativado: {token.nomeDoToken}");
            }
        }
        else if (tokenGO.CompareTag("Token Oponente"))
        {
            if (!tokensJogadosNesteTurnoOponente.Contains(tokenGO))
                tokensJogadosNesteTurnoOponente.Add(tokenGO);

            // Adiciona aos buffs ativos se for um token de buff
            if (token.tokenType == Token.TokenType.Buff && !oponenteActiveBuffs.Contains(token))
            {
                oponenteActiveBuffs.Add(token);
                Debug.Log($"Buff {token.buffType} ({token.buffPercentage}%) do Oponente ativado: {token.nomeDoToken}");
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
            // Remove dos buffs ativos se for um token de buff
            if (tokenGO.CompareTag("Token Player"))
            {
                if (playerActiveBuffs.Remove(token))
                {
                    Debug.Log($"Buff {token.buffType} ({token.buffPercentage}%) do Player desativado: {token.nomeDoToken}");
                }
            }
            else if (tokenGO.CompareTag("Token Oponente"))
            {
                if (oponenteActiveBuffs.Remove(token))
                {
                    Debug.Log($"Buff {token.buffType} ({token.buffPercentage}%) do Oponente desativado: {token.nomeDoToken}");
                }
            }
        }
    }

    // --- Funções para obter o total de buffs ativos ---
    public float GetTotalLuckBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> activeBuffs = isPlayer ? playerActiveBuffs : oponenteActiveBuffs;
        foreach (Token buffToken in activeBuffs)
        {
            if (buffToken.buffType == Token.BuffType.Sorte)
            {
                total += buffToken.buffPercentage;
            }
        }
        return total;
    }

    public float GetTotalStrengthBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> activeBuffs = isPlayer ? playerActiveBuffs : oponenteActiveBuffs;
        foreach (Token buffToken in activeBuffs)
        {
            if (buffToken.buffType == Token.BuffType.Forca)
            {
                total += buffToken.buffPercentage;
            }
        }
        return total;
    }

    public float GetTotalEnergyBuffPercentage(bool isPlayer)
    {
        float total = 0f;
        List<Token> activeBuffs = isPlayer ? playerActiveBuffs : oponenteActiveBuffs;
        foreach (Token buffToken in activeBuffs)
        {
            if (buffToken.buffType == Token.BuffType.Energia)
            {
                total += buffToken.buffPercentage;
            }
        }
        return total;
    }

    // --- Fim das Novas Adições ---

    public void PassarTurno()
    {
        Debug.Log($"Fim do turno do {turnoAtual}.");
        combateScript.FecharDetalhes(); // Fecha qualquer painel de UI aberto

        ExecutarAcoesDeTurno(); // Executa ataques e habilidades
        TrocarTurno();
        Debug.Log($"Início do turno do {turnoAtual}.");
    }

    void ExecutarAcoesDeTurno()
    {
        if (turnoAtual == Turno.Player)
        {
            Debug.Log("Executando ataques do Player.");
            ExecutarAtaques(true);
            // Redefine flag de habilidade após o ataque
            foreach (Token token in slotsScript.GetTokensNoTabuleiro(true))
            {
                token.habilidadeAtivada = false;
            }
        }
        else // Turno do Oponente
        {
            Debug.Log("Executando ataques do Oponente.");
            ExecutarAtaques(false);
            foreach (Token token in slotsScript.GetTokensNoTabuleiro(false))
            {
                token.habilidadeAtivada = false;
            }
        }
        // Limpa a lista de tokens jogados APÓS os ataques, para garantir que todos que foram jogados ataquem neste turno.
        tokensJogadosNesteTurnoPlayer.Clear();
        tokensJogadosNesteTurnoOponente.Clear();
    }

    void TrocarTurno()
    {
        if (turnoAtual == Turno.Player)
        {
            turnoAtual = Turno.Oponente;
            if (botaoPassarTurno != null) botaoPassarTurno.interactable = false;
            float delay = Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao);
            Invoke("OponenteFazAcao", delay); // Oponente faz sua primeira ação após um delay
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

    void ExecutarAtaques(bool atacantesSaoPlayer)
    {
        List<Token> atacantes = slotsScript.GetTokensNoTabuleiro(atacantesSaoPlayer);
        List<Token> defensores = slotsScript.GetTokensNoTabuleiro(!atacantesSaoPlayer);
        
        // Pega o buff de força atual para o lado atacante
        float forcaBuffPercent = GetTotalStrengthBuffPercentage(atacantesSaoPlayer);

        // Ordenar atacantes: Frente primeiro, depois Esquerda para Direita
        // Isso garante que tokens da frente atacam antes dos de trás na mesma coluna
        atacantes = atacantes.OrderBy(t => t.PosicaoNoTab == Token.PosicaoTabuleiro.Tras ? 1 : 0) // Frente = 0, Trás = 1
                               .ThenBy(t => t.transform.position.x)
                               .ToList();

        Debug.Log($"Total de atacantes {(atacantesSaoPlayer ? "Player" : "Oponente")}: {atacantes.Count}");

        foreach (Token atacante in atacantes)
        {
            // Apenas tokens de DANO podem atacar
            if (atacante.tokenType == Token.TokenType.Buff) {
                Debug.Log($"Token {atacante.nomeDoToken} é um token de Buff, não ataca.");
                continue;
            }

            if (!atacante.estaVivo)
            {
                Debug.Log($"Atacante {atacante.nomeDoToken} não está vivo, pulando ataque.");
                continue; // Pula se o atacante morreu no meio dos ataques (ex: contra-ataque)
            }

            Token alvo = null;
            Transform slotAtacante = atacante.transform.parent;
            if (slotAtacante == null) {
                 Debug.LogWarning($"Atacante {atacante.nomeDoToken} não tem pai (slot). Ignorando ataque.");
                 continue;
            }

            // Tentar encontrar alvo na frente na mesma coluna
            Transform slotAlvoFrente = slotsScript.GetSlotCorrespondenteNaColuna(slotAtacante, !atacantesSaoPlayer, Token.PosicaoTabuleiro.Frente);
            
            if (slotAlvoFrente != null && slotAlvoFrente.childCount > 0)
            {
                Token tokenNaFrente = slotAlvoFrente.GetComponentInChildren<Token>();
                if (tokenNaFrente != null && tokenNaFrente.estaVivo)
                {
                    alvo = tokenNaFrente;
                    Debug.Log($"Alvo direto na frente para {atacante.nomeDoToken}: {alvo.nomeDoToken}");
                }
            }

            // Se não encontrou alvo na frente ou o alvo da frente está morto, tenta encontrar atrás na mesma coluna
            if (alvo == null)
            {
                // Verifica explicitamente se a posição da frente na coluna do atacante está realmente livre ou morta
                bool frenteBloqueada = false;
                if (slotAlvoFrente != null && slotAlvoFrente.childCount > 0)
                {
                    Token tokenNaFrenteCheck = slotAlvoFrente.GetComponentInChildren<Token>();
                    if (tokenNaFrenteCheck != null && tokenNaFrenteCheck.estaVivo)
                    {
                        frenteBloqueada = true;
                    }
                }

                if (!frenteBloqueada) // Se a frente não está bloqueada, pode atacar o de trás
                {
                    Transform slotAlvoTras = slotsScript.GetSlotCorrespondenteNaColuna(slotAtacante, !atacantesSaoPlayer, Token.PosicaoTabuleiro.Tras);
                    if (slotAlvoTras != null && slotAlvoTras.childCount > 0)
                    {
                        Token tokenNaTras = slotAlvoTras.GetComponentInChildren<Token>();
                        if (tokenNaTras != null && tokenNaTras.estaVivo)
                        {
                            alvo = tokenNaTras;
                            Debug.Log($"Alvo encontrado atrás para {atacante.nomeDoToken} (frente livre): {alvo.nomeDoToken}");
                        }
                    }
                }
            }

            // Executa o ataque se um alvo válido e vivo foi encontrado
            if (alvo != null && alvo.estaVivo)
            {
                if (atacante.habilidadeAtivada)
                {
                    atacante.UsarHabilidadeEspecial(alvo, forcaBuffPercent); // Passa o buff de força
                }
                else
                {
                    atacante.Atacar(alvo, forcaBuffPercent); // Passa o buff de força
                }
            }
            else
            {
                Debug.Log($"{atacante.nomeDoToken} não encontrou um alvo válido ou vivo para atacar neste turno.");
            }
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

        // Tenta usar habilidade (apenas para tokens de DANO)
        if (Random.Range(0, 100) < chanceOponenteUsarHabilidade)
        {
            acoesDisponiveis.Add(() => TentarUsarHabilidadeOponente());
        }

        // Tenta comprar carta
        if (Random.Range(0, 100) < chanceOponenteComprarCarta)
        {
            acoesDisponiveis.Add(() => TentarComprarCartaOponente());
        }

        // Se nenhuma ação foi adicionada, adiciona a ação de passar o turno como fallback
        if (acoesDisponiveis.Count == 0)
        {
            acoesDisponiveis.Add(() => PassarTurnoAposDelay());
        }
        else
        {
            // Embaralha as ações para aleatoriedade
            acoesDisponiveis = acoesDisponiveis.OrderBy(x => Random.value).ToList();
        }

        // Executa a primeira ação disponível e válida
        bool agiu = false;
        foreach (var acao in acoesDisponiveis)
        {
            try {
                acao.Invoke();
                agiu = true;
                break; 
            } catch (System.Exception e) {
                Debug.LogError($"Erro ao tentar executar ação do oponente: {e.Message}");
            }
        }

        if (!agiu) {
            PassarTurnoAposDelay();
        }
    }

    void TentarJogarCartaOponente()
    {
        List<Transform> oponenteHandSlotsComTokens = slotsScript.oponenteHandSlots.Where(s => s.childCount > 0).ToList();
        if (oponenteHandSlotsComTokens.Count > 0)
        {
            // Ordena pela manaCusto para tentar jogar cartas mais baratas primeiro
            oponenteHandSlotsComTokens = oponenteHandSlotsComTokens.OrderBy(s => s.GetComponentInChildren<Token>().manaCusto).ToList();
            
            // Tenta as 3 cartas mais baratas ou todas se tiver menos de 3
            int numToConsider = Mathf.Min(oponenteHandSlotsComTokens.Count, 3);

            for (int i = 0; i < numToConsider; i++)
            {
                Transform chosenHandSlot = oponenteHandSlotsComTokens[i]; // Tenta a carta mais barata primeiro, depois a próxima
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
                            AdicionarTokenJogado(token.gameObject);
                            Debug.Log($"Oponente jogou {token.nomeDoToken} para o tabuleiro.");
                            Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao)); // Tenta outra ação
                            return; // Ação bem sucedida, sai da função
                        }
                    }
                }
            }
        }
        Debug.Log("Oponente não conseguiu jogar carta. Tentando próxima ação.");
        Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao)); // Tenta próxima ação se não conseguiu jogar
    }

    void TentarUsarHabilidadeOponente()
    {
        // Filtra apenas tokens de DANO que estão vivos e podem usar habilidade
        List<Token> oponenteTokensNoTab = slotsScript.GetTokensNoTabuleiro(false)
                                             .Where(t => t.tokenType == Token.TokenType.Dano && !t.habilidadeAtivada && manaScript.manaOponente >= t.custoManaEspecial)
                                             .ToList();
        if (oponenteTokensNoTab.Count > 0)
        {
            Token tokenParaHabilidade = oponenteTokensNoTab[Random.Range(0, oponenteTokensNoTab.Count)];
            
            tokenParaHabilidade.habilidadeAtivada = true;
            manaScript.GastarManaOponente(tokenParaHabilidade.custoManaEspecial);
            Debug.Log($"Oponente ativou habilidade especial de {tokenParaHabilidade.nomeDoToken}.");
            Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao)); // Tenta outra ação
            return;
        }
        Debug.Log("Oponente não conseguiu usar habilidade. Tentando próxima ação.");
        Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao)); // Tenta próxima ação
    }

    void TentarComprarCartaOponente()
    {
        if (slotsScript.OponenteHandSlotDisponivel() && manaScript.manaOponente >= caixaScript.precoCompra)
        {
            caixaScript.OponenteTentarComprarToken();
            Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao)); // Tenta outra ação
            return;
        }
        Debug.Log("Oponente não conseguiu comprar carta. Passando turno.");
        PassarTurnoAposDelay(); // Passa o turno se não pode mais fazer nada
    }

    void PassarTurnoAposDelay()
    {
        Invoke("PassarTurno", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao));
    }
}