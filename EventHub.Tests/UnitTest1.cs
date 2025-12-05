using EventHub.Models;
using NUnit.Framework;

namespace EventHub.Tests;

public class TicketOrderTests
{
    [Test]
    public void TotalPrice_ShouldBe_QuantityTimesEventPrice()
    {
        // Arrange
        var ev = new Event
        {
            Id = 1,
            TicketPrice = 10.5m
        };

        var order = new TicketOrder
        {
            EventId    = ev.Id,
            Event      = ev,
            Quantity   = 3,
            TotalPrice = 31.5m // value we expect to be: 3 * 10.5
        };

        // Act
        decimal expected = order.Quantity * ev.TicketPrice;

        // Assert
        Assert.That(order.TotalPrice, Is.EqualTo(expected));
    }
}