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

    [Header("Probabilidade por raridade (pode somar qualquer valor)")]
    [Range(0, 100)] public int chanceComum = 60;
    [Range(0, 100)] public int chanceIncomum = 25;
    [Range(0, 100)] public int chanceRaro = 10;
    [Range(0, 100)] public int chanceEpico = 4;
    [Range(0, 100)] public int chanceLendario = 1;
    [Range(0, 100)] public int chanceMitico = 0;

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
    public GameObject painelInventarioCheio;

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
                if (AutoSell.instance != null && AutoSell.instance.EstaMarcadoParaAutoSell(sorteado.raridade))
                {
                    int valor = AutoSell.instance.ValorVenda(sorteado.raridade);
                    Moeda.instance.AdicionarMoedas(valor);
                }
                else
                {
                    Token novoToken = Inventario.instance.AdicionarToken(sorteado);
                    if (novoToken != null)
                    {
                        tokensParaRevelar = new List<Token> { novoToken };
                        indiceTokenAtual = 0;
                        MostrarPainelRevelacao();
                    }
                    else
                    {
                        MostrarPainelInventarioCheio("Inventário cheio! Não é possível adicionar o token.");
                    }
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
                    if (AutoSell.instance != null && AutoSell.instance.EstaMarcadoParaAutoSell(sorteado.raridade))
                    {
                        int valor = AutoSell.instance.ValorVenda(sorteado.raridade);
                        Moeda.instance.AdicionarMoedas(valor);
                    }
                    else
                    {
                        Token novoToken = Inventario.instance.AdicionarToken(sorteado);
                        if (novoToken != null)
                        {
                            tokensParaRevelar.Add(novoToken);
                        }
                        else
                        {
                            MostrarPainelInventarioCheio("Inventário ficou cheio durante a roleta.");
                            break;
                        }
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

    // Método robusto de sorteio de raridade
    Token.Raridade SorteiaRaridade()
    {
        int[] chances = new int[] { chanceComum, chanceIncomum, chanceRaro, chanceEpico, chanceLendario, chanceMitico };
        int soma = 0;
        foreach (var c in chances) soma += c;

        if (soma <= 0)
        {
            Debug.LogError("A soma das chances das raridades está zero ou negativa!");
            return Token.Raridade.Comum;
        }

        int rand = Random.Range(0, soma); // [0, soma)
        for (int i = 0; i < chances.Length; i++)
        {
            if (rand < chances[i])
                return (Token.Raridade)i;
            rand -= chances[i];
        }

        Debug.LogWarning("Falha ao sortear raridade, retornando Comum.");
        return Token.Raridade.Comum;
    }
}