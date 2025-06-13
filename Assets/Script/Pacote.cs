using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pacote : MonoBehaviour
{
    [System.Serializable]
    public class PossivelToken
    {
        public Token prefabToken;
    }

    [Header("Configurações do Pacote")]
    public string nomeDoPacote = "Pacote Básico";
    public List<PossivelToken> tokensPossiveis = new List<PossivelToken>();

    [Header("Probabilidade por raridade (soma 100%)")]
    [Range(0, 100)] public int chanceComum = 60;
    [Range(0, 100)] public int chanceIncomum = 25;
    [Range(0, 100)] public int chanceRaro = 10;
    [Range(0, 100)] public int chanceEpico = 4;
    [Range(0, 100)] public int chanceLendario = 1;
    [Range(0, 100)] public int chanceMitico = 0; // nova linha para mitico

    public int preco1Token = 500;
    public int preco10Tokens = 4000;

    [Header("Botões de Abertura")]
    public Button botaoAbrir1;
    public Button botaoAbrir10;

    [Header("Painel de Revelação")]
    public GameObject painelReveal;
    public Image imagemToken;
    public Text textoNomeToken;

    [Header("Painel Aviso Inventário Cheio")]
    public GameObject painelInventarioCheio; // Opcional: painel para mostrar msg de inventário cheio

    private List<Token> tokensParaRevelar = new List<Token>();
    private int indiceTokenAtual = 0;
    private bool revelando = false;

    void Start()
    {
        if (botaoAbrir1 != null)
        {
            botaoAbrir1.onClick.RemoveAllListeners();
            botaoAbrir1.onClick.AddListener(Abrir1Token);
        }
        if (botaoAbrir10 != null)
        {
            botaoAbrir10.onClick.RemoveAllListeners();
            botaoAbrir10.onClick.AddListener(Abrir10Tokens);
        }

        if (painelReveal != null)
            painelReveal.SetActive(false);

        if (painelInventarioCheio != null)
            painelInventarioCheio.SetActive(false);
    }

    public void Abrir1Token()
    {
        if (revelando) return;

        // Verifica se o inventário está cheio antes de gastar moedas
        if (Inventario.instance == null || Inventario.instance.EstaCheio())
        {
            MostrarPainelInventarioCheio("Inventário cheio! Não é possível roletar.");
            return;
        }

        if (Moeda.instance.GastarMoedas(preco1Token))
        {
            Token sorteado = SorteiaToken();
            if (sorteado != null)
            {
                if (Inventario.instance.AdicionarToken(sorteado))
                {
                    tokensParaRevelar = new List<Token> { sorteado };
                    indiceTokenAtual = 0;
                    MostrarPainelRevelacao();
                }
                else
                {
                    MostrarPainelInventarioCheio("Inventário cheio! Não é possível adicionar o token.");
                }
            }
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
        }
    }

    public void Abrir10Tokens()
    {
        if (revelando) return;

        // Verifica se há espaço suficiente para 10 tokens
        if (Inventario.instance == null || !Inventario.instance.TemEspacoParaAdicionar(10))
        {
            MostrarPainelInventarioCheio("Espaço insuficiente no inventário para 10 tokens.");
            return;
        }

        if (Moeda.instance.GastarMoedas(preco10Tokens))
        {
            tokensParaRevelar = new List<Token>();
            for (int i = 0; i < 10; i++)
            {
                Token sorteado = SorteiaToken();
                if (sorteado != null)
                {
                    if (Inventario.instance.AdicionarToken(sorteado))
                    {
                        tokensParaRevelar.Add(sorteado);
                    }
                    else
                    {
                        // Caso algum token não possa ser adicionado (difícil acontecer aqui)
                        MostrarPainelInventarioCheio("Inventário ficou cheio durante a roleta.");
                        break;
                    }
                }
            }
            if (tokensParaRevelar.Count > 0)
            {
                indiceTokenAtual = 0;
                MostrarPainelRevelacao();
            }
        }
        else
        {
            Debug.Log("Moedas insuficientes!");
        }
    }

    void MostrarPainelRevelacao()
    {
        revelando = true;
        if (painelReveal != null)
        {
            painelReveal.SetActive(true);
            MostrarTokenAtual();
        }
    }

    void MostrarPainelInventarioCheio(string mensagem)
    {
        if (painelInventarioCheio != null)
        {
            painelInventarioCheio.SetActive(true);
            Text texto = painelInventarioCheio.GetComponentInChildren<Text>();
            if (texto != null)
                texto.text = mensagem;
        }
        else
        {
            Debug.Log(mensagem);
        }
    }

    void MostrarTokenAtual()
    {
        if (tokensParaRevelar == null || tokensParaRevelar.Count == 0) return;
        Token token = tokensParaRevelar[indiceTokenAtual];
        textoNomeToken.text = token.nomeDoToken + " (" + token.raridade + ")";

        SpriteRenderer sr = token.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            imagemToken.sprite = sr.sprite;
            imagemToken.enabled = true;
        }
        else
        {
            imagemToken.enabled = false;
        }
    }

    void Update()
    {
        if (revelando && painelReveal != null && painelReveal.activeSelf && Input.GetMouseButtonDown(0))
        {
            indiceTokenAtual++;
            if (indiceTokenAtual < tokensParaRevelar.Count)
            {
                MostrarTokenAtual();
            }
            else
            {
                painelReveal.SetActive(false);
                revelando = false;
            }
        }
    }

    Token SorteiaToken()
    {
        Token.Raridade raridade = SorteiaRaridade();
        var candidatos = tokensPossiveis.FindAll(t => t.prefabToken.raridade == raridade);
        if (candidatos.Count == 0)
        {
            Debug.LogWarning($"Nenhum token da raridade {raridade} neste pacote");
            return null;
        }
        int idx = Random.Range(0, candidatos.Count);
        return candidatos[idx].prefabToken;
    }

    Token.Raridade SorteiaRaridade()
    {
        int rand = Random.Range(1, 101);
        if (rand <= chanceComum) return Token.Raridade.Comum;
        if (rand <= chanceComum + chanceIncomum) return Token.Raridade.Incomum;
        if (rand <= chanceComum + chanceIncomum + chanceRaro) return Token.Raridade.Raro;
        if (rand <= chanceComum + chanceIncomum + chanceRaro + chanceEpico) return Token.Raridade.Epico;
        if (rand <= chanceComum + chanceIncomum + chanceRaro + chanceEpico + chanceLendario) return Token.Raridade.Lendario;
        return Token.Raridade.Mitico;
    }
}