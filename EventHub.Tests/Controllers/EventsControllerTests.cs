using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventHub.Controllers;
using EventHub.Data;
using EventHub.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NUnit.Framework;

namespace EventHub.Tests.Controllers
{
    [TestFixture]
    public class EventsControllerTests
    {
        // ========== Helper: إنشاء DbContext InMemory لكل Test ==========
        private ApplicationDbContext CreateInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // DB جديدة لكل Test
                .Options;

            return new ApplicationDbContext(options);
        }

        // ========== Helper: إنشاء EventsController لاختبارات بسيطة ==========
        private EventsController CreateController(ApplicationDbContext context)
        {
            // حالياً ما نستخدم UserManager داخل Index / Search / Availability في التستات
            UserManager<ApplicationUser> userManager = null!;

            // Logger بسيط (ما يكتب شيء فعلياً)
            ILogger<EventsController> logger = new NullLogger<EventsController>();

            return new EventsController(context, userManager, logger);
        }

        // ========== Test 1: Index يرجّع View مع الأحداث مرتّبة ==========

        [Test]
        public async Task Index_Returns_View_With_Ordered_Events()
        {
            // Arrange – InMemory DB
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            using var context = new ApplicationDbContext(options);

            context.Events.AddRange(
                new Event { Id = 1, Title = "B Event", StartDate = new DateTime(2025, 2, 1) },
                new Event { Id = 2, Title = "A Event", StartDate = new DateTime(2025, 1, 1) }
            );
            await context.SaveChangesAsync();

            UserManager<ApplicationUser> userManager = null!; // مو مستخدم في Index

            ILogger<EventsController> logger = new NullLogger<EventsController>();
            var controller = new EventsController(context, userManager, logger);

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            var model = result!.Model as List<Event>;
            Assert.That(model, Is.Not.Null);

            var ids = model!.Select(e => e.Id).ToArray();
            // لازم تكون مرتّبة حسب StartDate (الأقدم أولاً)
            Assert.That(ids, Is.EqualTo(new[] { 2, 1 }));
        }

        // ========== Test 2: Search يفلتر بالكلمة والفئة ==========

        [Test]
        public async Task Search_Filters_By_Query_And_Category()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            context.Events.AddRange(
                new Event
                {
                    Id = 1,
                    Title = "Music Festival",
                    Category = "Music",
                    Description = "Big music event",
                    Location = "Toronto",
                    StartDate = DateTime.UtcNow.AddDays(5)
                },
                new Event
                {
                    Id = 2,
                    Title = "Sport Game",
                    Category = "Sport",
                    Description = "Football match",
                    Location = "Toronto",
                    StartDate = DateTime.UtcNow.AddDays(3)
                },
                new Event
                {
                    Id = 3,
                    Title = "Music Night",
                    Category = "Music",
                    Description = "Small music show",
                    Location = "Ottawa",
                    StartDate = DateTime.UtcNow.AddDays(10)
                }
            );
            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            // نبحث عن كلمة "Music" وفئة "Music" فقط
            var result = await controller.Search("Music", "Music", onlyUpcoming: false);

            // Assert
            var partial = result as PartialViewResult;
            Assert.That(partial, Is.Not.Null, "Search should return PartialViewResult");

            var model = partial!.Model as IEnumerable<Event>;
            Assert.That(model, Is.Not.Null);

            var list = model!.ToList();

            // لازم يرجّع الحدثين اللي Category = Music
            Assert.That(list.Count, Is.EqualTo(2));
            Assert.That(list.All(e => e.Category == "Music"), Is.True);
        }

        // ========== Test 3: Availability يعيد التذاكر المتبقية صح ==========

        [Test]
        public async Task Availability_Returns_Correct_Remaining_Tickets()
        {
            // Arrange
            using var context = CreateInMemoryDbContext();

            var ev = new Event
            {
                Id = 1,
                Title = "Test Event",
                TotalTickets = 100,
                StartDate = DateTime.UtcNow.AddDays(1)
            };
            context.Events.Add(ev);

            context.TicketOrders.AddRange(
                new TicketOrder
                {
                    EventId = 1,
                    AttendeeId = "user1",
                    Quantity = 30,
                    TotalPrice = 300
                },
                new TicketOrder
                {
                    EventId = 1,
                    AttendeeId = "user2",
                    Quantity = 10,
                    TotalPrice = 100
                }
            );

            await context.SaveChangesAsync();

            var controller = CreateController(context);

            // Act
            var result = await controller.Availability(1);

            // Assert
            var json = result as JsonResult;
            Assert.That(json, Is.Not.Null, "Availability should return JsonResult");

            var data = json!.Value!;
            var remainingProperty = data.GetType().GetProperty("remainingTickets");
            Assert.That(remainingProperty, Is.Not.Null, "JSON should contain remainingTickets");

            var remaining = (int)remainingProperty!.GetValue(data)!;

            // 100 total - (30 + 10) = 60
            Assert.That(remaining, Is.EqualTo(60));
        }

        // ========== Logger بسيط لا يعمل شيء فعليًا (بدون Moq) ==========
        private class NullLogger<T> : ILogger<T>, IDisposable
        {
            public IDisposable BeginScope<TState>(TState state) => this;
            public void Dispose() { }
            public bool IsEnabled(LogLevel logLevel) => false;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                // لا شيء
            }
        }
    }
}
