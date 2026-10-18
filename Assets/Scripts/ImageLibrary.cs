using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ImageData
{
    public Texture2D Texture { get; }
    public Sprite Sprite { get; }

    public string FileName { get; }
    public string FilePath { get; }

    private const int MaxImageDimension = 1280;

    private const int MaxImageBytes = 2 * 1024 * 1024;

    public ImageData(Texture2D texture, string fileName, string filePath)
    {
        Texture = texture;

        Sprite = Sprite.Create(
            texture,
            new Rect(
                0,
                0,
                texture.width,
                texture.height
            ),
            new Vector2(0.5f, 0.5f)
        );

        FileName = fileName;
        FilePath = filePath;
    }

    public byte[] GetImageBytes()
    {
        Texture2D sourceTexture = Texture;      

        if (!string.IsNullOrEmpty(FilePath) &&
            File.Exists(FilePath))
        {
            byte[] fileBytes = File.ReadAllBytes(FilePath);

            sourceTexture = new Texture2D(
                2,
                2,
                TextureFormat.RGBA32,
                false
            );

            if (!sourceTexture.LoadImage(fileBytes))
            {
                Object.Destroy(sourceTexture);
                return null;
            }                      
        }

        if (sourceTexture == null)
            return null;

        Texture2D readableTexture = CreateReadableTexture(sourceTexture);      

        Texture2D resizedTexture = ResizeTexture(readableTexture, MaxImageDimension);

        byte[] bytes = resizedTexture.EncodeToJPG(75);

        if (resizedTexture != readableTexture)
            Object.Destroy(resizedTexture);

        Object.Destroy(readableTexture);

        return bytes;
    }

    private Texture2D CreateReadableTexture(Texture2D source)
    {
        RenderTexture renderTexture =
            RenderTexture.GetTemporary(
                source.width,
                source.height,
                0,
                RenderTextureFormat.ARGB32
            );

        Graphics.Blit(source, renderTexture);

        RenderTexture previous =
            RenderTexture.active;

        RenderTexture.active = renderTexture;

        Texture2D readableTexture =
            new Texture2D(
                source.width,
                source.height,
                TextureFormat.RGBA32,
                false
            );

        readableTexture.ReadPixels(
            new Rect(
                0,
                0,
                source.width,
                source.height
            ),
            0,
            0
        );

        readableTexture.Apply();

        RenderTexture.active = previous;

        RenderTexture.ReleaseTemporary(renderTexture);

        return readableTexture;
    }

    private Texture2D ResizeTexture(Texture2D source, int maxDimension)
    {
        if (source.width <= maxDimension &&
            source.height <= maxDimension)
        {
            return source;
        }

        float scale = (float)maxDimension / Mathf.Max(source.width, source.height);

        int width =
            Mathf.RoundToInt(source.width * scale);

        int height =
            Mathf.RoundToInt(source.height * scale);

        RenderTexture renderTexture =
            RenderTexture.GetTemporary(
                width,
                height,
                0,
                RenderTextureFormat.ARGB32
            );

        Graphics.Blit(source, renderTexture);

        RenderTexture previous =
            RenderTexture.active;

        RenderTexture.active = renderTexture;

        Texture2D resized =
            new Texture2D(
                width,
                height,
                TextureFormat.RGBA32,
                false
            );

        resized.ReadPixels(
            new Rect(0, 0, width, height),
            0,
            0
        );

        resized.Apply();

        RenderTexture.active = previous;

        RenderTexture.ReleaseTemporary(renderTexture);

        return resized;
    }
}


public class ImageLibrary : MonoBehaviour
{
    public static ImageLibrary Instance { get; private set; }

    public IReadOnlyList<ImageData> Images => images;

    private readonly List<ImageData> images = new();

    private void Awake()
    {
        Instance = this;

#if UNITY_EDITOR
        LoadEditorImages();
#else
    LoadBuildImages();
#endif
    }

    private void LoadEditorImages()
    {
        images.Clear();

        Texture2D[] loadedImages =
            Resources.LoadAll<Texture2D>("Images");

        foreach (Texture2D texture in loadedImages)
        {
            images.Add(
                new ImageData(
                    texture,
                    texture.name,
                    null
                )
            );
        }

        //Debug.Log($"Loaded {images.Count} editor images.");
    }

    private void LoadBuildImages()
    {
        images.Clear();

        string imageFolder =
            Path.Combine(Application.dataPath, "../Images");

        if (!Directory.Exists(imageFolder))
        {
            Directory.CreateDirectory(imageFolder);

            Debug.Log(
                $"Images folder created at:\n{imageFolder}");

            return;
        }

        string[] files = Directory.GetFiles(imageFolder);

        foreach (string file in files)
        {
            string extension =
                Path.GetExtension(file).ToLower();

            if (extension != ".png" &&
                extension != ".jpg" &&
                extension != ".jpeg")
                continue;

            byte[] bytes = File.ReadAllBytes(file);

            Texture2D texture = new Texture2D(2, 2);

            if (texture.LoadImage(bytes))
            {
                string fileName =
                    Path.GetFileNameWithoutExtension(file);

                texture.name = fileName;

                images.Add(
                    new ImageData(
                        texture,
                        fileName,
                        file
                    )
                );
            }
            else
            {
                Destroy(texture);
            }
        }

        Debug.Log($"Loaded {images.Count} build images.");
    }

}
