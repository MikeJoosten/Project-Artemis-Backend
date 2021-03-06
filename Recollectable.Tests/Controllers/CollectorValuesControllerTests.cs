﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Recollectable.API.Controllers;
using Recollectable.API.Models.Collectables;
using Recollectable.Core.Entities.Collectables;
using Recollectable.Core.Entities.ResourceParameters;
using Recollectable.Core.Interfaces;
using Recollectable.Core.Shared.Entities;
using Recollectable.Tests.Builders;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Recollectable.Tests.Controllers
{
    public class CollectorValuesControllerTests : RecollectableTestBase
    {
        private readonly CollectorValuesController _controller;
        private readonly Mock<ICollectorValueService> _mockCollectorValueService;
        private readonly CollectorValuesResourceParameters resourceParameters;
        private readonly CollectorValueTestBuilder _builder;

        public CollectorValuesControllerTests()
        {
            _mockCollectorValueService = new Mock<ICollectorValueService>();
            _mockCollectorValueService.Setup(c => c.Save()).ReturnsAsync(true);

            _controller = new CollectorValuesController(_mockCollectorValueService.Object, _mapper);
            SetupTestController(_controller);

            _builder = new CollectorValueTestBuilder();
            resourceParameters = new CollectorValuesResourceParameters();
            resourceParameters.Fields = "Id, G4";
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsBadRequestResponse_GivenInvalidOrderByParameter()
        {
            //Arrange
            resourceParameters.OrderBy = "Invalid";

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsBadRequestResponse_GivenInvalidFieldsParameter()
        {
            //Arrange
            resourceParameters.Fields = "Invalid";

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsBadRequestObjectResponse_GivenFieldParameterWithNoId()
        {
            //Arrange
            string mediaType = "application/json+hateoas";
            var collectorValues = _builder.Build(2);
            var pagedList = PagedList<CollectorValue>.Create(collectorValues,
                resourceParameters.Page, resourceParameters.PageSize);
            resourceParameters.Fields = "G4";

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValues(resourceParameters))
                .ReturnsAsync(pagedList);

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, mediaType);

            //Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("application/json+hateoas")]
        public async Task GetCollectorValues_ReturnsOkResponse_GivenAnyMediaType(string mediaType)
        {
            //Arrange
            var collectorValues = _builder.Build(2);
            var pagedList = PagedList<CollectorValue>.Create(collectorValues, 
                resourceParameters.Page, resourceParameters.PageSize);

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValues(resourceParameters))
                .ReturnsAsync(pagedList);

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, mediaType);

            //Assert
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsAllCollectorValues_GivenAnyMediaType()
        {
            //Arrange
            var collectorValues = _builder.Build(2);
            var pagedList = PagedList<CollectorValue>.Create(collectorValues, 
                resourceParameters.Page, resourceParameters.PageSize);

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValues(resourceParameters))
                .ReturnsAsync(pagedList);

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, null) as OkObjectResult;
            var result = response.Value as List<ExpandoObject>;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsAllCollectorValues_GivenHateoasMediaType()
        {
            //Arrange
            string mediaType = "application/json+hateoas";
            var collectorValues = _builder.Build(2);
            var pagedList = PagedList<CollectorValue>.Create(collectorValues, 
                resourceParameters.Page, resourceParameters.PageSize);

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValues(resourceParameters))
                .ReturnsAsync(pagedList);

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, mediaType) as OkObjectResult;
            var result = response.Value as LinkedCollectionResource;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsCollectorValues_GivenAnyMediaTypeAndPagingParameters()
        {
            //Arrange
            var collectorValues = _builder.Build(4);
            var pagedList = PagedList<CollectorValue>.Create(collectorValues, 1, 2);

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValues(resourceParameters))
                .ReturnsAsync(pagedList);

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, null) as OkObjectResult;
            var result = response.Value as List<ExpandoObject>;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetCollectorValues_ReturnsCollectorValues_GivenHateoasMediaTypeAndPagingParameters()
        {
            //Arrange
            string mediaType = "application/json+hateoas";
            var collectorValues = _builder.Build(4);
            var pagedList = PagedList<CollectorValue>.Create(collectorValues, 1, 2);

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValues(resourceParameters))
                .ReturnsAsync(pagedList);

            //Act
            var response = await _controller.GetCollectorValues(resourceParameters, mediaType) as OkObjectResult;
            var result = response.Value as LinkedCollectionResource;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetCollectorValue_ReturnsBadRequestResponse_GivenInvalidFieldsParameter()
        {
            //Arrange
            string fields = "Invalid";

            //Act
            var response = await _controller.GetCollectorValue(Guid.Empty, fields, null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task GetCollectorValue_ReturnsNotFoundResponse_GivenInvalidId()
        {
            //Act
            var response = await _controller.GetCollectorValue(Guid.Empty, null, null);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task GetCollectorValue_ReturnsBadRequestObjectResponse_GivenFieldParameterWithNoId()
        {
            //Arrange
            string mediaType = "application/json+hateoas";
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            var collectorValue = _builder.WithId(id).WithG4(15.54).Build();
            resourceParameters.Fields = "G4";

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValueById(id))
                .ReturnsAsync(collectorValue);

            //Act
            var response = await _controller.GetCollectorValue(id, resourceParameters.Fields, mediaType);

            //Assert
            Assert.IsType<BadRequestObjectResult>(response);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("application/json+hateoas")]
        public async Task GetCollectorValue_ReturnsOkResponse_GivenAnyMediaType(string mediaType)
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            var collectorValue = _builder.Build();

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValueById(id))
                .ReturnsAsync(collectorValue);

            //Act
            var response = await _controller.GetCollectorValue(id, resourceParameters.Fields, mediaType);

            //Assert
            Assert.IsType<OkObjectResult>(response);
        }

        [Fact]
        public async Task GetCollectorValue_ReturnsCollectorValue_GivenAnyMediaType()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            var collectorValue = _builder.WithId(id).WithG4(15.54).Build();

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValueById(id))
                .ReturnsAsync(collectorValue);

            //Act
            var response = await _controller.GetCollectorValue(id, null, null) as OkObjectResult;
            dynamic result = response.Value as ExpandoObject;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(15.54, result.G4);
        }

        [Fact]
        public async Task GetCollectorValue_ReturnsCollectorValue_GivenHateoasMediaType()
        {
            //Arrange
            string mediaType = "application/json+hateoas";
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            var collectorValue = _builder.WithId(id).WithG4(15.54).Build();

            _mockCollectorValueService
                .Setup(c => c.FindCollectorValueById(id))
                .ReturnsAsync(collectorValue);

            //Act
            var response = await _controller.GetCollectorValue(id, resourceParameters.Fields, mediaType) as OkObjectResult;
            dynamic result = response.Value as IDictionary<string, object>;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(id, result.Id);
            Assert.Equal(15.54, result.G4);
        }

        [Fact]
        public async Task CreateCollectorValue_ReturnsBadRequestResponse_GivenNoCollection()
        {
            //Act
            var response = await _controller.CreateCollectorValue(null, null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task CreateCollectorValue_ReturnsUnprocessableEntityObjectResponse_GivenInvalidCollectorValue()
        {
            //Arrange
            var collectorValue = _builder.BuildCreationDto();
            _controller.ModelState.AddModelError("G4", "Required");

            //Act
            var response = await _controller.CreateCollectorValue(collectorValue, null);

            //Assert
            Assert.IsType<UnprocessableEntityObjectResult>(response);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("application/json+hateoas")]
        public async Task CreateCollectorValue_ReturnsCreatedResponse_GivenValidCollectorValue(string mediaType)
        {
            //Arrange
            var collectorValue = _builder.WithG4(18.64).BuildCreationDto();

            //Act
            var response = await _controller.CreateCollectorValue(collectorValue, mediaType);

            //Assert
            Assert.IsType<CreatedAtRouteResult>(response);
        }

        [Fact]
        public async Task CreateCollectorValue_CreatesNewCollectorValue_GivenAnyMediaTypeAndValidCollectorValue()
        {
            //Arrange
            var collectorValue = _builder.WithG4(18.64).BuildCreationDto();

            //Act
            var response = await _controller.CreateCollectorValue(collectorValue, null) as CreatedAtRouteResult;
            var result = response.Value as CollectorValueDto;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(18.64, result.G4);
        }

        [Fact]
        public async Task CreateCollectorValue_CreatesNewCollectorValue_GivenHateoasMediaTypeAndValidCollectorValue()
        {
            //Arrange
            string mediaType = "application/json+hateoas";
            var collectorValue = _builder.WithG4(18.64).BuildCreationDto();

            //Act
            var response = await _controller.CreateCollectorValue(collectorValue, mediaType) as CreatedAtRouteResult;
            dynamic result = response.Value as IDictionary<string, object>;

            //Assert
            Assert.NotNull(result);
            Assert.Equal(18.64, result.G4);
        }

        [Fact]
        public async Task BlockCountryCollectorValue_ReturnsConflictResponse_GivenExistingId()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            _mockCollectorValueService.Setup(c => c.CollectorValueExists(It.IsAny<Guid>())).ReturnsAsync(true);

            //Act
            var response = await _controller.BlockCollectorValueCreation(id) as StatusCodeResult;

            //Assert
            Assert.Equal(StatusCodes.Status409Conflict, response.StatusCode);
            _mockCollectorValueService.Verify(c => c.CollectorValueExists(id));
        }

        [Fact]
        public async Task BlockCollectorValueCreation_ReturnsNotFoundResponse_GivenUnexistingId()
        {
            //Act
            var response = await _controller.BlockCollectorValueCreation(Guid.Empty);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task UpdateCollectorValue_ReturnsBadRequestResponse_GivenNoCollectorValue()
        {
            //Act
            var response = await _controller.UpdateCollectorValue(Guid.Empty, null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task UpdateCollectorValue_ReturnsUnprocessableEntityObjectResponse_GivenInvalidCollectorValue()
        {
            //Arrange
            var collectorValue = _builder.BuildUpdateDto();
            _controller.ModelState.AddModelError("G4", "Required");

            //Act
            var response = await _controller.UpdateCollectorValue(Guid.Empty, collectorValue);

            //Assert
            Assert.IsType<UnprocessableEntityObjectResult>(response);
        }

        [Fact]
        public async Task UpdateCollectorValue_ReturnsNotFoundResponse_GivenInvalidCollectorValueId()
        {
            //Arrange
            var collectorValue = _builder.WithG4(18.64).BuildUpdateDto();

            //Act
            var response = await _controller.UpdateCollectorValue(Guid.Empty, collectorValue);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task UpdateCollectorValue_ReturnsNoContentResponse_GivenValidCollectorValue()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            var collectorValue = _builder.WithG4(18.64).BuildUpdateDto();
            var retrievedCollectorValue = _builder.Build();

            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(retrievedCollectorValue);

            //Act
            var response = await _controller.UpdateCollectorValue(id, collectorValue);

            //Assert
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task UpdateCollectorValue_UpdatesExistingCollectorValue_GivenValidCollectorValue()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");
            var collectorValue = _builder.WithG4(18.64).BuildUpdateDto();
            var retrievedCollectorValue = _builder.WithId(id).Build();

            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(retrievedCollectorValue);
            _mockCollectorValueService.Setup(c => c.UpdateCollectorValue(It.IsAny<CollectorValue>()));

            //Act
            var response = await _controller.UpdateCollectorValue(id, collectorValue);

            //Assert
            _mockCollectorValueService.Verify(c => c.UpdateCollectorValue(retrievedCollectorValue));
        }

        [Fact]
        public async Task PartiallyUpdateCollectorValue_ReturnsBadRequestResponse_GivenNoPatchDocument()
        {
            //Act
            var response = await _controller.PartiallyUpdateCollectorValue(Guid.Empty, null);

            //Assert
            Assert.IsType<BadRequestResult>(response);
        }

        [Fact]
        public async Task PartiallyUpdateCollectorValue_ReturnsNotFoundResponse_GivenInvalidCollectorValueId()
        {
            //Arrange
            JsonPatchDocument<CollectorValueUpdateDto> patchDoc = new JsonPatchDocument<CollectorValueUpdateDto>();

            //Act
            var response = await _controller.PartiallyUpdateCollectorValue(Guid.Empty, patchDoc);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task PartiallyUpdateCollectorValue_ReturnsUnprocessableEntityObjectResponse_GivenInvalidCollectorValue()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");

            var collectorValue = _builder.Build();
            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(collectorValue);

            JsonPatchDocument<CollectorValueUpdateDto> patchDoc = new JsonPatchDocument<CollectorValueUpdateDto>();
            _controller.ModelState.AddModelError("G4", "Required");

            //Act
            var response = await _controller.PartiallyUpdateCollectorValue(id, patchDoc);

            //Assert
            Assert.IsType<UnprocessableEntityObjectResult>(response);
        }

        [Fact]
        public async Task PartiallyUpdateCollectorValue_ReturnsNoContentResponse_GivenValidPatchDocument()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");

            var collectorValue = _builder.Build();
            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(collectorValue);

            JsonPatchDocument<CollectorValueUpdateDto> patchDoc = new JsonPatchDocument<CollectorValueUpdateDto>();
            patchDoc.Replace(c => c.G4, 18.64);

            //Act
            var response = await _controller.PartiallyUpdateCollectorValue(id, patchDoc);

            //Assert
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task PartiallyUpdateCollectorValue_UpdatesExistingCollectorValue_GivenValidPatchDocument()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");

            var collectorValue = _builder.WithId(id).Build();
            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(collectorValue);
            _mockCollectorValueService.Setup(c => c.UpdateCollectorValue(It.IsAny<CollectorValue>()));

            JsonPatchDocument<CollectorValueUpdateDto> patchDoc = new JsonPatchDocument<CollectorValueUpdateDto>();
            patchDoc.Replace(c => c.G4, 18.64);

            //Act
            var response = await _controller.PartiallyUpdateCollectorValue(id, patchDoc);

            //Assert
            _mockCollectorValueService.Verify(c => c.UpdateCollectorValue(collectorValue));
        }

        [Fact]
        public async Task DeleteCollectorValue_ReturnsNotFoundResponse_GivenInvalidCollectorValueId()
        {
            //Act
            var response = await _controller.DeleteCollectorValue(Guid.Empty);

            //Assert
            Assert.IsType<NotFoundResult>(response);
        }

        [Fact]
        public async Task DeleteCollectorValue_ReturnsNoContentResponse_GivenValidCollectorValueId()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");

            var collectorValue = _builder.Build();
            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(collectorValue);

            //Act
            var response = await _controller.DeleteCollectorValue(id);

            //Assert
            Assert.IsType<NoContentResult>(response);
        }

        [Fact]
        public async Task DeleteCollectorValue_RemovesCollectorValueFromDatabase()
        {
            //Arrange
            Guid id = new Guid("843a6427-48ab-421c-ba35-3159b1b024a5");

            var collectorValue = _builder.WithId(id).Build();
            _mockCollectorValueService.Setup(c => c.FindCollectorValueById(id)).ReturnsAsync(collectorValue);
            _mockCollectorValueService.Setup(c => c.RemoveCollectorValue(It.IsAny<CollectorValue>()));

            //Act
            await _controller.DeleteCollectorValue(id);

            //Assert
            _mockCollectorValueService.Verify(c => c.RemoveCollectorValue(collectorValue));
        }
    }
}