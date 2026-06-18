using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class MobileScrollListHelper
{
    public static void ClearChildren(Transform container)
    {
        if (container == null)
        {
            return;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            UnityEngine.Object.Destroy(container.GetChild(i).gameObject);
        }
    }

    public static T CreateRow<T>(Transform container, T prefab) where T : MonoBehaviour
    {
        if (container == null || prefab == null)
        {
            return null;
        }

        T row = UnityEngine.Object.Instantiate(prefab, container);
        row.gameObject.SetActive(true);
        return row;
    }

    public static int PopulateList<TData, TRow>(
        Transform container,
        TRow rowPrefab,
        List<TData> data,
        int maxRows,
        Action<TRow, TData> initialize) where TRow : MonoBehaviour
    {
        if (container == null || rowPrefab == null || data == null || initialize == null)
        {
            return 0;
        }

        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == rowPrefab.transform)
            {
                continue;
            }

            UnityEngine.Object.Destroy(child.gameObject);
        }

        rowPrefab.gameObject.SetActive(false);
        int shown = UiDisplayLimitConfig.ClampRowCount(data.Count, maxRows);
        for (int i = 0; i < shown; i++)
        {
            TRow row = UnityEngine.Object.Instantiate(rowPrefab, container);
            row.gameObject.SetActive(true);
            initialize(row, data[i]);
        }

        return shown;
    }

    public static void EnsureVerticalLayout(GameObject target)
    {
        if (target == null || target.GetComponent<VerticalLayoutGroup>() != null)
        {
            return;
        }

        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        layout.spacing = MobileUiConfig.RowSpacing;
        layout.childAlignment = TextAnchor.UpperCenter;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    public static void EnsureContentSizeFitter(GameObject target)
    {
        if (target == null || target.GetComponent<ContentSizeFitter>() != null)
        {
            return;
        }

        ContentSizeFitter fitter = target.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    public static void SetPreferredRowHeight(RectTransform rect, float height)
    {
        if (rect == null)
        {
            return;
        }

        rect.sizeDelta = new Vector2(rect.sizeDelta.x, height);
        LayoutElement layout = rect.GetComponent<LayoutElement>();
        if (layout == null)
        {
            layout = rect.gameObject.AddComponent<LayoutElement>();
        }

        layout.minHeight = height;
        layout.preferredHeight = height;
    }
}
