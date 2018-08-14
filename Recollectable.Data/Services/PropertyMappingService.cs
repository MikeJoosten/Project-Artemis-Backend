﻿using Recollectable.Domain.Entities;
using Recollectable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Recollectable.Data.Services
{
    public class PropertyMappingService : IPropertyMappingService
    {
        private Dictionary<string, PropertyMappingValue> _userPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) },
                { "Name", new PropertyMappingValue(new List<string>() { "FirstName", "LastName" }) },
                { "Email", new PropertyMappingValue(new List<string>() { "Email" }) }
            };

        private Dictionary<string, PropertyMappingValue> _currencyPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) },
                { "Value", new PropertyMappingValue(new List<string>() { "Country.Name", "Type", "FaceValue", "ReleaseDate" }) },
                { "Country", new PropertyMappingValue(new List<string>() { "Country.Name" }) },
                { "ReleaseDate", new PropertyMappingValue(new List<string>() { "ReleaseDate" }) }
            };

        private Dictionary<string, PropertyMappingValue> _collectablePropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) },
                { "Country", new PropertyMappingValue(new List<string>() { "Collectable.Country.Name" }) },
                { "ReleaseDate", new PropertyMappingValue(new List<string>() { "Collectable.ReleaseDate" }) }
            };

        private Dictionary<string, PropertyMappingValue> _collectionPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) },
                { "Type", new PropertyMappingValue(new List<string>() { "Type" }) }
            };

        private Dictionary<string, PropertyMappingValue> _countryPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) },
                { "Name", new PropertyMappingValue(new List<string>() { "Name" }) }
            };

        private Dictionary<string, PropertyMappingValue> _conditionPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) },
                { "Grade", new PropertyMappingValue(new List<string>() { "Grade" }) }
            };

        private Dictionary<string, PropertyMappingValue> _collectorValuesPropertyMapping =
            new Dictionary<string, PropertyMappingValue>(StringComparer.OrdinalIgnoreCase)
            {
                { "Id", new PropertyMappingValue(new List<string>() { "Id.ToString()" }) }
            };

        private IList<IPropertyMapping> propertyMappings = new List<IPropertyMapping>();

        public PropertyMappingService()
        {
            propertyMappings.Add(new PropertyMapping<UserDto, User>(_userPropertyMapping));
            propertyMappings.Add(new PropertyMapping<CollectionDto, Collection>(_collectionPropertyMapping));
            propertyMappings.Add(new PropertyMapping<CoinDto, Coin>(_currencyPropertyMapping));
            propertyMappings.Add(new PropertyMapping<BanknoteDto, Banknote>(_currencyPropertyMapping));
            propertyMappings.Add(new PropertyMapping<CollectableDto, Collectable>(_collectablePropertyMapping));
            propertyMappings.Add(new PropertyMapping<CountryDto, Country>(_countryPropertyMapping));
            propertyMappings.Add(new PropertyMapping<ConditionDto, Condition>(_conditionPropertyMapping));
            propertyMappings.Add(new PropertyMapping<CollectorValueDto, CollectorValue>(_collectorValuesPropertyMapping));
        }

        public Dictionary<string, PropertyMappingValue> GetPropertyMapping<TSource, TDestination>()
        {
            var matchingMapping = propertyMappings.OfType<PropertyMapping<TSource, TDestination>>();

            if (matchingMapping.Count() == 1)
            {
                return matchingMapping.First()._mappingDictionary;
            }

            throw new Exception($"Cannot find exact property mapping instance for <{typeof(TSource)}, {typeof(TDestination)}>");
        }

        public bool ValidMappingExistsFor<TSource, TDestination>(string fields)
        {
            var propertyMapping = GetPropertyMapping<TSource, TDestination>();

            if (string.IsNullOrWhiteSpace(fields))
            {
                return true;
            }

            var fieldsAfterSplit = fields.Split(',');

            foreach (var field in fieldsAfterSplit)
            {
                var trimmedField = field.Trim();
                var indexOfFirstSpace = trimmedField.IndexOf(" ");
                var propertyName = indexOfFirstSpace == -1 ? trimmedField :
                    trimmedField.Remove(indexOfFirstSpace);

                if (!propertyMapping.ContainsKey(propertyName))
                {
                    return false;
                }
            }
            return true;
        }
    }
}