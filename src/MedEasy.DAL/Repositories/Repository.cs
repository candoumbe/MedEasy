
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MedEasy.DAL.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace MedEasy.DAL.Repositories
{
    public class Repository<TEntry> : RepositoryBase<TEntry>, IRepository<TEntry> where TEntry : class
    {
         protected DbSet<TEntry> Entries { get; set; }
        public Repository(IDbContext context)
            : base(context)
        {
            Entries = Context.Set<TEntry>();
        }

        public virtual PagedResult<TEntry> ReadAll(IEnumerable<OrderClause<TEntry>> orderBy, int limit, int offset)
        {
            return ReadAll(item => item, orderBy, limit, offset);
        }

        public virtual PagedResult<TResult> ReadAll<TResult>(Expression<Func<TEntry, TResult>> selector, IEnumerable<OrderClause<TResult>> orderBy, int limit, int offset)
        {

            IQueryable<TResult> query = Entries.Select(selector)
                    .Skip((offset - 1) * offset)
                    .Take(limit);
            
            
            return new PagedResult<TResult>(query.ToArray(), Entries.Count());
        }

        public virtual async Task<PagedResult<TEntry>> ReadPageAsync(IEnumerable<OrderClause<TEntry>> orderBy, int limit, int offset)
        {
            return await ReadPageAsync(item => item, limit, offset, orderBy);
        }

        public virtual async Task<PagedResult<TResult>> ReadPageAsync<TResult>(Expression<Func<TEntry, TResult>> selector, int limit, int offset, IEnumerable<OrderClause<TResult>> orderBy)
        {

            
            IQueryable<TResult> resultQuery = Entries.Select(selector);

            if (orderBy != null)
            {
                resultQuery = resultQuery.OrderBy(orderBy);
            }

            IEnumerable<TResult> result = await resultQuery
                .Skip(offset < 1 ? 0 : (offset - 1) * limit)
                .Take(limit)
                .ToArrayAsync();

            return new PagedResult<TResult>(result.ToArray(), await Entries.CountAsync());
        }

        public IEnumerable<TResult> ReadAll<TResult>(Expression<Func<TEntry, TResult>> selector)
        {

            return Entries.Select(selector).ToArray();
        }

        public virtual IEnumerable<TEntry> ReadAll()
        {
            return ReadAll(item => item);
        }


        public virtual async Task<IEnumerable<TResult>> ReadAllAsync<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return await Entries.Select(selector).ToArrayAsync();
        }

        public virtual async Task<IEnumerable<TEntry>> ReadAllAsync()
        {
            return await ReadAllAsync(item => item);
        }

        public virtual IEnumerable<TEntry> Where(Expression<Func<TEntry, bool>> predicate)
        {
            return Where(item => item, predicate);
        }

        public virtual IEnumerable<TResult> Where<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            IEnumerable<TResult> entries = Entries.Where(predicate).Select(selector).ToArray();


            return entries;
        }

        public IEnumerable<TResult> Where<TKey, TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, Expression<Func<TResult, TKey>> keySelector, Expression<Func<IGrouping<TKey, TResult>, TResult>> groupBySelector )
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (groupBySelector == null)
            {
                throw new ArgumentNullException(nameof(groupBySelector));
            }
            
            IEnumerable<TResult> results = Entries
                    .Select(selector)
                    .GroupBy(keySelector)
                    .Select(groupBySelector)
                    .ToArray();
            
            return results;

        }

        public virtual async Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector).ToArrayAsync();
        }


        public async Task<IEnumerable<TResult>> WhereAsync<TKey, TResult>(Expression<Func<TEntry, bool>> predicate, Expression<Func<TEntry, TKey>> keySelector, Expression<Func<IGrouping<TKey, TEntry>, TResult>> groupBySelector)
        {

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (keySelector == null)
            {
                throw new ArgumentNullException(nameof(keySelector));
            }
            if (groupBySelector == null)
            {
                throw new ArgumentNullException(nameof(groupBySelector));
            }


            IEnumerable<TResult> results = await Entries
                    .Where(predicate)
                    .GroupBy(keySelector)
                    .Select(groupBySelector)
                    .ToArrayAsync();
            
            return results;
        }



        public virtual async Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await WhereAsync(item => item, predicate);

        }

        public virtual async Task<IEnumerable<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null)
        {
            return await WhereAsync(item => item, predicate, orderBy, includedProperties);
        }

        public virtual async Task<IEnumerable<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null)
        {


            return await Entries
                .Where(predicate)
                //.Include(includedProperties)
                .Select(selector)
                //.OrderBy(orderBy)
                .ToArrayAsync();
        }

        public async Task<PagedResult<TEntry>> WhereAsync(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy, int limit, int offset)
        {

            return await WhereAsync(item => item, predicate, orderBy, limit, offset);
        }

        public async Task<PagedResult<TResult>> WhereAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy, int limit, int offset)
        {

            if (orderBy == null)
            {
                throw new ArgumentNullException(nameof(orderBy), $"{nameof(orderBy)} expression must be set");
            }
            IQueryable<TResult> query = Entries
                .Where(predicate)
                .Select(selector)
                //.OrderBy(orderBy)
                .Skip(limit * (offset - 1))
                .Take(limit);
            


            PagedResult<TResult> pagedResult = new PagedResult<TResult>(await query.ToArrayAsync(), await CountAsync(predicate));

            return pagedResult;
        }

        public virtual IEnumerable<TResult> Where<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TResult>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null)
        {

            IQueryable<TResult> query = Entries
                    .Where(predicate)
                    //.Include(includedProperties)
                    .Select(selector);
                    //.OrderBy(orderBy);

            return query.ToArray();

        }

        public virtual IEnumerable<TEntry> Where(Expression<Func<TEntry, bool>> predicate, IEnumerable<OrderClause<TEntry>> orderBy = null, IEnumerable<IncludeClause<TEntry>> includedProperties = null)
        {
            return Where(item => item, predicate, orderBy, includedProperties);
        }

        public bool Any()
        {
            return Entries.Any();
        }

        public async Task<bool> AnyAsync()
        {
            return await Entries.AnyAsync();
        }

        public TResult Max<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return Entries.Max(selector);
        }

        public async Task<TResult> MaxAsync<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return await Entries.MaxAsync(selector);
        }

        public TResult Min<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return Entries.Min(selector);
        }

        public async Task<TResult> MinAsync<TResult>(Expression<Func<TEntry, TResult>> selector)
        {
            return await Entries.MinAsync(selector);
        }

        public bool Any(Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Any(predicate);
        }

        public async Task<bool> AnyAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.AnyAsync(predicate);
        }

        public int Count()
        {
            return Entries.Count();
        }

        public async Task<int> CountAsync()
        {
            return await Entries.CountAsync();
        }

        public int Count(Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Count(predicate);
        }

        public async Task<int> CountAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.CountAsync(predicate);
        }

        public TEntry Single()
        {
            return Entries.Single();
        }

        public TEntry Single(Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Single(predicate);
        }

        public async Task<TEntry> SingleAsync()
        {
            return await Entries.SingleAsync();
        }

        public async Task<TEntry> SingleAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.SingleAsync(predicate);
        }

        public TResult Single<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Where(predicate).Select(selector).Single();
        }

        public async Task<TResult> SingleAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector).SingleAsync();
        }

        public TEntry SingleOrDefault()
        {
            return Entries.SingleOrDefault();
        }

        public async Task<TEntry> SingleOrDefaultAsync()
        {
            return await Entries.SingleOrDefaultAsync();
        }

        public TEntry SingleOrDefault(Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.SingleOrDefault(predicate);
        }

        public async Task<TEntry> SingleOrDefaultAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.SingleOrDefaultAsync(predicate);
        }

        public TResult SingleOrDefault<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Where(predicate).Select(selector).SingleOrDefault();
        }

        public async Task<TResult> SingleOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector).SingleOrDefaultAsync();
        }

        public TEntry First()
        {
            return Entries.First();
        }

        public async Task<TEntry> FirstAsync()
        {
            return await Entries.FirstAsync();
        }

        public TEntry FirstOrDefault()
        {
            return Entries.FirstOrDefault();
        }

        public async Task<TEntry> FirstOrDefaultAsync()
        {
            return await Entries.FirstOrDefaultAsync();
        }

        public TEntry First(Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.First(predicate);
        }

        public async Task<TEntry> FirstAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.FirstAsync(predicate);
        }

        public void Delete(Expression<Func<TEntry, bool>> predicate)
        {
            IEnumerable<TEntry> entries = Entries.Where(predicate);
            Entries.RemoveRange(entries);
        }

        public TResult First<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Where(predicate).Select(selector).First();
        }

        public async Task<TResult> FirstAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector).FirstAsync();
        }

        public TResult FirstOrDefault<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.Where(predicate).Select(selector).FirstOrDefault();
        }

        public async Task<TResult> FirstOrDefaultAsync<TResult>(Expression<Func<TEntry, TResult>> selector, Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.Where(predicate).Select(selector).FirstOrDefaultAsync();
        }

        public TEntry FirstOrDefault(Expression<Func<TEntry, bool>> predicate)
        {
            return Entries.FirstOrDefault(predicate);
        }

        public async Task<TEntry> FirstOrDefaultAsync(Expression<Func<TEntry, bool>> predicate)
        {
            return await Entries.FirstOrDefaultAsync(predicate);
        }
    }
}