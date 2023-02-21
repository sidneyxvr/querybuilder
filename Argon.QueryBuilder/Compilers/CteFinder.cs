using Argon.QueryBuilder.Clauses;

namespace Argon.QueryBuilder.Compilers;

public class CteFinder
{
    private readonly Query _query;
    private HashSet<string>? _namesOfPreviousCtes;
    private List<AbstractFrom>? _orderedCteList;

    public CteFinder(Query query)
    {
        _query = query;
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
        var cteList = queryToSearch.GetComponents<AbstractFrom>(Component.Cte);

        var resultList = new List<AbstractFrom>();

        foreach (var cte in cteList)
        {
            if (_namesOfPreviousCtes!.Contains(cte.Alias!))
                continue;

            _namesOfPreviousCtes.Add(cte.Alias!);
            resultList.Add(cte);

            if (cte is QueryFromClause queryFromClause)
            {
                resultList.InsertRange(0, FindInternal(queryFromClause.Query));
            }
        }

        return resultList;
    }
}
