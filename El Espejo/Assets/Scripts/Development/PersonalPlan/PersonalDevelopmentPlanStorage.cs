
using System;
using System.IO;
using System.Text;
using UnityEngine;

public static class PersonalDevelopmentPlanStorage
{
    private static string GetFilePath(string assessmentID)
    {
        // Crear un nombre seguro y estable para el archivo.
        string fileID = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(assessmentID)
        )
        .TrimEnd('=')
        .Replace('+', '-')
        .Replace('/', '_');

        return Path.Combine(
            Application.persistentDataPath,
            $"development_plan_{fileID}.json"
        );
    }

    public static bool Save(
        PersonalDevelopmentPlanData plan)
    {
        if (plan == null ||
            string.IsNullOrWhiteSpace(plan.assessmentID))
        {
            Debug.LogWarning(
                "No se puede guardar un PPD sin Assessment ID."
            );
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(plan, true);

            string path = GetFilePath(plan.assessmentID);

            // Escribir primero en un archivo temporal.
            string temporaryPath = path + ".tmp";

            File.WriteAllText(
                temporaryPath,
                json,
                Encoding.UTF8
            );

            if (File.Exists(path))
            {
                File.Replace(temporaryPath, path, null);
            }
            else
            {
                File.Move(temporaryPath, path);
            }

            Debug.Log("PPD guardado correctamente.");

            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Error al guardar PPD: {exception.Message}"
            );

            return false;
        }
    }

    public static PersonalDevelopmentPlanData Load(
        string assessmentID)
    {
        if (string.IsNullOrWhiteSpace(assessmentID))
            return null;

        try
        {
            string path = GetFilePath(assessmentID);

            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(
                path,
                Encoding.UTF8
            );

            PersonalDevelopmentPlanData plan =
                JsonUtility.FromJson<
                    PersonalDevelopmentPlanData
                >(json);

            if (plan == null ||
                plan.assessmentID != assessmentID)
            {
                Debug.LogWarning(
                    "El archivo no corresponde a la evaluación."
                );

                return null;
            }

            return plan;
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Error al cargar PPD: {exception.Message}"
            );

            return null;
        }
    }
}
