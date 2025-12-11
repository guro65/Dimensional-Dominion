using System.Collections.Generic; // Necessário para List
using UnityEngine;

public class Token : MonoBehaviour
{
    [Header("Dados do Token")]
    public string nomeDoToken;
    public int danoBase; // Dano base do token, sem buffs
    public int vida;
    public int manaCusto; // Custo para jogar o token
    public Raridade raridade;
    public float chanceDeAparicao = 25f;

    public enum Raridade
    {
        Comum,
        Incomum,
        Raro,
        Epico,
        Lendario,
        Mitico,
        Alter,    // Nova raridade
        Potencial // Nova raridade
    }

    // Nova enum para a posição no tabuleiro
    public enum PosicaoTabuleiro
    {
        NaoNoTabuleiro,
        Frente,
        Tras
    }
    public PosicaoTabuleiro PosicaoNoTab = PosicaoTabuleiro.NaoNoTabuleiro;

    // --- Tipos de Token (Passivos) ---
    public enum TokenType // Tipo de Token: Dano normal ou Buff Passivo (sem botão de habilidade)
    {
        Dano, // Tokens de dano normais que podem ter habilidades ativas
        Buff  // Tokens de buff passivos, que aplicam buff contínuo enquanto estão no tabuleiro
    }
    public TokenType tokenType = TokenType.Dano; // Padrão é token de Dano

    // --- Configuração de Buff Passivo (se TokenType for Buff) ---
    public enum BuffType // Tipo específico de Buff (Sorte, Forca, Energia)
    {
        None,   // Sem buff
        Sorte,  // Aumenta chances de raridades altas (afeta EscolherTokenPorChance)
        Forca,  // Aumenta dano de todas as cartas (afeta Ataque)
        Energia, // Aumenta mana ganha ao derrotar cartas (afeta ReceberDano/Mana)
        ReducaoCustoHabilidade, // NOVO: Reduz custo de mana de habilidades
        AumentoDanoGeral,       // NOVO: Aumenta dano de todas as cartas no tabuleiro
        AumentoVidaGeral        // NOVO: Aumenta vida de todas as cartas no tabuleiro
    }
    public BuffType passiveBuffType = BuffType.None; // Padrão é sem buff
    [Range(0, 100)] public float passiveBuffPercentage = 0; // Porcentagem do buff passivo (0-100)

    // --- Configuração de Habilidade Ativa (se TokenType for Dano) ---
    public enum ActiveAbilityType
    {
        None,   // Nenhuma habilidade ativa
        Damage, // Dano direto (como já existe)
        Buff,   // Concede buffs temporários
        Summon, // Invoca outra carta
        Copy    // NOVO: Copia a habilidade de um token alvo
    }
    [Header("Habilidade Ativa")]
    public ActiveAbilityType activeAbilityType = ActiveAbilityType.None;
    public int abilityCost = 0; // Custo de mana para usar a habilidade ativa
    public bool abilityUsedThisTurn = false; // Flag para habilidade ativada no turno

    // Parâmetros para Habilidade de Dano (se ActiveAbilityType for Damage)
    public int abilityDamage = 8; // Dano da habilidade

    // Parâmetros para Habilidade de Buff (se ActiveAbilityType for Buff)
    [System.Serializable]
    public class BuffEffect // Classe para múltiplos buffs em uma habilidade
    {
        public BuffType buffType;
        [Range(0, 100)] public float percentage;
    }
    public List<BuffEffect> abilityBuffEffects = new List<BuffEffect>();

    // Parâmetros para Habilidade de Invocar (se ActiveAbilityType for Summon)
    public List<GameObject> summonableCards = new List<GameObject>(); // Lista de prefabs de cartas que podem ser invocadas
    public int numCardsToSummon = 1; // Quantidade de cartas a serem invocadas

    // --- NOVO: Propriedades para a raridade Potencial ---
    [Header("Propriedades de Potencial")]
    public bool isSealed = false; // Indica se a carta está selada
    public BuffType sealedBuffType = BuffType.None; // Tipo de buff que a carta selada oferece
    [Range(0, 100)] public float sealedBuffPercentage = 0; // Porcentagem do buff selado
    public int divineManaCost = 0; // Custo de Mana Divina para deselar

    // --- NOVO: Variáveis para Habilidade de Cópia ---
    [Header("Habilidade de Cópia")]
    [HideInInspector] public bool isAbilityCopied = false; // Indica se a habilidade atual é uma cópia
    private ActiveAbilityType originalActiveAbilityType;
    private int originalAbilityCost;
    private int originalAbilityDamage;
    private int originalNumCardsToSummon;
    private List<BuffEffect> originalAbilityBuffEffects = new List<BuffEffect>();
    private List<GameObject> originalSummonableCards = new List<GameObject>();

