using UnityEngine;

namespace Assets.Scripts
{
    [ExecuteInEditMode]
    class CausticsProjector : MonoBehaviour
    {
        public int FramesPerSecond = 30;

        private new Renderer renderer;

        public Texture[] causticsTextures = null;

        void Start()
        {
            renderer = GetComponent<Renderer>();
        }

        void Update()
        {

            if (causticsTextures != null && causticsTextures.Length >= 1)
            {
                int causticsIndex = (int)(Time.time * FramesPerSecond) % causticsTextures.Length;
                renderer.sharedMaterial.SetTexture("_EmissionMap", causticsTextures[causticsIndex]);
            }

            Vector3 LightDirection = new Vector3(10, 10, 5);
            var lightMatrix = Matrix4x4.TRS(new Vector3(0, 0, 0), Quaternion.LookRotation(LightDirection, new Vector3(LightDirection.z, LightDirection.x, LightDirection.y)), Vector3.one);
            renderer.sharedMaterial.SetMatrix("_CausticsLightOrientation", lightMatrix);
            print(lightMatrix);
        }
    }
}
