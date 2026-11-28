using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoleSelectionUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private UserSessionManager sessionManager;

    [Header("Panels")]
    [SerializeField]
    private GameObject roleSelectionPanel;

    [SerializeField]
    private GameObject mainMenuPanel;

    [Header("Buttons")]
    [SerializeField]
    private Button managerButton;

    [SerializeField]
    private Button bossButton;

    [SerializeField]
    private Button hrButton;

    [Header("Feedback")]
    [SerializeField]
    private TMP_Text feedbackText;

    public event Action<UserRole> OnRoleSelected;

    private void Awake()
    {
        if (managerButton != null)
            managerButton.onClick.AddListener(SelectManager);

        if (bossButton != null)
            bossButton.onClick.AddListener(SelectBoss);

        if (hrButton != null)
            hrButton.onClick.AddListener(SelectHR);
    }

    private void OnDestroy()
    {
        if (managerButton != null)
            managerButton.onClick.RemoveListener(SelectManager);

        if (bossButton != null)
            bossButton.onClick.RemoveListener(SelectBoss);

        if (hrButton != null)
            hrButton.onClick.RemoveListener(SelectHR);
    }

    public void OpenRoleSelection()
    {
        if (roleSelectionPanel == null)
            return;

        if (feedbackText != null)
            feedbackText.text = "";

        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(false);

        roleSelectionPanel.SetActive(true);
    }

    private void SelectManager()
    {
        SelectRole(UserRole.Manager);
    }

    private void SelectBoss()
    {
        SelectRole(UserRole.Boss);
    }

    private void SelectHR()
    {
        SelectRole(UserRole.HR);
    }

    private void SelectRole(UserRole role)
    {
        if (sessionManager == null)
        {
            if (feedbackText != null)
                feedbackText.text =
                    "No se encontró el administrador de sesión.";

            return;
        }

        bool started = sessionManager.StartSession(role);

        if (!started)
        {
            if (feedbackText != null)
                feedbackText.text =
                    "No se pudo iniciar la sesión.";

            return;
        }

        if (feedbackText != null)
            feedbackText.text = "";

        roleSelectionPanel.SetActive(false);

        // El menú principal será provisionalmente
        // el punto de entrada después de elegir un rol.
        if (mainMenuPanel != null)
            mainMenuPanel.SetActive(true);

        OnRoleSelected?.Invoke(role);
    }
}