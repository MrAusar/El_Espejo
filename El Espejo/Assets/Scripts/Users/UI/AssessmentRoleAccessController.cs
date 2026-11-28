using UnityEngine;

public class AssessmentRoleAccessController : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField]
    private UserSessionManager sessionManager;

    [SerializeField]
    private RolePermissionManager permissionManager;

    [Header("Assessment UI")]
    [SerializeField]
    private GameObject mainMenuPanel;

    [SerializeField]
    private GameObject assessmentPanel;

    public void OpenAssessmentByRole()
    {
        if (sessionManager == null ||
            permissionManager == null ||
            assessmentPanel == null)
        {
            Debug.LogWarning(
                "Faltan referencias para abrir la evaluación."
            );
            return;
        }

        if (!sessionManager.HasActiveSession)
        {
            Debug.LogWarning(
                "Debes seleccionar un perfil antes de continuar."
            );
            return;
        }

        UserRole role = sessionManager.CurrentRole;

        if (role == UserRole.Manager)
        {
            if (!permissionManager.HasPermission(
                UserPermission.AnswerSelfAssessment))
                return;

            OpenAssessment("manager");
        }
        else if (role == UserRole.Boss)
        {
            if (!permissionManager.HasPermission(
                UserPermission.AnswerBossAssessment))
                return;

            OpenAssessment("boss");
        }
        else
        {
            Debug.LogWarning(
                "El perfil seleccionado no puede responder evaluaciones."
            );
        }
    }

    private void OpenAssessment(string assessmentRole)
    {
        Debug.Log(
            $"Abriendo evaluación correspondiente a: {assessmentRole}"
        );

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        assessmentPanel.SetActive(true);

        // La selección del conjunto de respuestas
        // se conectará al controlador del cuestionario.
    }
}
