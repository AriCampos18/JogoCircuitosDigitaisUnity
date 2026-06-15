using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

[ExecuteInEditMode]
public class AICentralRoomBuilder : MonoBehaviour
{
    [Header("Ligacao com a cena")]
    public string roomRootName = "AI Circuit Puzzle Room";
    public string preferredProfileIntersectionName = "ProfileIntersection";
    public bool buildInEditor = true;
    public bool autoBuildOnStart = false;

    [Header("Dimensoes")]
    public float roomWidth = 5f;
    public float roomDepth = 5.5f;
    public float roomHeight = 3f;
    public float roomDistanceFromProfile = 6f;

    private Material wallMaterial;
    private Material floorMaterial;
    private Material screenMaterial;
    private Material gateMaterial;
    private Material slotMaterial;
    private Material buttonMaterial;

    private const string FloorPrefab = "Assets/Sci Fi Modular Pack/Prefabs/Floor1.prefab";
    private const string SideFloor1Prefab = "Assets/Sci Fi Modular Pack/Prefabs/SideFloor1.prefab";
    private const string SideFloor2Prefab = "Assets/Sci Fi Modular Pack/Prefabs/SideFloor2.prefab";
    private const string WallPrefab = "Assets/Sci Fi Modular Pack/Prefabs/Wall12.prefab";
    private const string SideWallPrefab = "Assets/Sci Fi Modular Pack/Prefabs/SideWall1.1.prefab";
    private const string TopWallPrefab = "Assets/Sci Fi Modular Pack/Prefabs/TopWall4.prefab";
    private const string DoorFramePrefab = "Assets/Sci Fi Modular Pack/Prefabs/DoorFrame.prefab";
    private const string WallDoorPrefab = "Assets/Sci Fi Modular Pack/Prefabs/WallDoor.prefab";
    private const string LightPrefab = "Assets/Sci Fi Modular Pack/Prefabs/Light1.prefab";
    private const string BoxPrefab = "Assets/Sci Fi Modular Pack/Prefabs/Box1.prefab";

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

