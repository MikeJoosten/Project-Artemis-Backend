﻿using Recollectable.API.Models.Collections;
using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.Collections;
using Recollectable.Core.Entities.Locations;
using System;
using System.Collections.Generic;

namespace Recollectable.Tests.Builders
{
    public class CollectionCollectableTestBuilder
    {
        private CollectionCollectable collectionCollectable;

        public CollectionCollectableTestBuilder()
        {
            collectionCollectable = new CollectionCollectable();
        }

        public CollectionCollectableTestBuilder WithId(Guid id)
        {
            collectionCollectable.Id = id;
            return this;
        }

        public CollectionCollectableTestBuilder WithCountryName(string name)
        {
            collectionCollectable.Collectable = new Collectable();
            collectionCollectable.Collectable.Country = new Country();
            collectionCollectable.Collectable.Country.Name = name;
            return this;
        }

        public CollectionCollectableTestBuilder WithCollectableId(Guid collectableId)
        {
            collectionCollectable.CollectableId = collectableId;
            return this;
        }

        public CollectionCollectable Build()
        {
            return collectionCollectable;
        }

        public CollectionCollectableCreationDto BuildCreationDto()
        {
            return new CollectionCollectableCreationDto
            {
                CollectableId = collectionCollectable.CollectableId
            };
        }

        public CollectionCollectableUpdateDto BuildUpdateDto()
        {
            return new CollectionCollectableUpdateDto
            {
                CollectableId = collectionCollectable.CollectableId
            };
        }

        public List<CollectionCollectable> Build(int count)
        {
            var collectionCollectables = new List<CollectionCollectable>();

            for (int i = 0; i < count; i++)
            {
                collectionCollectables.Add(collectionCollectable);
            }

            return collectionCollectables;
        }
    }
}