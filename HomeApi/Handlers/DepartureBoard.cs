using HomeApi.Integration;
using HomeApi.Models;
using MediatR;

namespace HomeApi.Handlers;

public static class DepartureBoard
{
    public record Command : IRequest<List<TimeTable>>;

    public class Handler(IDepartureBoardService departureBoardService) : IRequestHandler<Command, List<TimeTable>>
    {
        public async Task<List<TimeTable>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await departureBoardService.GetDepartureBoard() ?? new List<TimeTable>();
        }
    }
}