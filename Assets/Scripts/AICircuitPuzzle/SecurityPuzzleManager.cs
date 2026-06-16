using System;
using System.Collections.Generic;
using UnityEngine;

public enum SecurityPuzzleMode
{
    ConfigureSignals
}

public enum SecurityPuzzleGate
{
    XOR,
    NOT
}

[Serializable]
public class SecurityPuzzleChallenge
{
    public SecurityPuzzleMode mode = SecurityPuzzleMode.ConfigureSignals;
    public SecurityPuzzleGate gate = SecurityPuzzleGate.XOR;
    public string title = "Desafio";
    [TextArea(1, 3)]
    public string instruction;
    public bool inputA;
    public bool inputB;
    public bool useInputB = true;
    public bool desiredOutput = true;
    public bool initialA;
    public bool initialB;
    public bool initialInverter;
}

public class SecurityPuzzleManager : MonoBehaviour
{
    [Header("Modo")]
    public bool randomModeOnStart = true;
    public SecurityPuzzleMode forcedMode = SecurityPuzzleMode.ConfigureSignals;

    [Header("Desafios")]
    public SecurityPuzzleChallenge[] challenges;

    [Header("Pontuacao")]
    public int lives = 3;
    public int score;
    public int maxScore = 50;
    public int pointsPerSuccess = 10;
    public string noLivesMessage = "Seguranca bloqueada. Sem vidas.";

    [Header("Cena")]
    public SecuritySignalSwitch signalA;
    public SecuritySignalSwitch signalB;
    public SecurityNotInverter notInverter;
    public GameObject[] xorObjects;
    public GameObject[] notObjects;

    [Header("Textos")]
    public TextMesh challengeTextMesh;
    public TextMesh feedbackTextMesh;
    public TextMesh scoreTextMesh;
    public TextMesh livesTextMesh;

    private SecurityPuzzleMode currentMode;
    private SecurityPuzzleChallenge[] activeChallenges;
    private bool[] completedChallenges;
    private SecurityPuzzleChallenge currentChallenge;
    private int currentChallengeIndex;
    private bool interactionsLocked;

    private void Start()
    {
        ConfigureReferences();
        StartMode(randomModeOnStart ? GetRandomMode() : forcedMode);
        UpdateScoreUI();
    }

    public void ToggleSignal(SecuritySignalSwitch signalSwitch)
    {
        if (InteractionsAreLocked() || currentChallenge == null)
        {
            return;
        }

        if (currentMode != SecurityPuzzleMode.ConfigureSignals || currentChallenge.gate != SecurityPuzzleGate.XOR)
        {
            SetFeedback("As chaves sao usadas na autenticacao XOR.");
            return;
        }

        if (signalSwitch == null)
        {
            return;
        }

        signalSwitch.SetSignalValue(!signalSwitch.Value);
        SetFeedback("Chave alternada. Somente uma deve ficar ativa.");
    }

    public void ToggleInverter(SecurityNotInverter inverter)
    {
        if (InteractionsAreLocked() || currentChallenge == null)
        {
            return;
        }

        if (currentMode != SecurityPuzzleMode.ConfigureSignals || currentChallenge.gate != SecurityPuzzleGate.NOT)
        {
            SetFeedback("O modulo NOT e usado para inverter os lasers.");
            return;
        }

        if (inverter == null)
        {
            return;
        }

        inverter.SetInverterEnabled(!inverter.Enabled);
        SetFeedback("Modulo NOT alternado. Olhe para VALIDAR.");
    }

    public void ValidateSignals()
    {
        if (InteractionsAreLocked() || currentChallenge == null)
        {
            return;
        }

        if (currentMode != SecurityPuzzleMode.ConfigureSignals)
        {
            SetFeedback("Escolha a saida 0 ou 1.");
            return;
        }

        bool output = EvaluateCurrentConfiguration();
        if (output == currentChallenge.desiredOutput)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }

