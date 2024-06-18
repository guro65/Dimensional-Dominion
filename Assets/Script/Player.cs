using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    public List<GameObject> deck = new List<GameObject>();
    public Baralho baralho;
    public int limite = 5;
    public GameObject localDeck;
    public float offsetX;
    public float tempo;
    // Start is called before the first frame update
    void Start()
    {
        baralho = GameObject.Find("Baralho").GetComponent<Baralho>();
        deck = baralho.DeckInicial(limite);
        StartCoroutine("ColocarCartasNaMesa");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator ColocarCartasNaMesa()
    {
        Vector3 posCarta = localDeck.transform.position;
        Vector3 offset = new Vector3(offsetX,0,0);
        foreach(GameObject carta in deck)
        {
            Instantiate(carta, posCarta += offset, Quaternion.identity);
            yield return new WaitForSeconds(tempo);
        }
    }
}