        if (autoBuildOnStart && GameObject.Find(roomRootName) == null)
        {
            BuildRoom();
        }
    }

    [ContextMenu("Build AI Circuit Puzzle Room")]
    public void BuildRoom()
    {
        RemoveExistingRoom();
        CreateMaterials();

        // Usa o ProfileIntersection da cena como ponto de encaixe da nova sala.
        Transform profile = FindProfileIntersection();
        Vector3 profilePosition = profile != null ? profile.position : transform.position;
        float floorY = profilePosition.y - 1f;
        Vector3 roomCenter = profilePosition + new Vector3(-roomDistanceFromProfile, 0f, 0f);

        GameObject roomRoot = new GameObject(roomRootName);
        roomRoot.transform.position = Vector3.zero;

        BuildConnection(profilePosition, roomCenter, floorY, roomRoot.transform);
        BuildRoomShell(roomCenter, floorY, roomRoot.transform);
        BuildPuzzleStation(roomCenter, floorY, roomRoot.transform);
        BuildSafetyColliders(profilePosition, roomCenter, floorY, roomRoot.transform);
        SaveSceneIfEditing();
    }

    private void BuildConnection(Vector3 profilePosition, Vector3 roomCenter, float floorY, Transform parent)
    {
        // Pequeno corredor para ligar a estrutura modular existente ate a sala da IA.
        float entranceX = profilePosition.x - 1.5f;
        float roomDoorX = roomCenter.x + roomWidth * 0.5f;
        float corridorCenterX = (entranceX + roomDoorX) * 0.5f;
        float corridorLength = Mathf.Abs(entranceX - roomDoorX);
        Quaternion leftFacingRotation = Quaternion.Euler(0f, 90f, 0f);

        InstantiateModularPrefab(DoorFramePrefab, "AI Room Door Frame", new Vector3(entranceX, profilePosition.y, profilePosition.z), leftFacingRotation, parent);
        InstantiateModularPrefab(WallDoorPrefab, "AI Room Door Wall", new Vector3(entranceX, profilePosition.y, profilePosition.z), leftFacingRotation, parent);

        for (int i = 0; i < 3; i++)
        {
            float x = Mathf.Lerp(entranceX, roomDoorX, (i + 1f) / 4f);
            InstantiateModularPrefab(FloorPrefab, "AI Connection Floor " + i, new Vector3(x, floorY, profilePosition.z), leftFacingRotation, parent);
            InstantiateModularPrefab(TopWallPrefab, "AI Connection Ceiling " + i, new Vector3(x, floorY + 2.35f, profilePosition.z), leftFacingRotation, parent);
            InstantiateModularPrefab(SideWallPrefab, "AI Connection Front Wall " + i, new Vector3(x, floorY + 1.2f, profilePosition.z - 1.25f), leftFacingRotation, parent);
            InstantiateModularPrefab(SideWallPrefab, "AI Connection Back Wall " + i, new Vector3(x, floorY + 1.2f, profilePosition.z + 1.25f), leftFacingRotation, parent);
        }

        GameObject corridorCollider = CreateCube("Connection Safety Collider", new Vector3(corridorCenterX, floorY - 0.08f, profilePosition.z),
            new Vector3(corridorLength + 2f, 0.1f, 2.6f), floorMaterial, parent, true);
        DisableRenderer(corridorCollider);
    }

    private void BuildRoomShell(Vector3 roomCenter, float floorY, Transform parent)
    {
        Quaternion alongX = Quaternion.Euler(0f, 90f, 0f);
        Quaternion alongZ = Quaternion.identity;

        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                string prefab = z < 0 ? SideFloor1Prefab : (z > 0 ? SideFloor2Prefab : FloorPrefab);
                InstantiateModularPrefab(prefab, "AI Room Floor Module", roomCenter + new Vector3(x * 1.55f, floorY, z * 1.5f), Quaternion.identity, parent);
            }
        }

        for (int z = -1; z <= 1; z++)
        {
            InstantiateModularPrefab(SideWallPrefab, "AI Room Far Wall", new Vector3(roomCenter.x - roomWidth * 0.5f, floorY + 1.2f, roomCenter.z + z * 1.5f), alongZ, parent);
            InstantiateModularPrefab(SideWallPrefab, "AI Room Entrance Side Trim", new Vector3(roomCenter.x + roomWidth * 0.5f, floorY + 1.2f, roomCenter.z + z * 1.5f), alongZ, parent);
        }

        for (int x = -1; x <= 1; x++)
        {
            InstantiateModularPrefab(WallPrefab, "AI Room Front Wall", new Vector3(roomCenter.x + x * 1.55f, floorY + 1.25f, roomCenter.z - roomDepth * 0.5f), alongX, parent);
            InstantiateModularPrefab(WallPrefab, "AI Room Back Wall", new Vector3(roomCenter.x + x * 1.55f, floorY + 1.25f, roomCenter.z + roomDepth * 0.5f), alongX, parent);
            InstantiateModularPrefab(TopWallPrefab, "AI Room Ceiling Module", new Vector3(roomCenter.x + x * 1.55f, floorY + 2.35f, roomCenter.z), Quaternion.identity, parent);
        }

        InstantiateModularPrefab(LightPrefab, "AI Room Sci Fi Light", new Vector3(roomCenter.x, floorY + 2.2f, roomCenter.z), Quaternion.identity, parent);
        InstantiateModularPrefab(BoxPrefab, "AI Room Equipment Box", new Vector3(roomCenter.x - 1.9f, floorY + 0.25f, roomCenter.z - 1.8f), Quaternion.Euler(0f, 40f, 0f), parent);

        GameObject roomCollider = CreateCube("AI Room Safety Floor Collider", new Vector3(roomCenter.x, floorY - 0.08f, roomCenter.z),
            new Vector3(roomWidth + 0.8f, 0.1f, roomDepth + 0.8f), floorMaterial, parent, true);
        DisableRenderer(roomCollider);
    }

    private void BuildPuzzleStation(Vector3 roomCenter, float floorY, Transform parent)
    {
        // Cria a tela, o desafio inicial, os blocos de portas, o slot e o botao de validar.
        float screenX = roomCenter.x - roomWidth * 0.5f + 0.12f;
        GameObject managerObject = new GameObject("AI Circuit Puzzle Manager");
        managerObject.transform.SetParent(parent);
        managerObject.transform.position = roomCenter;
        CircuitPuzzleManager manager = managerObject.AddComponent<CircuitPuzzleManager>();
        manager.useDefaultFiveChallenges = true;
        manager.randomChallengeOnStart = true;
        manager.advanceAfterSuccess = false;

        GameObject screen = CreateCube("Puzzle Screen", new Vector3(screenX, floorY + 1.75f, roomCenter.z),
            new Vector3(0.08f, 1.6f, 3.4f), screenMaterial, parent, false);
        screen.transform.rotation = Quaternion.identity;

        manager.challengeTextMesh = CreateText("Challenge Text", new Vector3(screenX + 0.08f, floorY + 2.15f, roomCenter.z + 1.45f),
            0.12f, Color.cyan, parent, Quaternion.Euler(0f, -90f, 0f));
        manager.feedbackTextMesh = CreateText("Feedback Text", new Vector3(screenX + 0.08f, floorY + 1.45f, roomCenter.z + 1.45f),
            0.09f, Color.white, parent, Quaternion.Euler(0f, -90f, 0f));
        manager.scoreTextMesh = CreateText("Score Text", new Vector3(screenX + 0.08f, floorY + 1.25f, roomCenter.z + 1.45f),
            0.08f, Color.green, parent, Quaternion.Euler(0f, -90f, 0f));
        manager.livesTextMesh = CreateText("Lives Text", new Vector3(screenX + 0.08f, floorY + 1.25f, roomCenter.z - 0.1f),
            0.08f, Color.green, parent, Quaternion.Euler(0f, -90f, 0f));

        LogicGateBlock andBlock = CreateGateBlock("Gate AND", LogicGateType.AND, roomCenter + new Vector3(0.55f, floorY + 0.75f - roomCenter.y, -1.65f), manager, parent);
        LogicGateBlock orBlock = CreateGateBlock("Gate OR", LogicGateType.OR, roomCenter + new Vector3(0.55f, floorY + 0.75f - roomCenter.y, -0.55f), manager, parent);
        LogicGateBlock xorBlock = CreateGateBlock("Gate XOR", LogicGateType.XOR, roomCenter + new Vector3(0.55f, floorY + 0.75f - roomCenter.y, 0.55f), manager, parent);
        LogicGateBlock notBlock = CreateGateBlock("Gate NOT", LogicGateType.NOT, roomCenter + new Vector3(0.55f, floorY + 0.75f - roomCenter.y, 1.65f), manager, parent);

        LogicGateSlot slot = CreateSlot("Circuit Slot", roomCenter + new Vector3(-0.9f, floorY + 0.75f - roomCenter.y, -0.55f), manager, parent);
        ValidateCircuitButton validateButton = CreateValidateButton("Validate Button", roomCenter + new Vector3(-0.9f, floorY + 0.75f - roomCenter.y, 1.05f), manager, parent);

        manager.slots = new LogicGateSlot[] { slot };
        CreateText("AND Label", andBlock.transform.position + new Vector3(-0.22f, 0.38f, -0.18f), 0.08f, Color.white, parent, Quaternion.Euler(70f, 0f, 0f));
        CreateText("OR Label", orBlock.transform.position + new Vector3(-0.16f, 0.38f, -0.18f), 0.08f, Color.white, parent, Quaternion.Euler(70f, 0f, 0f));
        CreateText("XOR Label", xorBlock.transform.position + new Vector3(-0.22f, 0.38f, -0.18f), 0.08f, Color.white, parent, Quaternion.Euler(70f, 0f, 0f));
        CreateText("NOT Label", notBlock.transform.position + new Vector3(-0.22f, 0.38f, -0.18f), 0.08f, Color.white, parent, Quaternion.Euler(70f, 0f, 0f));
        CreateText("Slot Label", slot.transform.position + new Vector3(-0.24f, 0.42f, -0.18f), 0.07f, Color.cyan, parent, Quaternion.Euler(70f, 0f, 0f)).text = "SLOT";
        CreateText("Validate Label", validateButton.transform.position + new Vector3(-0.34f, 0.42f, -0.18f), 0.07f, Color.green, parent, Quaternion.Euler(70f, 0f, 0f)).text = "VALIDAR";
    }

    private void BuildSafetyColliders(Vector3 profilePosition, Vector3 roomCenter, float floorY, Transform parent)
    {
        float safetyStartX = roomCenter.x - roomWidth * 0.5f - 2f;
        float safetyEndX = profilePosition.x + 2f;
        float safetyCenterX = (safetyStartX + safetyEndX) * 0.5f;
        float safetyWidth = safetyEndX - safetyStartX;

        GameObject floorCollider = CreateCube("Safety Floor Collider",
            new Vector3(safetyCenterX, floorY - 0.08f, profilePosition.z),
            new Vector3(safetyWidth, 0.1f, roomDepth + 2f), floorMaterial, parent, true);
        DisableRenderer(floorCollider);

        GameObject frontWallCollider = CreateCube("Safety Front Wall Collider",
            new Vector3(safetyCenterX, floorY + 1.3f, profilePosition.z - roomDepth * 0.5f - 0.6f),
            new Vector3(safetyWidth, 2.8f, 0.2f), wallMaterial, parent, true);
        DisableRenderer(frontWallCollider);

        GameObject backWallCollider = CreateCube("Safety Back Wall Collider",
            new Vector3(safetyCenterX, floorY + 1.3f, profilePosition.z + roomDepth * 0.5f + 0.6f),
            new Vector3(safetyWidth, 2.8f, 0.2f), wallMaterial, parent, true);
        DisableRenderer(backWallCollider);
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

    private void DisableRenderer(GameObject target)
    {
        Renderer renderer = target.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.enabled = false;
        }
    }

    private TextMesh CreateText(string name, Vector3 position, float characterSize, Color color, Transform parent, Quaternion rotation)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent);
        textObject.transform.position = position;
        textObject.transform.rotation = rotation;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = name.Replace(" Label", "").Replace(" Text", "");
        textMesh.characterSize = characterSize;
        textMesh.anchor = TextAnchor.UpperLeft;
        textMesh.alignment = TextAlignment.Left;
        textMesh.color = color;
        return textMesh;
    }

    private GameObject InstantiateModularPrefab(string assetPath, string objectName, Vector3 position, Quaternion rotation, Transform parent)
    {
#if UNITY_EDITOR
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.name = objectName;
            instance.transform.SetParent(parent);
            instance.transform.position = position;
            instance.transform.rotation = rotation;
            instance.transform.localScale = Vector3.one;
            return instance;
        }
