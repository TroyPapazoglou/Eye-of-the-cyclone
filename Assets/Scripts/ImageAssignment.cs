using System.Collections.Generic;
using UnityEngine;

public class ImageAssignment : MonoBehaviour
{  

    public static void Assign(params MeshRenderer[][] rendererGroups)
    {
        List<MeshRenderer> renderers = new();

        foreach (MeshRenderer[] group in rendererGroups)
        {
            renderers.AddRange(group);
        }

        IReadOnlyList<ImageData> images = ImageLibrary.Instance.Images;

        if (images.Count == 0)
            return;

        List<ImageData> chosenImages = BuildImageList(images, renderers.Count);

        for (int i = 0; i < renderers.Count; i++)
        {
            MaterialPropertyBlock block = new();

            renderers[i].GetPropertyBlock(block);

            block.SetTexture(
                "_BaseMap",
                chosenImages[i].Texture
            );

            renderers[i].SetPropertyBlock(block);
        }
    }

    private static List<ImageData> BuildImageList(IReadOnlyList<ImageData> images, int requiredCount)
    {
        List<ImageData> result = new();

        while (result.Count < requiredCount)
        {
            List<ImageData> shuffled = new(images);

            Shuffle(shuffled);

            foreach (ImageData image in shuffled)
            {
                if (result.Count >= requiredCount)
                    break;

                result.Add(image);
            }
        }

        SeparateDuplicates(result);

        return result;
    }

    private static void SeparateDuplicates(List<ImageData> images)
    {
        for (int i = 1; i < images.Count; i++)
        {
            if (images[i] != images[i - 1])
                continue;

            for (int j = i + 1; j < images.Count; j++)
            {
                if (images[j] != images[i])
                {
                    (images[i], images[j]) =
                        (images[j], images[i]);

                    break;
                }
            }
        }
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int random = Random.Range(0, i + 1);

            (list[i], list[random]) = (list[random], list[i]);
        }
    }
}
