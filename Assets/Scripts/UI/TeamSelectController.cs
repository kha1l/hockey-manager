using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TeamSelectController : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    private const string SelectedTeamIdKey = "SelectedTeamId";
    private const string StartNewGamePendingKey = "StartNewGamePending";
    private const float SwipeThreshold = 90f;

    [SerializeField] private Transform _teamsContainer;
    [SerializeField] private TeamButtonView _teamButtonPrefab;
    [SerializeField] private GameObject _selectionRoot;
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private Slider _loadingProgressSlider;
    [SerializeField] private Text _loadingTitleText;
    [SerializeField] private Text _loadingStatusText;
    [SerializeField] private Text _loadingPercentText;
    [SerializeField] private Image _backgroundTintImage;
    [SerializeField] private Image _accentTintImage;
    [SerializeField] private Image _teamLogoImage;
    [SerializeField] private Image _teamPlayerImage;
    [SerializeField] private Text _teamNameText;
    [SerializeField] private Text _teamIdentityText;
    [SerializeField] private Text _teamRatingText;
    [SerializeField] private Text _conferenceBlockText;
    [SerializeField] private Text _ratingBlockText;
    [SerializeField] private Text _teamCounterText;
    [SerializeField] private Text _prewarmStatusText;
    [SerializeField] private Button _previousButton;
    [SerializeField] private Button _nextButton;
    [SerializeField] private Button _selectButton;

    private bool _isStartingGame;
    private float _loadingProgress;
    private List<TeamData> _teams = new List<TeamData>();
    private int _selectedTeamIndex;
    private Vector2 _dragStartPosition;

    private void Start()
    {
        LoadTeams();
        ConfigureCarouselButtons();
        ShowLoading(true);
        StartCoroutine(PrepareTeamSelectScreen());
    }

    public void Configure(Transform teamsContainer, TeamButtonView teamButtonPrefab)
    {
        _teamsContainer = teamsContainer;
        _teamButtonPrefab = teamButtonPrefab;
    }

    public void ConfigureCarousel(
        GameObject selectionRoot,
        GameObject loadingPanel,
        Slider loadingProgressSlider,
        Text loadingTitleText,
        Text loadingStatusText,
        Text loadingPercentText,
        Image backgroundTintImage,
        Image accentTintImage,
        Image teamLogoImage,
        Image teamPlayerImage,
        Text teamNameText,
        Text teamIdentityText,
        Text teamRatingText,
        Text conferenceBlockText,
        Text ratingBlockText,
        Text teamCounterText,
        Text prewarmStatusText,
        Button previousButton,
        Button nextButton,
        Button selectButton)
    {
        _selectionRoot = selectionRoot;
        _loadingPanel = loadingPanel;
        _loadingProgressSlider = loadingProgressSlider;
        _loadingTitleText = loadingTitleText;
        _loadingStatusText = loadingStatusText;
        _loadingPercentText = loadingPercentText;
        _backgroundTintImage = backgroundTintImage;
        _accentTintImage = accentTintImage;
        _teamLogoImage = teamLogoImage;
        _teamPlayerImage = teamPlayerImage;
        _teamNameText = teamNameText;
        _teamIdentityText = teamIdentityText;
        _teamRatingText = teamRatingText;
        _conferenceBlockText = conferenceBlockText;
        _ratingBlockText = ratingBlockText;
        _teamCounterText = teamCounterText;
        _prewarmStatusText = prewarmStatusText;
        _previousButton = previousButton;
        _nextButton = nextButton;
        _selectButton = selectButton;
    }

    public void SelectTeam(string teamId)
    {
        if (_isStartingGame)
        {
            return;
        }

        if (string.IsNullOrEmpty(teamId))
        {
            Debug.LogWarning("Команда для новой игры не выбрана");
            return;
        }

        _isStartingGame = true;
        StartCoroutine(SelectTeamRoutine(teamId));
    }

    private IEnumerator SelectTeamRoutine(string teamId)
    {
        ShowLoading(true);
        UpdateLoading("Идёт создание игры", "Финальная подготовка...", 0.96f);
        yield return null;

        if (!GameSession.IsNewGameTemplateReady)
        {
            yield return GameSession.PrepareNewGameTemplateAsync(UpdateLoadingProgress);
        }

        PlayerPrefs.SetString(SelectedTeamIdKey, teamId);
        PlayerPrefs.SetInt(StartNewGamePendingKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Выбрана команда: " + teamId);
        SceneManager.LoadScene("Game");
    }

    public void SelectCurrentTeam()
    {
        TeamData team = GetSelectedTeam();
        SelectTeam(team == null ? "" : team.Id);
    }

    public void ShowPreviousTeam()
    {
        if (_teams.Count == 0)
        {
            return;
        }

        _selectedTeamIndex = (_selectedTeamIndex - 1 + _teams.Count) % _teams.Count;
        UpdateSelectedTeamView();
    }

    public void ShowNextTeam()
    {
        if (_teams.Count == 0)
        {
            return;
        }

        _selectedTeamIndex = (_selectedTeamIndex + 1) % _teams.Count;
        UpdateSelectedTeamView();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartPosition = eventData == null ? Vector2.zero : eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (eventData == null)
        {
            return;
        }

        float deltaX = eventData.position.x - _dragStartPosition.x;
        if (Mathf.Abs(deltaX) < SwipeThreshold)
        {
            return;
        }

        if (deltaX < 0f)
        {
            ShowNextTeam();
            return;
        }

        ShowPreviousTeam();
    }

    public void BackToMainMenu()
    {
        PlayerPrefs.DeleteKey(StartNewGamePendingKey);
        PlayerPrefs.Save();

        Debug.Log("Возврат в главное меню");
        SceneManager.LoadScene("MainMenu");
    }

    private void LoadTeams()
    {
        _teams = LeagueSeedService.CreateTeamSummaries();
        if (_teams == null)
        {
            _teams = new List<TeamData>();
        }

        _teams.Sort(CompareTeamsByDisplayName);
    }

    private IEnumerator PrepareTeamSelectScreen()
    {
        UpdateLoading("Идёт создание игры", "Загрузка команд...", 0.02f);
        yield return null;

        yield return PreloadTeamAssetsAsync();

        UpdateLoading("Идёт создание игры", "Подготовка лиги...", 0.24f);
        yield return GameSession.PrepareNewGameTemplateAsync(UpdateLoadingProgress);

        if (IsCarouselConfigured())
        {
            UpdateSelectedTeamView();
        }
        else
        {
            CreateTeamButtons();
        }

        UpdateLoading("Игра готова", "Переход к выбору команды", 1f);
        yield return null;
        ShowLoading(false);
        SetText(_prewarmStatusText, "Данные новой игры готовы");
    }

    private IEnumerator PreloadTeamAssetsAsync()
    {
        int total = Mathf.Max(1, _teams.Count);
        for (int i = 0; i < _teams.Count; i++)
        {
            TeamData team = _teams[i];
            TeamIdentityService.EnsureTeamIdentity(team);
            UpdateLoading(
                "Идёт создание игры",
                "Загрузка формы: " + TeamIdentityService.GetDisplayName(team),
                Mathf.Lerp(0.05f, 0.22f, (i + 1) / (float)total));
            TeamAssetService.LoadLogo(team);
            TeamAssetService.LoadFullBody(team);
            yield return null;
        }
    }

    private void ConfigureCarouselButtons()
    {
        if (_previousButton != null)
        {
            _previousButton.onClick.RemoveAllListeners();
            _previousButton.onClick.AddListener(ShowPreviousTeam);
        }

        if (_nextButton != null)
        {
            _nextButton.onClick.RemoveAllListeners();
            _nextButton.onClick.AddListener(ShowNextTeam);
        }

        if (_selectButton != null)
        {
            _selectButton.onClick.RemoveAllListeners();
            _selectButton.onClick.AddListener(SelectCurrentTeam);
        }
    }

    private bool IsCarouselConfigured()
    {
        return _teamNameText != null
            && _teamIdentityText != null
            && _teamRatingText != null
            && _teamCounterText != null;
    }

    private void UpdateSelectedTeamView()
    {
        TeamData team = GetSelectedTeam();
        if (team == null)
        {
            SetText(_teamNameText, "Команда не найдена");
            SetText(_teamIdentityText, "");
            SetText(_teamRatingText, "");
            SetText(_conferenceBlockText, "");
            SetText(_ratingBlockText, "");
            SetText(_teamCounterText, "0 / 0");
            return;
        }

        TeamIdentityService.EnsureTeamIdentity(team);
        Color primaryColor = TeamIdentityService.GetPrimaryColor(team);
        Color secondaryColor = TeamIdentityService.GetSecondaryColor(team);
        int rating = LeagueSeedGenerator.GetTeamPreviewRating(team.Id);
        SetImageColor(_backgroundTintImage, WithAlpha(primaryColor, 0.20f));
        SetImageColor(_accentTintImage, WithAlpha(secondaryColor, 0.16f));
        SetText(_teamNameText, TeamIdentityService.GetDisplayName(team));
        SetText(_teamIdentityText, _conferenceBlockText == null ? team.ConferenceName + "\n" + team.DivisionName : "");
        SetText(_teamRatingText, _ratingBlockText == null ? "Рейтинг: " + rating : "");
        SetText(_conferenceBlockText, team.ConferenceName + "\n" + team.DivisionName);
        SetText(_ratingBlockText, rating + "\n" + GetRatingLabel(rating));
        SetText(_teamCounterText, (_selectedTeamIndex + 1) + " / " + _teams.Count);

        if (_teamLogoImage != null)
        {
            _teamLogoImage.sprite = TeamAssetService.LoadLogo(team);
            _teamLogoImage.color = _teamLogoImage.sprite == null ? Color.clear : Color.white;
            _teamLogoImage.preserveAspect = true;
        }

        if (_teamPlayerImage != null)
        {
            _teamPlayerImage.sprite = TeamAssetService.LoadFullBody(team);
            _teamPlayerImage.color = _teamPlayerImage.sprite == null ? Color.clear : Color.white;
            _teamPlayerImage.preserveAspect = true;
        }
    }

    private TeamData GetSelectedTeam()
    {
        if (_teams == null || _teams.Count == 0)
        {
            return null;
        }

        _selectedTeamIndex = Mathf.Clamp(_selectedTeamIndex, 0, _teams.Count - 1);
        return _teams[_selectedTeamIndex];
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void UpdateLoadingProgress(string status, float progress)
    {
        UpdateLoading("Идёт создание игры", status, progress);
    }

    private void UpdateLoading(string title, string status, float progress)
    {
        progress = Mathf.Clamp01(progress);
        if (progress < _loadingProgress)
        {
            progress = _loadingProgress;
        }

        _loadingProgress = progress;
        SetText(_loadingTitleText, title);
        SetText(_loadingStatusText, status);
        SetText(_loadingPercentText, Mathf.RoundToInt(progress * 100f) + "%");
        SetText(_prewarmStatusText, status);
        if (_loadingProgressSlider != null)
        {
            _loadingProgressSlider.value = progress;
        }
    }

    private void ShowLoading(bool isLoading)
    {
        if (isLoading)
        {
            _loadingProgress = 0f;
        }

        if (_loadingPanel != null)
        {
            _loadingPanel.SetActive(isLoading);
        }

        if (_selectionRoot != null)
        {
            _selectionRoot.SetActive(!isLoading);
        }
    }

    private static void SetImageColor(Image image, Color color)
    {
        if (image != null)
        {
            image.color = color;
        }
    }

    private static Color WithAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }

    private static string GetRatingLabel(int rating)
    {
        if (rating >= 84)
        {
            return "Элитный состав";
        }

        if (rating >= 78)
        {
            return "Сильный состав";
        }

        if (rating >= 72)
        {
            return "Средний состав";
        }

        return "Команда развития";
    }

    private static int CompareTeamsByDisplayName(TeamData left, TeamData right)
    {
        string leftName = TeamIdentityService.GetDisplayName(left);
        string rightName = TeamIdentityService.GetDisplayName(right);
        return string.Compare(leftName, rightName, StringComparison.Ordinal);
    }

    private void CreateTeamButtons()
    {
        if (_teamsContainer == null || _teamButtonPrefab == null)
        {
            Debug.LogError("TeamSelectController: UI references are not configured.");
            return;
        }

        ClearTeamButtons();
        _teamButtonPrefab.gameObject.SetActive(false);

        foreach (TeamData team in _teams)
        {
            TeamButtonView teamButton = Instantiate(_teamButtonPrefab, _teamsContainer);
            teamButton.name = team.Id + "-button";
            teamButton.gameObject.SetActive(true);
            teamButton.Initialize(team, this);
        }
    }

    private void ClearTeamButtons()
    {
        for (int i = _teamsContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = _teamsContainer.GetChild(i);
            if (child == _teamButtonPrefab.transform)
            {
                continue;
            }

            Destroy(child.gameObject);
        }
    }
}
