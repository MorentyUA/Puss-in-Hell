// NormalMapGenerator.cs — запусти один раз в Editor
using UnityEngine;
using UnityEditor;

public class NormalMapGenerator : MonoBehaviour
{
    [MenuItem("Tools/Generate Water Normal")]
    static void Generate()
    {
        int size = 256;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, true);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = x / (float)size;
                float fy = y / (float)size;

                // Несколько слоёв шума
                float nx = Mathf.PerlinNoise(fx * 8, fy * 8) - 0.5f;
                float ny = Mathf.PerlinNoise(fx * 8 + 100, fy * 8 + 100) - 0.5f;
                nx += (Mathf.PerlinNoise(fx * 16, fy * 16) - 0.5f) * 0.5f;
                ny += (Mathf.PerlinNoise(fx * 16 + 100, fy * 16 + 100) - 0.5f) * 0.5f;

                // Конвертим в normal map формат
                Color c = new Color(nx * 0.5f + 0.5f, ny * 0.5f + 0.5f, 1f, 1f);
                tex.SetPixel(x, y, c);
            }
        }

        tex.Apply();

        byte[] bytes = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes("Assets/WaterNormal.png", bytes);
        AssetDatabase.Refresh();

        Debug.Log("Normal map created at Assets/WaterNormal.png");
    }
}
