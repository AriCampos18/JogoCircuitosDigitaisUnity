using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AICentralRoomSceneBaker
{
    private const string TargetScenePath = "Assets/Sci Fi Modular Pack/Scenes/Demo 1.0 Free.unity";
    private const string SessionKey = "AICentralRoomSceneBaker_RanForCurrentSession";

    static AICentralRoomSceneBaker()
    {
        EditorSceneManager.sceneOpened += OnSceneOpened;
        EditorApplication.delayCall += BakeLoadedDemoSceneOnce;
    }

    [MenuItem("Tools/AI Puzzle/Bake Room Into Demo Scene")]
    public static void BakeMenuItem()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        BakeScene(scene, true);
    }

    public static void BakeDemoSceneFromCommandLine()
    {
        Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);
        BakeScene(scene, true);
    }

    private static void BakeLoadedDemoSceneOnce()
    {
        Scene scene = SceneManager.GetSceneByPath(TargetScenePath);
        if (scene.IsValid() && scene.isLoaded)
        {
            BakeScene(scene, SceneNeedsUpgrade(scene));
            SessionState.SetBool(SessionKey, true);
        }
    }

    private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
    {
        if (scene.path == TargetScenePath)
        {
            BakeScene(scene, SceneNeedsUpgrade(scene));
        }
    }

    private static void BakeScene(Scene scene, bool forceReplace)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        GameObject existingRoom = FindRootObject(scene, "AI Circuit Puzzle Room");
        if (existingRoom != null)
        {
            if (!forceReplace)
            {
                return;
            }

            Object.DestroyImmediate(existingRoom);
        }

        GameObject builderObject = FindRootObject(scene, "AI Central Room Builder");
        if (builderObject == null)
        {
            builderObject = new GameObject("AI Central Room Builder");
            SceneManager.MoveGameObjectToScene(builderObject, scene);
        }

        AICentralRoomBuilder builder = builderObject.GetComponent<AICentralRoomBuilder>();
        if (builder == null)
        {
            builder = builderObject.AddComponent<AICentralRoomBuilder>();
        }

        builder.roomRootName = "AI Circuit Puzzle Room";
        builder.preferredProfileIntersectionName = "ProfileIntersection";
        builder.buildInEditor = true;
        builder.autoBuildOnStart = false;
        builder.BuildRoom();

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    private static GameObject FindRootObject(Scene scene, string objectName)
    {
        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            if (roots[i].name == objectName)
            {
                return roots[i];
            }
        }

        return null;
    }

    private static bool SceneNeedsUpgrade(Scene scene)
    {
        GameObject room = FindRootObject(scene, "AI Circuit Puzzle Room");
        if (room == null)
        {
            return false;
        }

        return FindChildRecursive(room.transform, "AI Room Door Frame") == null;
    }

    private static Transform FindChildRecursive(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform found = FindChildRecursive(parent.GetChild(i), childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}
