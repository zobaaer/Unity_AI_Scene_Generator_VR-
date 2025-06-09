using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public GameObject slotPrefab;
    public GameObject pageContainerPrefab;
    public Button nextButton;
    public Button backButton;
    public TMP_Text nextButtonText;
    
    private int currentPage = 0;
    private int totalPages = 1;
    private const int slotsPerPage = 4;

    private void Start()
    {
        // Initialize first page
        CreateNewPage(0);
        
        // Setup button listeners
        nextButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(PreviousPage);
        
        UpdateUI();
    }

    private void CreateNewPage(int pageIndex)
    {
        GameObject newPage = Instantiate(pageContainerPrefab, transform);
        newPage.name = $"Page {pageIndex + 1}";
        
        for (int i = 0; i < slotsPerPage; i++)
        {
            GameObject slot = Instantiate(slotPrefab, newPage.transform);
            slot.name = $"Slot {pageIndex * slotsPerPage + i + 1}";
        }
        
        // Hide all pages except first
        newPage.SetActive(pageIndex == 0);
        totalPages = Mathf.Max(totalPages, pageIndex + 1);
    }

    private void NextPage()
    {
        // If on last page, add new page
        if (currentPage == totalPages - 1)
        {
            CreateNewPage(totalPages);
        }
        
        // Switch to next page
        transform.GetChild(currentPage).gameObject.SetActive(false);
        currentPage++;
        transform.GetChild(currentPage).gameObject.SetActive(true);
        
        UpdateUI();
        Debug.Log($"Total pages = {totalPages}, Current page = {currentPage}");
    }

    private void PreviousPage()
    {
        transform.GetChild(currentPage).gameObject.SetActive(false);
        currentPage--;
        transform.GetChild(currentPage).gameObject.SetActive(true);
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        // Update button visibility
        backButton.gameObject.SetActive(currentPage > 0);
        
        // Update next button text
        nextButtonText.text = (currentPage == totalPages - 1) ? 
            "Add New Page" : "Next Page";
    }
}