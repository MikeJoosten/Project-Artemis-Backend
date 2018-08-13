﻿using Microsoft.EntityFrameworkCore;
using Recollectable.Data.Helpers;
using Recollectable.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Recollectable.Data.Repositories
{
    public class CollectableRepository : ICollectableRepository
    {
        private RecollectableContext _context;
        private ICollectionRepository _collectionRepository;

        public CollectableRepository(RecollectableContext context, 
            ICollectionRepository collectionRepository)
        {
            _context = context;
            _collectionRepository = collectionRepository;
        }

        public PagedList<CollectionCollectable> GetCollectables(Guid collectionId,
            CollectablesResourceParameters resourceParameters)
        {
            if (!_collectionRepository.CollectionExists(collectionId))
            {
                return null;
            }

            var collectables = _context.CollectionCollectables
                .Include(cc => cc.Condition)
                .Include(cc => cc.Collectable)
                .ThenInclude(c => c.Country)
                .Include(cc => cc.Collectable)
                .ThenInclude(c => c.CollectorValue)
                .Where(cc => cc.CollectionId == collectionId)
                .OrderBy(cc => cc.Collectable.Country)
                .ThenBy(cc => cc.Collectable.ReleaseDate)
                .AsQueryable();

            if (!string.IsNullOrEmpty(resourceParameters.Country))
            {
                var country = resourceParameters.Country.Trim().ToLowerInvariant();
                collectables = collectables.Where(c => 
                    c.Collectable.Country.Name.ToLowerInvariant() == country);
            }

            if (!string.IsNullOrEmpty(resourceParameters.Search))
            {
                var search = resourceParameters.Search.Trim().ToLowerInvariant();
                collectables = collectables.Where(c => c.Collectable.Country.Name.ToLowerInvariant().Contains(search)
                    || c.Collectable.ReleaseDate.ToLowerInvariant().Contains(search));
            }

            return PagedList<CollectionCollectable>.Create(collectables,
                resourceParameters.Page,
                resourceParameters.PageSize);
        }

        public Collectable GetCollectable(Guid collectableId)
        {
            return _context.Collectables.FirstOrDefault(c => c.Id == collectableId);
        }

        public CollectionCollectable GetCollectable(Guid collectionId, Guid Id)
        {
            return _context.CollectionCollectables
                .Include(cc => cc.Condition)
                .Include(cc => cc.Collectable)
                .ThenInclude(c => c.Country)
                .Include(cc => cc.Collectable)
                .ThenInclude(c => c.CollectorValue)
                .Where(cc => cc.CollectionId == collectionId)
                .FirstOrDefault(cc => cc.Id == Id);
        }

        public void AddCollectable(CollectionCollectable collectable)
        {
            if (collectable.Id == Guid.Empty)
            {
                collectable.Id = Guid.NewGuid();
            }

            _context.CollectionCollectables.Add(collectable);
        }

        public void UpdateCollectable(CollectionCollectable collectable) { }

        public void DeleteCollectable(CollectionCollectable collectable)
        {
            _context.CollectionCollectables.Remove(collectable);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public bool CollectableExists(Guid Id)
        {
            return _context.CollectionCollectables.Any(cc => cc.Id == Id);
        }
    }
}