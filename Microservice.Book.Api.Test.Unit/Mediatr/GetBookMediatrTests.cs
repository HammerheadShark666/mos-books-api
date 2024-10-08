// Ignore Spelling: Mediatr

using MediatR;
using Microservice.Book.Api.Data.Repository.Interfaces;
using Microservice.Book.Api.Helpers;
using Microservice.Book.Api.Helpers.Exceptions;
using Microservice.Book.Api.MediatR.GetBook;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System.Reflection;

namespace Microservice.Book.Api.Test.Unit.Mediatr;

[TestFixture]
public class GetBookMediatrTests
{
    private readonly Mock<IBookRepository> bookRepositoryMock = new();
    private readonly Mock<ILogger<GetBookQueryHandler>> loggerMock = new();
    private readonly ServiceCollection services = new();
    private ServiceProvider serviceProvider;
    private IMediator mediator;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(GetBookQueryHandler).Assembly));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidatorBehavior<,>));
        services.AddScoped(sp => bookRepositoryMock.Object);
        services.AddScoped(sp => loggerMock.Object);
        services.AddAutoMapper(Assembly.GetAssembly(typeof(GetBookMapper)));

        serviceProvider = services.BuildServiceProvider();
        mediator = serviceProvider.GetRequiredService<IMediator>();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        services.Clear();
        serviceProvider.Dispose();
    }

    [Test]
    public async Task Get_book_successfully_return_book()
    {
        Guid id = new("07c06c3f-0897-44b6-ae05-a70540e73a12");

        Domain.Book? book = new()
        {
            Id = id,
            Title = "Infinity Son",
            ISBN = "9780063376120",
            AuthorId = new Guid("c95ba8ff-06a1-49d0-bc45-83f89b3ce820"),
            PublisherId = 3,
            SeriesId = 2,
            Summary = "Growing up in New York, brothers Emil and Brighton always idolized the Spell Walkers�a vigilante group sworn to rid the world of specters. While the Spell Walkers and other celestials are born with powers, specters take them, violently stealing the essence of endangered magical creatures.",
            Condition = "New",
            NumberInStock = 50,
            Price = 7.50m,
            Discount = null,
            DiscountTypeId = null,
            Created = DateTime.Now,
            LastUpdated = DateTime.Now
        };

        bookRepositoryMock
                .Setup(x => x.ByIdAsync(id))
                .Returns(Task.FromResult(book));

        var getBookRequest = new GetBookRequest(id);
        var actualResult = await mediator.Send(getBookRequest);

        Assert.Multiple(() =>
        {
            Assert.That(actualResult.Id, Is.EqualTo(id));
            Assert.That(actualResult.Title, Is.EqualTo("Infinity Son"));
        });
    }

    [Test]
    public void Get_book_return_exception()
    {
        var id = Guid.NewGuid();

        bookRepositoryMock
                .Setup(x => x.ByIdAsync(id));

        var getBookRequest = new GetBookRequest(id);

        var validationException = Assert.ThrowsAsync<NotFoundException>(async () =>
        {
            await mediator.Send(getBookRequest);
        });

        Assert.That(validationException.Message, Is.EqualTo(string.Format("Book not found.")));
    }
}