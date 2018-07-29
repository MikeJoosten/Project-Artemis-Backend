﻿using Microsoft.EntityFrameworkCore;
using Recollectable.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Recollectable.Data.Repositories
{
    public class BanknoteRepository : IBanknoteRepository
    {
        private RecollectableContext _context;

        public BanknoteRepository(RecollectableContext context)
        {
            _context = context;
        }

        public IEnumerable<Banknote> GetBanknotes()
        {
            return _context.Banknotes.OrderBy(b => b.Country.Name);
        }

        public IEnumerable<Banknote> GetBanknotesByCollection(Guid collectionId)
        {
            return _context.Banknotes
                .Include(b => b.CollectionCollectables)
                .ThenInclude(cc => cc.CollectionId == collectionId);
        }

        public Banknote GetBanknote(Guid banknoteId)
        {
            return _context.Banknotes.FirstOrDefault(b => b.Id == banknoteId);
        }

        public Banknote GetBanknoteByCollection(Guid collectionId, Guid banknoteId)
        {
            return _context.Banknotes
                .Include(b => b.CollectionCollectables)
                .ThenInclude(cc => cc.CollectionId == collectionId)
                .FirstOrDefault(b => b.Id == banknoteId);
        }

        public void AddBanknote(Banknote banknote)
        {
            banknote.Id = Guid.NewGuid();
            _context.Banknotes.Add(banknote);

            if (banknote.Country.Id == Guid.Empty)
            {
                banknote.Country.Id = Guid.NewGuid();
            }

            if (banknote.CollectorValue.Id == Guid.Empty)
            {
                banknote.CollectorValue.Id = Guid.NewGuid();
            }
        }

        public void UpdateBanknote(Banknote banknote) { }

        public void DeleteBanknote(Banknote banknote)
        {
            _context.Banknotes.Remove(banknote);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }
    }
}