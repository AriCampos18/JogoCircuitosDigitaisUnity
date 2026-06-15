using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class CircuitPuzzleManager : MonoBehaviour
{
    [Header("Modo")]
    public bool randomModeOnStart = true;
    public CircuitPuzzleMode forcedMode = CircuitPuzzleMode.BuildCircuit;

    [Header("Desafios")]
    public bool useDefaultFiveChallenges = true;
    public CircuitPuzzleChallenge[] buildCircuitChallenges;
    public CircuitPuzzleChallenge[] solveExerciseChallenges;

    [Header("Compatibilidade")]
    public bool randomChallengeOnStart = true;
    public int firstChallengeIndex;

    [Header("Montagem")]
    public LogicGateSlot[] slots;
    public bool requireExactGateSequence = true;

    [Header("Visibilidade por modo")]
    public bool updateModeVisibility = true;
    public bool autoFindModeObjects = true;
    public bool autoFindModeLabels = true;
    public GameObject[] buildCircuitModeObjects;
    public GameObject[] solveExerciseModeObjects;
    public string[] buildCircuitLabelNames = new string[]
    {
        "AND Label",
        "OR Label",
        "XOR Label",
        "NOT Label",
        "Slot Label",
        "Validate Label"
    };
    public string[] solveExerciseLabelNames = new string[]
    {
        "Answer 0 Label",
        "Answer 1 Label",
        "Option 0 Label",
        "Option 1 Label"
    };

    [Header("Pontuacao")]
    public int score;
    public int lives = 3;
    public int pointsPerSuccess = 10;
    public int pointsLostOnError = 2;
    public bool advanceAfterSuccess = true;

    [Header("UI")]
    public Text challengeText;
    public Text feedbackText;
    public Text scoreText;
    public Text livesText;

    [Header("Textos 3D opcionais")]
    public TextMesh challengeTextMesh;
    public TextMesh feedbackTextMesh;
    public TextMesh scoreTextMesh;
    public TextMesh livesTextMesh;

    [Header("Eventos opcionais")]
    public UnityEvent onCorrectAnswer;
    public UnityEvent onWrongAnswer;
    public UnityEvent onAllChallengesCompleted;

    private CircuitPuzzleMode currentMode;
    private CircuitPuzzleChallenge currentChallenge;
    private int currentChallengeIndex;
    private bool[] completedBuildChallenges;
    private bool[] completedExerciseChallenges;
    private LogicGateBlock selectedGate;

    [Header("Mensagem final")]
    public string successMessage = "Correto!";
    public string successChallengeText = "";

    private void Start()
    {
        ConfigureSceneReferences();
        ConfigureDefaultChallenges();
        StartMode(randomModeOnStart ? GetRandomMode() : forcedMode);
        UpdateScoreUI();
    }

    public void SelectGate(LogicGateBlock gateBlock)
    {
        if (currentMode != CircuitPuzzleMode.BuildCircuit)
        {
            SetFeedback("Este modo usa resposta 0 ou 1.");
            return;
        }

        if (selectedGate != null)
        {
            selectedGate.SetActiveState(false);
        }

        selectedGate = gateBlock;

        if (selectedGate != null)
        {
            selectedGate.SetActiveState(true);
            SetFeedback("Selecionado: " + LogicGateUtility.GetGateLabel(selectedGate.gateType) + ". Olhe para um slot.");
        }
    }

    public void PlaceSelectedGate(LogicGateSlot slot)
    {
        if (currentMode != CircuitPuzzleMode.BuildCircuit)
        {
            return;
        }

        if (slot == null)
        {
            return;
        }

        if (selectedGate == null)
        {
            SetFeedback("Selecione uma porta logica primeiro.");
            return;
        }

        if (slot.PlaceGate(selectedGate))
        {
            SetFeedback("Porta encaixada. Olhe para Validar.");
        }
        else
        {
            SetFeedback("Este slot ja esta ocupado.");
        }
    }

    public void ValidateCircuit()
    {
        if (currentChallenge == null)
        {
            SetFeedback("Nenhum desafio ativo.");
            return;
        }

        if (currentMode != CircuitPuzzleMode.BuildCircuit)
        {
            SetFeedback("Escolha 0 ou 1 para responder.");
            return;
        }

        int requiredSlots = currentChallenge.RequiredSlotCount;
        if (requiredSlots == 0)
        {
            SetFeedback("Desafio sem circuito configurado.");
            return;
        }

        if (slots == null || slots.Length < requiredSlots)
        {
            SetFeedback("Este desafio precisa de " + requiredSlots + " slots.");
            return;
        }

        LogicGateType[] placedGates = new LogicGateType[requiredSlots];
        for (int i = 0; i < requiredSlots; i++)
        {
            if (slots[i] == null || !slots[i].HasGate)
            {
                SetFeedback("Preencha todos os slots em ordem.");
                return;
            }

            placedGates[i] = slots[i].PlacedGateType;
        }

        bool outputMatches = currentChallenge.EvaluateWithPlacedGates(placedGates) == currentChallenge.desiredOutput;
        bool sequenceMatches = !requireExactGateSequence || GateSequenceMatches(placedGates, currentChallenge.steps);

        if (outputMatches && sequenceMatches)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }

    public void SelectExerciseAnswer(bool answer)
    {
        if (currentChallenge == null)
        {
            SetFeedback("Nenhum desafio ativo.");
            return;
        }

        if (currentMode != CircuitPuzzleMode.SolveExercise)
        {
            SetFeedback("Este modo usa os slots.");
            return;
        }

        bool correctAnswer = currentChallenge.EvaluateWithConfiguredSteps();
        if (answer == correctAnswer)
        {
            HandleCorrectAnswer();
        }
        else
        {
            HandleWrongAnswer();
        }
    }

    public void StartChallenge(int index)
    {
        CircuitPuzzleChallenge[] activeChallenges = GetActiveChallenges();
        if (activeChallenges == null || activeChallenges.Length == 0)
        {
            SetFeedback("Cadastre desafios no Inspector.");
            return;
        }

        currentChallengeIndex = Mathf.Clamp(index, 0, activeChallenges.Length - 1);
        currentChallenge = activeChallenges[currentChallengeIndex];
        currentChallenge.mode = currentMode;

        ClearSlots();
        ClearSelection();
        UpdateChallengeUI();

        if (currentMode == CircuitPuzzleMode.BuildCircuit)
        {
            SetFeedback("Monte o circuito e valide.");
        }
        else
        {
            SetFeedback("Olhe para 0 ou 1.");
        }
    }

    public void StartRandomChallenge()
    {
        StartNextRandomChallengeInCurrentMode();
    }

    public void NextChallenge()
    {
        StartNextRandomChallengeInCurrentMode();
    }

    private void StartMode(CircuitPuzzleMode mode)
    {
        currentMode = mode;
        UpdateModeVisibility();
        ResetCompletionForMode(currentMode);
        StartNextRandomChallengeInCurrentMode();
    }

    public void UpdateModeVisibility()
    {
        if (!updateModeVisibility)
        {
            return;
        }

        bool showBuildObjects = currentMode == CircuitPuzzleMode.BuildCircuit;
        SetObjectsActive(buildCircuitModeObjects, showBuildObjects);
        SetObjectsActive(solveExerciseModeObjects, !showBuildObjects);
    }

    public void SetModeObjects(CircuitPuzzleMode mode)
    {
        currentMode = mode;
        UpdateModeVisibility();
    }

    private void StartNextRandomChallengeInCurrentMode()
    {
        CircuitPuzzleChallenge[] activeChallenges = GetActiveChallenges();
        bool[] completedChallenges = GetActiveCompletionArray();

        if (activeChallenges == null || activeChallenges.Length == 0 || completedChallenges == null)
        {
            SetFeedback("Cadastre desafios no Inspector.");
            return;
        }

        int remaining = CountRemaining(completedChallenges);
        if (remaining <= 0)
        {
            SetFeedback("Modo concluido.");
            if (onAllChallengesCompleted != null)
            {
                onAllChallengesCompleted.Invoke();
            }
            return;
        }

        int chosenRemainingIndex = Random.Range(0, remaining);
        int challengeIndex = 0;
        for (int i = 0; i < completedChallenges.Length; i++)
        {
            if (completedChallenges[i])
            {
                continue;
            }

            if (chosenRemainingIndex == 0)
            {
                challengeIndex = i;
                break;
            }

            chosenRemainingIndex--;
        }

        StartChallenge(challengeIndex);
    }

    private void ConfigureSceneReferences()
    {
        LogicGateBlock[] gateBlocks = FindObjectsOfType<LogicGateBlock>();
        for (int i = 0; i < gateBlocks.Length; i++)
        {
            if (gateBlocks[i].puzzleManager == null)
            {
                gateBlocks[i].puzzleManager = this;
            }
        }

        if (slots == null || slots.Length == 0)
        {
            slots = FindObjectsOfType<LogicGateSlot>();
        }

        SortSlotsByOrder();

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && slots[i].puzzleManager == null)
            {
                slots[i].puzzleManager = this;
            }
        }

        ValidateCircuitButton[] buttons = FindObjectsOfType<ValidateCircuitButton>();
        for (int i = 0; i < buttons.Length; i++)
        {
            if (buttons[i].puzzleManager == null)
            {
                buttons[i].puzzleManager = this;
            }
        }

        ExerciseAnswerOption[] answerOptions = FindObjectsOfType<ExerciseAnswerOption>();
        for (int i = 0; i < answerOptions.Length; i++)
        {
            if (answerOptions[i].puzzleManager == null)
            {
                answerOptions[i].puzzleManager = this;
            }
        }

        ConfigureModeVisibilityObjects(gateBlocks, buttons, answerOptions);
    }

    private void ConfigureDefaultChallenges()
    {
        if (!useDefaultFiveChallenges)
        {
            EnsureCompletionArrays();
            return;
        }

        buildCircuitChallenges = CreateDefaultBuildChallenges();
        solveExerciseChallenges = CreateDefaultExerciseChallenges();
        advanceAfterSuccess = true;
        EnsureCompletionArrays();
    }

    private CircuitPuzzleChallenge[] CreateDefaultBuildChallenges()
    {
        CircuitPuzzleChallenge[] defaults = new CircuitPuzzleChallenge[5];
        defaults[0] = CreateChallenge(CircuitPuzzleMode.BuildCircuit, "Montagem 1", true, false, false, false, true, "A + B",
            Step(LogicGateType.OR, CircuitValueSource.A, CircuitValueSource.B));
        defaults[1] = CreateChallenge(CircuitPuzzleMode.BuildCircuit, "Montagem 2", true, true, false, false, true, "A * B",
            Step(LogicGateType.AND, CircuitValueSource.A, CircuitValueSource.B));
        defaults[2] = CreateChallenge(CircuitPuzzleMode.BuildCircuit, "Montagem 3", false, false, false, false, true, "!A",
            Step(LogicGateType.NOT, CircuitValueSource.A, CircuitValueSource.B));
        defaults[3] = CreateChallenge(CircuitPuzzleMode.BuildCircuit, "Montagem 4", true, false, false, false, true, "(A (+) B) + C",
            Step(LogicGateType.XOR, CircuitValueSource.A, CircuitValueSource.B),
            Step(LogicGateType.OR, CircuitValueSource.Previous, CircuitValueSource.C));
        defaults[4] = CreateChallenge(CircuitPuzzleMode.BuildCircuit, "Montagem 5", true, true, false, false, true, "(A * B) (+) C",
            Step(LogicGateType.AND, CircuitValueSource.A, CircuitValueSource.B),
            Step(LogicGateType.XOR, CircuitValueSource.Previous, CircuitValueSource.C));
        return defaults;
    }

    private CircuitPuzzleChallenge[] CreateDefaultExerciseChallenges()
    {
        CircuitPuzzleChallenge[] defaults = new CircuitPuzzleChallenge[5];
        defaults[0] = CreateChallenge(CircuitPuzzleMode.SolveExercise, "Exercicio 1", true, false, false, false, false, "A * B",
            Step(LogicGateType.AND, CircuitValueSource.A, CircuitValueSource.B));
        defaults[1] = CreateChallenge(CircuitPuzzleMode.SolveExercise, "Exercicio 2", false, false, false, false, false, "A + B",
            Step(LogicGateType.OR, CircuitValueSource.A, CircuitValueSource.B));
        defaults[2] = CreateChallenge(CircuitPuzzleMode.SolveExercise, "Exercicio 3", true, false, false, false, false, "!A",
            Step(LogicGateType.NOT, CircuitValueSource.A, CircuitValueSource.B));
        defaults[3] = CreateChallenge(CircuitPuzzleMode.SolveExercise, "Exercicio 4", true, false, false, false, false, "(A * B) + C",
            Step(LogicGateType.AND, CircuitValueSource.A, CircuitValueSource.B),
            Step(LogicGateType.OR, CircuitValueSource.Previous, CircuitValueSource.C));
        defaults[4] = CreateChallenge(CircuitPuzzleMode.SolveExercise, "Exercicio 5", true, true, false, false, false, "(A (+) B) + C",
            Step(LogicGateType.XOR, CircuitValueSource.A, CircuitValueSource.B),
            Step(LogicGateType.OR, CircuitValueSource.Previous, CircuitValueSource.C));
        return defaults;
    }

    private CircuitPuzzleChallenge CreateChallenge(CircuitPuzzleMode mode, string name, bool inputA, bool inputB, bool inputC, bool inputD, bool desiredOutput, string circuitText, params CircuitOperationStep[] steps)
    {
        CircuitPuzzleChallenge challenge = new CircuitPuzzleChallenge();
        challenge.mode = mode;
        challenge.challengeName = name;
        challenge.inputA = inputA;
        challenge.inputB = inputB;
        challenge.inputC = inputC;
        challenge.inputD = inputD;
        challenge.desiredOutput = desiredOutput;
        challenge.circuitText = circuitText;
        challenge.steps = steps;
        return challenge;
    }

    private CircuitOperationStep Step(LogicGateType gateType, CircuitValueSource leftInput, CircuitValueSource rightInput)
    {
        CircuitOperationStep step = new CircuitOperationStep();
        step.gateType = gateType;
        step.leftInput = leftInput;
        step.rightInput = rightInput;
        return step;
    }

    private void HandleCorrectAnswer()
    {
        score += pointsPerSuccess;
        MarkCurrentChallengeCompleted();
        SetFeedback(successMessage);

        if (!string.IsNullOrEmpty(successChallengeText))
        {
            if (challengeText != null)
                challengeText.text = successChallengeText;

            if (challengeTextMesh != null)
                challengeTextMesh.text = successChallengeText;
        }
        UpdateScoreUI();

        if (onCorrectAnswer != null)
        {
            onCorrectAnswer.Invoke();
        }

        if (advanceAfterSuccess)
        {
            Invoke("NextChallenge", 1.5f);
        }
    }

    private void HandleWrongAnswer()
    {
        lives = Mathf.Max(0, lives - 1);
        score = Mathf.Max(0, score - pointsLostOnError);
        SetFeedback("Incorreto. Tente novamente.");
        UpdateScoreUI();

        if (onWrongAnswer != null)
        {
            onWrongAnswer.Invoke();
        }
    }

    private bool GateSequenceMatches(LogicGateType[] placedGates, CircuitOperationStep[] expectedSteps)
    {
        if (placedGates == null || expectedSteps == null || placedGates.Length < expectedSteps.Length)
        {
            return false;
        }

        for (int i = 0; i < expectedSteps.Length; i++)
        {
            if (expectedSteps[i] == null || placedGates[i] != expectedSteps[i].gateType)
            {
                return false;
            }
        }

        return true;
    }

    private void ClearSlots()
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].ClearSlot();
            }
        }
    }

    private void ClearSelection()
    {
        if (selectedGate != null)
        {
            selectedGate.SetActiveState(false);
            selectedGate = null;
        }
    }

    private void UpdateChallengeUI()
    {
        if (currentChallenge == null)
        {
            return;
        }

        string text = currentChallenge.GetDisplayText();
        if (challengeText != null)
        {
            challengeText.text = text;
        }

        if (challengeTextMesh != null)
        {
            challengeTextMesh.text = text;
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Pontos: " + score;
        }

        if (livesText != null)
        {
            livesText.text = "Vidas: " + lives;
        }

        if (scoreTextMesh != null)
        {
            scoreTextMesh.text = "Pontos: " + score;
        }

        if (livesTextMesh != null)
        {
            livesTextMesh.text = "Vidas: " + lives;
        }
    }

    private void SetFeedback(string message)
    {
        if (feedbackText != null)
        {
            feedbackText.text = message;
        }

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

    private void ConfigureModeVisibilityObjects(LogicGateBlock[] gateBlocks, ValidateCircuitButton[] buttons, ExerciseAnswerOption[] answerOptions)
    {
        if (!autoFindModeObjects)
        {
            return;
        }

        if (buildCircuitModeObjects == null || buildCircuitModeObjects.Length == 0)
        {
            List<GameObject> buildObjects = new List<GameObject>();
            AddUniqueObjects(buildObjects, gateBlocks);
            AddUniqueObjects(buildObjects, slots);
            AddUniqueObjects(buildObjects, buttons);
            AddNamedObjects(buildObjects, buildCircuitLabelNames);
            buildCircuitModeObjects = buildObjects.ToArray();
        }
        else if (autoFindModeLabels)
        {
            List<GameObject> buildObjects = new List<GameObject>(buildCircuitModeObjects);
            AddNamedObjects(buildObjects, buildCircuitLabelNames);
            buildCircuitModeObjects = buildObjects.ToArray();
        }

        if (solveExerciseModeObjects == null || solveExerciseModeObjects.Length == 0)
        {
            List<GameObject> exerciseObjects = new List<GameObject>();
            AddUniqueObjects(exerciseObjects, answerOptions);
            AddNamedObjects(exerciseObjects, solveExerciseLabelNames);
            solveExerciseModeObjects = exerciseObjects.ToArray();
        }
        else if (autoFindModeLabels)
        {
            List<GameObject> exerciseObjects = new List<GameObject>(solveExerciseModeObjects);
            AddNamedObjects(exerciseObjects, solveExerciseLabelNames);
            solveExerciseModeObjects = exerciseObjects.ToArray();
        }
    }

    private void AddUniqueObjects<T>(List<GameObject> targetList, T[] components) where T : Component
    {
        if (components == null)
        {
            return;
        }

        for (int i = 0; i < components.Length; i++)
        {
            if (components[i] != null && !targetList.Contains(components[i].gameObject))
            {
                targetList.Add(components[i].gameObject);
            }
        }
    }

    private void AddNamedObjects(List<GameObject> targetList, string[] objectNames)
    {
        if (!autoFindModeLabels || objectNames == null)
        {
            return;
        }

        for (int i = 0; i < objectNames.Length; i++)
        {
            if (string.IsNullOrEmpty(objectNames[i]))
            {
                continue;
            }

            GameObject foundObject = GameObject.Find(objectNames[i]);
            if (foundObject != null && !targetList.Contains(foundObject))
            {
                targetList.Add(foundObject);
            }
        }
    }

    private CircuitPuzzleMode GetRandomMode()
    {
        return Random.Range(0, 2) == 0 ? CircuitPuzzleMode.BuildCircuit : CircuitPuzzleMode.SolveExercise;
    }

    private CircuitPuzzleChallenge[] GetActiveChallenges()
    {
        return currentMode == CircuitPuzzleMode.BuildCircuit ? buildCircuitChallenges : solveExerciseChallenges;
    }

    private bool[] GetActiveCompletionArray()
    {
        return currentMode == CircuitPuzzleMode.BuildCircuit ? completedBuildChallenges : completedExerciseChallenges;
    }

    private void EnsureCompletionArrays()
    {
        completedBuildChallenges = new bool[buildCircuitChallenges != null ? buildCircuitChallenges.Length : 0];
        completedExerciseChallenges = new bool[solveExerciseChallenges != null ? solveExerciseChallenges.Length : 0];
    }

    private void ResetCompletionForMode(CircuitPuzzleMode mode)
    {
        EnsureCompletionArrays();
        bool[] completed = mode == CircuitPuzzleMode.BuildCircuit ? completedBuildChallenges : completedExerciseChallenges;
        for (int i = 0; i < completed.Length; i++)
        {
            completed[i] = false;
        }
    }

    private int CountRemaining(bool[] completedChallenges)
    {
        int remaining = 0;
        for (int i = 0; i < completedChallenges.Length; i++)
        {
            if (!completedChallenges[i])
            {
                remaining++;
            }
        }

        return remaining;
    }

    private void MarkCurrentChallengeCompleted()
    {
        bool[] completed = GetActiveCompletionArray();
        if (completed != null && currentChallengeIndex >= 0 && currentChallengeIndex < completed.Length)
        {
            completed[currentChallengeIndex] = true;
        }
    }

    private void SortSlotsByOrder()
    {
        if (slots == null)
        {
            return;
        }

        for (int i = 0; i < slots.Length - 1; i++)
        {
            for (int j = i + 1; j < slots.Length; j++)
            {
                if (slots[i] != null && slots[j] != null && slots[j].slotOrder < slots[i].slotOrder)
                {
                    LogicGateSlot temp = slots[i];
                    slots[i] = slots[j];
                    slots[j] = temp;
                }
            }
        }
    }
}
