using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GmCareerController : MonoBehaviour
{
    [SerializeField] private Text _summaryText;
    [SerializeField] private Text _selectedOfferText;
    [SerializeField] private Transform _offersContainer;
    [SerializeField] private Transform _eventsContainer;
    [SerializeField] private GmJobOfferRowView _offerRowPrefab;
    [SerializeField] private GmCareerEventRowView _eventRowPrefab;
    private GameScreenController _screenController;

    public void Configure(
        Text summaryText,
        Text selectedOfferText,
        Transform offersContainer,
        Transform eventsContainer,
        GmJobOfferRowView offerRowPrefab,
        GmCareerEventRowView eventRowPrefab,
        GameScreenController screenController)
    {
        _summaryText = summaryText;
        _selectedOfferText = selectedOfferText;
        _offersContainer = offersContainer;
        _eventsContainer = eventsContainer;
        _offerRowPrefab = offerRowPrefab;
        _eventRowPrefab = eventRowPrefab;
        _screenController = screenController;
    }

    public void ShowGmCareer(GameState state, string selectedOfferId)
    {
        if (!HasRequiredReferences())
        {
            Debug.LogError("GmCareerController: UI references are not configured.");
            return;
        }

        GameSession.EnsureGmCareer();
        RenderSummary(state);
        RenderSelectedOffer(state, selectedOfferId);
        RenderOffers(state);
        RenderEvents(state);
    }

    private void RenderSummary(GameState state)
    {
        GmCareerData career = state == null ? null : state.GmCareer;
        if (career == null)
        {
            _summaryText.text = "GM Career недоступна";
            return;
        }

        string warning = career.IsFired || career.IsUnemployed
            ? "\nВы были уволены. Выберите новое предложение работы."
            : "";
        _summaryText.text = career.GmName
            + " | " + career.CareerStatus
            + "\nCurrent team: " + SafeText(career.CurrentTeamName, "no team")
            + "\nJob Security: " + GmJobSecurityConfig.GetJobSecurityLabel(career.CurrentJobSecurity)
            + " " + career.CurrentJobSecurity
            + " | Owner Trust: " + career.CurrentOwnerTrust
            + "\nSeasons: " + career.SeasonsCompleted
            + " | Teams: " + career.TeamsManaged
            + " | Record: " + MobileUiConfig.FormatRecord(career.CareerWins, career.CareerLosses, career.CareerOvertimeLosses)
            + "\nPlayoffs: " + career.CareerPlayoffAppearances
            + " | Rounds won: " + career.CareerPlayoffRoundsWon
            + " | Cups: " + career.CareerChampionships
            + warning;
    }

    private void RenderSelectedOffer(GameState state, string selectedOfferId)
    {
        GmJobOfferData offer = FindOffer(state, selectedOfferId);
        if (offer == null)
        {
            _selectedOfferText.text = "Выберите предложение работы";
            return;
        }

        _selectedOfferText.text = offer.TeamName
            + "\nDirection: " + offer.TeamDirection
            + " | OVR " + offer.TeamOverall
            + " | Last points " + offer.LastSeasonPoints
            + "\nReason: " + offer.OfferReason
            + "\nChallenge: " + offer.ChallengeSummary
            + "\nExpectations: " + offer.ExpectationsSummary
            + "\nStarting trust/security: " + offer.OwnerTrustStartingValue + " / " + offer.JobSecurityStartingValue;
    }

    private void RenderOffers(GameState state)
    {
        ClearRows(_offersContainer, _offerRowPrefab.transform);
        _offerRowPrefab.gameObject.SetActive(false);

        List<GmJobOfferData> offers = state == null || state.ActiveGmJobOffers == null
            ? new List<GmJobOfferData>()
            : state.ActiveGmJobOffers;
        if (offers.Count == 0)
        {
            CreateInfoRow(_offersContainer, state != null && state.GmCareer != null && state.GmCareer.IsUnemployed
                ? "Нет активных предложений. Нажмите Generate Offers."
                : "Активных предложений работы нет");
            return;
        }

        foreach (GmJobOfferData offer in offers)
        {
            if (offer == null)
            {
                continue;
            }

            GmJobOfferRowView row = Instantiate(_offerRowPrefab, _offersContainer);
            row.name = "gm-offer-" + offer.TeamId;
            row.gameObject.SetActive(true);
            row.Initialize(offer, _screenController);
        }
    }

    private void RenderEvents(GameState state)
    {
        ClearRows(_eventsContainer, _eventRowPrefab.transform);
        _eventRowPrefab.gameObject.SetActive(false);

        List<GmCareerEventData> events = GameSession.GetGmCareerEvents(12);
        if (events.Count == 0)
        {
            CreateInfoRow(_eventsContainer, "История карьеры пока пуста");
            return;
        }

        foreach (GmCareerEventData careerEvent in events)
        {
            if (careerEvent == null)
            {
                continue;
            }

            GmCareerEventRowView row = Instantiate(_eventRowPrefab, _eventsContainer);
            row.name = "gm-career-event-" + careerEvent.EventId;
            row.gameObject.SetActive(true);
            row.Initialize(careerEvent);
        }
    }

    private bool HasRequiredReferences()
    {
        return _summaryText != null
            && _selectedOfferText != null
            && _offersContainer != null
            && _eventsContainer != null
            && _offerRowPrefab != null
            && _eventRowPrefab != null;
    }

    private static GmJobOfferData FindOffer(GameState state, string offerId)
    {
        if (state == null || state.ActiveGmJobOffers == null || string.IsNullOrEmpty(offerId))
        {
            return null;
        }

        foreach (GmJobOfferData offer in state.ActiveGmJobOffers)
        {
            if (offer != null && offer.OfferId == offerId)
            {
                return offer;
            }
        }

        return null;
    }

    private static void ClearRows(Transform container, Transform prefabTransform)
    {
        if (container == null || prefabTransform == null)
        {
            return;
        }

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
        rectTransform.sizeDelta = new Vector2(820f, 54f);
        LayoutElement layoutElement = rowObject.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = 54f;
        layoutElement.minHeight = 54f;
        Text text = rowObject.AddComponent<Text>();
        text.text = value;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 15;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
    }

    private static string SafeText(string value, string fallback)
    {
        return string.IsNullOrEmpty(value) ? fallback : value;
    }
}
