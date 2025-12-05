using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using EventHub.Models;
using NUnit.Framework;

namespace EventHub.Tests.Models
{
    [TestFixture]
    public class TicketOrderTests
    {
        // Helper عام للتحقق من الـ DataAnnotations
        private IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        // ========== Test 1: Order صحيح ==========

        [Test]
        public void TicketOrder_With_Valid_Data_Is_Valid()
        {
            var order = new TicketOrder
            {
                EventId      = 1,
                AttendeeId   = "user1",
                Quantity     = 2,
                TotalPrice   = 100m,
                PurchaseDate = DateTime.UtcNow
            };

            var results = ValidateModel(order);

            // ما نتوقع أخطاء
            Assert.That(results.Count, Is.EqualTo(0));
        }

        // ========== Test 2: Quantity أقل من 1 غير صالح ==========

        [Test]
        public void TicketOrder_With_Zero_Quantity_Is_Invalid()
        {
            var order = new TicketOrder
            {
                EventId      = 1,
                AttendeeId   = "user1",
                Quantity     = 0,      // ❌
                TotalPrice   = 0m,
                PurchaseDate = DateTime.UtcNow
            };

            var results = ValidateModel(order);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("Quantity")),
                Is.True,
                "Expected validation error for Quantity, but not found."
            );
        }

        [Test]
        public void TicketOrder_With_Negative_Quantity_Is_Invalid()
        {
            var order = new TicketOrder
            {
                EventId      = 1,
                AttendeeId   = "user1",
                Quantity     = -5,     // ❌
                TotalPrice   = 0m,
                PurchaseDate = DateTime.UtcNow
            };

            var results = ValidateModel(order);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("Quantity")),
                Is.True,
                "Expected validation error for Quantity, but not found."
            );
        }

        // ========== Test 3: TotalPrice سالب غير صالح ==========

        [Test]
        public void TicketOrder_With_Negative_TotalPrice_Is_Invalid()
        {
            var order = new TicketOrder
            {
                EventId      = 1,
                AttendeeId   = "user1",
                Quantity     = 1,
                TotalPrice   = -10m,   // ❌
                PurchaseDate = DateTime.UtcNow
            };

            var results = ValidateModel(order);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("TotalPrice")),
                Is.True,
                "Expected validation error for TotalPrice, but not found."
            );
        }

        // ========== Test 4: AttendeeId مطلوب ==========

        [Test]
        public void TicketOrder_Without_AttendeeId_Is_Invalid()
        {
            var order = new TicketOrder
            {
                EventId      = 1,
                AttendeeId   = "",     // ❌
                Quantity     = 1,
                TotalPrice   = 50m,
                PurchaseDate = DateTime.UtcNow
            };

            var results = ValidateModel(order);

            Assert.That(results.Count, Is.GreaterThan(0));
            Assert.That(
                results.Any(r => r.MemberNames.Contains("AttendeeId")),
                Is.True,
                "Expected validation error for AttendeeId, but not found."
            );
        }
    }
}
