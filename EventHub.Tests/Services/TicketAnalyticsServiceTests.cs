using System;
using System.Threading.Tasks;
using EventHub.Data;
using EventHub.Models;
using EventHub.Services;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace EventHub.Tests.Services
{
    [TestFixture]
    public class TicketAnalyticsServiceTests
    {
        // Helper لإنشاء InMemory DbContext
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new ApplicationDbContext(options);
        }

        [Test]
        public async Task GetTotalRevenueForUserAsync_Returns_Correct_Sum()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            context.TicketOrders.AddRange(
                new TicketOrder
                {
                    EventId = 1,
                    AttendeeId = "user1",
                    Quantity = 2,
                    TotalPrice = 40m,
                    PurchaseDate = DateTime.UtcNow
                },
                new TicketOrder
                {
                    EventId = 2,
                    AttendeeId = "user1",
                    Quantity = 1,
                    TotalPrice = 25m,
                    PurchaseDate = DateTime.UtcNow
                },
                new TicketOrder
                {
                    EventId = 3,
                    AttendeeId = "user2",
                    Quantity = 3,
                    TotalPrice = 60m,
                    PurchaseDate = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var service = new TicketAnalyticsService(context);

            // Act
            var total = await service.GetTotalRevenueForUserAsync("user1");

            // Assert
            Assert.That(total, Is.EqualTo(65m)); // 40 + 25
        }

        [Test]
        public async Task GetTotalTicketsForEventAsync_Returns_Correct_Sum()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            context.TicketOrders.AddRange(
                new TicketOrder
                {
                    EventId = 10,
                    AttendeeId = "user1",
                    Quantity = 2,
                    TotalPrice = 40m,
                    PurchaseDate = DateTime.UtcNow
                },
                new TicketOrder
                {
                    EventId = 10,
                    AttendeeId = "user2",
                    Quantity = 3,
                    TotalPrice = 60m,
                    PurchaseDate = DateTime.UtcNow
                },
                new TicketOrder
                {
                    EventId = 11,
                    AttendeeId = "user3",
                    Quantity = 1,
                    TotalPrice = 20m,
                    PurchaseDate = DateTime.UtcNow
                }
            );

            await context.SaveChangesAsync();

            var service = new TicketAnalyticsService(context);

            // Act
            var totalTickets = await service.GetTotalTicketsForEventAsync(10);

            // Assert
            Assert.That(totalTickets, Is.EqualTo(5)); // 2 + 3
        }
    }
}
