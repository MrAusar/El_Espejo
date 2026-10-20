
using System;
using System.IO;
using UnityEngine;

public static class AssessmentLocalStorage
{
    private static string GetFilePath(string assessmentID, string role)
    {
        string safeID = MakeSafeFileName(assessmentID);
        string safeRole = MakeSafeFileName(role);

        string fileName = $"{safeID}_{safeRole}.json";

        return Path.Combine(
            Application.persistentDataPath,
            fileName
        );
    }

    private static string MakeSafeFileName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "Unknown";

        foreach (char invalidChar in Path.GetInvalidFileNameChars())
        {
            value = value.Replace(invalidChar, '_');
        }

        return value;
    }

    public static bool Save(AssesmentResponses responses)
    {
        if (responses == null)
        {
            Debug.LogError("No hay respuestas para guardar.");
            return false;
        }

        try
        {
            string path = GetFilePath(
                responses.assesmentID,
                responses.participantRole
            );

            string json = JsonUtility.ToJson(responses, true);

            File.WriteAllText(path, json);

            Debug.Log($"Evaluación guardada en: {path}");
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Error al guardar la evaluación: {exception.Message}"
            );

            return false;
        }
    }

    public static AssesmentResponses Load(
        string assessmentID,
        string role)
    {
        try
        {
            string path = GetFilePath(assessmentID, role);

            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);

            AssesmentResponses responses =
                JsonUtility.FromJson<AssesmentResponses>(json);

            Debug.Log($"Evaluación cargada desde: {path}");

            return responses;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Error al cargar la evaluación: {exception.Message}"
            );

            return null;
        }
    }
}