using Application.Interfaces.General;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Concrete;
using Persistence.DBContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataAccess.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        // TODO: Use AOP for transcation management
        public async Task BeginTransaction()
        {
            _transaction = _context.Database.BeginTransaction();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task SaveChangesAsync()
        {
            var entries = _context.ChangeTracker.Entries()
               .Where(e => e.Entity is BaseModel &&
                          (e.State == Microsoft.EntityFrameworkCore.EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseModel)entityEntry.Entity;

                entity.ModifiedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<int> SaveChanges()
        {
            var entries = _context.ChangeTracker.Entries()
               .Where(e => e.Entity is BaseModel &&
                          (e.State == Microsoft.EntityFrameworkCore.EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseModel)entityEntry.Entity;

                entity.ModifiedDate = DateTime.UtcNow;
            }

            return _context.SaveChanges();
        }

        public async Task Commit()
        {
            await _transaction?.CommitAsync();
            _transaction?.DisposeAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
            }
        }

        public async Task Rollback()
        {
             _transaction?.Rollback();
            _transaction?.Dispose();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }

        public async Task DetachAllEntries()
        {
            var entries = _context.ChangeTracker.Entries().ToList();
            foreach (var entry in entries)
            {
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Detached;
            }
        }
    }
}
