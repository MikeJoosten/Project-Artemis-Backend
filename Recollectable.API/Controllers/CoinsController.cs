﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Recollectable.Data.Helpers;
using Recollectable.Data.Repositories;
using Recollectable.Data.Services;
using Recollectable.Domain.Entities;
using Recollectable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Recollectable.API.Controllers
{
    [Route("api/coins")]
    public class CoinsController : Controller
    {
        private ICoinRepository _coinRepository;
        private ICountryRepository _countryRepository;
        private ICollectorValueRepository _collectorValueRepository;
        private IUrlHelper _urlHelper;
        private IPropertyMappingService _propertyMappingService;
        private ITypeHelperService _typeHelperService;

        public CoinsController(ICoinRepository coinRepository, 
            ICollectorValueRepository collectorValueRepository, 
            ICountryRepository countryRepository, IUrlHelper urlHelper,
            IPropertyMappingService propertyMappingService, 
            ITypeHelperService typeHelperService)
        {
            _coinRepository = coinRepository;
            _countryRepository = countryRepository;
            _collectorValueRepository = collectorValueRepository;
            _urlHelper = urlHelper;
            _propertyMappingService = propertyMappingService;
            _typeHelperService = typeHelperService;
        }

        [HttpHead]
        [HttpGet(Name = "GetCoins")]
        public IActionResult GetCoins(CurrenciesResourceParameters resourceParameters,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_propertyMappingService.ValidMappingExistsFor<CoinDto, Coin>
                (resourceParameters.OrderBy))
            {
                return BadRequest();
            }

            if (!_typeHelperService.TypeHasProperties<CoinDto>
                (resourceParameters.Fields))
            {
                return BadRequest();
            }

            var coinsFromRepo = _coinRepository.GetCoins(resourceParameters);
            var coins = Mapper.Map<IEnumerable<CoinDto>>(coinsFromRepo);

            if (mediaType == "application/json+hateoas")
            {
                var paginationMetadata = new
                {
                    totalCount = coinsFromRepo.TotalCount,
                    pageSize = coinsFromRepo.PageSize,
                    currentPage = coinsFromRepo.CurrentPage,
                    totalPages = coinsFromRepo.TotalPages
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                var links = CreateCoinsLinks(resourceParameters,
                    coinsFromRepo.HasNext, coinsFromRepo.HasPrevious);
                var shapedCoins = coins.ShapeData(resourceParameters.Fields);

                var linkedCoins = shapedCoins.Select(coin =>
                {
                    var coinAsDictionary = coin as IDictionary<string, object>;
                    var coinLinks = CreateCoinLinks((Guid)coinAsDictionary["Id"],
                        resourceParameters.Fields);

                    coinAsDictionary.Add("links", coinLinks);

                    return coinAsDictionary;
                });

                var linkedCollectionResource = new
                {
                    value = linkedCoins,
                    links
                };

                return Ok(linkedCollectionResource);
            }
            else if (mediaType == "application/json")
            {
                var previousPageLink = coinsFromRepo.HasPrevious ?
                    CreateCoinsResourceUri(resourceParameters,
                    ResourceUriType.PreviousPage) : null;

                var nextPageLink = coinsFromRepo.HasNext ?
                    CreateCoinsResourceUri(resourceParameters,
                    ResourceUriType.NextPage) : null;

                var paginationMetadata = new
                {
                    totalCount = coinsFromRepo.TotalCount,
                    pageSize = coinsFromRepo.PageSize,
                    currentPage = coinsFromRepo.CurrentPage,
                    totalPages = coinsFromRepo.TotalPages,
                    previousPageLink,
                    nextPageLink,
                };

                Response.Headers.Add("X-Pagination",
                    JsonConvert.SerializeObject(paginationMetadata));

                return Ok(coins.ShapeData(resourceParameters.Fields));
            }
            else
            {
                return Ok(coins);
            }
        }

        [HttpGet("{id}", Name = "GetCoin")]
        public IActionResult GetCoin(Guid id, [FromQuery] string fields,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (!_typeHelperService.TypeHasProperties<CoinDto>(fields))
            {
                return BadRequest();
            }

            var coinFromRepo = _coinRepository.GetCoin(id);

            if (coinFromRepo == null)
            {
                return NotFound();
            }

            var coin = Mapper.Map<CoinDto>(coinFromRepo);

            if (mediaType == "application/json+hateoas")
            {
                var links = CreateCoinLinks(id, fields);
                var linkedResource = coin.ShapeData(fields)
                    as IDictionary<string, object>;

                linkedResource.Add("links", links);

                return Ok(linkedResource);
            }
            else if (mediaType == "application/json")
            {
                return Ok(coin.ShapeData(fields));
            }
            else
            {
                return Ok(coin);
            }
        }

        [HttpPost(Name = "CreateCoin")]
        public IActionResult CreateCoin([FromBody] CoinCreationDto coin,
            [FromHeader(Name = "Accept")] string mediaType)
        {
            if (coin == null)
            {
                return BadRequest();
            }

            var country = _countryRepository.GetCountry(coin.CountryId);

            if (country != null && coin.Country == null)
            {
                coin.Country = country;
            }
            else if (coin.CountryId != Guid.Empty || 
                coin.Country.Id != Guid.Empty)
            {
                return BadRequest();
            }

            var collectorValue = _collectorValueRepository
                .GetCollectorValue(coin.CollectorValueId);

            if (collectorValue != null && coin.CollectorValue == null)
            {
                coin.CollectorValue = collectorValue;
            }
            else if (coin.CollectorValueId != Guid.Empty || 
                coin.CollectorValue.Id != Guid.Empty)
            {
                return BadRequest();
            }

            var newCoin = Mapper.Map<Coin>(coin);
            _coinRepository.AddCoin(newCoin);

            if (!_coinRepository.Save())
            {
                throw new Exception("Creating a coin failed on save.");
            }

            var returnedCoin = Mapper.Map<CoinDto>(newCoin);

            if (mediaType == "application/json+hateoas")
            {
                var links = CreateCoinLinks(returnedCoin.Id, null);
                var linkedResource = returnedCoin.ShapeData(null)
                    as IDictionary<string, object>;

                linkedResource.Add("links", links);

                return CreatedAtRoute("GetCoin", 
                    new { id = returnedCoin.Id }, 
                    linkedResource);
            }
            else
            {
                return CreatedAtRoute("GetCoin",
                    new { id = returnedCoin.Id },
                    returnedCoin);
            }
        }

        [HttpPost("{id}")]
        public IActionResult BlockCoinCreation(Guid id)
        {
            if (_coinRepository.CoinExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpPut("{id}", Name = "UpdateCoin")]
        public IActionResult UpdateCoin(Guid id, [FromBody] CoinUpdateDto coin)
        {
            if (coin == null)
            {
                return BadRequest();
            }

            if (!_countryRepository.CountryExists(coin.CountryId))
            {
                return BadRequest();
            }

            if (!_collectorValueRepository.CollectorValueExists(coin.CollectorValueId))
            {
                return BadRequest();
            }

            var coinFromRepo = _coinRepository.GetCoin(id);

            if (coinFromRepo == null)
            {
                return NotFound();
            }

            coinFromRepo.CountryId = coin.CountryId;
            coinFromRepo.CollectorValueId = coin.CollectorValueId;

            Mapper.Map(coin, coinFromRepo);
            _coinRepository.UpdateCoin(coinFromRepo);

            if (!_coinRepository.Save())
            {
                throw new Exception($"Updating coin {id} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{id}", Name = "PartiallyUpdateCoin")]
        public IActionResult PartiallyUpdateCoin(Guid id,
            [FromBody] JsonPatchDocument<CoinUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var coinFromRepo = _coinRepository.GetCoin(id);

            if (coinFromRepo == null)
            {
                return NotFound();
            }

            var patchedCoin = Mapper.Map<CoinUpdateDto>(coinFromRepo);
            patchDoc.ApplyTo(patchedCoin);

            if (!_countryRepository.CountryExists(patchedCoin.CountryId))
            {
                return BadRequest();
            }

            if (!_collectorValueRepository.CollectorValueExists(patchedCoin.CollectorValueId))
            {
                return BadRequest();
            }

            coinFromRepo.CountryId = patchedCoin.CountryId;
            coinFromRepo.CollectorValueId = patchedCoin.CollectorValueId;

            Mapper.Map(patchedCoin, coinFromRepo);
            _coinRepository.UpdateCoin(coinFromRepo);

            if (!_coinRepository.Save())
            {
                throw new Exception($"Patching coin {id} failed on save.");
            }

            return NoContent();
        }

        [HttpDelete("{id}", Name = "DeleteCoin")]
        public IActionResult DeleteCoin(Guid id)
        {
            var coinFromRepo = _coinRepository.GetCoin(id);

            if (coinFromRepo == null)
            {
                return NotFound();
            }

            _coinRepository.DeleteCoin(coinFromRepo);

            if (!_coinRepository.Save())
            {
                throw new Exception($"Deleting coin {id} failed on save.");
            }

            return NoContent();
        }

        [HttpOptions]
        public IActionResult GetCoinsOptions()
        {
            Response.Headers.Add("Allow", "GET - OPTIONS - POST - PUT - PATCH - DELETE");
            return Ok();
        }

        private string CreateCoinsResourceUri(CurrenciesResourceParameters resourceParameters, 
            ResourceUriType type)
        {
            switch (type)
            {
                case ResourceUriType.PreviousPage:
                    return _urlHelper.Link("GetCoins", new
                    {
                        type = resourceParameters.Type,
                        country = resourceParameters.Country,
                        search = resourceParameters.Search,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page - 1,
                        pageSize = resourceParameters.PageSize
                    });
                case ResourceUriType.NextPage:
                    return _urlHelper.Link("GetCoins", new
                    {
                        type = resourceParameters.Type,
                        country = resourceParameters.Country,
                        search = resourceParameters.Search,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page + 1,
                        pageSize = resourceParameters.PageSize
                    });
                default:
                    return _urlHelper.Link("GetCoins", new
                    {
                        type = resourceParameters.Type,
                        country = resourceParameters.Country,
                        search = resourceParameters.Search,
                        orderBy = resourceParameters.OrderBy,
                        fields = resourceParameters.Fields,
                        page = resourceParameters.Page,
                        pageSize = resourceParameters.PageSize
                    });
            }
        }

        private IEnumerable<LinkDto> CreateCoinLinks(Guid id, string fields)
        {
            var links = new List<LinkDto>();

            if (string.IsNullOrEmpty(fields))
            {
                links.Add(new LinkDto(_urlHelper.Link("GetCoins",
                    new { id }), "self", "GET"));

                links.Add(new LinkDto(_urlHelper.Link("CreateCoins",
                    new { }), "create_coins", "POST"));

                links.Add(new LinkDto(_urlHelper.Link("UpdateCoins",
                    new { id }), "update_coins", "PUT"));

                links.Add(new LinkDto(_urlHelper.Link("PartiallyUpdateCoins",
                    new { id }), "partially_update_coins", "PATCH"));

                links.Add(new LinkDto(_urlHelper.Link("DeleteCoins",
                    new { id }), "delete_coins", "DELETE"));
            }

            return links;
        }

        private IEnumerable<LinkDto> CreateCoinsLinks
            (CurrenciesResourceParameters resourceParameters,
            bool hasNext, bool hasPrevious)
        {
            var links = new List<LinkDto>();

            links.Add(new LinkDto(CreateCoinsResourceUri(resourceParameters,
                ResourceUriType.Current), "self", "GET"));

            if (hasNext)
            {
                links.Add(new LinkDto(CreateCoinsResourceUri(resourceParameters,
                    ResourceUriType.NextPage), "nextPage", "GET"));
            }

            if (hasPrevious)
            {
                links.Add(new LinkDto(CreateCoinsResourceUri(resourceParameters,
                    ResourceUriType.PreviousPage), "previousPage", "GET"));
            }

            return links;
        }
    }
}