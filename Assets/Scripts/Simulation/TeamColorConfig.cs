using UnityEngine;

public static class TeamColorConfig
{
    public static string ColorNameToHex(string colorName)
    {
        string normalized = NormalizeColorName(colorName);
        switch (normalized)
        {
            case "красный":
                return "#D71920";
            case "синий":
                return "#0033A0";
            case "темно-синий":
                return "#001F4E";
            case "голубой":
                return "#4DB7E5";
            case "бирюзовый":
                return "#00A6A6";
            case "белый":
                return "#FFFFFF";
            case "черный":
                return "#111111";
            case "серебряный":
                return "#C0C0C0";
            case "золотой":
                return "#D4AF37";
            case "зеленый":
                return "#00843D";
            case "оранжевый":
                return "#F58220";
            case "фиолетовый":
                return "#5B2C83";
            case "серый":
                return "#808080";
            case "желтый":
                return "#FFD200";
            default:
                Debug.LogWarning("Unknown team color name: " + colorName);
                return "#FFFFFF";
        }
    }

    public static string NormalizeColorName(string colorName)
    {
        if (string.IsNullOrEmpty(colorName))
        {
            return "";
        }

        return colorName.Trim().ToLowerInvariant().Replace('ё', 'е');
    }

    public static Color HexToColor(string hex)
    {
        if (string.IsNullOrEmpty(hex))
        {
            return Color.white;
        }

        return ColorUtility.TryParseHtmlString(hex, out Color color) ? color : Color.white;
    }
}