#endif
        return CreateCube(objectName, position, Vector3.one, wallMaterial, parent, true);
    }

    private void EnsureGazeController()
    {
        Camera camera = Camera.main;
        if (camera != null && camera.GetComponent<GazeSelectionController>() == null)
        {
            camera.gameObject.AddComponent<GazeSelectionController>();
        }
    }

    private Transform FindProfileIntersection()
    {
        GameObject preferred = GameObject.Find(preferredProfileIntersectionName);
        if (preferred != null)
        {
            return preferred.transform;
        }

        GameObject fallback = GameObject.Find("ProfileIntersection (1)");
        return fallback != null ? fallback.transform : null;
    }

    private void CreateMaterials()
    {
        wallMaterial = CreateMaterial("AI Wall Material", new Color(0.16f, 0.18f, 0.22f));
        floorMaterial = CreateMaterial("AI Floor Material", new Color(0.08f, 0.09f, 0.11f));
        screenMaterial = CreateMaterial("AI Screen Material", new Color(0.01f, 0.05f, 0.08f));
        gateMaterial = CreateMaterial("AI Gate Material", new Color(0.1f, 0.28f, 0.42f));
        slotMaterial = CreateMaterial("AI Slot Material", new Color(0.18f, 0.18f, 0.18f));
        buttonMaterial = CreateMaterial("AI Button Material", new Color(0.05f, 0.45f, 0.18f));
    }

    private Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = color;
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
