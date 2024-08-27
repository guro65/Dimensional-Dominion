using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Para mudar de cena
using UnityEngine.UI; // Para usar o UI

public class Combate : MonoBehaviour
{
    public Player player;
    public Player oponente;
    public TextMeshProUGUI textoIndicador;
    public BoxCollider2D localCartas;
    public GameObject textoBotao;
    public GameObject painelResultado; // Painel de Resultado
    public TextMeshProUGUI textoResultado; // Texto do Painel de Resultado
    public Button botaoMudarCena; // Botão para mudar de cena

    private GameObject cartaAtivaPlayer;
    private GameObject cartaAtivaOponente;
    private int vez;
    public bool aguardaVez = true;
    public float tempoTurno;
    private bool cartaOponenteSelecionada = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<Player>();
        textoIndicador = GameObject.FindWithTag("Indicador").GetComponent<TextMeshProUGUI>();
        localCartas = GameObject.FindWithTag("Local").GetComponent<BoxCollider2D>();
        textoBotao = GameObject.FindWithTag("TextoBotao");
        painelResultado.SetActive(false); // Inicialmente escondido
        botaoMudarCena.onClick.AddListener(MudarCena);
        vez = 1;
        AtualizaTextoBotao("Inicie o Turno");
    }

    void Update()
    {
        if (aguardaVez)
        {
            if (vez == 1)
            {
                VezDoPlayer();
            }
            else if (vez == 2)
            {
                if (!cartaOponenteSelecionada)
                {
                    textoIndicador.text = "Aguarde o oponente jogar";
                    AtualizaTextoBotao("Oponente Jogar");
                }
            }
            else if (vez == 3)
            {
                FimdoTurno();
            }
        }
    }

    public void JogarCartaOponente()
    {
        if (vez == 2)
        {
            if (cartaAtivaOponente == null)
            {
                cartaAtivaOponente = EscolherCartaAleatoria(oponente);
                if (cartaAtivaOponente != null)
                {
                    cartaOponenteSelecionada = true;
                    MudaPosicaoCartaOponente();
                    StartCoroutine(ContadorTurno());
                }
                else
                {
                    textoIndicador.text = "Nenhuma carta disponível para o oponente.";
                    AtualizaTextoBotao("Inicie o próximo turno");
                    aguardaVez = false;
                    cartaOponenteSelecionada = true;
                }
            }
        }
    }

    private void VezDoPlayer()
    {
        if (cartaAtivaPlayer == null)
        {
            textoIndicador.text = "Sua vez! Selecione sua carta";
            AtualizaTextoBotao("Continuar");
            aguardaVez = false;
        }
        else
        {
            textoIndicador.text = "Você já tem uma carta ativa.";
            AtualizaTextoBotao("Aguarde o oponente");
        }
    }

    private void FimdoTurno()
    {
        textoIndicador.text = "Fim do Turno";
        AtualizaTextoBotao("Aguarde...");
        aguardaVez = false;
    }

    IEnumerator ContadorTurno()
    {
        yield return new WaitForSeconds(tempoTurno);
        FinalizarTurno();
    }

    public void FinalizarTurno()
    {
        StopCoroutine(ContadorTurno());

        if (vez == 1)
        {
            vez++;
            VerificaCartaAtivaPlayer();
            if (cartaAtivaPlayer != null)
            {
                MudaPosicaoCartaPlayer();
            }
        }
        else if (vez == 2)
        {
            vez++;
            cartaOponenteSelecionada = false;
        }
        else if (vez == 3)
        {
            vez = 1;
            FinalizaCombate();
            FimdoTurno();
        }

        aguardaVez = true;
        AtualizaTextoBotao("Inicie o Turno");
    }

    private void FinalizaCombate()
    {
        if (cartaAtivaPlayer != null && cartaAtivaOponente != null)
        {
            Carta cartaPlayer = cartaAtivaPlayer.GetComponent<Carta>();
            Carta cartaOponente = cartaAtivaOponente.GetComponent<Carta>();

            cartaOponente.CalculaDano(cartaPlayer.DanoCarta());
            cartaPlayer.CalculaDano(cartaOponente.DanoCarta());

            if (cartaPlayer.defesa <= 0)
            {
                Destroy(cartaAtivaPlayer);
                cartaAtivaPlayer = null;
                VerificaVitoria();
            }

            if (cartaOponente.defesa <= 0)
            {
                Destroy(cartaAtivaOponente);
                cartaAtivaOponente = null;
                VerificaDerrota();
            }
        }
    }

    private void VerificaVitoria()
    {
        if (oponente.DeckNaTela().Count == 0)
        {
            MostrarResultado("Você venceu!");
        }
    }

    private void VerificaDerrota()
    {
        if (player.DeckNaTela().Count == 0)
        {
            MostrarResultado("Você perdeu!");
        }
    }

    private void VerificaEmpate()
    {
        if (oponente.DeckNaTela().Count == 0 && player.DeckNaTela().Count == 0)
        {
            MostrarResultado("Empate!");
        }
    }

    private void MostrarResultado(string resultado)
    {
        textoResultado.text = resultado;
        painelResultado.SetActive(true);
        AtualizaTextoBotao(""); // Desativa o texto do botão enquanto exibe o painel
        StopAllCoroutines(); // Para qualquer corrotina que esteja rodando
        aguardaVez = false;
    }

    public void MudarCena()
    {
        SceneManager.LoadScene("Menu"); // Substitua pelo nome da sua cena
    }

    private GameObject EscolherCartaAleatoria(Player jogador)
    {
        List<GameObject> deck = jogador.DeckNaTela();
        if (deck != null && deck.Count > 0)
        {
            int indiceAleatorio = Random.Range(0, deck.Count);
            Debug.Log("Carta escolhida: " + deck[indiceAleatorio].name);
            return deck[indiceAleatorio];
        }
        Debug.Log("Nenhuma carta disponível no deck.");
        return null;
    }

    public void VerificaCartaAtivaPlayer()
    {
        List<GameObject> deckPlayer = player.DeckNaTela();
        cartaAtivaPlayer = null;
        foreach (GameObject carta in deckPlayer)
        {
            if (carta != null)
            {
                Carta cartaAtual = carta.GetComponent<Carta>();
                if (cartaAtual != null && cartaAtual.CartaClicada())
                {
                    cartaAtivaPlayer = carta;
                    cartaAtual.DesativaCarta();
                    break;
                }
            }
        }
    }

    public void MudaPosicaoCartaPlayer()
    {
        if (cartaAtivaPlayer != null)
        {
            float posCartaX = localCartas.transform.position.x + 2.3f;
            cartaAtivaPlayer.transform.position = new Vector3(posCartaX, localCartas.transform.position.y, 0);
        }
    }

    public void MudaPosicaoCartaOponente()
    {
        if (cartaAtivaOponente != null)
        {
            float posCartaX = localCartas.transform.position.x - 1.9f;
            cartaAtivaOponente.transform.position = new Vector3(posCartaX, localCartas.transform.position.y, 0);
        }
    }

    private void AtualizaTextoBotao(string texto)
    {
        if (textoBotao != null)
        {
            textoBotao.GetComponent<TextMeshProUGUI>().text = texto;
        }
    }
}
