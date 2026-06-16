using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class SecurityRoomBuilder : MonoBehaviour
{
    [Header("Ligacao com a cena")]
    public string roomRootName = "Security Puzzle Room";
    public bool buildInEditor = false;
    public bool autoBuildOnStart = true;
    public bool rebuildExistingOnPlay = true;

    [Header("Posicao")]
    public Vector3 roomCenter = new Vector3(5.95f, 0f, 1.9f);
    public float corridorX = 0.05f;
    public float floorY = 0.02f;

    [Header("Dimensoes")]
    public float roomWidth = 5.6f;
    public float roomDepth = 5.2f;
    public float roomHeight = 2.8f;

    private Material wallMaterial;
    private Material floorMaterial;
    private Material screenMaterial;
    private Material gateMaterial;
    private Material slotMaterial;
    private Material buttonMaterial;
    private Material laserMaterial;
    private Material metalMaterial;
    private Material droneMaterial;

    private void Start()
    {
        EnsureGazeController();

        if (!Application.isPlaying)
        {
            if (buildInEditor && GameObject.Find(roomRootName) == null)
            {
                BuildRoom();
            }

            return;
        }

        if (autoBuildOnStart && (rebuildExistingOnPlay || GameObject.Find(roomRootName) == null))
        {
            BuildRoom();
        }
    }

    [ContextMenu("Build Security Puzzle Room")]
    public void BuildRoom()
    {
        RemoveExistingRoom();
        CreateMaterials();

        GameObject roomRoot = new GameObject(roomRootName);
        roomRoot.transform.position = Vector3.zero;

        BuildConnection(roomRoot.transform);
        BuildRoomShell(roomRoot.transform);
        BuildSecurityDecor(roomRoot.transform);
        BuildPuzzleStation(roomRoot.transform);
        SaveSceneIfEditing();
    }

    private void BuildConnection(Transform parent)
    {
        float doorX = roomCenter.x - roomWidth * 0.5f;
        float centerX = (corridorX + doorX) * 0.5f;
        float length = Mathf.Abs(doorX - corridorX);

        CreateCube("Security Access Floor", new Vector3(centerX, floorY, roomCenter.z),
            new Vector3(length + 0.6f, 0.12f, 1.45f), floorMaterial, parent, true);
        CreateCube("Security Access Ceiling", new Vector3(centerX, floorY + roomHeight, roomCenter.z),
            new Vector3(length + 0.6f, 0.12f, 1.45f), wallMaterial, parent, false);

        CreateCube("Security Door Frame Top", new Vector3(doorX, floorY + 2.25f, roomCenter.z),
            new Vector3(0.22f, 0.25f, 1.75f), metalMaterial, parent, true);
        CreateCube("Security Door Frame Front", new Vector3(doorX, floorY + 1.25f, roomCenter.z - 0.9f),
            new Vector3(0.22f, 2.2f, 0.18f), metalMaterial, parent, true);
        CreateCube("Security Door Frame Back", new Vector3(doorX, floorY + 1.25f, roomCenter.z + 0.9f),
            new Vector3(0.22f, 2.2f, 0.18f), metalMaterial, parent, true);

        CreateCube("Security Blast Door Left Leaf", new Vector3(doorX + 0.02f, floorY + 1.12f, roomCenter.z - 0.64f),
            new Vector3(0.18f, 2.0f, 0.46f), metalMaterial, parent, true);
        CreateCube("Security Blast Door Right Leaf", new Vector3(doorX + 0.02f, floorY + 1.12f, roomCenter.z + 0.64f),
            new Vector3(0.18f, 2.0f, 0.46f), metalMaterial, parent, true);
        CreateCube("Security Blast Door Lock A", new Vector3(doorX - 0.09f, floorY + 1.22f, roomCenter.z - 0.26f),
            new Vector3(0.06f, 0.22f, 0.12f), laserMaterial, parent, false);
        CreateCube("Security Blast Door Lock B", new Vector3(doorX - 0.09f, floorY + 1.22f, roomCenter.z + 0.26f),
            new Vector3(0.06f, 0.22f, 0.12f), laserMaterial, parent, false);
        CreateCenteredText("Security Blast Door Label", new Vector3(doorX - 0.13f, floorY + 1.92f, roomCenter.z),
            "PORTA\nBLINDADA", 0.065f, Color.red, parent, Quaternion.Euler(0f, 90f, 0f));

        CreateLaserBeam("Entrance Laser A", new Vector3(centerX, floorY + 0.95f, roomCenter.z - 0.38f),
            new Vector3(length + 0.2f, 0.035f, 0.035f), parent);
        CreateLaserBeam("Entrance Laser B", new Vector3(centerX, floorY + 1.35f, roomCenter.z + 0.38f),
            new Vector3(length + 0.2f, 0.035f, 0.035f), parent);
    }

    private void BuildRoomShell(Transform parent)
    {
        CreateCube("Security Room Floor", new Vector3(roomCenter.x, floorY, roomCenter.z),
            new Vector3(roomWidth, 0.12f, roomDepth), floorMaterial, parent, true);
        CreateCube("Security Room Ceiling", new Vector3(roomCenter.x, floorY + roomHeight, roomCenter.z),
            new Vector3(roomWidth, 0.12f, roomDepth), wallMaterial, parent, false);

        CreateCube("Security Far Wall", new Vector3(roomCenter.x + roomWidth * 0.5f, floorY + roomHeight * 0.5f, roomCenter.z),
            new Vector3(0.18f, roomHeight, roomDepth), wallMaterial, parent, true);
        CreateCube("Security Front Wall", new Vector3(roomCenter.x, floorY + roomHeight * 0.5f, roomCenter.z - roomDepth * 0.5f),
            new Vector3(roomWidth, roomHeight, 0.18f), wallMaterial, parent, true);
        CreateCube("Security Back Wall", new Vector3(roomCenter.x, floorY + roomHeight * 0.5f, roomCenter.z + roomDepth * 0.5f),
            new Vector3(roomWidth, roomHeight, 0.18f), wallMaterial, parent, true);

        float doorHalfDepth = 0.95f;
        float sideWallDepth = (roomDepth - doorHalfDepth * 2f) * 0.5f;
        CreateCube("Security Entrance Wall Front", new Vector3(roomCenter.x - roomWidth * 0.5f, floorY + roomHeight * 0.5f, roomCenter.z - doorHalfDepth - sideWallDepth * 0.5f),
            new Vector3(0.18f, roomHeight, sideWallDepth), wallMaterial, parent, true);
        CreateCube("Security Entrance Wall Back", new Vector3(roomCenter.x - roomWidth * 0.5f, floorY + roomHeight * 0.5f, roomCenter.z + doorHalfDepth + sideWallDepth * 0.5f),
            new Vector3(0.18f, roomHeight, sideWallDepth), wallMaterial, parent, true);
        CreateCube("Security Entrance Wall Upper", new Vector3(roomCenter.x - roomWidth * 0.5f, floorY + 2.55f, roomCenter.z),
            new Vector3(0.18f, 0.5f, roomDepth), wallMaterial, parent, true);

        CreateCube("Security Armored Door", new Vector3(roomCenter.x + roomWidth * 0.5f - 0.12f, floorY + 1.05f, roomCenter.z),
            new Vector3(0.18f, 2.0f, 1.35f), metalMaterial, parent, true);
        CreateCube("Security Door Lock Panel", new Vector3(roomCenter.x + roomWidth * 0.5f - 0.24f, floorY + 1.15f, roomCenter.z - 0.85f),
            new Vector3(0.08f, 0.6f, 0.28f), buttonMaterial, parent, false);
    }

    private void BuildSecurityDecor(Transform parent)
    {
        CreateText("Security Room Title", new Vector3(roomCenter.x - 1.8f, floorY + 2.45f, roomCenter.z - 2.25f),
            "SETOR DE SEGURANCA", 0.12f, Color.red, parent, Quaternion.Euler(0f, 0f, 0f));
        CreateText("Security Warning Label", new Vector3(roomCenter.x + 1.15f, floorY + 2.25f, roomCenter.z - 2.25f),
            "ACESSO RESTRITO", 0.09f, Color.red, parent, Quaternion.Euler(0f, 0f, 0f));

        CreateCamera("Security Camera A", new Vector3(roomCenter.x - 1.9f, floorY + 2.25f, roomCenter.z - 2.25f),
            Quaternion.Euler(25f, 35f, 0f), parent);
        CreateCamera("Security Camera B", new Vector3(roomCenter.x + 1.9f, floorY + 2.25f, roomCenter.z + 2.25f),
            Quaternion.Euler(25f, 210f, 0f), parent);

        CreateDrone("Security Drone A", new Vector3(roomCenter.x + 0.55f, floorY + 2.05f, roomCenter.z - 1.75f), parent);
        CreateDrone("Security Drone B", new Vector3(roomCenter.x - 1.25f, floorY + 1.85f, roomCenter.z + 1.55f), parent);

        CreateLaserColumn("Laser Emitter Front A", new Vector3(roomCenter.x - 1.6f, floorY + 0.9f, roomCenter.z - 2.2f), parent);
        CreateLaserColumn("Laser Emitter Front B", new Vector3(roomCenter.x + 1.6f, floorY + 0.9f, roomCenter.z - 2.2f), parent);
        CreateLaserColumn("Laser Emitter Back A", new Vector3(roomCenter.x - 1.6f, floorY + 0.9f, roomCenter.z + 2.2f), parent);
        CreateLaserColumn("Laser Emitter Back B", new Vector3(roomCenter.x + 1.6f, floorY + 0.9f, roomCenter.z + 2.2f), parent);
        CreateLaserBeam("Security Side Laser Front", new Vector3(roomCenter.x, floorY + 1.15f, roomCenter.z - 2.2f),
            new Vector3(roomWidth - 1.2f, 0.03f, 0.03f), parent);
        CreateLaserBeam("Security Side Laser Back", new Vector3(roomCenter.x, floorY + 1.55f, roomCenter.z + 2.2f),
            new Vector3(roomWidth - 1.2f, 0.03f, 0.03f), parent);

        CreateSecurityConsole("Security Console Left", new Vector3(roomCenter.x - 1.85f, floorY + 0.35f, roomCenter.z - 1.35f), parent);
        CreateSecurityConsole("Security Console Right", new Vector3(roomCenter.x - 1.85f, floorY + 0.35f, roomCenter.z + 1.35f), parent);
        CreateServerRack("Security Server Rack A", new Vector3(roomCenter.x + 2.45f, floorY + 0.85f, roomCenter.z - 1.85f), parent);
        CreateServerRack("Security Server Rack B", new Vector3(roomCenter.x + 2.45f, floorY + 0.85f, roomCenter.z + 1.85f), parent);
        CreateAlarmLight("Security Alarm A", new Vector3(roomCenter.x - 2.35f, floorY + 2.55f, roomCenter.z - 2.25f), parent);
        CreateAlarmLight("Security Alarm B", new Vector3(roomCenter.x - 2.35f, floorY + 2.55f, roomCenter.z + 2.25f), parent);
    }

    private void BuildPuzzleStation(Transform parent)
    {
        List<GameObject> xorObjects = new List<GameObject>();
        List<GameObject> notObjects = new List<GameObject>();

        float screenX = roomCenter.x + roomWidth * 0.5f - 0.22f;
        GameObject managerObject = new GameObject("Security Puzzle Manager");
        managerObject.transform.SetParent(parent);
        managerObject.transform.position = roomCenter;
        SecurityPuzzleManager manager = managerObject.AddComponent<SecurityPuzzleManager>();
        manager.randomModeOnStart = true;
        manager.lives = 3;
        manager.score = 0;
        manager.pointsPerSuccess = 10;
        manager.maxScore = 50;
        manager.noLivesMessage = "Seguranca bloqueada. Sem vidas.";
        manager.challenges = CreateSecurityChallenges();

        CreateCube("Security Puzzle Screen", new Vector3(screenX, floorY + 1.65f, roomCenter.z),
            new Vector3(0.08f, 1.55f, 3.55f), screenMaterial, parent, false);
        CreateCube("Security Screen Frame Top", new Vector3(screenX - 0.01f, floorY + 2.46f, roomCenter.z),
            new Vector3(0.12f, 0.08f, 3.75f), metalMaterial, parent, false);
        CreateCube("Security Screen Frame Bottom", new Vector3(screenX - 0.01f, floorY + 0.84f, roomCenter.z),
            new Vector3(0.12f, 0.08f, 3.75f), metalMaterial, parent, false);
        CreateCube("Security Screen Frame Front", new Vector3(screenX - 0.01f, floorY + 1.65f, roomCenter.z - 1.82f),
            new Vector3(0.12f, 1.65f, 0.08f), metalMaterial, parent, false);
        CreateCube("Security Screen Frame Back", new Vector3(screenX - 0.01f, floorY + 1.65f, roomCenter.z + 1.82f),
            new Vector3(0.12f, 1.65f, 0.08f), metalMaterial, parent, false);

        Quaternion boardTextRotation = Quaternion.Euler(0f, 90f, 0f);
        manager.challengeTextMesh = CreateText("Security Challenge Text", new Vector3(screenX - 0.085f, floorY + 2.24f, roomCenter.z + 1.58f),
            "", 0.064f, Color.cyan, parent, boardTextRotation);
        manager.feedbackTextMesh = CreateText("Security Feedback Text", new Vector3(screenX - 0.085f, floorY + 1.12f, roomCenter.z + 1.58f),
            "", 0.055f, Color.white, parent, boardTextRotation);
        manager.scoreTextMesh = CreateText("Security Score Text", new Vector3(screenX - 0.085f, floorY + 0.94f, roomCenter.z + 0.4f),
            "", 0.055f, Color.green, parent, boardTextRotation);
        manager.livesTextMesh = CreateText("Security Lives Text", new Vector3(screenX - 0.085f, floorY + 0.94f, roomCenter.z + 1.5f),
            "", 0.055f, Color.green, parent, boardTextRotation);
        CreateText("Security Board Hint", new Vector3(screenX - 0.085f, floorY + 0.74f, roomCenter.z + 1.58f),
            "XOR: apenas uma chave ativa | NOT: inverte o sinal", 0.045f, new Color(0.55f, 0.95f, 1f), parent, boardTextRotation);

        SecuritySignalSwitch signalA = CreateSignalSwitch("Security Signal A", "A",
            new Vector3(roomCenter.x + 0.1f, floorY + 0.82f, roomCenter.z - 0.78f), manager, parent);
        SecuritySignalSwitch signalB = CreateSignalSwitch("Security Signal B", "B",
            new Vector3(roomCenter.x + 0.1f, floorY + 0.82f, roomCenter.z + 0.78f), manager, parent);
        SecurityPuzzleValidateButton xorValidateButton = CreateSecurityValidateButton("Security XOR Validate Button",
            new Vector3(roomCenter.x + 0.1f, floorY + 0.82f, roomCenter.z + 1.85f), manager, parent);
        SecurityPuzzleValidateButton notValidateButton = CreateSecurityValidateButton("Security NOT Validate Button",
            new Vector3(roomCenter.x + 0.1f, floorY + 0.82f, roomCenter.z + 1.85f), manager, parent);

        SecurityNotInverter notInverter = CreateNotInverter("Security NOT Inverter Module",
            new Vector3(roomCenter.x + 0.1f, floorY + 0.78f, roomCenter.z - 0.75f), manager, parent);
        GameObject laserSystemPanel = CreateCube("Security Laser System Panel",
            new Vector3(roomCenter.x + 0.1f, floorY + 0.78f, roomCenter.z + 0.45f), new Vector3(0.18f, 0.74f, 0.66f), laserMaterial, parent, false);
        TextMesh laserSystemLabel = CreateCenteredText("Security Laser System Label",
            laserSystemPanel.transform.position + new Vector3(-0.1f, 0.04f, 0f),
            "LASERS\nSEGURANCA", 0.043f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f));

        manager.signalA = signalA;
        manager.signalB = signalB;
        manager.notInverter = notInverter;

        AddObject(xorObjects, signalA.gameObject);
        AddObject(xorObjects, signalB.gameObject);
        AddObject(xorObjects, xorValidateButton.gameObject);
        AddObject(xorObjects, signalA.valueTextMesh.gameObject);
        AddObject(xorObjects, signalB.valueTextMesh.gameObject);
        AddObject(xorObjects, CreateCenteredText("Security Signal A Label", signalA.transform.position + new Vector3(-0.13f, 0.28f, 0f),
            "CHAVE A", 0.065f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f)).gameObject);
        TextMesh signalBLabel = CreateCenteredText("Security Signal B Label", signalB.transform.position + new Vector3(-0.13f, 0.28f, 0f),
            "CHAVE B", 0.065f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f));
        AddObject(xorObjects, signalBLabel.gameObject);
        AddObject(xorObjects, CreateCenteredText("Security Validate Front Label", xorValidateButton.transform.position + new Vector3(-0.13f, 0.02f, 0f),
            "VALIDAR", 0.07f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f)).gameObject);

        AddObject(notObjects, notInverter.gameObject);
        AddObject(notObjects, notInverter.gateTextMesh.gameObject);
        AddObject(notObjects, notInverter.stateTextMesh.gameObject);
        AddObject(notObjects, laserSystemPanel);
        AddObject(notObjects, laserSystemLabel.gameObject);
        AddObject(notObjects, notValidateButton.gameObject);
        AddObject(notObjects, CreateCenteredText("Security NOT Validate Front Label", notValidateButton.transform.position + new Vector3(-0.13f, 0.02f, 0f),
            "VALIDAR", 0.07f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f)).gameObject);

        manager.xorObjects = xorObjects.ToArray();
        manager.notObjects = notObjects.ToArray();
    }

    private SecurityPuzzleChallenge[] CreateSecurityChallenges()
    {
        return new SecurityPuzzleChallenge[]
        {
            CreateSecurityChallenge(SecurityPuzzleMode.ConfigureSignals, SecurityPuzzleGate.XOR, "Autenticacao XOR",
                "Ative somente UMA chave.", false, false, true, true, false, false, false),
            CreateSecurityChallenge(SecurityPuzzleMode.ConfigureSignals, SecurityPuzzleGate.XOR, "Autenticacao XOR",
                "Ative somente UMA chave.", true, true, true, true, true, true, false),
            CreateSecurityChallenge(SecurityPuzzleMode.ConfigureSignals, SecurityPuzzleGate.XOR, "Autenticacao XOR",
                "Ative somente UMA chave.", false, true, true, true, false, true, false),
            CreateSecurityChallenge(SecurityPuzzleMode.ConfigureSignals, SecurityPuzzleGate.NOT, "Inversor NOT",
                "Use NOT para desligar os lasers.", true, false, false, false, false, false, false),
            CreateSecurityChallenge(SecurityPuzzleMode.ConfigureSignals, SecurityPuzzleGate.NOT, "Inversor NOT",
                "Use NOT para reativar a vigilancia.", false, false, false, true, false, false, false)
        };
    }

    private SecurityPuzzleChallenge CreateSecurityChallenge(SecurityPuzzleMode mode, SecurityPuzzleGate gate, string title,
        string instruction, bool inputA, bool inputB, bool useInputB, bool desiredOutput, bool initialA, bool initialB, bool initialInverter)
    {
        SecurityPuzzleChallenge challenge = new SecurityPuzzleChallenge();
        challenge.mode = mode;
        challenge.gate = gate;
        challenge.title = title;
        challenge.instruction = instruction;
        challenge.inputA = inputA;
        challenge.inputB = inputB;
        challenge.useInputB = useInputB;
        challenge.desiredOutput = desiredOutput;
        challenge.initialA = initialA;
        challenge.initialB = initialB;
        challenge.initialInverter = initialInverter;
        return challenge;
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

    private SecuritySignalSwitch CreateSignalSwitch(string name, string signalName, Vector3 position, SecurityPuzzleManager manager, Transform parent)
    {
        GameObject switchObject = CreateCube(name, position, new Vector3(0.24f, 0.95f, 0.82f), gateMaterial, parent, true);
        SecuritySignalSwitch signalSwitch = switchObject.AddComponent<SecuritySignalSwitch>();
        signalSwitch.manager = manager;
        signalSwitch.signalName = signalName;
        signalSwitch.valueTextMesh = CreateCenteredText(name + " Value", position + new Vector3(-0.13f, -0.08f, 0f),
            "DESLIGADA", 0.065f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f));
        return signalSwitch;
    }

    private SecurityNotInverter CreateNotInverter(string name, Vector3 position, SecurityPuzzleManager manager, Transform parent)
    {
        GameObject inverterObject = CreateCube(name, position, new Vector3(0.2f, 0.82f, 0.72f), gateMaterial, parent, true);
        SecurityNotInverter inverter = inverterObject.AddComponent<SecurityNotInverter>();
        inverter.manager = manager;
        inverter.gateTextMesh = CreateCenteredText(name + " Gate Label", position + new Vector3(-0.12f, 0.12f, 0f),
            "NOT", 0.09f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f));
        inverter.stateTextMesh = CreateCenteredText(name + " State Label", position + new Vector3(-0.12f, -0.16f, 0f),
            "INVERSOR\nDESLIGADO", 0.036f, Color.white, parent, Quaternion.Euler(0f, 90f, 0f));
        return inverter;
    }

    private SecurityPuzzleValidateButton CreateSecurityValidateButton(string name, Vector3 position, SecurityPuzzleManager manager, Transform parent)
    {
        GameObject buttonObject = CreateCube(name, position, new Vector3(0.24f, 0.95f, 0.82f), buttonMaterial, parent, true);
        SecurityPuzzleValidateButton button = buttonObject.AddComponent<SecurityPuzzleValidateButton>();
        button.manager = manager;
        return button;
    }

    private LogicGateBlock CreateGateBlock(string name, LogicGateType type, Vector3 position, CircuitPuzzleManager manager, Transform parent)
    {
        GameObject block = CreateCube(name, position, new Vector3(0.55f, 0.35f, 0.55f), gateMaterial, parent, true);
        LogicGateBlock gateBlock = block.AddComponent<LogicGateBlock>();
        gateBlock.gateType = type;
        gateBlock.puzzleManager = manager;
        return gateBlock;
    }

    private LogicGateSlot CreateSlot(string name, Vector3 position, CircuitPuzzleManager manager, Transform parent)
    {
        GameObject slotObject = CreateCube(name, position, new Vector3(0.75f, 0.15f, 0.75f), slotMaterial, parent, true);
        LogicGateSlot slot = slotObject.AddComponent<LogicGateSlot>();
        slot.puzzleManager = manager;
        return slot;
    }

    private ValidateCircuitButton CreateValidateButton(string name, Vector3 position, CircuitPuzzleManager manager, Transform parent)
    {
        GameObject buttonObject = CreateCube(name, position, new Vector3(0.75f, 0.2f, 0.75f), buttonMaterial, parent, true);
        ValidateCircuitButton button = buttonObject.AddComponent<ValidateCircuitButton>();
        button.puzzleManager = manager;
        return button;
    }

    private ExerciseAnswerOption CreateAnswerOption(string name, bool answerValue, Vector3 position, CircuitPuzzleManager manager, Transform parent)
    {
        GameObject optionObject = CreateCube(name, position, new Vector3(0.22f, 0.9f, 1.05f), answerValue ? buttonMaterial : gateMaterial, parent, true);
        ExerciseAnswerOption option = optionObject.AddComponent<ExerciseAnswerOption>();
        option.answerValue = answerValue;
        option.puzzleManager = manager;
        return option;
    }

    private void CreateLaserColumn(string name, Vector3 position, Transform parent)
    {
        CreateCube(name, position, new Vector3(0.12f, 0.7f, 0.12f), metalMaterial, parent, false);
        CreateCube(name + " Lens", position + new Vector3(0f, 0.38f, 0f), new Vector3(0.18f, 0.08f, 0.18f), laserMaterial, parent, false);
    }

    private void CreateSecurityConsole(string name, Vector3 position, Transform parent)
    {
        CreateCube(name, position, new Vector3(0.85f, 0.7f, 0.45f), metalMaterial, parent, true);
        CreateCube(name + " Screen", position + new Vector3(0f, 0.37f, 0f), new Vector3(0.65f, 0.05f, 0.3f), screenMaterial, parent, false);
        CreateCube(name + " Green Key", position + new Vector3(-0.22f, 0.42f, 0.02f), new Vector3(0.12f, 0.03f, 0.08f), buttonMaterial, parent, false);
        CreateCube(name + " Red Key", position + new Vector3(0.22f, 0.42f, 0.02f), new Vector3(0.12f, 0.03f, 0.08f), laserMaterial, parent, false);
    }

    private void CreateServerRack(string name, Vector3 position, Transform parent)
    {
        CreateCube(name, position, new Vector3(0.38f, 1.65f, 0.75f), metalMaterial, parent, true);
        for (int i = 0; i < 4; i++)
        {
            CreateCube(name + " Light " + i, position + new Vector3(-0.2f, 0.55f - i * 0.28f, -0.18f),
                new Vector3(0.03f, 0.04f, 0.08f), i % 2 == 0 ? buttonMaterial : laserMaterial, parent, false);
        }
    }

    private void CreateAlarmLight(string name, Vector3 position, Transform parent)
    {
        GameObject alarm = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        alarm.name = name;
        alarm.transform.SetParent(parent);
        alarm.transform.position = position;
        alarm.transform.localScale = new Vector3(0.18f, 0.18f, 0.18f);
        alarm.GetComponent<Renderer>().material = laserMaterial;
        DestroyObjectSafe(alarm.GetComponent<Collider>());
    }

    private void CreateCamera(string name, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject body = CreateCube(name, position, new Vector3(0.35f, 0.2f, 0.22f), metalMaterial, parent, false);
        body.transform.rotation = rotation;
        GameObject lens = CreateCube(name + " Lens", position + body.transform.forward * 0.18f, new Vector3(0.12f, 0.12f, 0.06f), screenMaterial, parent, false);
        lens.transform.rotation = rotation;
    }

    private void CreateDrone(string name, Vector3 position, Transform parent)
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        body.name = name;
        body.transform.SetParent(parent);
        body.transform.position = position;
        body.transform.localScale = new Vector3(0.38f, 0.18f, 0.38f);
        body.GetComponent<Renderer>().material = droneMaterial;
        DestroyObjectSafe(body.GetComponent<Collider>());

        CreateCube(name + " Wing L", position + new Vector3(-0.35f, 0f, 0f), new Vector3(0.38f, 0.04f, 0.12f), droneMaterial, parent, false);
        CreateCube(name + " Wing R", position + new Vector3(0.35f, 0f, 0f), new Vector3(0.38f, 0.04f, 0.12f), droneMaterial, parent, false);
        CreateCube(name + " Eye", position + new Vector3(0f, -0.03f, -0.2f), new Vector3(0.14f, 0.07f, 0.03f), laserMaterial, parent, false);
    }

    private GameObject CreateLaserBeam(string name, Vector3 position, Vector3 scale, Transform parent)
    {
        return CreateCube(name, position, scale, laserMaterial, parent, false);
    }

    private GameObject CreateCube(string name, Vector3 position, Vector3 scale, Material material, Transform parent, bool keepCollider)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.SetParent(parent);
        cube.transform.position = position;
        cube.transform.localScale = scale;

        Renderer renderer = cube.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material = material;
        }

        if (!keepCollider)
        {
            Collider collider = cube.GetComponent<Collider>();
            if (collider != null)
            {
                DestroyObjectSafe(collider);
            }
        }

        return cube;
    }

    private TextMesh CreateText(string name, Vector3 position, string text, float characterSize, Color color, Transform parent, Quaternion rotation)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);
        textObject.transform.position = position;
        textObject.transform.rotation = rotation;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.UpperLeft;
        textMesh.alignment = TextAlignment.Left;
        textMesh.color = color;
        return textMesh;
    }

    private TextMesh CreateCenteredText(string name, Vector3 position, string text, float characterSize, Color color, Transform parent, Quaternion rotation)
    {
        TextMesh textMesh = CreateText(name, position, text, characterSize, color, parent, rotation);
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        return textMesh;
    }

    private void AddObject(List<GameObject> objects, GameObject objectToAdd)
    {
        if (objectToAdd != null && !objects.Contains(objectToAdd))
        {
            objects.Add(objectToAdd);
        }
    }

    private void EnsureGazeController()
    {
        Camera camera = Camera.main;
        if (camera != null && camera.GetComponent<GazeSelectionController>() == null)
        {
            camera.gameObject.AddComponent<GazeSelectionController>();
        }
    }

    private void CreateMaterials()
    {
        wallMaterial = CreateMaterial("Security Wall Material", new Color(0.18f, 0.2f, 0.22f));
        floorMaterial = CreateMaterial("Security Floor Material", new Color(0.07f, 0.08f, 0.09f));
        screenMaterial = CreateMaterial("Security Screen Material", new Color(0.01f, 0.04f, 0.06f));
        gateMaterial = CreateMaterial("Security Gate Material", new Color(0.13f, 0.25f, 0.38f));
        slotMaterial = CreateMaterial("Security Slot Material", new Color(0.16f, 0.16f, 0.18f));
        buttonMaterial = CreateMaterial("Security Button Material", new Color(0.02f, 0.45f, 0.2f));
        metalMaterial = CreateMaterial("Security Dark Metal Material", new Color(0.12f, 0.13f, 0.15f));
        droneMaterial = CreateMaterial("Security Drone Material", new Color(0.08f, 0.09f, 0.12f));
        laserMaterial = CreateEmissionMaterial("Security Laser Material", Color.red, 2.5f);
    }

    private Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private Material CreateEmissionMaterial(string name, Color color, float intensity)
    {
        Material material = CreateMaterial(name, color);
        material.EnableKeyword("_EMISSION");
        material.SetColor("_EmissionColor", color * intensity);
        return material;
    }

    private void RemoveExistingRoom()
    {
        GameObject existing = GameObject.Find(roomRootName);
        if (existing != null)
        {
            DestroyObjectSafe(existing);
        }
    }

    private void DestroyObjectSafe(Object objectToDestroy)
    {
        if (objectToDestroy == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(objectToDestroy);
        }
        else
        {
            DestroyImmediate(objectToDestroy);
        }
    }

    private void SaveSceneIfEditing()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(gameObject.scene);
            EditorSceneManager.SaveScene(gameObject.scene);
        }
#endif
    }
}
