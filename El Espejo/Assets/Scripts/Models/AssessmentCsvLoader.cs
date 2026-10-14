using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class AssessmentCsvLoader : MonoBehaviour
{
    [SerializeField] private TextAsset csvFile;

    public AssesmentData LoadAssessment()
    {
        if (csvFile == null)
        {
            Debug.LogError("No se ha asignado Assessment.csv.");
            return null;
        }

        List<List<string>> rows = ParseCsv(csvFile.text);

        if (rows.Count < 2)
        {
            Debug.LogError("El CSV no contiene preguntas.");
            return null;
        }

        Dictionary<string, int> columns = GetColumns(rows[0]);

        string[] requiredHeaders =
        {
            "Skill ID",
            "Essential Management Skill",
            "Question #",
            "Rateable Assessment Question",
            "Required Level"
        };

        foreach (string header in requiredHeaders)
        {
            if (!columns.ContainsKey(Normalize(header)))
            {
                Debug.LogError("Falta la columna: " + header);
                return null;
            }
        }

        AssesmentData data = new AssesmentData();

        Dictionary<string, AssesmentTopic> topics =
            new Dictionary<string, AssesmentTopic>();

        HashSet<string> questionIDs = new HashSet<string>();

        int skillColumn = columns[Normalize("Skill ID")];
        int titleColumn = columns[Normalize("Essential Management Skill")];
        int numberColumn = columns[Normalize("Question #")];
        int textColumn = columns[Normalize("Rateable Assessment Question")];
        int levelColumn = columns[Normalize("Required Level")];

        for (int i = 1; i < rows.Count; i++)
        {
            List<string> row = rows[i];

            if (row.Count == 1 && string.IsNullOrWhiteSpace(row[0]))
                continue;

            try
            {
                string skillID = GetCell(row, skillColumn).Trim();
                string title = GetCell(row, titleColumn).Trim();
                string numberText = GetCell(row, numberColumn).Trim();
                string questionText = GetCell(row, textColumn).Trim();
                string levelText = GetCell(row, levelColumn).Trim();

                if (string.IsNullOrWhiteSpace(skillID) ||
                    string.IsNullOrWhiteSpace(title) ||
                    string.IsNullOrWhiteSpace(questionText))
                {
                    throw new Exception("Falta el ID, el tema o el texto.");
                }

                if (!int.TryParse(numberText, out int questionNumber) ||
                    questionNumber < 1 || questionNumber > 5)
                {
                    throw new Exception(
                        "El número de pregunta debe estar entre 1 y 5.");
                }

                string normalizedLevel = levelText.Trim();

                // Aceptar decimales con punto o coma.
                // Ejemplos: 2.5 y 2,5.
                normalizedLevel = normalizedLevel.Replace(',', '.');

                if (!float.TryParse(
                    normalizedLevel,
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out float requiredLevel))
                {
                    throw new Exception(
                        "El nivel requerido no es válido: '" +
                        levelText + "'.");
                }

                if (!topics.TryGetValue(skillID, out AssesmentTopic topic))
                {
                    topic = new AssesmentTopic
                    {
                        id = skillID,
                        title = title,
                        requiredLevel = requiredLevel,
                        question = new List<AssesmentQuestion>()
                    };

                    topics.Add(skillID, topic);
                    data.topics.Add(topic);
                }
                else
                {
                    if (topic.title != title ||
                        !Mathf.Approximately(
                            topic.requiredLevel, requiredLevel))
                    {
                        throw new Exception(
                            "El tema tiene nombres o niveles inconsistentes.");
                    }
                }

                string questionID =
                    skillID + "_Q" + questionNumber.ToString("00");

                if (!questionIDs.Add(questionID))
                {
                    throw new Exception(
                        "Pregunta duplicada: " + questionID);
                }

                topic.question.Add(new AssesmentQuestion
                {
                    id = questionID,
                    text = questionText
                });
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "Error en la fila CSV " + (i + 1) + ": " +
                    exception.Message);

                return null;
            }
        }

        if (data.topics.Count != 13)
        {
            Debug.LogWarning(
                "Se esperaban 13 temas, pero se encontraron " +
                data.topics.Count + ".");
        }

        foreach (AssesmentTopic topic in data.topics)
        {
            if (topic.question.Count != 5)
            {
                Debug.LogError(
                    "El tema " + topic.id + " tiene " +
                    topic.question.Count + " preguntas en vez de 5.");

                return null;
            }
        }

        if (questionIDs.Count != 65)
        {
            Debug.LogError(
                "Se esperaban 65 preguntas, pero se encontraron " +
                questionIDs.Count + ".");

            return null;
        }

        Debug.Log("Assessment cargado: 13 temas y 65 preguntas.");

        return data;
    }

    private Dictionary<string, int> GetColumns(List<string> headers)
    {
        Dictionary<string, int> result =
            new Dictionary<string, int>();

        for (int i = 0; i < headers.Count; i++)
        {
            string header = Normalize(headers[i]);

            if (header.Contains("rateable assessment question"))
                header = Normalize("Rateable Assessment Question");

            if (!result.ContainsKey(header))
                result.Add(header, i);
        }

        return result;
    }

    private string Normalize(string value)
    {
        return value.Trim()
            .TrimStart('\uFEFF')
            .ToLowerInvariant();
    }

    private string GetCell(List<string> row, int index)
    {
        if (index >= row.Count)
            return "";

        return row[index];
    }


    private List<List<string>> ParseCsv(string text)
    {
        List<List<string>> rows = new List<List<string>>();
        List<string> row = new List<string>();
        System.Text.StringBuilder cell =
            new System.Text.StringBuilder();

        // Detectar el separador usando la primera línea.
        string firstLine = text.Split('\n')[0];

        int commas = firstLine.Split(',').Length;
        int semicolons = firstLine.Split(';').Length;

        char separator = semicolons > commas ? ';' : ',';

        bool insideQuotes = false;

        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c == '"')
            {
                if (insideQuotes &&
                    i + 1 < text.Length && text[i + 1] == '"')
                {
                    cell.Append('"');
                    i++;
                }
                else
                {
                    insideQuotes = !insideQuotes;
                }
            }
            else if (c == separator && !insideQuotes)
            {
                row.Add(cell.ToString());
                cell.Clear();
            }
            else if ((c == '\n' || c == '\r') && !insideQuotes)
            {
                if (c == '\r' &&
                    i + 1 < text.Length && text[i + 1] == '\n')
                {
                    i++;
                }

                row.Add(cell.ToString());
                cell.Clear();

                rows.Add(row);
                row = new List<string>();
            }
            else
            {
                cell.Append(c);
            }
        }

        if (cell.Length > 0 || row.Count > 0)
        {
            row.Add(cell.ToString());
            rows.Add(row);
        }
        return rows;
    }
}