using UnityEngine;

public class Token : MonoBehaviour
{
    [Header("Dados do Token")]
    public string nomeDoToken;
    public int dano;
    public int vida;
    public int manaCusto; // Esta vari�vel j� existe e � onde voc� define o custo
    public Raridade raridade;
    public float chanceDeAparicao = 25f; // Porcentagem ajust�vel para a distribui��o

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
            // Chama a fun��o que vai adicionar mana baseada na raridade do token derrotado
            if (manaScript != null && atacante != null)
            {
                // Verifica a tag do atacante para determinar quem derrotou o token
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