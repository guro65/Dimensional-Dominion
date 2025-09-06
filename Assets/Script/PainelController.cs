using UnityEngine;
using UnityEngine.UI; // Necessário para interagir com elementos da UI (como o Button e o Panel)

public class PanelController : MonoBehaviour
{
    [SerializeField]
    private GameObject targetPanel; // Variável para arrastar o painel que queremos mostrar/esconder

    void Start()
    {
        // Garante que o painel esteja inicialmente desativado ao iniciar o jogo.
        // Isso é útil caso você esqueça de desativá-lo manualmente no editor.
        if (targetPanel != null)
        {
            targetPanel.SetActive(false);
        }
    }

    // Este método será chamado quando o botão for clicado
    public void TogglePanelVisibility()
    {
        if (targetPanel != null)
        {
            // Inverte o estado de ativação do painel:
            // Se estiver ativo, desativa. Se estiver inativo, ativa.
            targetPanel.SetActive(!targetPanel.activeSelf);
        }
        else
        {
            Debug.LogWarning("O painel alvo não foi atribuído no script PanelController.");
        }
    }
}