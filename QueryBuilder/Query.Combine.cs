using QueryBuilder.Clauses;

namespace Zine.QueryBuilder;

public partial class Query
{
    public Query Combine(string operation, bool all, Query query)
    {
        if (Method != "select" || query.Method != "select")
        {
            throw new InvalidOperationException("Only select queries can be combined.");
        }

        return AddComponent(Component.Combine, new Combine
        {
            Query = query,
            Operation = operation,
            All = all,
        });
    }

    public Query CombineRaw(string sql, params object[] bindings)
    {
        if (Method != "select")
        {
            throw new InvalidOperationException("Only select queries can be combined.");
        }

        return AddComponent(Component.Combine, new RawCombine
        {
            Expression = sql,
            Bindings = bindings,
        });
    }

    public Query Union(Query query, bool all = false)
        => Combine("union", all, query);

    public Query UnionAll(Query query)
        => Union(query, true);

    public Query Union(Func<Query, Query> callback, bool all = false)
    {
        var query = callback.Invoke(new Query());
        return Union(query, all);
    }

    public Query UnionAll(Func<Query, Query> callback)
        => Union(callback, true);

    public Query Except(Query query, bool all = false)
        => Combine("except", all, query);

    public Query ExceptAll(Query query)
        => Except(query, true);

    public Query Except(Func<Query, Query> callback, bool all = false)
    {
        var query = callback.Invoke(new Query());
        return Except(query, all);
    }

    public Query ExceptAll(Func<Query, Query> callback)
        => Except(callback, true);

    public Query ExceptRaw(string sql, params object[] bindings)
        => CombineRaw(sql, bindings);

    public Query Intersect(Query query, bool all = false)
        => Combine("intersect", all, query);

    public Query IntersectAll(Query query)
        => Intersect(query, true);

    public Query Intersect(Func<Query, Query> callback, bool all = false)
    {
        var query = callback.Invoke(new Query());
        return Intersect(query, all);
    }

    public Query IntersectAll(Func<Query, Query> callback)
        => Intersect(callback, true);

    public Query IntersectRaw(string sql, params object[] bindings)
        => CombineRaw(sql, bindings);

}
