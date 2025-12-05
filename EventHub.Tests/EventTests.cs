using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EventHub.Models;
using NUnit.Framework;

namespace EventHub.Tests.Models
{
    [TestFixture]
    public class EventTests
    {
        // Helper عام للتحقق من الـ DataAnnotations
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        // ========== Test 1: Title مطلوب (ما يكون فاضي) ==========
        [Test]
        public void Event_With_Empty_Title_Is_Invalid()
        {
            var ev = new Event
            {
                Title        = "",              // ❌ فاضي
                Category     = "Music",
                Location     = "Toronto",
                TotalTickets = 100,
                TicketPrice  = 50,
                StartDate    = DateTime.UtcNow.AddDays(1),
                EndDate      = DateTime.UtcNow.AddDays(2)
            };

            var results = ValidateModel(ev);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("Title")),
                Is.True,
                "Expected validation error for Title, but not found."
            );
        }

        // ========== Test 2: TotalTickets ما تكون سالبة ==========
        [Test]
        public void Event_With_Negative_TotalTickets_Is_Invalid()
        {
            var ev = new Event
            {
                Title        = "Test Event",
                Category     = "Music",
                Location     = "Toronto",
                TotalTickets = -5,              // ❌ قيمة غير منطقية
                TicketPrice  = 50,
                StartDate    = DateTime.UtcNow.AddDays(1),
                EndDate      = DateTime.UtcNow.AddDays(2)
            };

            var results = ValidateModel(ev);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("TotalTickets")),
                Is.True,
                "Expected validation error for TotalTickets, but not found."
            );
        }

        // ========== Test 3: TicketPrice ما تكون سالبة ==========
        [Test]
        public void Event_With_Negative_TicketPrice_Is_Invalid()
        {
            var ev = new Event
            {
                Title        = "Test Event",
                Category     = "Music",
                Location     = "Toronto",
                TotalTickets = 100,
                TicketPrice  = -10,             // ❌
                StartDate    = DateTime.UtcNow.AddDays(1),
                EndDate      = DateTime.UtcNow.AddDays(2)
            };

            var results = ValidateModel(ev);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("TicketPrice")),
                Is.True,
                "Expected validation error for TicketPrice, but not found."
            );
        }

        // ========== Test 4: EndDate قبل StartDate → الموديل غير صالح ==========
        [Test]
        public void Event_With_EndDate_Before_StartDate_Is_Invalid()
        {
            var ev = new Event
            {
                Title        = "Test Event",
                Category     = "Music",
                Location     = "Toronto",
                TotalTickets = 100,
                TicketPrice  = 50,
                StartDate    = DateTime.UtcNow.AddDays(2),
                EndDate      = DateTime.UtcNow.AddDays(1) // قبل البداية
            };

            var results = ValidateModel(ev);

            // بما إن عندك فاليديشن على التواريخ، نتوقّع وجود أخطاء
            Assert.That(results.Count, Is.GreaterThan(0),
                "Model should be invalid when EndDate is before StartDate.");
        }
    }
}
