namespace TodosApi.Dapper;

public class AllowedNameProvider
{
    public bool IsAllowed(string name) =>
        !string.Equals(name, "Todo", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(name, "Title", StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(name, "Name", StringComparison.OrdinalIgnoreCase);
}
