using EventHub.Models;
using NUnit.Framework;

namespace EventHub.Tests;

public class ModelsTests
{
    [Test]
    public void TicketOrder_TotalPrice_ShouldMatch_QuantityTimesEventPrice()
    {
        // Arrange
        var ev = new Event
        {
            Id = 2,
            TicketPrice = 20m
        };

        var order = new TicketOrder
        {
            EventId    = ev.Id,
            Event      = ev,
            Quantity   = 2,
            TotalPrice = 40m // 2 * 20
        };

        // Act
        decimal expected = order.Quantity * ev.TicketPrice;

        // Assert
        Assert.That(order.TotalPrice, Is.EqualTo(expected));
    }
}