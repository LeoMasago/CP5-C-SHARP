using System.ComponentModel.DataAnnotations;
using Cp5Api.Models;

namespace Cp5Api.DTOs;

public record PedidoResponseDto(
    int Id,
    int ClienteId,
    string? ClienteNome,
    string Descricao,
    decimal Valor,
    StatusPedido Status,
    DateTime DataPedido);

public class PedidoRequestDto
{
    [Required(ErrorMessage = "O ClienteId é obrigatório.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [MaxLength(200, ErrorMessage = "A descrição deve ter no máximo 200 caracteres.")]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
    public decimal Valor { get; set; }

    public StatusPedido Status { get; set; } = StatusPedido.Pendente;
}
