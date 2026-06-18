using System;
using System.Collections.Generic;

public static class NewsFeedService
{
    public static void EnsureNewsFeed(GameState state)
    {
        if (state == null)
        {
            return;
        }

        if (state.NewsFeed == null)
        {
            state.NewsFeed = new NewsFeedData();
        }

        state.NewsFeed.EnsureItems();
    }

    public static NewsItemData AddNews(
        GameState state,
        string category,
        string title,
        string body,
        int importance,
        string teamId = "",
        string teamName = "",
        string playerId = "",
        string playerName = "",
        string relatedId = "")
    {
        EnsureNewsFeed(state);
        if (state == null)
        {
            return null;
        }

        if (!string.IsNullOrEmpty(relatedId) && NewsAlreadyExists(state, category, relatedId))
        {
            return FindNews(state, category, relatedId);
        }

        string createdAt = DateTime.UtcNow.ToString("o");
        NewsItemData item = new NewsItemData
        {
            NewsId = Guid.NewGuid().ToString("N"),
            SeasonStartYear = state.CurrentSeasonStartYear,
            SeasonEndYear = state.CurrentSeasonEndYear,
            DateLabel = BuildDateLabel(state),
            Category = string.IsNullOrEmpty(category) ? NewsConfig.CategoryTeam : category,
            Title = string.IsNullOrEmpty(title) ? "News update" : title,
            Body = string.IsNullOrEmpty(body) ? "" : body,
            TeamId = string.IsNullOrEmpty(teamId) ? "" : teamId,
            TeamName = string.IsNullOrEmpty(teamName) ? "" : teamName,
            PlayerId = string.IsNullOrEmpty(playerId) ? "" : playerId,
            PlayerName = string.IsNullOrEmpty(playerName) ? "" : playerName,
            RelatedId = string.IsNullOrEmpty(relatedId) ? "" : relatedId,
            Importance = NewsConfig.ClampImportance(importance),
            IsUserTeamRelated = IsUserTeamRelated(state, teamId, playerId),
            IsRead = false,
            CreatedAtUtc = createdAt
        };

        state.NewsFeed.Items.Add(item);
        state.NewsFeed.TotalCreated++;
        state.NewsFeed.LastNewsAtUtc = createdAt;
        TrimNewsFeed(state);
        return item;
    }

    public static List<NewsItemData> GetLatestNews(GameState state, int maxCount)
    {
        EnsureNewsFeed(state);
        List<NewsItemData> items = state == null || state.NewsFeed == null || state.NewsFeed.Items == null
            ? new List<NewsItemData>()
            : new List<NewsItemData>(state.NewsFeed.Items);
        items.Sort(CompareNewsNewestFirst);
        TrimList(items, maxCount);
        return items;
    }

    public static List<NewsItemData> GetUserTeamNews(GameState state, int maxCount)
    {
        List<NewsItemData> filtered = new List<NewsItemData>();
        foreach (NewsItemData item in GetLatestNews(state, 0))
        {
            if (item != null && item.IsUserTeamRelated)
            {
                filtered.Add(item);
            }
        }

        TrimList(filtered, maxCount);
        return filtered;
    }

    public static List<NewsItemData> GetNewsByCategory(GameState state, string category, int maxCount)
    {
        List<NewsItemData> filtered = new List<NewsItemData>();
        foreach (NewsItemData item in GetLatestNews(state, 0))
        {
            if (item != null && item.Category == category)
            {
                filtered.Add(item);
            }
        }

        TrimList(filtered, maxCount);
        return filtered;
    }

    public static void MarkNewsAsRead(GameState state, string newsId)
    {
        EnsureNewsFeed(state);
        if (state == null || state.NewsFeed == null || state.NewsFeed.Items == null || string.IsNullOrEmpty(newsId))
        {
            return;
        }

        foreach (NewsItemData item in state.NewsFeed.Items)
        {
            if (item != null && item.NewsId == newsId)
            {
                item.IsRead = true;
                return;
            }
        }
    }

    public static void TrimNewsFeed(GameState state)
    {
        EnsureNewsFeed(state);
        if (state == null || state.NewsFeed == null || state.NewsFeed.Items == null)
        {
            return;
        }

        state.NewsFeed.Items.Sort(CompareNewsNewestFirst);
        while (state.NewsFeed.Items.Count > NewsConfig.MaxNewsItemsToKeep)
        {
            state.NewsFeed.Items.RemoveAt(state.NewsFeed.Items.Count - 1);
        }
    }

    public static bool IsUserTeamRelated(GameState state, string teamId, string playerId)
    {
        if (state == null || string.IsNullOrEmpty(state.SelectedTeamId))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(teamId) && teamId == state.SelectedTeamId)
        {
            return true;
        }

        TeamData userTeam = FindTeam(state, state.SelectedTeamId);
        if (userTeam == null || string.IsNullOrEmpty(playerId))
        {
            return false;
        }

        userTeam.EnsurePlayers();
        foreach (PlayerData player in userTeam.Players)
        {
            if (player != null && player.Id == playerId)
            {
                return true;
            }
        }

        return false;
    }

    private static bool NewsAlreadyExists(GameState state, string category, string relatedId)
    {
        return FindNews(state, category, relatedId) != null;
    }

    private static NewsItemData FindNews(GameState state, string category, string relatedId)
    {
        if (state == null || state.NewsFeed == null || state.NewsFeed.Items == null || string.IsNullOrEmpty(relatedId))
        {
            return null;
        }

        foreach (NewsItemData item in state.NewsFeed.Items)
        {
            if (item != null && item.Category == category && item.RelatedId == relatedId)
            {
                return item;
            }
        }

        return null;
    }

    private static string BuildDateLabel(GameState state)
    {
        if (state == null)
        {
            return "";
        }

        DateTime date = LeagueDateService.GetCurrentLeagueDate(state);
        return date.ToString("yyyy-MM-dd");
    }

    private static TeamData FindTeam(GameState state, string teamId)
    {
        if (state == null || state.Teams == null || string.IsNullOrEmpty(teamId))
        {
            return null;
        }

        foreach (TeamData team in state.Teams)
        {
            if (team != null && team.Id == teamId)
            {
                return team;
            }
        }

        return null;
    }

    private static int CompareNewsNewestFirst(NewsItemData left, NewsItemData right)
    {
        string rightDate = right == null || string.IsNullOrEmpty(right.CreatedAtUtc) ? "" : right.CreatedAtUtc;
        string leftDate = left == null || string.IsNullOrEmpty(left.CreatedAtUtc) ? "" : left.CreatedAtUtc;
        int dateComparison = string.Compare(rightDate, leftDate, StringComparison.Ordinal);
        if (dateComparison != 0)
        {
            return dateComparison;
        }

        int importanceComparison = (right == null ? 0 : right.Importance).CompareTo(left == null ? 0 : left.Importance);
        if (importanceComparison != 0)
        {
            return importanceComparison;
        }

        return string.Compare(left == null ? "" : left.NewsId, right == null ? "" : right.NewsId, StringComparison.Ordinal);
    }

    private static void TrimList<T>(List<T> items, int maxCount)
    {
        if (items == null || maxCount <= 0)
        {
            return;
        }

        while (items.Count > maxCount)
        {
            items.RemoveAt(items.Count - 1);
        }
    }
}
