using global::Shared.DTOs.Medical;
using global::Shared.Common;

namespace Client.Pages.Medical;

public class PatientListState
{
    #region Properties

    public PagedResult<PacientListDto> PagedResult { get; set; } = new();
    public PacientiSearchQuery SearchQuery { get; set; } = new() { Page = 1, PageSize = 25 };
    public List<PacientListDto> AllPatients { get; private set; } = new();
    public bool IsLoading { get; set; } = false;
    
    // Control pentru expansiunea detaliilor
    public HashSet<Guid> ExpandedRows { get; set; } = new();
    
    #endregion

    #region Methods

    public void UpdatePagedResult(PagedResult<PacientListDto> result)
    {
        PagedResult = result;
        AllPatients = result.Items.ToList();
    }

    public void ToggleRowExpansion(Guid pacientId)
    {
        if (ExpandedRows.Contains(pacientId))
        {
            ExpandedRows.Remove(pacientId);
        }
        else
        {
            ExpandedRows.Add(pacientId);
        }
    }

    public bool IsRowExpanded(Guid pacientId)
    {
        return ExpandedRows.Contains(pacientId);
    }

    public void CollapseAllRows()
    {
        ExpandedRows.Clear();
    }

    public void ExpandAllRows()
    {
        ExpandedRows.Clear();
        foreach (var patient in PagedResult.Items)
        {
            ExpandedRows.Add(patient.PacientID);
        }
    }

    public void Reset()
    {
        PagedResult = new PagedResult<PacientListDto>();
        AllPatients.Clear();
        SearchQuery = new PacientiSearchQuery { Page = 1, PageSize = 25 };
        IsLoading = false;
        ExpandedRows.Clear();
    }

    #endregion
}