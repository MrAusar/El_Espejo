using UnityEngine;
using UnityEngine.UI;

public class MainMenuRoleUI : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private UserSessionManager sessionManager;

    [SerializeField]
    private RolePermissionManager permissionManager;

    [Header("Menu Buttons")]
    [SerializeField]
    private Button startAssessmentButton;

    [SerializeField]
    private Button openComparisonButton;

    [SerializeField]
    private Button openDevelopmentButton;

    [SerializeField]
    private Button manageAssessmentsButton;

    [SerializeField]
    private Button assessmentStatusButton;

    private void OnEnable()
    {
        if (sessionManager != null)
        {
            sessionManager.OnRoleChanged += RefreshMenu;
        }

        RefreshMenu();
    }

    private void OnDisable()
    {
        if (sessionManager != null)
        {
            sessionManager.OnRoleChanged -= RefreshMenu;
        }
    }

    private void RefreshMenu(UserRole role)
    {
        RefreshMenu();
    }

    public void RefreshMenu()
    {
        if (permissionManager == null)
        {
            Debug.LogWarning(
                "RolePermissionManager no asignado."
            );

            return;
        }

        bool canAnswerAssessment =
            permissionManager.HasPermission(
                UserPermission.AnswerSelfAssessment
            ) ||
            permissionManager.HasPermission(
                UserPermission.AnswerBossAssessment
            );

        SetButtonVisible(
            startAssessmentButton,
            canAnswerAssessment
        );

        SetButtonVisible(
            openComparisonButton,
            permissionManager.HasPermission(
                UserPermission.ViewComparison
            )
        );

        SetButtonVisible(
            openDevelopmentButton,
            permissionManager.HasPermission(
                UserPermission.AccessDevelopment
            )
        );

        SetButtonVisible(
            manageAssessmentsButton,
            permissionManager.HasPermission(
                UserPermission.ManageAssessments
            )
        );

        SetButtonVisible(
            assessmentStatusButton,
            permissionManager.HasPermission(
                UserPermission.ViewAssessmentStatus
            )
        );

        UpdateAssessmentButtonText();
    }

    private void UpdateAssessmentButtonText()
    {
        if (startAssessmentButton == null ||
            sessionManager == null)
            return;

        TMPro.TMP_Text buttonText =
            startAssessmentButton.GetComponentInChildren<
                TMPro.TMP_Text>(true);

        if (buttonText == null)
            return;

        switch (sessionManager.CurrentRole)
        {
            case UserRole.Manager:
                buttonText.text = "Mi autoevaluación";
                break;

            case UserRole.Boss:
                buttonText.text = "Evaluar al gerente";
                break;
        }
    }

    private void SetButtonVisible(
        Button button,
        bool visible)
    {
        if (button != null)
            button.gameObject.SetActive(visible);
    }
}