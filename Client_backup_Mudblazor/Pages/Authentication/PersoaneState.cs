using Shared.Models.Authentication;

namespace Client.Pages.Authentication;

public class PersoaneState
{
    #region Properties

    public List<Persoana> Persons { get; set; } = new();
    public List<Persoana> FilteredPersons { get; private set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public bool IsLoading { get; set; } = false;

    #endregion

    #region Methods

    public void FilterPersons()
    {
        var filtered = Persons.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var search = SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(p =>
                p.Nume.ToLowerInvariant().Contains(search) ||
                p.Prenume.ToLowerInvariant().Contains(search) ||
                p.NumeComplet.ToLowerInvariant().Contains(search) ||
                (p.CNP?.ToLowerInvariant().Contains(search) ?? false) ||
                (p.Judet?.ToLowerInvariant().Contains(search) ?? false) ||
                (p.Localitate?.ToLowerInvariant().Contains(search) ?? false) ||
                (p.PozitieOrganizatie?.ToLowerInvariant().Contains(search) ?? false));
        }

        FilteredPersons = filtered.OrderBy(p => p.Nume).ThenBy(p => p.Prenume).ToList();
    }

    public void Reset()
    {
        Persons.Clear();
        FilteredPersons.Clear();
        SearchTerm = string.Empty;
        IsLoading = false;
    }

    #endregion
}