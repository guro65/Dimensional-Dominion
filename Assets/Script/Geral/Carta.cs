using UnityEngine;
using TMPro;

public class Carta : MonoBehaviour
{
    public string carta;
    public string nomeEvolucao;
    public GameObject cartaEvolutivaPrefab;
    public int dano;
    public int defesa;
    public bool ativa;
    public Vector3 scalaInicial;
    public bool clicada = false;
    public bool evoluida = false;
    public SpriteRenderer sprite;
    [SerializeField] private TextMeshProUGUI statusDefesa;
    [SerializeField] private TextMeshProUGUI statusDano;
    [SerializeField] private TextMeshProUGUI statusNome;

    private Token tokenEvolucao;

    private void Awake() 
    {
        ativa = true;
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        scalaInicial = transform.localScale;
        statusDefesa = GameObject.FindWithTag("StatusDefesa").GetComponent<TextMeshProUGUI>();
        statusDano = GameObject.FindWithTag("StatusDano").GetComponent<TextMeshProUGUI>();
        statusNome = GameObject.FindWithTag("StatusNome").GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
       
    }

    public void AtivaCarta()
    {
        ativa = true;
    }

    public void DesativaCarta()
    {
        ativa = false;
    }

    private void OnMouseOver() 
    {
        if (ativa)
        {
            transform.localScale = new Vector3(0.15f, 0.15f, 1);
            sprite.sortingOrder = 2;
            statusDefesa.text = "Defesa: " + defesa;
            statusDano.text = "Dano: " + dano;
            statusNome.text = "Nome: " + (evoluida ? nomeEvolucao : carta);
        }
    }

    private void OnMouseExit() 
    {
        if (ativa && !clicada) 
        {
            transform.localScale = scalaInicial;
            sprite.sortingOrder = 2;
            statusDefesa.text = "";
            statusDano.text = "";
            statusNome.text = "";
        }
    }

    private void OnMouseDown()
    {
        if (ativa)
        {
            clicada = !clicada;
            transform.localScale = clicada ? new Vector3(0.15f, 0.15f, 1) : scalaInicial;

            Token token = FindObjectOfType<Token>();
            if (clicada && token != null && token.ativo)
            {
                token.SetCartaSelecionada(this);
            }
            else if (!clicada && token != null)
            {
                token.ResetToken();
            }
        }
    }

    public void SetTokenEvolucao(Token token)
    {
        tokenEvolucao = token;
    }

    public Token GetTokenEvolucao()
    {
        return tokenEvolucao;
    }

    public bool CartaClicada()
    {
        return clicada;
    }

    public bool VerificaCartaAtiva()
    {
        return ativa;
    }

    public int DanoCarta()
    {
        return dano;
    }

    public void CalculaDano(int danoInimigo)
    {
        defesa -= danoInimigo;

        if(defesa <= 0)
        {
            Destroy(gameObject);
        }
        else
        {
            sprite.sortingOrder = 2;
        }
    }

    public string NomeCarta()
    {
        return evoluida ? nomeEvolucao : carta;
    }

    public bool Evoluiu()
    {
        return evoluida;
    }

    public void Evoluir()
    {
        if (!evoluida)
        {
            evoluida = true;
            carta = nomeEvolucao;
            dano += 10;
            defesa += 10;
        }
    }
}