    private void ConfigureReferences()
    {
        if (signalA != null)
        {
            signalA.manager = this;
        }

        if (signalB != null)
        {
            signalB.manager = this;
        }

        if (notInverter != null)
        {
            notInverter.manager = this;
        }

    }

    private void StartMode(SecurityPuzzleMode mode)
    {
        interactionsLocked = false;
        currentMode = mode;
        activeChallenges = GetChallengesForMode(mode);
        completedChallenges = new bool[activeChallenges.Length];
        UpdateModeVisibility();
        StartNextChallenge();
    }

    private SecurityPuzzleMode GetRandomMode()
    {
        return SecurityPuzzleMode.ConfigureSignals;
    }

    private bool HasChallengesForMode(SecurityPuzzleMode mode)
    {
        if (challenges == null)
        {
            return false;
        }

        for (int i = 0; i < challenges.Length; i++)
        {
            if (challenges[i] != null && challenges[i].mode == mode)
            {
                return true;
            }
        }

        return false;
    }

    private SecurityPuzzleChallenge[] GetChallengesForMode(SecurityPuzzleMode mode)
    {
        List<SecurityPuzzleChallenge> filtered = new List<SecurityPuzzleChallenge>();
        if (challenges != null)
        {
            for (int i = 0; i < challenges.Length; i++)
            {
                if (challenges[i] != null && challenges[i].mode == mode)
                {
                    filtered.Add(challenges[i]);
                }
            }
        }

        return filtered.ToArray();
    }

    private void StartNextChallenge()
    {
        if (activeChallenges == null || activeChallenges.Length == 0)
        {
            SetFeedback("Cadastre desafios da seguranca.");
            return;
        }

        int remaining = CountRemaining();
        if (remaining <= 0)
        {
            interactionsLocked = true;
            SetFeedback("Setor de seguranca restaurado.");
            return;
        }

        int chosenRemainingIndex = UnityEngine.Random.Range(0, remaining);
        for (int i = 0; i < completedChallenges.Length; i++)
        {
            if (completedChallenges[i])
            {
                continue;
            }

            if (chosenRemainingIndex == 0)
            {
                interactionsLocked = false;
                currentChallengeIndex = i;
                currentChallenge = activeChallenges[i];
                ApplyChallengeState();
                return;
            }

            chosenRemainingIndex--;
        }
    }

    private void ApplyChallengeState()
    {
        if (currentChallenge == null)
        {
            return;
        }

        if (signalA != null)
        {
            signalA.SetSignalValue(currentChallenge.initialA);
        }

        if (signalB != null)
        {
            signalB.SetSignalValue(currentChallenge.initialB);
        }

        if (notInverter != null)
        {
            notInverter.SetInverterEnabled(currentChallenge.initialInverter);
        }

        UpdateModeVisibility();
        UpdateChallengeUI();

        if (currentMode == SecurityPuzzleMode.ConfigureSignals && currentChallenge.gate == SecurityPuzzleGate.XOR)
        {
            SetFeedback("Ative somente uma chave e valide.");
        }
        else if (currentMode == SecurityPuzzleMode.ConfigureSignals)
        {
            SetFeedback("Acione o modulo NOT e valide.");
        }
        else
        {
            SetFeedback("Olhe para o painel 0 ou 1.");
        }
    }

    private void UpdateModeVisibility()
    {
        bool configureMode = currentMode == SecurityPuzzleMode.ConfigureSignals;
        bool xorChallenge = configureMode && currentChallenge != null && currentChallenge.gate == SecurityPuzzleGate.XOR;
        bool notChallenge = configureMode && currentChallenge != null && currentChallenge.gate == SecurityPuzzleGate.NOT;

        SetObjectsActive(xorObjects, xorChallenge);
        SetObjectsActive(notObjects, notChallenge);
    }

    private void HandleCorrectAnswer()
    {
        interactionsLocked = true;
        score = Mathf.Min(maxScore, score + pointsPerSuccess);
        if (completedChallenges != null && currentChallengeIndex >= 0 && currentChallengeIndex < completedChallenges.Length)
        {
            completedChallenges[currentChallengeIndex] = true;
        }

        SetFeedback("Acesso autorizado!");
        UpdateScoreUI();
        Invoke("StartNextChallenge", 1.5f);
    }

