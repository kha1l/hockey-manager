using UnityEngine;
using UnityEngine.UI;

public class StaffMemberRowView : MonoBehaviour
{
    [SerializeField] private Text _infoText;

    public void Configure(Text infoText)
    {
        _infoText = infoText;
    }

    public void Initialize(StaffData staff)
    {
        if (_infoText == null)
        {
            return;
        }

        if (staff == null)
        {
            _infoText.text = "Staff member unavailable";
            return;
        }

        string keyRatings;
        if (staff.StaffRole == StaffConfig.RoleHeadCoach)
        {
            keyRatings = "Mot " + staff.MotivationRating
                + " | Disc " + staff.DisciplineRating
                + " | Fit " + staff.TacticalFitRating
                + " | Lead " + staff.LeadershipRating;
        }
        else if (staff.StaffRole == StaffConfig.RoleAssistantCoach)
        {
            keyRatings = "Off " + staff.OffenseRating
                + " | Def " + staff.DefenseRating
                + " | PP " + staff.PowerPlayRating
                + " | PK " + staff.PenaltyKillRating;
        }
        else if (staff.StaffRole == StaffConfig.RoleDevelopmentCoach)
        {
            keyRatings = "Dev " + staff.DevelopmentRating
                + " | Mot " + staff.MotivationRating
                + " | Lead " + staff.LeadershipRating;
        }
        else
        {
            keyRatings = "Goalie Dev " + staff.GoalieDevelopmentRating
                + " | Def " + staff.DefenseRating
                + " | Fit " + staff.TacticalFitRating;
        }

        _infoText.text = staff.StaffRole
            + " | " + staff.FullName
            + " | Age " + staff.Age
            + " | " + staff.CoachingStyle
            + " | OVR " + staff.Overall
            + " " + StaffConfig.GetStaffQualityLabel(staff.Overall)
            + " | " + keyRatings
            + " | $" + staff.Salary.ToString("N0")
            + " | " + staff.ContractYearsRemaining + "y";
    }
}
