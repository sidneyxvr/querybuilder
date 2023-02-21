using Argon.QueryBuilder.Clauses;
using System.Linq;

namespace Argon.QueryBuilder;

public abstract class AbstractQuery
{
    public AbstractQuery? Parent;
}

public abstract partial class BaseQuery<Q> : AbstractQuery where Q : BaseQuery<Q>
{
    public List<AbstractClause> Clauses { get; set; } = new();

    private bool orFlag = false;
    private bool notFlag = false;
    public string? EngineScope = null;

    public Q SetEngineScope(string engine)
    {
        EngineScope = engine;

        return (Q)this;
    }

    public BaseQuery()
    {
    }

    /// <summary>
    /// Return a cloned copy of the current query.
    /// </summary>
    /// <returns></returns>
    public virtual Q Clone()
    {
        var q = NewQuery();

        q.Clauses = Clauses.Select(x => x.Clone()).ToList();

        return q;
    }

    public Q SetParent(AbstractQuery parent)
    {
        if (this == parent)
        {
            throw new ArgumentException($"Cannot set the same {nameof(AbstractQuery)} as a parent of itself");
        }

        Parent = parent;
        return (Q)this;
    }

    public abstract Q NewQuery();

    public Q NewChild()
    {
        var newQuery = NewQuery().SetParent((Q)this);
        newQuery.EngineScope = EngineScope;
        return newQuery;
    }

    /// <summary>
    /// Add a component clause to the query.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="clause"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public Q AddComponent(Component component, AbstractClause clause)
    {
        clause.Component = component;
        Clauses.Add(clause);

        return (Q)this;
    }

    /// <summary>
    /// If the query already contains a clause for the given component
    /// and engine, replace it with the specified clause. Otherwise, just
    /// add the clause.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="clause"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public Q AddOrReplaceComponent(Component component, AbstractClause clause)
    {
        var current = GetComponents(component).SingleOrDefault();

        if (current != null)
            Clauses.Remove(current);

        return AddComponent(component, clause);
    }

    /// <summary>
    /// Get the list of clauses for a component.
    /// </summary>
    /// <returns></returns>
    public List<C> GetComponents<C>(Component component) where C : AbstractClause
    {
        var clauses = Clauses
            .Where(x => x.Component == component)
            .Cast<C>();

        return clauses.ToList();
    }

    /// <summary>
    /// Get the list of clauses for a component.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public List<AbstractClause> GetComponents(Component component)
        => GetComponents<AbstractClause>(component);

    /// <summary>
    /// Get a single component clause from the query.
    /// </summary>
    /// <returns></returns>
    public C? GetOneComponent<C>(Component component) where C : AbstractClause
        => GetComponents<C>(component).FirstOrDefault();

    /// <summary>
    /// Get a single component clause from the query.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public AbstractClause? GetOneComponent(Component component)
        => GetOneComponent<AbstractClause>(component);

    /// <summary>
    /// Return whether the query has clauses for a component.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public bool HasComponent(Component component)
        => GetComponents(component).Any();

    /// <summary>
    /// Remove all clauses for a component.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="engineCode"></param>
    /// <returns></returns>
    public Q ClearComponent(Component component)
    {
        Clauses = Clauses
            .Where(x => x.Component != component)
            .ToList();

        return (Q)this;
    }

    /// <summary>
    /// Set the next boolean operator to "and" for the "where" clause.
    /// </summary>
    /// <returns></returns>
    protected Q And()
    {
        orFlag = false;
        return (Q)this;
    }

    /// <summary>
    /// Set the next boolean operator to "or" for the "where" clause.
    /// </summary>
    /// <returns></returns>
    public Q Or()
    {
        orFlag = true;
        return (Q)this;
    }

    /// <summary>
    /// Set the next "not" operator for the "where" clause.
    /// </summary>
    /// <returns></returns>
    public Q Not(bool flag = true)
    {
        notFlag = flag;
        return (Q)this;
    }

    /// <summary>
    /// Get the boolean operator and reset it to "and"
    /// </summary>
    /// <returns></returns>
    protected bool GetOr()
    {
        var ret = orFlag;

        // reset the flag
        orFlag = false;
        return ret;
    }

    /// <summary>
    /// Get the "not" operator and clear it
    /// </summary>
    /// <returns></returns>
    protected bool GetNot()
    {
        var ret = notFlag;

        // reset the flag
        notFlag = false;
        return ret;
    }

    /// <summary>
    /// Add a from Clause
    /// </summary>
    /// <param name="table"></param>
    /// <returns></returns>
    public Q From(string table)
        => AddOrReplaceComponent(Component.From, new FromClause
        {
            Table = table,
        });

    public Q From(Query query, string? alias = null)
    {
        query = query.Clone();
        query.SetParent((Q)this);

        if (alias != null)
        {
            query.As(alias);
        };

        return AddOrReplaceComponent(Component.From, new QueryFromClause
        {
            Query = query
        });
    }

    public Q FromRaw(string sql, params object[] bindings)
        => AddOrReplaceComponent(Component.From, new RawFromClause
        {
            Expression = sql,
            Bindings = bindings,
        });

    public Q From(Func<Query, Query> callback, string? alias = null)
    {
        var query = new Query();

        query.SetParent((Q)this);

        return From(callback.Invoke(query), alias);
    }
}
