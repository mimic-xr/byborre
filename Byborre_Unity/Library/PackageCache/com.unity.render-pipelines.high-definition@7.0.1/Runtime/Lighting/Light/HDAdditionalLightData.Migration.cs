using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.HighDefinition;
#endif
using UnityEngine.Serialization;

namespace UnityEngine.Rendering.HighDefinition
{
    public partial class HDAdditionalLightData : ISerializationCallbackReceiver
    {
        // TODO: Use proper migration toolkit
        // 3. Added ShadowNearPlane to HDRP additional light data, we don't use Light.shadowNearPlane anymore
        // 4. Migrate HDAdditionalLightData.lightLayer to Light.renderingLayerMask
        // 5. Added the ShadowLayer
        enum Version
        {
            Initial,
            Intensity,
            ShadowNearPlane,
            LightLayer,
            ShadowLayer,

            // Note: Latest must be at the end of the enum
            Latest,
        }

        [SerializeField]
        private Version m_Version = Version.Latest;

        // To be able to have correct default values for our lights and to also control the conversion of intensity from the light editor (so it is compatible with GI)
        // we add intensity (for each type of light we want to manage).
        [System.Obsolete("directionalIntensity is deprecated, use intensity and lightUnit instead")]
        /// <summary>
        /// Obsolete, use intensity instead
        /// </summary>
        public float directionalIntensity = k_DefaultDirectionalLightIntensity;
        [System.Obsolete("punctualIntensity is deprecated, use intensity and lightUnit instead")]
        /// <summary>
        /// Obsolete, use intensity instead
        /// </summary>
        public float punctualIntensity = k_DefaultPunctualLightIntensity;
        [System.Obsolete("areaIntensity is deprecated, use intensity and lightUnit instead")]
        /// <summary>
        /// Obsolete, use intensity instead
        /// </summary>
        public float areaIntensity = k_DefaultAreaLightIntensity;

        [Obsolete("Use Light.renderingLayerMask instead")]
        /// <summary>
        /// Obsolete, use Light.renderingLayerMask instead
        /// </summary>
        public LightLayerEnum lightLayers = LightLayerEnum.LightLayerDefault;

        void ISerializationCallbackReceiver.OnAfterDeserialize() {}

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            UpdateBounds();
        }

        void OnEnable()
        {
            UpgradeLight();

            if (shadowUpdateMode == ShadowUpdateMode.OnEnable)
                m_ShadowMapRenderedSinceLastRequest = false;
        }

        internal void UpgradeLight()
        {
// Disable the warning generated by deprecated fields (areaIntensity, directionalIntensity, ...)
#pragma warning disable 618

            if ((int)m_Version <= 2)
            {
                // ShadowNearPlane have been move to HDRP as default legacy unity clamp it to 0.1 and we need to be able to go below that
                shadowNearPlane = legacyLight.shadowNearPlane;
            }
            if ((int)m_Version <= 3)
            {
                legacyLight.renderingLayerMask = LightLayerToRenderingLayerMask((int)lightLayers, legacyLight.renderingLayerMask);
            }
            if ((int)m_Version <= 4)
            {
                // When we upgrade the option to decouple light and shadow layers will be disabled
                // so we can sync the shadow layer mask (from the legacyLight) and the new light layer mask
                lightlayersMask = (LightLayerEnum)RenderingLayerMaskToLightLayer(legacyLight.renderingLayerMask);
            }

            m_Version = Version.Latest;

#pragma warning restore 0618
        }
    }
}