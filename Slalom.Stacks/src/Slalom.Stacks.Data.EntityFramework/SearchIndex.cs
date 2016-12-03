﻿using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Slalom.Stacks.Search;

namespace Slalom.Stacks.EntityFramework
{
    public abstract class SearchIndex<TSearchResult> : ISearchIndex<TSearchResult> where TSearchResult : class, ISearchResult
    {
        private readonly DbContext _context;

        protected SearchIndex(DbContext context)
        {
            _context = context;
            this.Set = _context.Set<TSearchResult>();
        }

        protected DbSet<TSearchResult> Set { get; private set; }

        public Task AddAsync(params TSearchResult[] instances)
        {
            _context.AddRange(instances);

            return _context.SaveChangesAsync();
        }

        public Task ClearAsync()
        {
            this.Set.RemoveRange(this.Set);

            return _context.SaveChangesAsync();
        }

        public IQueryable<TSearchResult> CreateQuery()
        {
            return this.Set.AsQueryable();
        }

        public Task DeleteAsync(Expression<Func<TSearchResult, bool>> predicate)
        {
            this.Set.RemoveRange(this.Set.Where(predicate));

            return _context.SaveChangesAsync();
        }

        public Task DeleteAsync(TSearchResult[] instances)
        {
            this.Set.RemoveRange(instances);

            return _context.SaveChangesAsync();
        }

        public Task<TSearchResult> FindAsync(int id)
        {
            return this.Set.Where(e => e.Id == id).FirstOrDefaultAsync();
        }

        public virtual Task RebuildIndexAsync()
        {
            return Task.FromResult(0);
        }

        public Task UpdateAsync(TSearchResult[] instances)
        {
            this.Set.UpdateRange(instances);

            return _context.SaveChangesAsync();
        }

        public Task UpdateAsync(Expression<Func<TSearchResult, bool>> predicate, Expression<Func<TSearchResult, TSearchResult>> expression)
        {
            throw new NotImplementedException();
        }
    }
}