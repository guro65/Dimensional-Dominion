using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Slots : MonoBehaviour
{
    [Header("Slots da Mão")]
    public List<Transform> playerHandSlots;
    public List<Transform> oponenteHandSlots;

    [Header("Slots do Tabuleiro (Configurados Manualmente)")]
    public List<Transform> playerFrontBoardSlots;
    public List<Transform> playerBackBoardSlots;
    public List<Transform> oponenteFrontBoardSlots;
    public List<Transform> oponenteBackBoardSlots;

    [HideInInspector] public List<Transform> playerBoardSlots;
    [HideInInspector] public List<Transform> oponenteBoardSlots;

    void Awake()
    {
        playerBoardSlots = new List<Transform>(playerFrontBoardSlots);
        playerBoardSlots.AddRange(playerBackBoardSlots);

        oponenteBoardSlots = new List<Transform>(oponenteFrontBoardSlots);
        oponenteBoardSlots.AddRange(oponenteBackBoardSlots);
    }

    public bool PlayerHandSlotDisponivel()
    {
        foreach (Transform slot in playerHandSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool OponenteHandSlotDisponivel()
    {
        foreach (Transform slot in oponenteHandSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool PlayerBoardSlotDisponivel()
    {
        foreach (Transform slot in playerBoardSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool OponenteBoardSlotDisponivel()
    {
        foreach (Transform slot in oponenteBoardSlots)
        {
            if (SlotEstaLivre(slot)) return true;
        }
        return false;
    }

    public bool SlotEstaLivre(Transform slot)
    {
        if (slot == null) return false;

        if (slot.childCount == 0) return true;

        Token tokenNoSlot = slot.GetComponentInChildren<Token>();
        return tokenNoSlot == null || !tokenNoSlot.estaVivo;
    }

    public Token.PosicaoTabuleiro GetPosicaoNoTabuleiro(Transform slotTransform, bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerFrontBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Frente;
            if (playerBackBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Tras;
        }
        else // is Oponente
        {
            if (oponenteFrontBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Frente;
            if (oponenteBackBoardSlots.Contains(slotTransform)) return Token.PosicaoTabuleiro.Tras;
        }
        return Token.PosicaoTabuleiro.NaoNoTabuleiro;
    }

    public List<Token> GetTokensNoTabuleiro(bool isPlayer)
    {
        List<Token> tokensNoTab = new List<Token>();
        List<Transform> slots = isPlayer ? playerBoardSlots : oponenteBoardSlots;

        foreach (Transform slot in slots)
        {
            if (slot.childCount > 0)
            {
                Token token = slot.GetComponentInChildren<Token>();
                if (token != null && token.estaVivo)
                {
                    tokensNoTab.Add(token);
                }
            }
        }
        return tokensNoTab;
    }

    public List<Transform> GetSlotsVaziosNoTabuleiro(bool isPlayer)
    {
        List<Transform> vazios = new List<Transform>();
        List<Transform> slots = isPlayer ? playerBoardSlots : oponenteBoardSlots;

        foreach (Transform slot in slots)
        {
            if (SlotEstaLivre(slot))
            {
                vazios.Add(slot);
            }
        }
        return vazios;
    }

    public Transform GetSlotCorrespondenteNaColuna(Transform currentSlot, bool isPlayer, Token.PosicaoTabuleiro targetPosicao)
    {
        List<Transform> boardSlotsToSearch = isPlayer ? playerBoardSlots : oponenteBoardSlots;

        foreach (Transform slot in boardSlotsToSearch)
        {
            if (GetPosicaoNoTabuleiro(slot, isPlayer) == targetPosicao)
            {
                if (Mathf.Abs(slot.position.x - currentSlot.position.x) < 0.1f)
                {
                    return slot;
                }
            }
        }
        return null;
    }

    // Função para encontrar um slot vazio adjacente a um slot específico
    public Transform FindAdjacentEmptySlot(Transform originalSlot, bool isPlayer)
    {
        List<Transform> boardSlots = isPlayer ? playerBoardSlots : oponenteBoardSlots;
        
        // Ordena os slots para garantir que a busca por adjacência seja consistente (ex: por X)
        boardSlots = boardSlots.OrderBy(s => s.position.x).ToList();

        int originalSlotIndex = boardSlots.IndexOf(originalSlot);
        if (originalSlotIndex == -1) return null; // Slot original não encontrado nas listas do tabuleiro

        // Prioriza slots adjacentes na mesma linha (Frente ou Trás)
        Token.PosicaoTabuleiro originalPos = GetPosicaoNoTabuleiro(originalSlot, isPlayer);
        List<Transform> targetRowSlots = originalPos == Token.PosicaoTabuleiro.Frente ? 
                                         (isPlayer ? playerFrontBoardSlots : oponenteFrontBoardSlots) :
                                         (isPlayer ? playerBackBoardSlots : oponenteBackBoardSlots);
        targetRowSlots = targetRowSlots.OrderBy(s => s.position.x).ToList(); // Ordena também

        int originalRowSlotIndex = targetRowSlots.IndexOf(originalSlot);
        
        // Tenta encontrar um slot adjacente na mesma linha (esquerda ou direita)
        if (originalRowSlotIndex != -1)
        {
            // Tenta slot à direita
            if (originalRowSlotIndex + 1 < targetRowSlots.Count)
            {
                Transform rightSlot = targetRowSlots[originalRowSlotIndex + 1];
                if (SlotEstaLivre(rightSlot)) return rightSlot;
            }
            // Tenta slot à esquerda
            if (originalRowSlotIndex - 1 >= 0)
            {
                Transform leftSlot = targetRowSlots[originalRowSlotIndex - 1];
                if (SlotEstaLivre(leftSlot)) return leftSlot;
            }
        }

        // Se não encontrou adjacente na mesma linha, tenta qualquer slot vazio restante na mesma linha
        foreach (Transform slot in targetRowSlots)
        {
            if (SlotEstaLivre(slot))
            {
                return slot;
            }
        }

        // Se a linha original estiver cheia, procura qualquer slot vazio no tabuleiro
        foreach (Transform slot in boardSlots)
        {
            if (SlotEstaLivre(slot))
            {
                return slot;
            }
        }
        
        return null; // Nenhum slot vazio adjacente ou no tabuleiro
    }

    public Transform GetPrimeiroSlotVazioFrente(bool isPlayer)
    {
        List<Transform> slotsFrente = isPlayer ? playerFrontBoardSlots : oponenteFrontBoardSlots;
        slotsFrente = slotsFrente.OrderBy(s => s.position.x).ToList();

        foreach (Transform slot in slotsFrente)
        {
            if (SlotEstaLivre(slot))
            {
                return slot;
            }
        }
        return null;
    }

    public Transform GetPrimeiroSlotVazioTras(bool isPlayer)
    {
        List<Transform> slotsTras = isPlayer ? playerBackBoardSlots : oponenteBackBoardSlots;
        slotsTras = slotsTras.OrderBy(s => s.position.x).ToList();

        foreach (Transform slot in slotsTras)
        {
            if (SlotEstaLivre(slot))
            {
                return slot;
            }
        }
        return null;
    }
}