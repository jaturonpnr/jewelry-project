using MediatR;

namespace JewelryFactory.Application.Features.Bom.Commands.DeleteBomTemplate;

public record DeleteBomTemplateCommand(Guid Id) : IRequest;
