using CaWorkshop.Application.Common.Exceptions;
using CaWorkshop.Application.Common.Interfaces;
using CaWorkshop.Domain.Entities;

using MediatR;

namespace CaWorkshop.Application.TodoItems.Commands.DeleteTodoItem;

public class DeleteTodoItemCommand : IRequest
{
    public int Id { get; set; }
}

public class DeleteTodoItemCommandHandler
    : AsyncRequestHandler<DeleteTodoItemCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteTodoItemCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task Handle(DeleteTodoItemCommand request,
        CancellationToken cancellationToken)
    {
        var entity = await _context.TodoItems
            .FindAsync(new object[] { request.Id }, cancellationToken);

        if (entity == null)
        {
            throw new NotFoundException(nameof(TodoItem), request.Id);
        }

        _context.TodoItems.Remove(entity);

        await _context.SaveChangesAsync(cancellationToken);
    }
}