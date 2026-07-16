using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class TutorialSetup : MonoBehaviour
{
    private void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
        {
            var tileBuildRequest = new TileBuildRequest("crop_empty", new Vector2Int(0,0));
            var result = TileBuildService.instance.TryBuild(tileBuildRequest);
            Log.error(result.Message);
        }
    }
}