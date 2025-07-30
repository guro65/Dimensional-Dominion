using UnityEngine;

public class Token : MonoBehaviour
{
    [Header("Dados do Token")]
    public string nomeDoToken;
    public int dano;
    public int vida;
    public int manaCusto; // Custo para jogar o token
    public Raridade raridade;
    public float chanceDeAparicao = 25f;

    [Header("Habilidade Especial do Token")]
    public int danoEspecial = 8;        // Dano da habilidade especial deste token
    public int custoManaEspecial = 5;   // Custo de mana para usar a habilidade especial deste token
    public bool habilidadeAtivada = false; // Flag para habilidade ativada no turno

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

    public void Atacar(Token alvo)
    {
        if (alvo != null && estaVivo)
        {
            Debug.Log($"{nomeDoToken} atacou {alvo.nomeDoToken} causando {dano} de dano.");
            alvo.ReceberDano(dano, this);
        }
    }

    public void UsarHabilidadeEspecial(Token alvo)
    {
        if (manaScript != null)
        {
            // O gasto de mana para a habilidade é verificado e gasto no Combate.cs ou TurnManager.cs (quando ativada a flag)
            // Aqui, apenas executa o efeito se a flag está ativada.
            Debug.Log($"{nomeDoToken} (Habilidade Especial) atacou {alvo.nomeDoToken} causando {danoEspecial} de dano.");
            alvo.ReceberDano(danoEspecial, this);
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
                    manaScript.AdicionarManaPorRaridade(raridade, tagDoVencedor);
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