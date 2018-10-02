﻿using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.ResourceParameters;
using System;
using System.Linq;
using Xunit;

namespace Recollectable.Tests.Repositories
{
    public class CollectableRepositoryTests : RecollectableTestBase
    {
        private CollectablesResourceParameters resourceParameters;

        public CollectableRepositoryTests()
        {
            resourceParameters = new CollectablesResourceParameters();
        }

        [Fact]
        public void Get_ReturnsAllCoins_GivenValidCollectionId()
        {
            Guid collectionId = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789");

            var result = _unitOfWork.CollectableRepository
                .Get(collectionId, resourceParameters);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public void Get_ReturnsNull_GivenInvalidCollectionId()
        {
            Guid collectionId = new Guid("fa3551fc-3a11-48e8-860f-7457fcaa1fee");

            var result = _unitOfWork.CollectableRepository
                .Get(collectionId, resourceParameters);

            Assert.Null(result);
        }

        [Fact]
        public void GetById_ReturnsCollectable_GivenValidIds()
        {
            Guid id = new Guid("22e513a9-b851-4b93-931c-5904d9120f73");
            Guid collectionId = new Guid("ab76b149-09c9-40c8-9b35-e62e53e06c8a");

            var result = _unitOfWork.CollectableRepository.GetById(collectionId, id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Coin", result.Collection.Type);
        }

        [Theory]
        [InlineData("52c47433-7e1d-464c-b9a8-6832cee99b83", "22e513a9-b851-4b93-931c-5904d9120f73")]
        [InlineData("ab76b149-09c9-40c8-9b35-e62e53e06c8a", "a50266a3-826f-4ce1-b5da-5e6fb6978d63")]
        [InlineData("52c47433-7e1d-464c-b9a8-6832cee99b83", "a50266a3-826f-4ce1-b5da-5e6fb6978d63")]
        public void GetById_ReturnsNull_GivenInvalidIds
            (string collectionId, string collectableId)
        {
            var result = _unitOfWork.CollectableRepository.GetById
                (new Guid(collectionId), new Guid(collectableId));

            Assert.Null(result);
        }

        [Fact]
        public void GetCollectableItem_ReturnsCollectableItem_GivenValidCollectableItemId()
        {
            Guid id = new Guid("3a7fd6a5-d654-4647-8374-eba27001b0d3");

            var result = _unitOfWork.CollectableRepository.GetCollectableItem(id);

            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal("Mexico", result.Country.Name);
        }

        [Fact]
        public void GetCollectableItem_ReturnsNull_GivenInvalidCollectableItemId()
        {
            var result = _unitOfWork.CollectableRepository
                .GetCollectableItem(new Guid("6ad88c33-4ccf-4850-a4ef-0f84db739a24"));

            Assert.Null(result);
        }

        [Fact]
        public void Add_AddsNewCollectable()
        {
            Guid id = new Guid("60e55387-ee18-4e5c-866f-7ca1d2d09c0f");
            Guid collectionId = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789");
            CollectionCollectable newCollectable = new CollectionCollectable
            {
                Id = id,
                CollectionId = collectionId,
                CollectableId = new Guid("dc94e4a0-8ad1-4eec-ad9d-e4c6cf147f48"),
                Condition = "MS62"
            };

            _unitOfWork.CollectableRepository.Add(newCollectable);
            _unitOfWork.Save();

            Assert.Equal(3, _unitOfWork.CollectableRepository
                .Get(collectionId, resourceParameters).Count());
            Assert.Equal("France", _unitOfWork.CollectableRepository
                .GetById(collectionId, id).Collectable.Country.Name);
        }

        [Fact]
        public void Update_UpdatesExistingCollectable()
        {
            Guid id = new Guid("355e785b-dd47-4fb7-b112-1fb34d189569");
            Guid collectionId = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789");
            CollectionCollectable updatedCollectable = 
                _unitOfWork.CollectableRepository.GetById(collectionId, id);
            Coin coin = _unitOfWork.CoinRepository
                .GetById(new Guid("db14f24e-aceb-4315-bfcf-6ace1f9b3613"));
            updatedCollectable.Collectable = coin;

            _unitOfWork.CollectableRepository.Update(updatedCollectable);
            _unitOfWork.Save();

            Assert.Equal(2, _unitOfWork.CollectableRepository
                .Get(collectionId, resourceParameters).Count());
            Assert.Equal("Japan", _unitOfWork.CollectableRepository
                .GetById(collectionId, id).Collectable.Country.Name);
        }

        [Fact]
        public void Delete_RemovesCollectableFromDatabase()
        {
            Guid id = new Guid("355e785b-dd47-4fb7-b112-1fb34d189569");
            Guid collectionId = new Guid("46df9402-62e1-4ff6-9cb0-0955957ec789");
            CollectionCollectable collectable = 
                _unitOfWork.CollectableRepository.GetById(collectionId, id);

            _unitOfWork.CollectableRepository.Delete(collectable);
            _unitOfWork.Save();

            Assert.Single(_unitOfWork.CollectableRepository
                .Get(collectionId, resourceParameters));
            Assert.Null(_unitOfWork.CollectableRepository
                .GetById(collectionId, id));
        }

        [Fact]
        public void Exists_ReturnsTrue_GivenValidIds()
        {
            var result = _unitOfWork.CollectableRepository
                .Exists(new Guid("ab76b149-09c9-40c8-9b35-e62e53e06c8a"), 
                new Guid("22e513a9-b851-4b93-931c-5904d9120f73"));

            Assert.True(result);
        }

        [Theory]
        [InlineData("52c47433-7e1d-464c-b9a8-6832cee99b83", "22e513a9-b851-4b93-931c-5904d9120f73")]
        [InlineData("ab76b149-09c9-40c8-9b35-e62e53e06c8a", "a50266a3-826f-4ce1-b5da-5e6fb6978d63")]
        [InlineData("52c47433-7e1d-464c-b9a8-6832cee99b83", "a50266a3-826f-4ce1-b5da-5e6fb6978d63")]
        public void Exists_ReturnsFalse_GivenInvalidIds(string collectionId, string collectableId)
        {
            var result = _unitOfWork.CollectableRepository
                .Exists(new Guid(collectionId), new Guid(collectableId));

            Assert.False(result);
        }
    }
}