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
        Alter,
        Potencial
    }

    // Nova enum para a posição no tabuleiro
    public enum PosicaoTabuleiro
    {
        NaoNoTabuleiro,
        Frente,
        Tras
    }
    public PosicaoTabuleiro PosicaoNoTab = PosicaoTabuleiro.NaoNoTabuleiro;

    // --- Novas Adições para Sistema de Buff ---
    public enum TokenType // Tipo de Token: Dano normal ou Buff
    {
        Dano,
        Buff
    }
    public TokenType tokenType = TokenType.Dano; // Padrão é token de Dano

    public enum BuffType // Tipo específico de Buff (se TokenType for Buff)
    {
        None,   // Sem buff
        Sorte,  // Aumenta chances de raridades altas
        Forca,  // Aumenta dano de todas as cartas
        Energia // Aumenta mana ganha ao derrotar cartas
    }
    public BuffType buffType = BuffType.None; // Padrão é sem buff
    [Range(0, 100)] public float buffPercentage = 0; // Porcentagem do buff (0-100)

    [Header("Habilidade Especial do Token")]
    public int danoEspecialBase = 8;        // Dano base da habilidade especial
    public int custoManaEspecial = 5;       // Custo de mana para usar a habilidade especial
    public bool habilidadeAtivada = false; // Flag para habilidade ativada no turno

    private Mana manaScript;
    private TurnManager turnManager; // Referência ao TurnManager

    private void Awake() // Mudado para Awake para garantir que manaScript e turnManager existam antes de Start de outros scripts
    {
        manaScript = FindObjectOfType<Mana>();
        turnManager = FindObjectOfType<TurnManager>();

        if (manaScript == null) Debug.LogError("Mana script not found for Token: " + name);
        if (turnManager == null) Debug.LogError("TurnManager script not found for Token: " + name);

        // Adiciona um BoxCollider2D se não tiver (necessário para Drag&Drop e OnMouseDownAsButton)
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
        }
    }

    public void Atacar(Token alvo, float forcaBuffPercent) // Recebe o buff de força
    {
        if (alvo != null && estaVivo)
        {
            int danoEfetivo = Mathf.RoundToInt(danoBase * (1 + (forcaBuffPercent / 100f)));
            Debug.Log($"{nomeDoToken} atacou {alvo.nomeDoToken} causando {danoEfetivo} de dano (com {forcaBuffPercent}% Força Buff).");
            alvo.ReceberDano(danoEfetivo, this);
        } else {
            Debug.LogWarning($"{nomeDoToken} tentou atacar mas o alvo é nulo ou o atacante não está vivo.");
        }
    }

    public void UsarHabilidadeEspecial(Token alvo, float forcaBuffPercent) // Recebe o buff de força
    {
        if (alvo != null && estaVivo)
        {
            int danoEfetivo = Mathf.RoundToInt(danoEspecialBase * (1 + (forcaBuffPercent / 100f)));
            Debug.Log($"{nomeDoToken} (Habilidade Especial) atacou {alvo.nomeDoToken} causando {danoEfetivo} de dano (com {forcaBuffPercent}% Força Buff).");
            alvo.ReceberDano(danoEfetivo, this);
        } else {
             Debug.LogWarning($"{nomeDoToken} tentou usar habilidade especial mas o alvo é nulo ou o atacante não está vivo.");
        }
    }

    public void ReceberDano(int quantidade, Token atacante)
    {
        vida -= quantidade;
        Debug.Log($"{nomeDoToken} recebeu {quantidade} de dano. Vida restante: {vida}");
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
        if(dragDrop != null) dragDrop.SetDefeated(); // Desativa o script de drag/drop
        Destroy(gameObject);
    }
}