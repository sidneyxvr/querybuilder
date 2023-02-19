using QueryBuilder.Clauses;
using QueryBuilder.Exceptions;

namespace SqlKata.Compilers;

public class CteFinder
{
    private readonly Query _query;
    private readonly string? _engineCode;
    private HashSet<string>? _namesOfPreviousCtes;
    private List<AbstractFrom>? _orderedCteList;

    public CteFinder(Query query, string? engineCode)
    {
        _query = query;
        _engineCode = engineCode;
    }

    public List<AbstractFrom> Find()
    {
        if (_orderedCteList is not null)
            return _orderedCteList;

        _namesOfPreviousCtes = new();

        _orderedCteList = FindInternal(_query);

        _namesOfPreviousCtes.Clear();
        _namesOfPreviousCtes = null;

        return _orderedCteList;
    }

    private List<AbstractFrom> FindInternal(Query queryToSearch)
    {
        var cteList = queryToSearch.GetComponents<AbstractFrom>(Component.Cte, _engineCode);

        var resultList = new List<AbstractFrom>();

        foreach (var cte in cteList)
        {
            CustomNullReferenceException.ThrowIfNull(cte.Alias);

            if (_namesOfPreviousCtes!.Contains(cte.Alias))
                continue;

            _namesOfPreviousCtes.Add(cte.Alias);
            resultList.Add(cte);

            if (cte is QueryFromClause queryFromClause)
            {
                resultList.InsertRange(0, FindInternal(queryFromClause.Query));
            }
        }

        return resultList;
    }
}