    private void HandleWrongAnswer()
    {
        lives = Mathf.Max(0, lives - 1);
        if (lives <= 0)
        {
            interactionsLocked = true;
            SetFeedback(noLivesMessage);
        }
        else
        {
            SetFeedback("Incorreto. Revise a regra logica.");
        }

        UpdateScoreUI();
    }

    private bool EvaluateCurrentConfiguration()
    {
        if (currentChallenge == null)
        {
            return false;
        }

        if (currentChallenge.gate == SecurityPuzzleGate.NOT)
        {
            bool inverterEnabled = notInverter != null && notInverter.Enabled;
            return inverterEnabled ? !currentChallenge.inputA : currentChallenge.inputA;
        }

        bool a = signalA != null && signalA.Value;
        bool b = signalB != null && signalB.Value;
        return Evaluate(SecurityPuzzleGate.XOR, a, b);
    }

    private bool Evaluate(SecurityPuzzleGate gate, bool a, bool b)
    {
        switch (gate)
        {
            case SecurityPuzzleGate.XOR:
                return a ^ b;
            case SecurityPuzzleGate.NOT:
                return !a;
            default:
                return false;
        }
    }

    private void UpdateChallengeUI()
    {
        if (challengeTextMesh == null || currentChallenge == null)
        {
            return;
        }

        if (currentMode == SecurityPuzzleMode.ConfigureSignals)
        {
            if (currentChallenge.gate == SecurityPuzzleGate.XOR)
            {
                challengeTextMesh.text = currentChallenge.title
                    + "\nAutenticacao: XOR"
                    + "\nObjetivo: liberar a porta"
                    + "\nAtive somente UMA chave"
                    + "\ne olhe para VALIDAR.";
            }
            else
            {
                string inputState = currentChallenge.inputA ? "ATIVO" : "INATIVO";
                string targetState = currentChallenge.desiredOutput ? "ATIVO" : "INATIVO";
                challengeTextMesh.text = currentChallenge.title
                    + "\nCircuito: NOT A"
                    + "\nSistema de lasers: " + inputState
                    + "\nObjetivo: deixar " + targetState
                    + "\nUse o modulo NOT.";
            }

            return;
        }

        challengeTextMesh.text = currentChallenge.title;
    }

    private string GateText(SecurityPuzzleChallenge challenge)
    {
        if (challenge == null)
        {
            return "?";
        }

        return challenge.gate == SecurityPuzzleGate.NOT ? "NOT A" : "A XOR B";
    }

    private void UpdateScoreUI()
    {
        if (scoreTextMesh != null)
        {
            scoreTextMesh.text = "Pontos: " + score + "/" + maxScore;
        }

        if (livesTextMesh != null)
        {
            livesTextMesh.text = "Vidas: " + lives;
        }
    }

    private void SetFeedback(string message)
    {
        if (feedbackTextMesh != null)
        {
            feedbackTextMesh.text = message;
        }
    }

    private void SetObjectsActive(GameObject[] objectsToToggle, bool active)
    {
        if (objectsToToggle == null)
        {
            return;
        }

        for (int i = 0; i < objectsToToggle.Length; i++)
        {
            if (objectsToToggle[i] != null)
            {
                objectsToToggle[i].SetActive(active);
            }
        }
    }

    private int CountRemaining()
    {
        int remaining = 0;
        if (completedChallenges == null)
        {
            return remaining;
        }

        for (int i = 0; i < completedChallenges.Length; i++)
        {
            if (!completedChallenges[i])
            {
                remaining++;
            }
        }

        return remaining;
    }

    private bool InteractionsAreLocked()
    {
        if (!interactionsLocked)
        {
            return false;
        }

        SetFeedback(lives <= 0 ? noLivesMessage : "Aguarde o proximo desafio.");
        return true;
    }

    private string Bit(bool value)
    {
        return value ? "1" : "0";
    }
}
