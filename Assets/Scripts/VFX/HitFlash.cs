using UnityEngine;
using System.Collections;

public class HitFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    // The color property in the shader graph 
    private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");

    private Renderer[] renderers;
    private Coroutine flashRoutine; //shoutout corrotinas 

    private void Awake()
    {
        // Select all the renderers inside the object
        renderers = GetComponentsInChildren<Renderer>();
    }

    public void Flash()
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(DoFlash());
    }

    private IEnumerator DoFlash()
    {
        // Flash color = red, but it can change
        SetFlashColor(flashColor);

        yield return new WaitForSeconds(flashDuration);

        // reset to black, aka alpha = 0 in the shader graph
        SetFlashColor(Color.black);

        flashRoutine = null;
    }

    private void SetFlashColor(Color color)
    {
        foreach (var r in renderers)
        {
            // changes the color of the material, instead of creating a new material
            MaterialPropertyBlock block = new MaterialPropertyBlock();
            r.GetPropertyBlock(block);
            block.SetColor(FlashColorID, color);
            r.SetPropertyBlock(block);
        }
    }
}