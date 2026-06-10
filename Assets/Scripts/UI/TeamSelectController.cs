using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TeamSelectController : MonoBehaviour
{
    [SerializeField] private Transform _teamsContainer;
    [SerializeField] private TeamButtonView _teamButtonPrefab;

    private void Start()
    {
        CreateTeamButtons();
    }

    public void Configure(Transform teamsContainer, TeamButtonView teamButtonPrefab)
    {
        _teamsContainer = teamsContainer;
        _teamButtonPrefab = teamButtonPrefab;
    }

    public void SelectTeam(string teamId)
    {
        PlayerPrefs.SetString("SelectedTeamId", teamId);
        PlayerPrefs.Save();

        GameSession.StartNewGame(teamId);
        SaveLoadService.Save(GameSession.CurrentState);

        Debug.Log("Выбрана команда: " + teamId);
        SceneManager.LoadScene("Game");
    }

    public void BackToMainMenu()
    {
        Debug.Log("Возврат в главное меню");
        SceneManager.LoadScene("MainMenu");
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

        List<TeamData> teams = TeamSeedData.CreateTeams();
        foreach (TeamData team in teams)
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
