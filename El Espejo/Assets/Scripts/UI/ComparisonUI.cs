
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static AssessmentManager_script;

public class ComparisonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AssessmentManager_script assessmentManager;
    [SerializeField] private GameObject comparisonPanel;
    [SerializeField] private GameObject completionPanel;
    [SerializeField] private TMP_Text statusText;

    [Header("Competency List")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private GameObject competencyRowPrefab;

    [Header("Global Summary")]
    [SerializeField] private TMP_Text strengthsCountText;
    [SerializeField] private TMP_Text opportunitiesCountText;
    [SerializeField] private TMP_Text blindSpotsCountText;

    [SerializeField] private TMP_Text strengthsDescriptionText;
    [SerializeField] private TMP_Text opportunitiesDescriptionText;
    [SerializeField] private TMP_Text blindSpotsDescriptionText;

    [Header("Filter Buttons")]
    [SerializeField] private Button allButton;
    [SerializeField] private Button strengthsButton;
    [SerializeField] private Button opportunitiesButton;
    [SerializeField] private Button blindSpotsButton;

    [Header("Pagination")]
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private TMP_Text pageText;

    [Header("List Settings")]
    private System.Collections.Generic.List<TopicComparisonResult> currentComparisons;

    [Header("Filter Description")]
    [SerializeField] private TMP_Text filterDescriptionText;

    [Header("Empty State")]
    [SerializeField] private GameObject emptyStatePanel;
    [SerializeField] private TMP_Text emptyStateText;

    private string activeFilter = "All";
    private int currentPage = 0;

    private const int itemsPerPage = 5;

    [Header("Evidence Panel")]
    [SerializeField] private GameObject evidencePanel;
    [SerializeField] private TMP_Text evidenceCompetencyNameText;

    [SerializeField] private TMP_Text evidenceAScoreText;
    [SerializeField] private TMP_Text evidenceAText;

    [SerializeField] private TMP_Text evidenceBScoreText;
    [SerializeField] private TMP_Text evidenceBText;

    [SerializeField] private Button closeEvidenceButton;

    [Header("Competency Chart")]
    [SerializeField] private Transform chartContentContainer;
    [SerializeField] private GameObject competencyChartRowPrefab;

    [Header("Chart Panel")]
    [SerializeField] private GameObject chartPanel;
    [SerializeField] private Button viewChartButton;
    [SerializeField] private Button backToComparisonButton;

    private void Start()
    {
        evidencePanel.SetActive(false);

        closeEvidenceButton.onClick.AddListener(HideEvidence);

        if (comparisonPanel != null)
        {
            comparisonPanel.SetActive(false);
        }

        allButton.onClick.AddListener(FilterAll);
        strengthsButton.onClick.AddListener(FilterStrengths);
        opportunitiesButton.onClick.AddListener(FilterOpportunities);
        blindSpotsButton.onClick.AddListener(FilterBlindSpots);

        previousButton.onClick.AddListener(PreviousPage);
        nextButton.onClick.AddListener(NextPage);

        viewChartButton.onClick.AddListener(ShowChart);
        backToComparisonButton.onClick.AddListener(HideChart);

        chartPanel.SetActive(false);
    }



    public void ShowComparison()
    {
        if (assessmentManager == null || comparisonPanel == null || statusText == null)
        {
            Debug.LogError("ComparisonUI: faltan referencias en el Inspector.");
            return;
        }

        if (!assessmentManager.TryCompareManagerAndBoss(out var comparisons))
        {
            statusText.text = "La comparación estará disponible cuando " + "ambas evaluaciones estén finalizadas.";

            comparisonPanel.SetActive(true);
            completionPanel.SetActive(false);
            return;
        }

        statusText.text = "Comparación disponible. Competencias analizadas: " + comparisons.Count;

        currentComparisons = comparisons;
        GenerateChartRows(currentComparisons);

        UpdateGlobalSummary(comparisons);

        currentPage = 0;
        activeFilter = "All";

        RefreshCompetencyList();

        comparisonPanel.SetActive(true);
        completionPanel.SetActive(false);
    }


    public void HideComparison()
    {
        if (comparisonPanel != null)
        {
            comparisonPanel.SetActive(false);
            completionPanel.SetActive(true);
        }
    }


    private void GenerateComparisonRows(System.Collections.Generic.List<TopicComparisonResult> comparisons)
    {
        if (contentContainer == null || competencyRowPrefab == null)
        {
            Debug.LogError("ComparisonUI: falta asignar Content o Competency Row Prefab.");
            return;
        }

        // Eliminar las filas anteriores.
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(contentContainer.GetChild(i).gameObject);
        }

        // Crear una fila por cada competencia.
        foreach (TopicComparisonResult result in comparisons)
        {
            GameObject newRow = Instantiate(competencyRowPrefab, contentContainer, false);

            CompetencyRowUI rowUI =
                newRow.GetComponent<CompetencyRowUI>();

            if (rowUI == null)
            {
                Debug.LogError( "El prefab no tiene el componente CompetencyRowUI.");
                Destroy(newRow);
                continue;
            }

            rowUI.Setup(result);
            rowUI.OnEvidenceRequested += ShowEvidence;
        }

        Debug.Log("ComparisonUI: filas generadas: " + comparisons.Count);
    }



    private void UpdateGlobalSummary(System.Collections.Generic.List<TopicComparisonResult> comparisons)
    {
        int strengthsCount = 0;
        int opportunitiesCount = 0;
        int blindSpotsCount = 0;

        System.Collections.Generic.List<string> strengths = new System.Collections.Generic.List<string>();

        foreach (TopicComparisonResult result in comparisons)
        {
            if (result.requiredLevelStatus == AssessmentManager_script.RequiredLevelStatus.BothMeetRequiredLevel)
            {
                strengthsCount++;
                strengths.Add("• " + result.topicTitle);
            }
            else
            {
                opportunitiesCount++;
            }

            bool managerRatesHigher = result.perceptionAlignment == AssessmentManager_script.PerceptionAlignment.ManagerRatesHigher;

            bool bossBelowRequired = result.bossAverage < result.requiredLevel;

            if (managerRatesHigher && bossBelowRequired)
            {
                blindSpotsCount++;
            }
        }

        strengthsCountText.text = strengthsCount.ToString();
        opportunitiesCountText.text = opportunitiesCount.ToString();
        blindSpotsCountText.text = blindSpotsCount.ToString();

        //strengthsDescriptionText.text = "Competencias donde ambas evaluaciones alcanzan " + "el nivel requerido." + (strengths.Count > 0 ? string.Join("\n", strengths) : "Todavía no hay competencias identificadas.");

        //opportunitiesDescriptionText.text = "Competencias donde una o ambas perspectivas " + "identifican una brecha respecto al nivel requerido.";

        //blindSpotsDescriptionText.text = "Competencias donde te evalúas por encima de tu jefe " +  "y su evaluación señala que aún no alcanzas el nivel esperado.";
    }


    private System.Collections.Generic.List<TopicComparisonResult>
        GetFilteredComparisons()
    {
        var filtered = new System.Collections.Generic.List<TopicComparisonResult>();

        if (currentComparisons == null)
            return filtered;

        foreach (TopicComparisonResult result in currentComparisons)
        {
            bool include = false;

            switch (activeFilter)
            {
                case "All":
                    include = true;
                    break;

                case "Strengths":
                    include = result.requiredLevelStatus == AssessmentManager_script.RequiredLevelStatus.BothMeetRequiredLevel;
                    break;

                case "Opportunities":
                    include = result.requiredLevelStatus != AssessmentManager_script.RequiredLevelStatus.BothMeetRequiredLevel;
                    break;

                case "BlindSpots":
                    include = result.perceptionAlignment == AssessmentManager_script.PerceptionAlignment.ManagerRatesHigher && result.bossAverage < result.requiredLevel;
                    break;
            }

            if (include)
                filtered.Add(result);
        }

        return filtered;
    }


    private void RefreshCompetencyList()
    {
        UpdateFilterDescription();

        var filtered = GetFilteredComparisons();

        bool hasResults = filtered.Count > 0;

        emptyStatePanel.SetActive(!hasResults);
        contentContainer.gameObject.SetActive(hasResults);

        if (!hasResults)
        {
            emptyStateText.text = GetEmptyStateMessage();

            GenerateComparisonRows(new System.Collections.Generic.List<TopicComparisonResult>());

            pageText.text = "Sin resultados";

            previousButton.interactable = false;
            nextButton.interactable = false;

            return;
        }

        int totalPages = Mathf.Max(  1, Mathf.CeilToInt(filtered.Count / (float)itemsPerPage) );

        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        int startIndex = currentPage * itemsPerPage;

        int count = Mathf.Min( itemsPerPage,  filtered.Count - startIndex  );

        var pageResults = filtered.GetRange(startIndex, count);

        GenerateComparisonRows(pageResults);

        pageText.text = "Página " + (currentPage + 1) + " de " + totalPages;

        previousButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < totalPages - 1;
    }


    private string GetEmptyStateMessage()
    {
        switch (activeFilter)
        {
            case "Strengths":
                return "Todavía no se identifican fortalezas compartidas.";

            case "Opportunities":
                return "¡Excelente! Ambas perspectivas indican que todas " +
                       "las competencias alcanzan el nivel requerido.";

            case "BlindSpots":
                return "No se identificaron posibles puntos ciegos " +
                       "con los criterios de esta evaluación.";

            default:
                return "No hay competencias disponibles para mostrar.";
        }
    }


    private void UpdateFilterDescription()
    {
        switch (activeFilter)
        {
            case "All":
                filterDescriptionText.text = "Explora los resultados de las 13 competencias " + "y compara las dos perspectivas.";
                break;

            case "Strengths":
                filterDescriptionText.text = "Competencias en las que tanto tú como tu jefe " +  "consideran que alcanzas el nivel de desempeño esperado.";
                break;

            case "Opportunities":
                filterDescriptionText.text = "Competencias en las que una o ambas perspectivas " + "identifican una oportunidad para alcanzar el nivel esperado.";
                break;

            case "BlindSpots":
                filterDescriptionText.text =
                    "Competencias en las que tu autoevaluación es superior " +
                    "a la evaluación de tu jefe y existe una brecha respecto " +
                    "al nivel requerido desde su perspectiva. Son señales " +
                    "para reflexionar, no conclusiones definitivas.";
                break;
        }
    }

    private void ShowEvidence(TopicComparisonResult result)
    {
        if (result == null)
            return;

        var selectedTopic = assessmentManager.assessmentData.topics
            .Find(topic => topic.id == result.topicID);

        if (selectedTopic == null)
        {
            Debug.LogError("ComparisonUI: no se encontró la competencia.");
            return;
        }

        evidenceCompetencyNameText.text = result.topicTitle;

        evidenceAScoreText.text =
            "Promedio: " + result.managerAverage.ToString("F1") + " / 5";

        evidenceBScoreText.text =
            "Promedio: " + result.bossAverage.ToString("F1") + " / 5";

        evidenceAText.text = BuildEvidenceText(
            selectedTopic.question,
            assessmentManager.managerResponses.responses
        );

        evidenceBText.text = BuildEvidenceText(
            selectedTopic.question,
            assessmentManager.bossResponses.responses
        );

        comparisonPanel.SetActive(false);
        evidencePanel.SetActive(true);
    }

    private void HideEvidence()
    {
        evidencePanel.SetActive(false);
        comparisonPanel.SetActive(true);
    }

    private string BuildEvidenceText(
    System.Collections.Generic.List<AssesmentQuestion> questions,
    System.Collections.Generic.List<QuestionResponses> responses)
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < questions.Count; i++)
        {
            var question = questions[i];

            var response = responses.Find(
                r => r.questionID == question.id
            );

            builder.AppendLine("Pregunta " + (i + 1));
            builder.AppendLine(question.text);

            if (response != null)
            {
                builder.AppendLine("Puntuación: " + response.score + " / 5");

                builder.AppendLine(
                    "Evidencia: " +
                    (string.IsNullOrWhiteSpace(response.evidence)
                        ? "Sin evidencia registrada."
                        : response.evidence)
                );
            }
            else
            {
                builder.AppendLine("Sin respuesta registrada.");
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }


    private void GenerateChartRows( List<TopicComparisonResult> comparisons)
    {
        if (chartContentContainer == null || competencyChartRowPrefab == null)
        {
            Debug.LogError( "ComparisonUI: faltan referencias de la gráfica.");
            return;
        }

        // Limpiar filas anteriores.
        foreach (Transform child in chartContentContainer)
        {
            Destroy(child.gameObject);
        }

        // Crear una fila por competencia.
        foreach (TopicComparisonResult result in comparisons)
        {
            GameObject rowObject = Instantiate(  competencyChartRowPrefab, chartContentContainer);

            CompetencyChartRowUI rowUI = rowObject.GetComponent<CompetencyChartRowUI>();

            if (rowUI == null)
            {
                Debug.LogError( "ComparisonUI: el prefab no tiene CompetencyChartRowUI." );
                continue;
            }

            rowUI.Setup(result);
        }

        Debug.Log("ComparisonUI: filas gráficas generadas: " + comparisons.Count );
    }


    //BOTONES

    private void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshCompetencyList();
        }
    }
    private void NextPage()
    {
        currentPage++;

        RefreshCompetencyList();
    }

    private void FilterAll()
    {
        SetFilter("All");
    }

    private void FilterStrengths()
    {
        SetFilter("Strengths");
    }

    private void FilterOpportunities()
    {
        SetFilter("Opportunities");
    }

    private void FilterBlindSpots()
    {
        SetFilter("BlindSpots");
    }

    private void SetFilter(string filter)
    {
        activeFilter = filter;
        currentPage = 0;

        RefreshCompetencyList();
    }

    private void ShowChart()
    {
        comparisonPanel.SetActive(false);
        chartPanel.SetActive(true);
    }

    private void HideChart()
    {
        chartPanel.SetActive(false);
        comparisonPanel.SetActive(true);
    }
}
