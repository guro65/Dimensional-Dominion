using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CombateBoss : MonoBehaviour
{
    public PlayerBoss player;
    public PlayerBoss oponente; // Mudei para PlayerBoss
    public TextMeshProUGUI textoIndicador;
    public BoxCollider2D localCartas;
    public GameObject textoBotao;
    public GameObject painelResultado; 
    public TextMeshProUGUI textoResultado; 
    public Button botaoMudarCena; 
    public Button botaoReplay;
    private GameObject cartaAtivaPlayer;
    private GameObject cartaAtivaOponente;
    private int vez;
    public bool aguardaVez = true;
    public float tempoTurno;
    private bool cartaOponenteSelecionada = false;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<PlayerBoss>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<PlayerBoss>(); // Mudei para PlayerBoss
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
            else if (vez == 2 && !cartaOponenteSelecionada)
            {
                textoIndicador.text = "Aguarde o oponente jogar";
                AtualizaTextoBotao("Oponente Jogar");
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
                player.cartasPlayer--; // Atualiza o número de cartas do player
                VerificaVitoria();
            }

            if (cartaOponente.defesa <= 0)
            {
                Destroy(cartaAtivaOponente);
                cartaAtivaOponente = null;
                oponente.cartasOponente--; // Atualiza o número de cartas do oponente
                VerificaDerrota();
            }
        }
    }

    private void VerificaVitoria()
    {
        if (oponente.cartasOponente <= 0)
        {
            MostrarResultado("Você venceu!");
        }
    }

    private void VerificaDerrota()
    {
        if (player.cartasPlayer <= 0)
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

    private GameObject EscolherCartaAleatoria(PlayerBoss jogador)
    {
        List<GameObject> deck = jogador.DeckNaTela();
        if (deck != null && deck.Count > 0)
        {
            int indiceAleatorio = Random.Range(0, deck.Count);
            return deck[indiceAleatorio];
        }
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
