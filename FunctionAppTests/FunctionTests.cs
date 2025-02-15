﻿using Azure.Data.Tables;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Azure;

namespace CleverFunctionApp.Functions.Tests
{
    [TestClass()]
    public class FunctionTests
    {
        [TestMethod("Create Rfid")]
        [DataRow("12345.1")]
        [DataRow("23456.2")]
        [DataRow("34567.1")]
        public async Task CreateRfidFunctionTest(string rfid)
        {
            // Arrange
            Mock<TableServiceClient> mockTableServiceClient = new();
            Mock<TableClient> mockTableClient = new();
            Mock<ILogger<CreateRfidFunction>> mockLogger = new();

            mockTableServiceClient.Setup(c => c.GetTableClient(It.IsAny<string>())).Returns(mockTableClient.Object);

            var function = new CreateRfidFunction(mockTableServiceClient.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            var requestBody = $"\"{rfid}\"";
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(requestBody));

            // Act
            IActionResult result = await function.Run(context.Request);

            // Assert
            Assert.IsInstanceOfType<OkResult>(result);
        }

        [TestMethod("Authenticate Rfid")]
        [DataRow("12345.1")]
        [DataRow("23456.2")]
        [DataRow("34567.1")]
        public async Task AuthenticateRfidFunctionTest(string rfid)
        {
            // Arrange
            Mock<TableServiceClient> mockTableServiceClient = new();
            Mock<TableClient> mockTableClient = new();
            Mock<ILogger<AuthenticateRfidFunction>> mockLogger = new();

            mockTableClient.Setup(x => x.GetEntityIfExistsAsync<TableEntity>("PartitionKey", rfid, default, default))
                           .ReturnsAsync(Response.FromValue(new TableEntity(), new Mock<Response>().Object));

            mockTableServiceClient.Setup(x => x.GetTableClient(
                    It.IsAny<string>()
                )
            ).Returns(mockTableClient.Object);

            var function = new AuthenticateRfidFunction(mockTableServiceClient.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "rfid", rfid } });

            // Act
            IActionResult result = await function.Run(context.Request);

            // Assert 
            Assert.IsInstanceOfType<OkObjectResult>(result);
            var okResult = (OkObjectResult)result;
            Assert.IsTrue((bool?)okResult.Value);
        }

        [TestMethod("Unable to Create Rfid with empty rfid")]
        [DataRow("")]
        public async Task NegativeCreateRfidFunctionTest(string rfid)
        {
            // Arrange
            var mockTableServiceClient = new Mock<TableServiceClient>();
            var mockTableClient = new Mock<TableClient>();
            mockTableServiceClient.Setup(c => c.GetTableClient(It.IsAny<string>())).Returns(mockTableClient.Object);
            Mock<ILogger<CreateRfidFunction>> mockLogger = new();

            var function = new CreateRfidFunction(mockTableServiceClient.Object, mockLogger.Object);

            var invalidRequestBody = ""; // Empty string to simulate invalid input
            var context = new DefaultHttpContext();
            context.Request.Body = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalidRequestBody));

            // Act
            var result = await function.Run(context.Request);

            // Assert
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("RequestBody cannot be null or empty.", badRequestResult.Value);
        }
        
        [TestMethod("Unable to Authenticate Rfid with empty rfid")]
        [DataRow("")]
        public async Task NegativeAuthenticateRfidFunctionTest(string rfid)
        {
            // Arrange
            Mock<TableServiceClient> mockTableServiceClient = new();
            Mock<TableClient> mockTableClient = new();
            Mock<ILogger<AuthenticateRfidFunction>> mockLogger = new();

            mockTableClient.Setup(x => x.GetEntityIfExistsAsync<TableEntity>("PartitionKey", rfid, default, default))
                           .ReturnsAsync(Response.FromValue(new TableEntity(), new Mock<Response>().Object));

            mockTableServiceClient.Setup(x => x.GetTableClient(
                    It.IsAny<string>()
                )
            ).Returns(mockTableClient.Object);

            var function = new AuthenticateRfidFunction(mockTableServiceClient.Object, mockLogger.Object);

            var context = new DefaultHttpContext();
            context.Request.Query = new QueryCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues> { { "rfid", rfid } });

            // Act
            IActionResult result = await function.Run(context.Request);

            // Assert 
            Assert.IsInstanceOfType<BadRequestObjectResult>(result);
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.AreEqual("RFID query parameter is required.", badRequestResult.Value);
        }
    }
}