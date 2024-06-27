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

    // Start is called before the first frame update
    private void Awake() 
    {
        ativa = false;
    }
    void Start()
    {
        scalaInicial = transform.localScale;
    }

    // Update is called once per frame
    void Update()
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
        if(ativa)
        {
            transform.localScale = new Vector3(0.2f, 0.2f, 0);
        }
        
    }

    private void OnMouseExit() 
    {
        if(ativa && !clicada) 
        {
            transform.localScale = scalaInicial;
        }
        
    }

    private void OnMouseDown() 
    {
        if(ativa && !clicada)
        {
            clicada = true;
            transform.localScale = new Vector3(0.2f, 0.2f, 0);
        }
        else if(ativa && clicada)
        {
            clicada = false;
            transform.localScale = scalaInicial;
        }
    }

    public bool CartaClicada()
    {
        return clicada;
    }
}
