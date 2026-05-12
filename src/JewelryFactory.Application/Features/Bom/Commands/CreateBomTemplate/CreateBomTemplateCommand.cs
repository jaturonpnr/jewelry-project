using JewelryFactory.Application.Features.Bom.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Bom.Commands.CreateBomTemplate;

public record CreateBomTemplateCommand(CreateBomTemplateDto Request) : IRequest<Guid>;
