using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewsController : MonoBehaviour
{
    public const string FilterAll = "All";
    public const string FilterUserTeam = "UserTeam";

    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _filterText;
    [SerializeField] private Transform _newsContainer;
    [SerializeField] private NewsItemRowView _newsRowPrefab;

    public void Configure(
        Text summaryText,
        Text filterText,
        Transform newsContainer,
        NewsItemRowView newsRowPrefab)
    {
        _summaryText = summaryText;
        _filterText = filterText;
        _newsContainer = newsContainer;
        _newsRowPrefab = newsRowPrefab;
    }

    public void ShowNews(GameState state, string filter, GameScreenController screenController)
    {
        if (_newsContainer == null || _newsRowPrefab == null)
        {
            Debug.LogError("NewsController: UI references are not configured.");
            return;
        }

        NewsFeedService.EnsureNewsFeed(state);
        ClearRows();
        _newsRowPrefab.gameObject.SetActive(false);

        List<NewsItemData> items = GetFilteredNews(state, filter, UiDisplayLimitConfig.MaxNewsRows);
        SetText(_summaryText, BuildSummaryText(state, items));
        SetText(_filterText, "Фильтр: " + GetFilterLabel(filter));

        if (items.Count == 0)
        {
            CreateInfoRow("Новости появятся после важных событий: обменов, травм, драфта, наград и завершения сезона.");
            return;
        }

        foreach (NewsItemData item in items)
        {
            if (item == null)
            {
                continue;
            }

            NewsItemRowView row = Instantiate(_newsRowPrefab, _newsContainer);
            row.name = "news-" + item.NewsId;
            row.gameObject.SetActive(true);
            row.Initialize(item, screenController);
        }
    }

    private static List<NewsItemData> GetFilteredNews(GameState state, string filter, int maxCount)
    {
        if (filter == FilterUserTeam)
        {
            return NewsFeedService.GetUserTeamNews(state, maxCount);
        }

        if (string.IsNullOrEmpty(filter) || filter == FilterAll)
        {
            return NewsFeedService.GetLatestNews(state, maxCount);
        }

        return NewsFeedService.GetNewsByCategory(state, filter, maxCount);
    }

    private static string BuildSummaryText(GameState state, List<NewsItemData> filteredItems)
    {
        int total = state == null || state.NewsFeed == null || state.NewsFeed.Items == null
            ? 0
            : state.NewsFeed.Items.Count;
        int unread = 0;
        if (state != null && state.NewsFeed != null && state.NewsFeed.Items != null)
        {
            foreach (NewsItemData item in state.NewsFeed.Items)
            {
                if (item != null && !item.IsRead)
                {
                    unread++;
                }
            }
        }

        string lastNews = state == null || state.NewsFeed == null || string.IsNullOrEmpty(state.NewsFeed.LastNewsAtUtc)
            ? "нет"
            : state.NewsFeed.LastNewsAtUtc;

        return "Новостей: " + total
            + " | Непрочитано: " + unread
            + " | Показано: " + (filteredItems == null ? 0 : filteredItems.Count)
            + "\nПоследняя новость: " + lastNews
            + "\n" + UiDisplayLimitConfig.BuildLimitMessage(filteredItems == null ? 0 : filteredItems.Count, total);
    }

    private static string GetFilterLabel(string filter)
    {
        if (filter == FilterUserTeam)
        {
            return "Моя команда";
        }

        if (string.IsNullOrEmpty(filter) || filter == FilterAll)
        {
            return "Все";
        }

        return filter;
    }

    private void ClearRows()
    {
        Transform prefabTransform = _newsRowPrefab.transform;
        for (int i = _newsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _newsContainer.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private void CreateInfoRow(string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(_newsContainer, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 48f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 48f;
        layoutElement.minHeight = 48f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static void SetText(Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }
}
