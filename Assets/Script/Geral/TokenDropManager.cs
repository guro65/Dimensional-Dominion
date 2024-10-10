using System.Collections.Generic;
using UnityEngine;

public class TokenDropManager : MonoBehaviour
{
    public List<GameObject> tokens; // Lista de prefabs dos tokens disponíveis para dropar
    public List<GameObject> tokensDroppados; // Lista para rastrear os tokens já droppados na cena
    [Range(0, 1)]
    public float chanceDeDrop = 0.25f;

    // Espaçamento entre os tokens quando droppados
    public float espacamentoEntreTokens = 1.5f;
    public Vector3 posicaoInicial = new Vector3(-6.0f, -4.0f, 0); // Posição inicial do primeiro token dropado

    private void Awake()
    {
        tokensDroppados = new List<GameObject>(); // Inicializa a lista de tokens droppados
    }

    // Método chamado quando uma carta é derrotada
    public void OnCartaDerrotada()
    {
        DroparToken();
    }

    // Método para dropar o token
    private void DroparToken()
    {
        if (tokens.Count > 0)
        {
            if (Random.value <= chanceDeDrop)
            {
                // Calcula a posição do próximo token baseado na quantidade de tokens já droppados
                Vector3 posicaoDroppada = CalcularProximaPosicao();

                // Seleciona aleatoriamente um token da lista de tokens
                int indice = Random.Range(0, tokens.Count);
                GameObject tokenEscolhido = tokens[indice];

                // Instancia o token na posição calculada
                GameObject tokenDroppado = Instantiate(tokenEscolhido, posicaoDroppada, Quaternion.identity);
                tokensDroppados.Add(tokenDroppado); // Adiciona o token dropado à lista

                Debug.Log("Token droppado: " + tokenDroppado.name);
            }
            else
            {
                Debug.Log("Nenhum token foi droppado devido à chance de drop.");
            }
        }
        else
        {
            Debug.Log("Nenhum token disponível para drop.");
        }
    }

    // Método para calcular a posição do próximo token droppado
    private Vector3 CalcularProximaPosicao()
    {
        // Calcula a nova posição com base no número de tokens já droppados
        int quantidadeTokens = tokensDroppados.Count;
        float novaPosicaoX = posicaoInicial.x + (quantidadeTokens * espacamentoEntreTokens);
        return new Vector3(novaPosicaoX, posicaoInicial.y, posicaoInicial.z);
    }
}