    // --- NOVO: Sistema de Habilidades Passivas ---
    public enum PassiveAbilityType
    {
        None,
        Adaptacao // Redução de Dano Cumulativa
    }

    [Header("Habilidade Passiva")]
    public PassiveAbilityType passiveAbilityType = PassiveAbilityType.None;
    [Range(0, 100)] public float adaptacaoBaseReduction = 5f; // Redução base por golpe (5%)
    [Range(0, 100)] public float adaptacaoMaxReduction = 50f; // Redução máxima (50%)
    [HideInInspector] public float currentAdaptacaoReduction = 0f; // Redução acumulada atual

    private Mana manaScript;
    private TurnManager turnManager;

    private void Awake()
    {
        manaScript = FindObjectOfType<Mana>();
        turnManager = FindObjectOfType<TurnManager>();

        if (manaScript == null) Debug.LogError("Mana script not found for Token: " + name);
        if (turnManager == null) Debug.LogError("TurnManager script not found for Token: " + name);

        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void Atacar(Token alvo, float forcaBuffPercent) // Recebe o buff de força total (passivo + ativo)
    {
        if (alvo != null && estaVivo)
        {
            // NOVO: Aplica buff de dano geral se a carta for de dano
            float danoGeralBuff = 0f;
            if (tokenType == TokenType.Dano)
            {
                // Note: TurnManager.GetTotalDamageBuffPercentage precisa ser um método existente
                danoGeralBuff = turnManager.GetTotalDamageBuffPercentage(CompareTag("Token Player"));
            }

            int danoEfetivo = Mathf.RoundToInt(danoBase * (1 + (forcaBuffPercent / 100f) + (danoGeralBuff / 100f)));
            Debug.Log($"{nomeDoToken} atacou {alvo.nomeDoToken} causando {danoEfetivo} de dano (com {forcaBuffPercent}% Força Buff e {danoGeralBuff}% Dano Geral Buff).");
            alvo.ReceberDano(danoEfetivo, this);
        }
        else
        {
            Debug.LogWarning($"{nomeDoToken} tentou atacar mas o alvo é nulo ou o atacante não está vivo.");
        }
    }

    // Método para aplicar dano da HABILIDADE (diferente do ataque normal)
    public void AplicarDanoHabilidade(Token alvo, float forcaBuffPercent)
    {
        if (alvo != null && estaVivo)
        {
            // NOVO: Aplica buff de dano geral se a carta for de dano
            float danoGeralBuff = 0f;
            if (tokenType == TokenType.Dano)
            {
                danoGeralBuff = turnManager.GetTotalDamageBuffPercentage(CompareTag("Token Player"));
            }

            int danoEfetivo = Mathf.RoundToInt(abilityDamage * (1 + (forcaBuffPercent / 100f) + (danoGeralBuff / 100f)));
            Debug.Log($"{nomeDoToken} (Habilidade Ativa) atacou {alvo.nomeDoToken} causando {danoEfetivo} de dano.");
            alvo.ReceberDano(danoEfetivo, this);
        }
        else
        {
            Debug.LogWarning($"{nomeDoToken} tentou usar habilidade de dano mas o alvo é nulo ou o atacante não está vivo.");
        }
    }

    public void ReceberDano(int quantidade, Token atacante)
    {
        // --- NOVO: Aplica Habilidade Passiva "Adaptação" ---
        float danoRecebido = quantidade;
        if (passiveAbilityType == PassiveAbilityType.Adaptacao)
        {
            // Aplica a redução acumulada atual
            danoRecebido = danoRecebido * (1f - (currentAdaptacaoReduction / 100f));

            // Aumenta a redução acumulada para o próximo golpe
            currentAdaptacaoReduction += adaptacaoBaseReduction;
            currentAdaptacaoReduction = Mathf.Min(currentAdaptacaoReduction, adaptacaoMaxReduction);

            Debug.Log($"{nomeDoToken} (Adaptação) reduziu o dano de {quantidade} para {Mathf.RoundToInt(danoRecebido)} (Redução Atual: {currentAdaptacaoReduction}%).");
        }

        int danoFinal = Mathf.RoundToInt(danoRecebido);
        if (danoFinal < 0) danoFinal = 0; // Garante que o dano não seja negativo

        vida -= danoFinal;
        Debug.Log($"{nomeDoToken} recebeu {danoFinal} de dano. Vida restante: {vida}");

        if (vida <= 0)
        {
            if (manaScript != null && atacante != null)
            {
                string tagDoVencedor = atacante.CompareTag("Token Player") ? "Token Player" : (atacante.CompareTag("Token Oponente") ? "Token Oponente" : "");
                if (!string.IsNullOrEmpty(tagDoVencedor))
                {
                    // Passa o percentual de buff de Energia do vencedor para o script Mana
                    float energiaBuffPercent = turnManager.GetTotalEnergyBuffPercentage(tagDoVencedor == "Token Player");
                    manaScript.AdicionarManaPorRaridade(raridade, tagDoVencedor, energiaBuffPercent);

                    // NOVO: Adiciona Mana Divina se o token derrotado for de raridade específica
                    if (raridade == Raridade.Lendario || raridade == Raridade.Mitico || raridade == Raridade.Alter || raridade == Raridade.Potencial)
                    {
                        manaScript.AdicionarManaDivina(1, tagDoVencedor == "Token Player"); // Adiciona 1 de Mana Divina
                        Debug.Log($"{tagDoVencedor} ganhou 1 Mana Divina por derrotar {nomeDoToken} ({raridade}).");
                    }

                    Debug.Log($"{tagDoVencedor} ganhou mana por derrotar {nomeDoToken}.");
                }
            }
            DerrotarToken();
        }
    }

    public bool estaVivo => vida > 0;

    private void DerrotarToken()
    {
        Debug.Log($"{nomeDoToken} foi derrotado!");
        if (turnManager != null)
        {
            turnManager.RemoverTokenDerrotado(gameObject); // Informa ao TurnManager para remover
        }
        // O TokenDragDrop também precisa saber que o token foi derrotado para não tentar manipulá-lo
        TokenDragDrop dragDrop = GetComponent<TokenDragDrop>();
        if (dragDrop != null) dragDrop.SetDefeated(); // Desativa o script de drag/drop
        Destroy(gameObject);
    }

    // NOVO: Método para deselar a carta
    public void Deselar()
    {
        isSealed = false;
        Debug.Log($"{nomeDoToken} foi deselado!");
        // A lógica de remoção do buff selado será tratada no TurnManager
    }

    // NOVO: Método para fazer a Cópia da Habilidade
    public void CopiarHabilidade(Token alvo)
    {
        // 1. Se for a primeira cópia, salva os atributos originais
        if (!isAbilityCopied)
        {
            originalActiveAbilityType = activeAbilityType;
            originalAbilityCost = abilityCost;
            originalAbilityDamage = abilityDamage;
            originalNumCardsToSummon = numCardsToSummon;

            // Cópia profunda das listas de buffs e invocáveis
            originalAbilityBuffEffects.Clear();
            if (abilityBuffEffects != null) originalAbilityBuffEffects.AddRange(abilityBuffEffects);
            originalSummonableCards.Clear();
            if (summonableCards != null) originalSummonableCards.AddRange(summonableCards);
        }

        // 2. Aplica os atributos do alvo (Garantindo que as listas não sejam nulas no alvo)
        activeAbilityType = alvo.activeAbilityType;
        abilityCost = alvo.abilityCost;
        abilityDamage = alvo.abilityDamage;
        numCardsToSummon = alvo.numCardsToSummon;

        abilityBuffEffects.Clear();
        if (alvo.abilityBuffEffects != null) abilityBuffEffects.AddRange(alvo.abilityBuffEffects);
        summonableCards.Clear();
        if (alvo.summonableCards != null) summonableCards.AddRange(alvo.summonableCards);

        // 3. Marca como copiado
        isAbilityCopied = true;

        Debug.Log($"{nomeDoToken} copiou a habilidade '{activeAbilityType}' de {alvo.nomeDoToken}.");
    }

    // NOVO: Método para restaurar a Habilidade Original (Chamado pelo TurnManager)
    public void RestaurarHabilidadeOriginal()
    {
        if (isAbilityCopied)
        {
            activeAbilityType = originalActiveAbilityType;
            abilityCost = originalAbilityCost;
            abilityDamage = originalAbilityDamage;
            numCardsToSummon = originalNumCardsToSummon;

            abilityBuffEffects.Clear();
            abilityBuffEffects.AddRange(originalAbilityBuffEffects);
            originalAbilityBuffEffects.Clear(); // Limpa backup após restauração

            summonableCards.Clear();
            summonableCards.AddRange(originalSummonableCards);
            originalSummonableCards.Clear(); // Limpa backup após restauração

            isAbilityCopied = false;

            Debug.Log($"{nomeDoToken} restaurou sua habilidade original '{originalActiveAbilityType}'.");
        }
    }
}