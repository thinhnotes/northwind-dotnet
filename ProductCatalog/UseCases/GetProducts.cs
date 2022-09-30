using ProductCatalog.Domain;
using ProductCatalog.Domain.Specs;

namespace ProductCatalog.UseCases;

public struct GetProducts
{
    public readonly record struct Query : IListQuery
    {
        public List<string> Includes { get; init; }
        public List<FilterModel> Filters { get; init; }
        public List<string> Sorts { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }

        public Query()
        {
            Includes = new(new[] { "SupplierInfo", "Category" });
            Filters = new();
            Sorts = new();
            Page = 1;
            PageSize = 20;
        }

        internal class Validator : AbstractValidator<Query>
        {
            public Validator()
            {
                RuleFor(x => x.Page)
                    .GreaterThanOrEqualTo(1).WithMessage("Page should at least greater than or equal to 1.");

                RuleFor(x => x.PageSize)
                    .GreaterThanOrEqualTo(1).WithMessage("PageSize should at least greater than or equal to 1.");
            }
        }

        internal class Handler : IRequestHandler<Query, IResult>
        {
            private readonly IGridRepository<Product> _productRepository;

            public Handler(IGridRepository<Product> productRepository)
            {
                _productRepository = productRepository;
            }

            public async Task<IResult> Handle(Query request, CancellationToken cancellationToken)
            {
                var spec = new EntityListQuerySpec<Product>(request);

                var products = await _productRepository.FindAsync(spec, cancellationToken);

                var productModels = products.Select(x =>
                    new ProductDto(x.Id, x.Name, x.Discontinued));

                var totalProducts = await _productRepository.CountAsync(spec, cancellationToken);

                var resultModel = ListResultModel<ProductDto>.Create(
                    productModels.ToList(), totalProducts, request.Page, request.PageSize);

                return Results.Ok(ResultModel<ListResultModel<ProductDto>>.Create(resultModel));
            }
        }
    }
}
