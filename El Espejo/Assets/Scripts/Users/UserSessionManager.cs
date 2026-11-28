using System;
using UnityEngine;

public class UserSessionManager : MonoBehaviour
{
    public static UserSessionManager Instance
    {
        get;
        private set;
    }

    public UserRole CurrentRole
    {
        get;
        private set;
    } = UserRole.None;

    public bool HasActiveSession => CurrentRole != UserRole.None;

    public event Action<UserRole> OnRoleChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool StartSession(UserRole role)
    {
        if (role == UserRole.None)
        {
            Debug.LogWarning( "No se puede iniciar una sesión sin rol.");

            return false;
        }

        CurrentRole = role;

        Debug.Log($"Sesión iniciada. Rol: {CurrentRole}");

        OnRoleChanged?.Invoke(CurrentRole);

        return true;
    }

    public void EndSession()
    {
        CurrentRole = UserRole.None;

        Debug.Log("Sesión finalizada.");

        OnRoleChanged?.Invoke(CurrentRole);
    }

    public bool HasRole(UserRole role)
    {
        return HasActiveSession && CurrentRole == role;
    }
}
