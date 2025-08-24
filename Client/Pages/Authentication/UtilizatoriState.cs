using Shared.Models.Authentication;

namespace Client.Pages.Authentication;

public class UtilizatoriState
{
    #region Properties

    public List<Utilizator> Users { get; set; } = new();
    public List<Utilizator> FilteredUsers { get; private set; } = new();
    public string SearchTerm { get; set; } = string.Empty;
    public bool IsLoading { get; set; } = false;

    #endregion

    #region Methods

    public void FilterUsers()
    {
        var filtered = Users.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var search = SearchTerm.ToLowerInvariant();
            filtered = filtered.Where(u =>
                u.NumeUtilizator.ToLowerInvariant().Contains(search) ||
                u.Email.ToLowerInvariant().Contains(search) ||
                u.NumeComplet.ToLowerInvariant().Contains(search) ||
                (u.Telefon?.ToLowerInvariant().Contains(search) ?? false));
        }

        FilteredUsers = filtered.OrderBy(u => u.NumeUtilizator).ToList();
    }

    public void Reset()
    {
        Users.Clear();
        FilteredUsers.Clear();
        SearchTerm = string.Empty;
        IsLoading = false;
    }

    #endregion
}