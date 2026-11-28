using UnityEngine;

public class RolePermissionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private UserSessionManager sessionManager;

    public bool HasPermission(UserPermission permission)
    {
        if (sessionManager == null ||
            !sessionManager.HasActiveSession)
        {
            return false;
        }

        return HasPermission(
            sessionManager.CurrentRole,
            permission
        );
    }

    public bool HasPermission(
        UserRole role,
        UserPermission permission)
    {
        switch (role)
        {
            case UserRole.Manager:
                return HasManagerPermission(permission);

            case UserRole.Boss:
                return HasBossPermission(permission);

            case UserRole.HR:
                return HasHRPermission(permission);

            default:
                return false;
        }
    }

    private bool HasManagerPermission(
        UserPermission permission)
    {
        switch (permission)
        {
            case UserPermission.AnswerSelfAssessment:
            case UserPermission.ViewComparison:
            case UserPermission.AccessDevelopment:
            case UserPermission.ViewPersonalPlan:
                return true;

            default:
                return false;
        }
    }

    private bool HasBossPermission(
        UserPermission permission)
    {
        return permission == UserPermission.AnswerBossAssessment;
    }

    private bool HasHRPermission(
        UserPermission permission)
    {
        switch (permission)
        {
            case UserPermission.ViewComparison:
            case UserPermission.ViewPersonalPlan:
            case UserPermission.ManageAssessments:
            case UserPermission.ViewAssessmentStatus:
                return true;

            default:
                return false;
        }
    }
}
