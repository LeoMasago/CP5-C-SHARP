using Cp5Api.Data;
using Cp5Api.DTOs;
using Cp5Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cp5Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly AppDbContext _context;

    public PedidosController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todos os pedidos, com filtro opcional por status.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PedidoResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<PedidoResponseDto>>> GetPedidos([FromQuery] StatusPedido? status)
    {
        var query = _context.Pedidos.Include(p => p.Cliente).AsQueryable();

        if (status.HasValue)
            query = query.Where(p => p.Status == status.Value);

        var pedidos = await query.OrderBy(p => p.Id).ToListAsync();

        return Ok(pedidos.Select(ToResponseDto));
    }

    /// <summary>Busca um pedido pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PedidoResponseDto>> GetPedido(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pedido is null)
            return NotFound(new { mensagem = $"Pedido com id {id} não encontrado." });

        return Ok(ToResponseDto(pedido));
    }

    /// <summary>Cria um novo pedido para um cliente existente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(PedidoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PedidoResponseDto>> CriarPedido(PedidoRequestDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
        if (cliente is null)
            return BadRequest(new { mensagem = $"Cliente com id {dto.ClienteId} não existe." });

        var pedido = new Pedido
        {
            ClienteId = dto.ClienteId,
            Cliente = cliente,
            Descricao = dto.Descricao,
            Valor = dto.Valor,
            Status = dto.Status,
            DataPedido = DateTime.UtcNow
        };

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPedido), new { id = pedido.Id }, ToResponseDto(pedido));
    }

    /// <summary>Atualiza um pedido existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarPedido(int id, PedidoRequestDto dto)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido is null)
            return NotFound(new { mensagem = $"Pedido com id {id} não encontrado." });

        var clienteExiste = await _context.Clientes.CountAsync(c => c.Id == dto.ClienteId) > 0;
        if (!clienteExiste)
            return BadRequest(new { mensagem = $"Cliente com id {dto.ClienteId} não existe." });

        pedido.ClienteId = dto.ClienteId;
        pedido.Descricao = dto.Descricao;
        pedido.Valor = dto.Valor;
        pedido.Status = dto.Status;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Remove um pedido.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirPedido(int id)
    {
        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido is null)
            return NotFound(new { mensagem = $"Pedido com id {id} não encontrado." });

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    internal static PedidoResponseDto ToResponseDto(Pedido p) =>
        new(p.Id, p.ClienteId, p.Cliente?.Nome, p.Descricao, p.Valor, p.Status, p.DataPedido);
}
