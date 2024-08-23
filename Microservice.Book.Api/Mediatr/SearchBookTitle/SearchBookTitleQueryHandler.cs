using AutoMapper;
using MediatR;
using Microservice.Book.Api.Data.Repository.Interfaces;
using Microservice.Book.Api.Helpers.Exceptions;

namespace Microservice.Book.Api.MediatR.SearchBookTitle;

public class SearchBookTitleQueryHandler(IBookRepository bookRepository, IMapper mapper) : IRequestHandler<SearchBookTitleRequest, SearchBookTitleResponse>
{
    private IBookRepository _bookRepository { get; set; } = bookRepository;
    private IMapper _mapper { get; set; } = mapper;

    public async Task<SearchBookTitleResponse> Handle(SearchBookTitleRequest request, CancellationToken cancellationToken)
    {
        var books = await _bookRepository.SearchTitleAsync(request.Criteria);

        if (books == null)
            throw new NotFoundException($"Books not found for id - '{request.Criteria}'");

        return _mapper.Map<SearchBookTitleResponse>(books);
    }
}