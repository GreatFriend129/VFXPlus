using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ModLoader;

namespace VFXPlus.Common
{
    internal static class ShaderHelper
    {
        /// <summary>
        /// Function for filling the parameters of the RadialPulse2 shader.
        /// </summary>
        /// <param name="causticTexture">The noise texture</param>
        /// <param name="uTime">Overall time of the effect</param>
        /// <param name="progress">0-1 progress of the entire effect</param>
        /// <param name="rotation"></param>
        /// <param name="flowSpeed">How fast the caustic texture flows outward</param>
        /// <param name="zoom">How zoomed into the causticTexture it should be</param>
        /// <param name="colors">The colors of the effect. Should generally be ordered darkest->brightest and start with Black</param>
        /// <param name="finalColIntensity">How intense the color of the effect is</param>
        /// <param name="posterizationSteps">How many posterization steps the effect should use. 0 means no posterization at all</param>
        /// <param name="totalAlpha">Total alpha of the effect</param>
        /// <param name="zFadeStrength">How much the effect fades out horizontally. Useful for making the effect appear like a 3d ring</param>
        /// <param name="ringRadiusStart">Starting radius of the ring</param>
        /// <param name="ringThicknessStart">Starting thickness of the ring</param>
        /// <param name="ringPower"></param>
        /// <param name="ringMult"></param>
        /// <param name="ringWaveSpeed"></param>
        /// <param name="ringWaveStrength"></param>
        /// <param name="ringWaveLength"></param>
        /// <returns>Returns the RadialPulse2 effect</returns>
        public static Effect SetRadialPulse2Params(Texture2D causticTexture, float uTime, float progress, float rotation, float flowSpeed, float zoom,
            Vector3[] colors, float finalColIntensity, float posterizationSteps, float totalAlpha, float zFadeStrength,
            float ringRadiusStart, float ringThicknessStart, float ringPower, float ringMult, float ringWaveSpeed = 0f, float ringWaveStrength = 0f, float ringWaveLength = 0f)
        {
            Effect myEffect = VFXPlus.RadialPulse2Shader;

            myEffect.Parameters["causticTexture"].SetValue(causticTexture);
            myEffect.Parameters["causticTexture2"].SetValue(causticTexture);
            myEffect.Parameters["uTime"].SetValue(uTime);
            myEffect.Parameters["progress"].SetValue(progress);
            myEffect.Parameters["customRotation"].SetValue(rotation);

            myEffect.Parameters["texture1Alpha"].SetValue(1f);
            myEffect.Parameters["texture2Alpha"].SetValue(0f);

            myEffect.Parameters["ringRadiusStart"].SetValue(ringRadiusStart);
            myEffect.Parameters["ringThicknessStart"].SetValue(ringThicknessStart);
            myEffect.Parameters["ringPower"].SetValue(ringPower);
            myEffect.Parameters["ringMult"].SetValue(ringMult);
            myEffect.Parameters["ringWaveSpeed"].SetValue(ringWaveSpeed);
            myEffect.Parameters["ringWaveStrength"].SetValue(ringWaveStrength);
            myEffect.Parameters["ringWaveLength"].SetValue(ringWaveLength);

            myEffect.Parameters["gradColors"].SetValue(colors);
            myEffect.Parameters["numberOfColors"].SetValue(colors.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(finalColIntensity); 
            myEffect.Parameters["posterizationSteps"].SetValue(posterizationSteps);

            myEffect.Parameters["totalAlpha"].SetValue(totalAlpha);
            myEffect.Parameters["fadeStrength"].SetValue(zFadeStrength); 


            myEffect.Parameters["zoom"].SetValue(zoom);
            myEffect.Parameters["flowSpeed"].SetValue(flowSpeed);

            return myEffect;
        }
        /// <summary>
        /// Function for filling the parameters of the RadialPulse2 shader.
        /// </summary>
        /// <param name="causticTexture">The first noise texture</param>
        /// <param name="causticTexture2">The second noise texture</param>
        /// <param name="uTime">Overall time of the effect</param>
        /// <param name="progress">0-1 progress of the entire effect</param>
        /// <param name="tex1Alpha">Alpha of the first caustic texture</param>
        /// <param name="tex2Alpha">Alpha of the second caustic texture</param>
        /// <param name="rotation"></param>
        /// <param name="flowSpeed">How fast the caustic texture flows outward</param>
        /// <param name="zoom">How zoomed into the causticTexture it should be</param>
        /// <param name="colors">The colors of the effect. Should generally be ordered darkest->brightest and start with Black</param>
        /// <param name="finalColIntensity">How intense the color of the effect is</param>
        /// <param name="posterizationSteps">How many posterization steps the effect should use. 0 means no posterization at all</param>
        /// <param name="totalAlpha">Total alpha of the effect</param>
        /// <param name="zFadeStrength">How much the effect fades out horizontally. Useful for making the effect appear like a 3d ring</param>
        /// <param name="ringRadiusStart">Starting radius of the ring</param>
        /// <param name="ringThicknessStart">Starting thickness of the ring</param>
        /// <param name="ringPower"></param>
        /// <param name="ringMult"></param>
        /// <param name="ringWaveSpeed"></param>
        /// <param name="ringWaveStrength"></param>
        /// <param name="ringWaveLength"></param>
        /// <returns>Returns the RadialPulse2 effect</returns>
        public static Effect SetRadialPulse2Params(Texture2D causticTexture, Texture2D causticTexture2, float uTime, float progress, float tex1Alpha, float tex2Alpha, float rotation, float flowSpeed, float zoom,
            Vector3[] colors, float finalColIntensity, float posterizationSteps, float totalAlpha, float zFadeStrength,
            float ringRadiusStart, float ringThicknessStart, float ringPower, float ringMult, float ringWaveSpeed = 0f, float ringWaveStrength = 0f, float ringWaveLength = 0f)
        {
            Effect myEffect = VFXPlus.RadialPulse2Shader;

            myEffect.Parameters["causticTexture"].SetValue(causticTexture);
            myEffect.Parameters["causticTexture2"].SetValue(causticTexture2);
            myEffect.Parameters["uTime"].SetValue(uTime);
            myEffect.Parameters["progress"].SetValue(progress);
            myEffect.Parameters["customRotation"].SetValue(rotation);

            myEffect.Parameters["texture1Alpha"].SetValue(tex1Alpha);
            myEffect.Parameters["texture2Alpha"].SetValue(tex2Alpha);

            myEffect.Parameters["ringRadiusStart"].SetValue(ringRadiusStart);
            myEffect.Parameters["ringThicknessStart"].SetValue(ringThicknessStart);
            myEffect.Parameters["ringPower"].SetValue(ringPower);
            myEffect.Parameters["ringMult"].SetValue(ringMult);
            myEffect.Parameters["ringWaveSpeed"].SetValue(ringWaveSpeed);
            myEffect.Parameters["ringWaveStrength"].SetValue(ringWaveStrength);
            myEffect.Parameters["ringWaveLength"].SetValue(ringWaveLength);

            myEffect.Parameters["gradColors"].SetValue(colors);
            myEffect.Parameters["numberOfColors"].SetValue(colors.Length);
            myEffect.Parameters["finalColIntensity"].SetValue(finalColIntensity);
            myEffect.Parameters["posterizationSteps"].SetValue(posterizationSteps);

            myEffect.Parameters["totalAlpha"].SetValue(totalAlpha);
            myEffect.Parameters["fadeStrength"].SetValue(zFadeStrength);

            myEffect.Parameters["zoom"].SetValue(zoom);
            myEffect.Parameters["flowSpeed"].SetValue(flowSpeed);

            return myEffect;
        }
    }
}
