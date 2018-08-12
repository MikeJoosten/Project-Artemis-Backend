﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Recollectable.API.Models;
using Recollectable.Data.Repositories;
using Recollectable.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recollectable.API.Controllers
{
    [Route("api/collections/{collectionId}/collectables")]
    public class CollectablesController : Controller
    {
        private ICollectableRepository _collectableRepository;
        private ICollectionRepository _collectionRepository;
        private IConditionRepository _conditionRepository;

        public CollectablesController(ICollectableRepository collectableRepository,
            ICollectionRepository collectionRepository, IConditionRepository conditionRepository)
        {
            _collectableRepository = collectableRepository;
            _collectionRepository = collectionRepository;
            _conditionRepository = conditionRepository;
        }

        [HttpGet]
        public IActionResult GetCollectables(Guid collectionId)
        {
            var collectablesFromRepo = _collectableRepository.GetCollectables(collectionId);

            if (collectablesFromRepo == null)
            {
                return BadRequest();
            }

            var collectables = Mapper.Map<IEnumerable<CollectableDto>>(collectablesFromRepo);
            return Ok(collectables);
        }

        [HttpGet("{id}", Name = "GetCollectable")]
        public IActionResult GetCollectable(Guid collectionId, Guid id)
        {
            var collectableFromRepo = _collectableRepository.GetCollectable(collectionId, id);

            if (collectableFromRepo == null)
            {
                return NotFound();
            }

            var collectable = Mapper.Map<CollectableDto>(collectableFromRepo);
            return Ok(collectable);
        }

        [HttpPost]
        public IActionResult CreateCollectable(Guid collectionId, 
            [FromBody] CollectableCreationDto collectable)
        {
            var collection = _collectionRepository.GetCollection(collectionId);

            if (collection == null)
            {
                return NotFound();
            }

            var collectableItem = _collectableRepository.GetCollectable(collectable.CollectableId);

            if (collectableItem == null || !collectableItem.GetType().ToString()
                .ToLower().Contains(collection.Type.ToLower()))
            {
                return BadRequest();
            }

            var condition = _conditionRepository.GetCondition(collectable.ConditionId);

            if (condition == null)
            {
                return BadRequest();
            }

            var newCollectable = Mapper.Map<CollectionCollectable>(collectable);
            newCollectable.CollectionId = collectionId;
            newCollectable.Collection = collection;
            newCollectable.Collectable = collectableItem;
            newCollectable.Condition = condition;

            _collectableRepository.AddCollectable(newCollectable);

            if (!_collectableRepository.Save())
            {
                throw new Exception("Creating a collectable failed on save.");
            }

            var returnedCollectable = Mapper.Map<CollectableDto>(newCollectable);
            return CreatedAtRoute("GetCollectable",
                new { id = returnedCollectable.Id },
                returnedCollectable);
        }

        [HttpPost("{id}")]
        public IActionResult BlockCollectableCreation(Guid id)
        {
            if (_collectableRepository.CollectableExists(id))
            {
                return new StatusCodeResult(StatusCodes.Status409Conflict);
            }

            return NotFound();
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCoin(Guid collectionId, Guid id,
            [FromBody] CollectableUpdateDto collectable)
        {
            if (collectable == null)
            {
                return BadRequest();
            }

            var collection = _collectionRepository.GetCollection(collectable.CollectionId);

            if (collection == null)
            {
                return BadRequest();
            }

            var collectableItem = _collectableRepository.GetCollectable(collectable.CollectableId);

            if (collectableItem == null || !collectableItem.GetType().ToString()
                .ToLower().Contains(collection.Type.ToLower()))
            {
                return BadRequest();
            }

            if (!_conditionRepository.ConditionExists(collectable.ConditionId))
            {
                return BadRequest();
            }

            var collectableFromRepo = _collectableRepository.GetCollectable(collectionId, id);

            if (collectableFromRepo == null)
            {
                return NotFound();
            }

            collectableFromRepo.CollectionId = collectable.CollectionId;
            collectableFromRepo.CollectableId = collectable.CollectableId;
            collectableFromRepo.ConditionId = collectable.ConditionId;

            Mapper.Map(collectable, collectableFromRepo);
            _collectableRepository.UpdateCollectable(collectableFromRepo);

            if (!_collectableRepository.Save())
            {
                throw new Exception($"Updating collectable {id} failed on save.");
            }

            return NoContent();
        }

        [HttpPatch("{id}")]
        public IActionResult PartiallyUpdateCoin(Guid collectionId, Guid id,
            [FromBody] JsonPatchDocument<CollectableUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var collectableFromRepo = _collectableRepository.GetCollectable(collectionId, id);

            if (collectableFromRepo == null)
            {
                return NotFound();
            }

            var patchedCollectable = Mapper.Map<CollectableUpdateDto>(collectableFromRepo);
            patchDoc.ApplyTo(patchedCollectable);

            var collection = _collectionRepository.GetCollection(patchedCollectable.CollectionId);

            if (collection == null)
            {
                return BadRequest();
            }

            var collectableItem = _collectableRepository.GetCollectable(patchedCollectable.CollectableId);

            if (collectableItem == null || !collectableItem.GetType().ToString()
                .ToLower().Contains(collection.Type.ToLower()))
            {
                return BadRequest();
            }

            if (!_conditionRepository.ConditionExists(patchedCollectable.ConditionId))
            {
                return BadRequest();
            }

            collectableFromRepo.CollectionId = patchedCollectable.CollectionId;
            collectableFromRepo.CollectableId = patchedCollectable.CollectableId;
            collectableFromRepo.ConditionId = patchedCollectable.ConditionId;

            Mapper.Map(patchedCollectable, collectableFromRepo);
            _collectableRepository.UpdateCollectable(collectableFromRepo);

            if (!_collectableRepository.Save())
            {
                throw new Exception($"Patching collectable {id} failed on save.");
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCoin(Guid collectionId, Guid id)
        {
            var collectableFromRepo = _collectableRepository.GetCollectable(collectionId, id);

            if (collectableFromRepo == null)
            {
                return NotFound();
            }

            _collectableRepository.DeleteCollectable(collectableFromRepo);

            if (!_collectableRepository.Save())
            {
                throw new Exception($"Deleting collectable {id} failed on save.");
            }

            return NoContent();
        }
    }
}