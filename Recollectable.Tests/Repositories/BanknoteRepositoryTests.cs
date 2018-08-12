﻿using Microsoft.EntityFrameworkCore;
using Recollectable.Data.Repositories;
using Recollectable.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace Recollectable.Tests.Repositories
{
    public class BanknoteRepositoryTests : RecollectableTestBase
    {
        private IBanknoteRepository _banknoteRepository;
        private ICountryRepository _countryRepository;
        private ICollectionRepository _collectionRepository;
        private IConditionRepository _conditionRepository;

        public BanknoteRepositoryTests()
        {
            _countryRepository = new CountryRepository(_context);
            _collectionRepository = new CollectionRepository
                (_context, new UserRepository(_context));
            _conditionRepository = new ConditionRepository(_context);
            _banknoteRepository = new BanknoteRepository(_context, _countryRepository,
                _collectionRepository, _conditionRepository);
        }

        [Fact]
        public void GetBanknotes_ReturnsAllBanknotes()
        {
            var result = _banknoteRepository.GetBanknotes();
            Assert.NotNull(result);
            Assert.Equal(6, result.Count());
        }

        [Fact]
        public void GetBanknotes_OrdersCollectionsByCountry()
        {
            var result = _banknoteRepository.GetBanknotes();
            Assert.Equal("Canada", result.First().Country.Name);
        }

        [Theory]
        [InlineData("8c29c8a2-93ae-483d-8235-b0c728d3a034", 1, "Mexico")]
        [InlineData("c8f2031e-c780-4d27-bf13-1ee48a7207a3", 2, "United States of America")]
        public void GetBanknotesByCountry_ReturnsAllBanknotesFromCountry_GivenValidCountryId
            (string countryId, int expectedCount, string expectedName)
        {
            var result = _banknoteRepository
                .GetBanknotesByCountry(new Guid(countryId));
            Assert.NotNull(result);
            Assert.Equal(expectedCount, result.Count());
            Assert.Equal(expectedName, result.First().Country.Name);
        }

        [Fact]
        public void GetBanknotesByCountry_OrdersByBanknoteType_GivenValidCountryId()
        {
            var result = _banknoteRepository
                .GetBanknotesByCountry(new Guid("c8f2031e-c780-4d27-bf13-1ee48a7207a3"));
            Assert.Equal("Dollars", result.First().Type);
        }

        [Fact]
        public void GetBanknotesByCountry_ReturnsNull_GivenInvalidCountryId()
        {
            var result = _banknoteRepository
                .GetBanknotesByCountry(new Guid("61a01d02-666c-4d88-b867-ba8d7c134987"));
            Assert.Null(result);
        }

        [Fact]
        public void GetBanknote_ReturnsBanknote_GivenValidBanknoteId()
        {
            var result = _banknoteRepository
                .GetBanknote(new Guid("3da0c34f-dbfb-41a3-801f-97b7f4cdde89"));
            Assert.NotNull(result);
            Assert.Equal("3da0c34f-dbfb-41a3-801f-97b7f4cdde89", result.Id.ToString());
            Assert.Equal("Pounds", result.Type);
        }

        [Fact]
        public void GetBanknote_ReturnsNull_GivenInvalidBanknoteId()
        {
            var result = _collectionRepository
                .GetCollection(new Guid("358a071b-9bf7-49d8-ac50-3296684e3ea7"));
            Assert.Null(result);
        }

        [Fact]
        public void AddBanknote_AddsNewBanknote()
        {
            Banknote newBanknote = new Banknote
            {
                Id = new Guid("86dbe5cf-df75-41a5-af56-6e2f2de181a4"),
                Type = "Euros",
                CountryId = new Guid("1b38bfce-567c-4d49-9dd2-e0fbef480367"),
                CollectorValueId = new Guid("5e9cb33b-b12c-4e20-8113-d8e002aeb38d")
            };

            _banknoteRepository.AddBanknote(newBanknote);
            _banknoteRepository.Save();

            Assert.Equal(7, _banknoteRepository.GetBanknotes().Count());
            Assert.Equal("Euros", _banknoteRepository
                .GetBanknote(new Guid("86dbe5cf-df75-41a5-af56-6e2f2de181a4"))
                .Type);
        }

        [Fact]
        public void UpdateBanknote_UpdatesExistingBanknote()
        {
            Banknote updatedBanknote = _banknoteRepository
                .GetBanknote(new Guid("48d9049b-04f0-4c24-a1c3-c3668878013e"));
            updatedBanknote.Type = "Euros";

            _banknoteRepository.UpdateBanknote(updatedBanknote);
            _banknoteRepository.Save();

            Assert.Equal(6, _banknoteRepository.GetBanknotes().Count());
            Assert.Equal("Euros", _banknoteRepository
                .GetBanknote(new Guid("48d9049b-04f0-4c24-a1c3-c3668878013e"))
                .Type);
        }

        [Fact]
        public void DeleteBanknote_RemovesBanknoteFromDatabase()
        {
            Banknote banknote = _banknoteRepository
                .GetBanknote(new Guid("0acf8863-1bec-49a6-b761-ce27dd219e7c"));

            _banknoteRepository.DeleteBanknote(banknote);
            _banknoteRepository.Save();

            Assert.Equal(5, _banknoteRepository.GetBanknotes().Count());
            Assert.Null(_banknoteRepository
                .GetBanknote(new Guid("0acf8863-1bec-49a6-b761-ce27dd219e7c")));
        }
    }
}