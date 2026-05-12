using JewelryFactory.Application.Features.Bom.DTOs;
using MediatR;

namespace JewelryFactory.Application.Features.Bom.Commands.UpdateBomTemplate;

public record UpdateBomTemplateCommand(Guid Id, UpdateBomTemplateDto Request) : IRequest;
