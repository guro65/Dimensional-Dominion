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

    private Combate combateScript;
    private Mana manaScript;

    private void Start()
    {
        combateScript = FindObjectOfType<Combate>();
        manaScript = FindObjectOfType<Mana>();
    }

    private void OnMouseDown()
    {
        if (combateScript != null)
        {
            combateScript.SelecionarToken(gameObject);
        }
    }

    public enum Raridade
    {
        Comum,
        Incomum,
        Raro,
        Epico,
        Lendario,
        Mitico
    }

    public void Atacar(Token alvo)
    {
        if (alvo != null && estaVivo)
        {
            alvo.ReceberDano(dano, this);
        }
    }

    public void ReceberDano(int quantidade, Token atacante)
    {
        vida -= quantidade;
        if (vida <= 0)
        {
            if (manaScript != null && atacante != null)
            {
                string tagDoVencedor = atacante.CompareTag("Token Player") ? "Token Player" : (atacante.CompareTag("Token Oponente") ? "Token Oponente" : "");
                if (!string.IsNullOrEmpty(tagDoVencedor))
                {
                    manaScript.AdicionarManaPorRaridade(raridade, tagDoVencedor);
                }
            }
            DerrotarToken();
        }
    }

    public bool estaVivo => vida > 0;

    private void DerrotarToken()
    {
        Destroy(gameObject);
    }
}