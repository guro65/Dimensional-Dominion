using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Combate : MonoBehaviour
{
    public Player player;
    public Player oponente;
    public TextMeshProUGUI textoIndicador;
    public BoxCollider2D localCartas;
    public GameObject textoBotao;
    public GameObject painelResultado;
    public TextMeshProUGUI textoResultado;
    public Button botaoMudarCena;
    public Button botaoReplay;
    public TokenDropManager tokenDropManager;
    private GameObject cartaAtivaPlayer;
    private GameObject cartaAtivaOponente;
    private int vez;
    public bool aguardaVez = true;
    public float tempoTurno;
    private bool cartaOponenteSelecionada = false;
    public int cartasPlayer = 5;
    public int cartasOponente = 5;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<Player>();
        textoIndicador = GameObject.FindWithTag("Indicador").GetComponent<TextMeshProUGUI>();
        localCartas = GameObject.FindWithTag("Local").GetComponent<BoxCollider2D>();
        textoBotao = GameObject.FindWithTag("TextoBotao");
        painelResultado.SetActive(false);
        botaoMudarCena.onClick.AddListener(Voltar);
        botaoReplay.onClick.AddListener(Replay);
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

        VerificaDerrota();
        VerificaVitoria();
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
                cartasPlayer--;
                VerificaVitoria();
            }

            
            if (cartaOponente.defesa <= 0)
            {
                
                tokenDropManager.OnCartaDerrotada(); 

                Destroy(cartaAtivaOponente);
                cartaAtivaOponente = null;
                cartasOponente--;
                VerificaDerrota();
            }
        }
    }


    private void VerificaVitoria()
    {
        if (cartasOponente <= 0)
        {
            MostrarResultado("Você venceu!");
        }
    }

    private void VerificaDerrota()
    {
        if (cartasPlayer <= 0)
        {
            MostrarResultado("Você perdeu!");
        }
    }

    private void MostrarResultado(string resultado)
    {
        painelResultado.SetActive(true);
        textoResultado.text = resultado;
        AtualizaTextoBotao("");
        StopAllCoroutines();
        aguardaVez = false;
    }

    public void Voltar()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Replay()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private GameObject EscolherCartaAleatoria(Player jogador)
    {
        List<GameObject> deck = jogador.DeckNaTela();
        bool naoTemCarta = true;
        do
        {
            int indiceAleatorio = Random.Range(0, deck.Count);
            if (deck[indiceAleatorio] != null)
            {
                Debug.Log("Carta escolhida: " + deck[indiceAleatorio].name);
                return deck[indiceAleatorio];
            }
        }
        while(naoTemCarta);
        
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


    public void ClicarToken(Token token)
    {
        if (cartaAtivaPlayer != null)
        {
            Carta cartaSelecionada = cartaAtivaPlayer.GetComponent<Carta>();
            
            if (token.ativo && cartaSelecionada != null)
            {
                token.SetCartaSelecionada(cartaSelecionada);
                Debug.Log($"Token clicado. Carta selecionada: {cartaSelecionada.NomeCarta()}");

                if (cartaSelecionada.Evoluiu())
                {
                    AtualizarCartaAtiva(cartaSelecionada);
                    Debug.Log("Carta evoluída e ativada no combate.");
                }
                else
                {
                    Debug.Log("Carta não evoluiu.");
                }
            }
            else
            {
                Debug.Log("Token não está ativo ou carta não selecionada.");
            }
        }
        else
        {
            Debug.Log("Nenhuma carta ativa selecionada.");
        }
    }

    
    private void AtualizarCartaAtiva(Carta novaCarta)
    {
        cartaAtivaPlayer = novaCarta.gameObject;
        Debug.Log($"Carta atualizada para: {novaCarta.NomeCarta()} após evolução.");
    }

}