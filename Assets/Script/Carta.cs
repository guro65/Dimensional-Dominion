using UnityEngine;

public class Carta : MonoBehaviour
{
    [Header("Informações da Carta")]
    public string nomeDaCarta;

    public Raridade raridade;

    [Header("Atributos da Carta")]
    public int dano;
    public int defesa;
    public int vida;

    // Aqui você pode adicionar funções futuramente, como atacar, receber dano, etc.
}

public enum Raridade
{
    Comum,
    Raro,
    Epico,
    Lendario,
    Mitico
}
