using HomeApi.Integration;
using HomeApi.Models;
using MediatR;

namespace HomeApi.Handlers;

public static class DepartureBoard
{
    public record Command : IRequest<List<TimeTable>>;

    public class Handler : IRequestHandler<Command, List<TimeTable>>
    {
        private readonly IDepartureBoardService _departureBoardService;

        public Handler(IDepartureBoardService departureBoardService)
        {
            _departureBoardService = departureBoardService;
        }

        public async Task<List<TimeTable>> Handle(Command request, CancellationToken cancellationToken)
        {
            return await _departureBoardService.GetDepartureBoard() ?? new List<TimeTable>();
        }
    }
}