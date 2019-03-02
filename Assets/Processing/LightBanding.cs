using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

[Serializable]
[PostProcess(typeof(LightBandingRenderer), PostProcessEvent.BeforeStack, "Custom/LightBanding")]
public sealed class LightBanding : PostProcessEffectSettings {
    [Range(0.0f, 1.0f)] public FloatParameter colourAccuracy = new FloatParameter { value = 0.1f };
}

public sealed class LightBandingRenderer : PostProcessEffectRenderer<LightBanding> {
    public override void Render(PostProcessRenderContext context) {
        var sheet = context.propertySheets.Get(Shader.Find("Hidden/LightBanding"));
        sheet.properties.SetFloat("_ColourAccuracy", settings.colourAccuracy);
        context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
    }
}
