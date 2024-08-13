using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Combate : MonoBehaviour
{
    public Player player;
    public Player oponente;
    public TextMeshProUGUI textoIndicador;
    public BoxCollider2D localCartas;
    public GameObject cartaAtivaPlayer;
    public GameObject cartaAtivaOponente;
    private int vez;
    public bool aguardaVez = true;
    public float tempoTurno;
     
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        oponente = GameObject.FindWithTag("Oponente").GetComponent<Player>();
        textoIndicador = GameObject.FindWithTag("Indicador").GetComponent<TextMeshProUGUI>();
        localCartas = GameObject.FindWithTag("Local").GetComponent<BoxCollider2D>();
        vez = 1;
    }

    // Update is called once per frame
    void Update()
    {
        if(vez == 1 && aguardaVez)
        {
            VezDoPlayer();
            StartCoroutine("ContadorTurno");
        }
        else if(vez == 2 && aguardaVez)
        {
            VezDoOponente();
            StartCoroutine("ContadorTurno");
        }
    }

    private void VezDoPlayer()
    {
        textoIndicador.text = "Sua vez!";
        player.MinhaVez(true);
        oponente.MinhaVez(false);
        aguardaVez = !aguardaVez;
    }

    private void VezDoOponente()
    {
        textoIndicador.text = "Vez do oponente";
        player.MinhaVez(false);
        oponente.MinhaVez(true);
        aguardaVez = !aguardaVez;
    }

    IEnumerator ContadorTurno()
    {
        yield return new WaitForSeconds(tempoTurno);
        FinalizarTurno();
    }

    public void FinalizarTurno()
    {   
        if(vez == 1)
        {
           vez++; 
           VerificaCartaAtivaPlayer();
           MudaPosicaoCartaPlayer();
        }
        else
        {
            vez--;
            VerificaCartaAtivaOponente();
            MudaPosicaoCartaOponente();
        }

        aguardaVez = !aguardaVez;
        StopCoroutine("ContadorTurno");
    }

    public void VerificaCartaAtivaPlayer()
    {
        List<GameObject> deckPlayer = player.DeckNaTela();
        foreach(GameObject carta in deckPlayer)
        {
            Carta cartaAtual = carta.GetComponent<Carta>();
            if(cartaAtual.CartaClicada())
            {
                cartaAtivaPlayer = carta;
            }
        }
    }

    public void VerificaCartaAtivaOponente()
    {
        List<GameObject> deckOponente = oponente.DeckNaTela();
        foreach(GameObject carta in deckOponente)
        {
            Carta cartaAtual = carta.GetComponent<Carta>();
            if(cartaAtual.CartaClicada())
            {
                cartaAtivaOponente = carta;
            }
        }
    }

    public void MudaPosicaoCartaPlayer()
    {
        float posCartaX = localCartas.transform.position.x + 2.3f;
        cartaAtivaPlayer.transform.position = new Vector3(posCartaX,localCartas.transform.position.y,0);
    }

    public void MudaPosicaoCartaOponente()
    {
        float posCartaX = localCartas.transform.position.x + -1.9f;
        cartaAtivaOponente.transform.position = new Vector3(posCartaX,localCartas.transform.position.y,0);
    }
}