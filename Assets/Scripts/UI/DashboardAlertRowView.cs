using UnityEngine;
using UnityEngine.UI;

public class DashboardAlertRowView : MonoBehaviour
{
    public Text TitleText;
    public Text MessageText;
    public Text CategoryText;
    public Button OpenButton;

    private DashboardAlertData _alert;
    private GameScreenController _screenController;

    public void Initialize(DashboardAlertData alert, GameScreenController screenController)
    {
        _alert = alert;
        _screenController = screenController;

        if (TitleText != null)
        {
            TitleText.text = alert == null ? "Alert" : alert.Title;
        }

        if (MessageText != null)
        {
            MessageText.text = alert == null ? "" : alert.Message;
        }

        if (CategoryText != null)
        {
            CategoryText.text = alert == null ? "" : "[" + alert.Category + "]";
        }

        if (OpenButton != null)
        {
            OpenButton.onClick.RemoveAllListeners();
            OpenButton.onClick.AddListener(OpenTargetPanel);
        }
    }

    private void OpenTargetPanel()
    {
        if (_screenController == null || _alert == null)
        {
            return;
        }

        string target = string.IsNullOrEmpty(_alert.TargetPanel) ? "" : _alert.TargetPanel;
        if (target == "Lineup")
        {
            _screenController.ShowLineup();
        }
        else if (target == "Organization")
        {
            _screenController.ShowOrganization();
        }
        else if (target == "Contracts")
        {
            _screenController.ShowContracts();
        }
        else if (target == "Extensions")
        {
            _screenController.ShowExtensions();
        }
        else if (target == "Morale")
        {
            _screenController.ShowMorale();
        }
        else if (target == "Injuries")
        {
            _screenController.ShowInjuries();
        }
        else if (target == "Owner")
        {
            _screenController.ShowOwner();
        }
        else if (target == "GmCareer")
        {
            _screenController.ShowGmCareer();
        }
        else if (target == "News")
        {
            _screenController.ShowNews();
        }
        else if (target == "FreeAgency")
        {
            _screenController.ShowFreeAgency();
        }
        else if (target == "Draft")
        {
            _screenController.ShowDraft();
        }
        else
        {
            _screenController.ShowDashboard();
        }
    }
}
