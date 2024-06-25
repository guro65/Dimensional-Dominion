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
    private int vez;
    public bool aguardaVez = true;
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
        }
        else if(vez == 2 && aguardaVez)
        {
            VezDoOponente();
        }
    }

    private void VezDoPlayer()
    {
        textoIndicador.text = "Vez do jogador";
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
}
