using UnityEngine;

public class Caixa : MonoBehaviour
{
    [SerializeField]private bool comprarToken;
    private Combate combatecript;
    private Mana mana;
    private int cartasLado;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        combatecript = GetComponent<Combate>();
        mana = GetComponent<Mana>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void QuantidadeDeCartas()
    {
        
    }

}
