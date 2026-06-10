using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TradeHistoryRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(TradeProposalData trade)
    {
        if (trade == null)
        {
            _infoText.text = "История обмена недоступна";
            return;
        }

        trade.EnsureAssets();
        string text = trade.FromTeamName + " отдал: " + BuildAssetsText(trade.AssetsFromUserTeam, trade.PlayerFromUserTeam)
            + "\n" + trade.ToTeamName + " отдал: " + BuildAssetsText(trade.AssetsFromOtherTeam, trade.PlayerFromOtherTeam)
            + " | " + trade.Status;

        if (trade.Status == "Rejected" && !string.IsNullOrEmpty(trade.RejectionReason))
        {
            text += " | " + trade.RejectionReason;
        }

        _infoText.text = text;
    }

    private static string BuildAssetsText(List<TradeAssetData> assets, TradeAssetData fallbackPlayer)
    {
        List<string> parts = new List<string>();
        if (assets != null)
        {
            foreach (TradeAssetData asset in assets)
            {
                string value = FormatAsset(asset);
                if (!string.IsNullOrEmpty(value))
                {
                    parts.Add(value);
                }
            }
        }

        if (parts.Count == 0 && fallbackPlayer != null && !string.IsNullOrEmpty(fallbackPlayer.PlayerName))
        {
            parts.Add(fallbackPlayer.PlayerName);
        }

        return parts.Count == 0 ? "ничего" : string.Join(" + ", parts.ToArray());
    }

    private static string FormatAsset(TradeAssetData asset)
    {
        if (asset == null)
        {
            return "";
        }

        if (asset.AssetType == "Player")
        {
            return asset.PlayerName;
        }

        if (asset.AssetType == "DraftPick")
        {
            return "Round " + asset.DraftRound + " pick from " + asset.OriginalTeamName;
        }

        return "";
    }
}
