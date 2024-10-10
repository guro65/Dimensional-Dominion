using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Token : MonoBehaviour
{
    public bool ativo; // Define se o token está ativo
    public List<Carta> cartasValidas; // Lista de cartas que este token pode evoluir (aceita prefabs)
    public List<GameObject> prefabsCartaRaca; // Lista de prefabs de cartas raça correspondente
    public Carta cartaSelecionada; // Carta que foi selecionada para evolução
    [SerializeField] private TextMeshProUGUI statusEvolve; // Referência ao componente TextMeshProUGUI

    private void Awake()
    {
        ativo = false; // O token começa inativo
    }

    private void Start()
    {
        // Tenta encontrar o texto na cena por tag. Certifique-se de que o objeto está configurado corretamente.
        statusEvolve = GameObject.FindWithTag("StatusEvolve").GetComponent<TextMeshProUGUI>();
    }

    // Método para ativar o token ao clicar nele
    private void OnMouseDown()
    {
        ativo = !ativo; // Alterna o estado ativo/inativo ao clicar
        Debug.Log($"Token {NomeToken()} está agora {(ativo ? "ativo" : "inativo")}.");
    }

    private void OnMouseOver()
    {
        // Exibe o texto para selecionar a carta válida
        if (cartasValidas.Count > 0)
        {
            statusEvolve.text = "Clique no token e depois selecione a carta: " + cartasValidas[0].NomeCarta(); // Exibe o nome da primeira carta válida
        }
    }

    private void OnMouseExit()
    {
        statusEvolve.text = ""; // Limpa o texto ao sair do mouse
    }

    public void SetCartaSelecionada(Carta carta)
    {
        // Verifica se a carta selecionada tem as mesmas características de uma das cartas válidas
        if (VerificarCartaValida(carta)) 
        {
            // Verifica se o token já tem uma carta selecionada
            if (cartaSelecionada == null)
            {
                cartaSelecionada = carta; // Define a carta como selecionada
                Debug.Log($"A carta {carta.NomeCarta()} foi associada ao token {NomeToken()}.");

                SubstituirPorCartaRaca(); // Substitui a carta pela carta raça após a seleção
            }
            else
            {
                Debug.LogWarning($"O token {NomeToken()} já tem uma carta associada.");
            }
        }
        else
        {
            Debug.LogWarning($"A carta {carta.NomeCarta()} não é válida para o token {NomeToken()}.");
        }
    }

    private bool VerificarCartaValida(Carta carta)
    {
        // Compara o nome da carta selecionada com o nome das cartas válidas (ou outro identificador, se necessário)
        foreach (Carta cartaValida in cartasValidas)
        {
            if (cartaValida.NomeCarta() == carta.NomeCarta())
            {
                return true; // A carta selecionada é válida
            }
        }
        return false; // A carta não é válida
    }

    private int ObterIndexCartaSelecionada()
    {
        // Encontra o índice da carta válida com base no nome da carta selecionada
        for (int i = 0; i < cartasValidas.Count; i++)
        {
            if (cartasValidas[i].NomeCarta() == cartaSelecionada.NomeCarta())
            {
                return i;
            }
        }
        return -1; // Retorna -1 se não encontrar uma carta válida
    }

    public void ResetToken()
    {
        ativo = false;
        cartaSelecionada = null;
    }

    private void SubstituirPorCartaRaca()
    {
        if (cartaSelecionada != null && VerificarCartaValida(cartaSelecionada))
        {
            int index = ObterIndexCartaSelecionada();

            if (index >= 0 && index < prefabsCartaRaca.Count)
            {
                Vector3 posicaoCarta = cartaSelecionada.transform.position;
                Destroy(cartaSelecionada.gameObject);

                GameObject novaCarta = Instantiate(prefabsCartaRaca[index], posicaoCarta, Quaternion.identity);
                novaCarta.tag = "CartaPlayer";
                Debug.Log($"Carta {cartaSelecionada.NomeCarta()} evoluiu para: {novaCarta.GetComponent<Carta>().NomeCarta()}");

                GameObject.Find("Player").GetComponent<Player>().AdicionaCarta(novaCarta, index);
                Destroy(gameObject);
                ResetToken();
            }
            else
            {
                Debug.LogWarning("Nenhuma carta raça correspondente foi encontrada.");
            }
        }
        else
        {
            Debug.LogWarning("Nenhuma carta válida foi selecionada ou o token não pode evoluir essa carta.");
        }
    }

    public string NomeToken()
    {
        return gameObject.name;
    }
}