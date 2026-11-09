using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Dev helper: polls Temp/ClaudeBridge/cmd.txt and runs simple editor commands so UI work can be
/// previewed without manual clicking. Commands (one per line):
///   apply                         -> run the Colorful UI restyler
///   shot <Scene> [Panel,Panel2]   -> render the scene's Canvas (optionally with panels enabled) to Temp/ClaudeBridge/<Scene>[_Panel].png
///   refresh                       -> AssetDatabase.Refresh
/// Results are appended to Temp/ClaudeBridge/log.txt. Delete this file when no longer needed.
/// </summary>
[InitializeOnLoad]
public static class ClaudeBridge
{
    private static readonly string Dir = Path.Combine(Directory.GetCurrentDirectory(), "Temp", "ClaudeBridge");
    private static double nextPoll;

    static ClaudeBridge()
    {
        Directory.CreateDirectory(Dir);
        EditorApplication.update += Poll;
        Log("bridge loaded");
    }

    private static void Poll()
    {
        if (EditorApplication.timeSinceStartup < nextPoll) return;
        nextPoll = EditorApplication.timeSinceStartup + 1.0;
        if (EditorApplication.isCompiling || EditorApplication.isUpdating || EditorApplication.isPlayingOrWillChangePlaymode) return;

        string cmdFile = Path.Combine(Dir, "cmd.txt");
        if (!File.Exists(cmdFile)) return;
        string[] lines = File.ReadAllLines(cmdFile);
        File.Delete(cmdFile);

        foreach (string raw in lines)
        {
            string line = raw.Trim();
            if (line.Length == 0) continue;
            try
            {
                Run(line);
            }
            catch (Exception e)
            {
                Log("ERROR " + line + " -> " + e);
            }
        }
        Log("done");
    }

    private static void Run(string line)
    {
        string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        switch (parts[0])
        {
            case "refresh":
                AssetDatabase.Refresh();
                Log("refreshed");
                break;
            case "apply":
                ColorfulUIRestyler.ApplySilently();
                Log("applied");
                break;
            case "identity":
                AppIdentity.Apply();
                Log("identity applied");
                break;
            case "shot":
                Shot(parts[1], parts.Length > 2 ? parts[2].Split(',') : new string[0]);
                break;
            default:
                Log("unknown command: " + line);
                break;
        }
    }

    private static void Shot(string sceneName, string[] panels)
    {
        string path = "Assets/Scenes/" + sceneName + ".unity";
        Scene scene = SceneManager.GetActiveScene();
        if (scene.path != path)
        {
            if (scene.isDirty) EditorSceneManager.SaveScene(scene);
            scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }

        GameObject canvasGo = scene.GetRootGameObjects().First(g => g.name == "Canvas");
        var canvas = canvasGo.GetComponent<Canvas>();

        var toggled = panels.Select(p => canvasGo.transform.Find(p)).Where(t => t != null && !t.gameObject.activeSelf).ToList();
        foreach (var t in toggled) t.gameObject.SetActive(true);

        const int w = 1080, h = 1920;
        var rt = new RenderTexture(w, h, 24, RenderTextureFormat.ARGB32);
        var camGo = new GameObject("__ShotCam") { hideFlags = HideFlags.HideAndDontSave };
        var cam = camGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        cam.targetTexture = rt;
        cam.orthographic = true;

        RenderMode oldMode = canvas.renderMode;
        Camera oldCam = canvas.worldCamera;
        float oldDist = canvas.planeDistance;
        try
        {
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = cam;
            canvas.planeDistance = 10f;
            Canvas.ForceUpdateCanvases();
            cam.Render();
            Canvas.ForceUpdateCanvases();
            cam.Render();

            RenderTexture.active = rt;
            var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
            tex.Apply();
            RenderTexture.active = null;

            string suffix = panels.Length > 0 ? "_" + string.Join("_", panels.Select(p => p.Replace("/", "-"))) : "";
            string outPath = Path.Combine(Dir, sceneName + suffix + ".png");
            File.WriteAllBytes(outPath, tex.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(tex);
            Log("shot " + outPath);
        }
        finally
        {
            canvas.renderMode = oldMode;
            canvas.worldCamera = oldCam;
            canvas.planeDistance = oldDist;
            foreach (var t in toggled) t.gameObject.SetActive(false);
            cam.targetTexture = null;
            UnityEngine.Object.DestroyImmediate(camGo);
            rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);
            // The temporary changes are reverted; don't leave the scene marked dirty.
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
    }

    private static void Log(string msg)
    {
        try
        {
            File.AppendAllText(Path.Combine(Dir, "log.txt"), DateTime.Now.ToString("HH:mm:ss") + " " + msg + Environment.NewLine);
        }
        catch
        {
            // ignored
        }
    }
}
