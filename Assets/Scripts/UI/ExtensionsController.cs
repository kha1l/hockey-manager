using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ExtensionsController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedPlayerText;
    [SerializeField] private Text _offerText;
    [SerializeField] private Transform _candidatesContainer;
    [SerializeField] private Transform _offersContainer;
    [SerializeField] private ExtensionCandidateRowView _candidateRowPrefab;
    [SerializeField] private ExtensionOfferRowView _offerRowPrefab;
    [SerializeField] private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedPlayerText,
        Text offerText,
        Transform candidatesContainer,
        Transform offersContainer,
        ExtensionCandidateRowView candidateRowPrefab,
        ExtensionOfferRowView offerRowPrefab,
        GameScreenController screenController)
    {
        _summaryText = summaryText;
        _selectedPlayerText = selectedPlayerText;
        _offerText = offerText;
        _candidatesContainer = candidatesContainer;
        _offersContainer = offersContainer;
        _candidateRowPrefab = candidateRowPrefab;
        _offerRowPrefab = offerRowPrefab;
        _screenController = screenController;
    }

    public void ShowExtensions(GameState state, string selectedPlayerId, int offerSalary, int offerYears)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("ExtensionsController: UI references are not configured.");
            return;
        }

        GameSession.EnsureContractExtensions();
        List<ContractExtensionCandidateData> candidates = GameSession.GetCurrentTeamExtensionCandidates();
        ContractExtensionSummaryData summary = GameSession.GetCurrentTeamExtensionSummary();
        RenderSummary(summary, GameSession.GetCurrentTeamClubFinances());
        RenderSelectedPlayer(candidates, selectedPlayerId);
        RenderOfferText(offerSalary, offerYears);
        RenderCandidates(candidates);
        RenderOffers(GameSession.GetRecentExtensionOffers(10));
    }

    private void RenderSummary(ContractExtensionSummaryData summary, ClubFinanceData finances)
    {
        if (summary == null)
        {
            _summaryText.text = "Продления: нет данных";
            return;
        }

        _summaryText.text = summary.TeamName
            + "\nEligible: " + summary.EligiblePlayers
            + " | Pending UFA: " + summary.PendingUfaCount
            + " | Pending RFA: " + summary.PendingRfaCount
            + " | ELC: " + summary.ElcExpiringCount
            + "\nHigh interest: " + summary.HighInterestCount
            + " | Low interest: " + summary.LowInterestCount
            + " | Refusing: " + summary.RefusingCount
            + "\nВажный игрок: " + (string.IsNullOrEmpty(summary.MostImportantPlayerName) ? "нет" : summary.MostImportantPlayerName)
            + "\nFinance: payroll " + FormatMoney(finances == null ? 0 : finances.Payroll)
            + " | cap space " + FormatMoney(finances == null ? 0 : finances.SalaryCapSpace)
            + " | budget " + FormatMoney(finances == null ? 0 : finances.Budget)
            + " | health " + (finances == null ? "нет данных" : finances.FinancialHealthLabel)
            + "\n" + summary.Summary;
    }

    private void RenderSelectedPlayer(List<ContractExtensionCandidateData> candidates, string selectedPlayerId)
    {
        ContractExtensionCandidateData selected = FindCandidate(candidates, selectedPlayerId);
        if (selected == null)
        {
            _selectedPlayerText.text = "Выберите игрока для продления";
            return;
        }

        _selectedPlayerText.text = selected.PlayerName
            + " | " + selected.Position
            + " | " + selected.Age
            + " | OVR " + selected.Overall
            + " | POT " + selected.Potential
            + "\nStatus: " + selected.ContractStatus
            + " | Salary: " + FormatMoney(selected.CurrentSalary)
            + " | Years: " + selected.ContractYearsRemaining
            + " | " + selected.Category
            + "\nRoster: " + selected.RosterStatus
            + " | Role: " + selected.PlayerRole
            + " | Usage: " + selected.UsageCategory
            + "\nMorale: " + selected.Morale
            + " | WantsTrade: " + selected.WantsTrade
            + " | Interest: " + selected.ExtensionInterest
            + " " + ContractExtensionConfig.GetInterestLabel(selected.ExtensionInterest)
            + "\nExpected: " + FormatMoney(selected.ExpectedSalary)
            + " x " + selected.ExpectedYears
            + " | Minimum: " + FormatMoney(selected.MinimumSalary)
            + " | Preferred years: " + selected.PreferredYears
            + "\n" + selected.InterestSummary
            + "\n" + selected.AskSummary
            + "\nEligibility: " + selected.EligibilityReason;
    }

    private void RenderOfferText(int offerSalary, int offerYears)
    {
        if (_offerText == null)
        {
            return;
        }

        _offerText.text = offerSalary > 0 && offerYears > 0
            ? "Текущее предложение: " + FormatMoney(offerSalary) + " x " + offerYears + " лет"
            : "Текущее предложение: expected offer или выберите пресет";
    }

    private void RenderCandidates(List<ContractExtensionCandidateData> candidates)
    {
        ClearRows(_candidatesContainer, _candidateRowPrefab.transform);
        _candidateRowPrefab.gameObject.SetActive(false);

        if (candidates == null || candidates.Count == 0)
        {
            CreateInfoRow(_candidatesContainer, "Игроки с 1 годом контракта могут быть продлены заранее.");
            return;
        }

        foreach (ContractExtensionCandidateData candidate in candidates)
        {
            ExtensionCandidateRowView row = Instantiate(_candidateRowPrefab, _candidatesContainer);
            row.name = candidate.PlayerId + "-extension-candidate-row";
            row.gameObject.SetActive(true);
            row.Initialize(candidate, _screenController);
        }
    }

    private void RenderOffers(List<ContractExtensionOfferData> offers)
    {
        ClearRows(_offersContainer, _offerRowPrefab.transform);
        _offerRowPrefab.gameObject.SetActive(false);

        if (offers == null || offers.Count == 0)
        {
            CreateInfoRow(_offersContainer, "История предложений пуста");
            return;
        }

        foreach (ContractExtensionOfferData offer in offers)
        {
            ExtensionOfferRowView row = Instantiate(_offerRowPrefab, _offersContainer);
            row.name = offer.OfferId + "-extension-offer-row";
            row.gameObject.SetActive(true);
            row.Initialize(offer);
        }
    }

    private bool HasRequiredReferences()
    {
        return _summaryText != null
            && _selectedPlayerText != null
            && _candidatesContainer != null
            && _offersContainer != null
            && _candidateRowPrefab != null
            && _offerRowPrefab != null;
    }

    private static ContractExtensionCandidateData FindCandidate(List<ContractExtensionCandidateData> candidates, string playerId)
    {
        if (candidates == null || string.IsNullOrEmpty(playerId))
        {
            return null;
        }

        foreach (ContractExtensionCandidateData candidate in candidates)
        {
            if (candidate != null && candidate.PlayerId == playerId)
            {
                return candidate;
            }
        }

        return null;
    }

    private static void ClearRows(Transform container, Transform prefabTransform)
    {
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            Transform child = container.GetChild(i);
            if (child == prefabTransform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }

    private static void CreateInfoRow(Transform container, string value)
    {
        GameObject rowObject = new GameObject("InfoRow");
        rowObject.transform.SetParent(container, false);

        RectTransform rectTransform = rowObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(820f, 44f);

        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 44f;
        layoutElement.minHeight = 44f;

        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static string FormatMoney(int value)
    {
        return "$" + value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
    }
}
