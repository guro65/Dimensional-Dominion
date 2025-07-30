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

    public void AdicionarTokenJogado(GameObject token)
    {
        if (token.CompareTag("Token Player"))
        {
            if (!tokensJogadosNesteTurnoPlayer.Contains(token))
                tokensJogadosNesteTurnoPlayer.Add(token);
        }
        else if (token.CompareTag("Token Oponente"))
        {
            if (!tokensJogadosNesteTurnoOponente.Contains(token))
                tokensJogadosNesteTurnoOponente.Add(token);
        }
    }

    public void RemoverTokenDerrotado(GameObject token)
    {
        tokensJogadosNesteTurnoPlayer.Remove(token);
        tokensJogadosNesteTurnoOponente.Remove(token);
    }

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

        // Ordenar atacantes: Frente primeiro, depois Esquerda para Direita
        // Isso garante que tokens da frente atacam antes dos de trás na mesma coluna
        atacantes = atacantes.OrderBy(t => t.PosicaoNoTab == Token.PosicaoTabuleiro.Tras ? 1 : 0) // Frente = 0, Trás = 1
                               .ThenBy(t => t.transform.position.x)
                               .ToList();

        Debug.Log($"Total de atacantes {(atacantesSaoPlayer ? "Player" : "Oponente")}: {atacantes.Count}");

        foreach (Token atacante in atacantes)
        {
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
                    atacante.UsarHabilidadeEspecial(alvo);
                }
                else
                {
                    atacante.Atacar(alvo);
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

        // Tenta usar habilidade
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
            // O Invoke é usado dentro das próprias funções de ação para encadear ou passar o turno
            // Então aqui, só chamamos a ação. Ela mesma se encarrega de chamar a próxima ação/passar turno
            try {
                acao.Invoke();
                agiu = true;
                break; // Apenas uma ação por Invoke, a próxima ação será encadeada ou o turno passado
            } catch (System.Exception e) {
                Debug.LogError($"Erro ao tentar executar ação do oponente: {e.Message}");
                // Continua para a próxima ação se uma falhar (ex: slot nulo)
            }
        }

        if (!agiu) {
            // Se por algum motivo nenhuma ação foi executada (ex: ações disponíveis mas sem mana/slots), passa o turno.
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
            Transform chosenHandSlot = oponenteHandSlotsComTokens[Random.Range(0, numToConsider)];

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
                        return;
                    }
                }
            }
        }
        Debug.Log("Oponente não conseguiu jogar carta. Tentando próxima ação.");
        Invoke("OponenteFazAcao", Random.Range(minDelayOponenteAcao, maxDelayOponenteAcao)); // Tenta próxima ação
    }

    void TentarUsarHabilidadeOponente()
    {
        List<Token> oponenteTokensNoTab = slotsScript.GetTokensNoTabuleiro(false)
                                             .Where(t => !t.habilidadeAtivada && manaScript.manaOponente >= t.custoManaEspecial)
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