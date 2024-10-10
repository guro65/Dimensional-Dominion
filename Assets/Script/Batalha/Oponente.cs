using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oponente : MonoBehaviour
{
    public List<GameObject> deck = new List<GameObject>();
    public Baralho baralho;
    public int limite = 5;
    // Start is called before the first frame update
    void Start()
    {
        baralho = GameObject.Find("Baralho").GetComponent<Baralho>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
