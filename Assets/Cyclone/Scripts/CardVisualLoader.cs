using UnityEngine;

public class CardVisualLoader : MonoBehaviour
{
    public static Sprite LoadSprite(CardInfo info)
    {
        string suitName = info.GetSuitName(info.suit);
        string rankName = info.GetRankName(info.rank);

        string path =
            $"UI/Cards/{rankName}of{suitName}s";

        Sprite sprite = Resources.Load<Sprite>(path);

        if (sprite == null)
        {
            Debug.LogError($"Couldn't load card sprite: {path}");
        }

        return sprite;
    }
}
