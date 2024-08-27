using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Carta : MonoBehaviour
{
    public string carta;
    public int dano;
    public int defesa;
    public bool ativa;
    public Vector3 scalaInicial;
    public bool clicada = false;
    public SpriteRenderer sprite;

    private void Awake() 
    {
        ativa = true;
        sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        scalaInicial = transform.localScale;
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
            sprite.sortingOrder = 1;
        }
    }

    private void OnMouseExit() 
    {
        if (ativa && !clicada) 
        {
            transform.localScale = scalaInicial;
            sprite.sortingOrder = 0;
        }
    }

    private void OnMouseDown() 
    {
        if (ativa)
        {
            clicada = !clicada;
            transform.localScale = clicada ? new Vector3(0.15f, 0.15f, 1) : scalaInicial;
            sprite.sortingOrder = clicada ? 1 : 0;
        }
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
    }
}