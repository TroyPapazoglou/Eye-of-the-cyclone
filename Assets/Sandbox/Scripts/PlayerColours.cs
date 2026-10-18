using UnityEngine;

public static class PlayerColours
{
    public const int Count = 24;

    public static readonly Color[] Colours =
    {
        // 0 - Black
        Hex("000000"),

        // 1 - White
        Hex("FFFFFF"),

        // 2 - Red
        Hex("E53935"),

        // 3 - Dark Red
        Hex("8E1B1B"),

        // 4 - Orange
        Hex("FB8C00"),

        // 5 - Yellow
        Hex("FDD835"),

        // 6 - Green
        Hex("43A047"),

        // 7 - Dark Green
        Hex("1B5E20"),

        // 8 - Cyan
        Hex("00ACC1"),

        // 9 - Light Blue
        Hex("29B6F6"),

        // 10 - Blue
        Hex("1E88E5"),

        // 11 - Dark Blue
        Hex("1565C0"),

        // 12 - Purple
        Hex("8E24AA"),

        // 13 - Dark Purple
        Hex("4A148C"),

        // 14 - Pink
        Hex("EC407A"),

        // 15 - Magenta
        Hex("D81B60"),

        // 16 - Brown
        Hex("795548"),

        // 17 - Light Brown
        Hex("A1887F"),

        // 18 - Lime
        Hex("C0CA33"),

        // 19 - Teal
        Hex("00897B"),

        // 20 - Navy
        Hex("283593"),

        // 21 - Lavender
        Hex("B39DDB"),

        // 22 - Light Grey
        Hex("BDBDBD"),

        // 23 - Dark Grey
        Hex("424242")
    };

    public static Color GetColour(int index)
    {
        if (index < 0 || index >= Colours.Length)
            return Color.white;

        return Colours[index];
    }

    private static Color Hex(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out Color colour))
            return colour;

        return Color.white;
    }
}