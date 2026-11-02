
using TMPro;
using UnityEngine;
using static AssessmentManager_script;
using UnityEngine.UI;

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



    private void Start()
    {
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
}
