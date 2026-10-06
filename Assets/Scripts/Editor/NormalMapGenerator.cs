using System.IO;
using UnityEditor;
using UnityEngine;

namespace PussInHell.EditorTools
{
    public static class NormalMapGenerator
    {
        private const string OutputPath = "Assets/Visual/Textures/Generated/WaterNormal.png";

        [MenuItem("Tools/Puss in Hell/Generate Water Normal Map")]
        private static void Generate()
        {
            const int size = 256;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, true);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float fx = x / (float)size;
                    float fy = y / (float)size;

                    float nx = Mathf.PerlinNoise(fx * 8f, fy * 8f) - 0.5f;
                    float ny = Mathf.PerlinNoise(fx * 8f + 100f, fy * 8f + 100f) - 0.5f;
                    nx += (Mathf.PerlinNoise(fx * 16f, fy * 16f) - 0.5f) * 0.5f;
                    ny += (Mathf.PerlinNoise(fx * 16f + 100f, fy * 16f + 100f) - 0.5f) * 0.5f;

                    texture.SetPixel(x, y, new Color(nx * 0.5f + 0.5f, ny * 0.5f + 0.5f, 1f, 1f));
                }
            }

            texture.Apply();
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            File.WriteAllBytes(OutputPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(OutputPath);
            if (AssetImporter.GetAtPath(OutputPath) is TextureImporter importer)
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.SaveAndReimport();
            }
        }
    }
}
