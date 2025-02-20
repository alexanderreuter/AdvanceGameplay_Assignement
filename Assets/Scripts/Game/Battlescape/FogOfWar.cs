using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.PostProcessing;
namespace Game.Battlescape
{
    [Serializable]
    [PostProcess(typeof(FogOfWarRenderer), PostProcessEvent.AfterStack, "Battle/Fog of War")]
    public sealed class FogOfWar : PostProcessEffectSettings
    {
        [Tooltip("Number Blur Iterations.")]
        public IntParameter BlurIterations = new IntParameter { value = 1 };

        [Range(0.1f, 5.0f), Tooltip("Blur Size.")]
        public FloatParameter BlurSize = new FloatParameter { value = 3.0f };
    }

    public sealed class FogOfWarRenderer : PostProcessEffectRenderer<FogOfWar>
    {
        public enum Pass
        {
            Downsample = 0,
            BlurVertical = 1,
            BlurHorizontal = 2,
        }

        public override void Render(PostProcessRenderContext context)
        {
            CommandBuffer command = context.command;

            command.BeginSample("FogOfWar");

            // blur background
            int downsample = 1;
            int blurIterations = settings.BlurIterations;
            float blurSize = settings.BlurSize;
            float widthMod = 1.0f / (1.0f * (1 << downsample));

            int rtW = context.width >> downsample;
            int rtH = context.height >> downsample;

            PropertySheet sheet = context.propertySheets.Get(Shader.Find("Hidden/Custom/PostProcessBlur"));
            sheet.properties.Clear();
            sheet.properties.SetVector("_Parameter", new Vector4(blurSize * widthMod, -blurSize * widthMod, 0.0f, 0.0f));

            int blurId = Shader.PropertyToID("_BlurPostProcessEffect");
            command.GetTemporaryRT(blurId, rtW, rtH, 0, FilterMode.Bilinear);
            command.BlitFullscreenTriangle(context.source, blurId, sheet, (int)Pass.Downsample);

            int pass = 0;
            int rtIndex = 0;
            for (int i = 0; i < blurIterations; i++)
            {
                float iterationOffs = i * 1.0f;
                sheet.properties.SetVector("_Parameter", new Vector4(blurSize * widthMod + iterationOffs, -blurSize * widthMod - iterationOffs, 0.0f, 0.0f));

                // Vertical blur..
                int rtId2 = Shader.PropertyToID("_BlurPostProcessEffect" + rtIndex++);
                command.GetTemporaryRT(rtId2, rtW, rtH, 0, FilterMode.Bilinear);
                command.BlitFullscreenTriangle(blurId, rtId2, sheet, (int)Pass.BlurVertical + pass);
                command.ReleaseTemporaryRT(blurId);
                blurId = rtId2;

                // Horizontal blur..
                rtId2 = Shader.PropertyToID("_BlurPostProcessEffect" + rtIndex++);
                command.GetTemporaryRT(rtId2, rtW, rtH, 0, FilterMode.Bilinear);
                command.BlitFullscreenTriangle(blurId, rtId2, sheet, (int)Pass.BlurHorizontal + pass);
                command.ReleaseTemporaryRT(blurId);
                blurId = rtId2;
            }

            // compose Fog of War
            Matrix4x4 projectionMatrix = GL.GetGPUProjectionMatrix(context.camera.projectionMatrix, false);
            sheet = context.propertySheets.Get(Shader.Find("Hidden/Battle/FogOfWar"));
            sheet.properties.SetMatrix("unity_ViewToWorldMatrix", context.camera.cameraToWorldMatrix);
            sheet.properties.SetMatrix("unity_InverseProjectionMatrix", projectionMatrix.inverse);
            command.SetGlobalTexture("_Blur", blurId);

            if (Level.Instance != null && Level.Instance.Sight != null)
            {
                sheet.properties.SetVector("_WorldSize", (Vector3)Level.Instance.m_vSize);
                sheet.properties.SetTexture("_Vision", Level.Instance.Sight);
            }

            // blit result
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);

            // cleanup
            command.ReleaseTemporaryRT(blurId);
            command.EndSample("FogOfWar");
        }
    }
}