using Cp5Api.Data;
using Cp5Api.DTOs;
using Cp5Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cp5Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly AppDbContext _context;

    public ClientesController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>Lista todos os clientes cadastrados.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ClienteResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ClienteResponseDto>>> GetClientes()
    {
        var clientes = await _context.Clientes
            .OrderBy(c => c.Id)
            .ToListAsync();

        return Ok(clientes.Select(ToResponseDto));
    }

    /// <summary>Busca um cliente pelo id.</summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteResponseDto>> GetCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);

        if (cliente is null)
            return NotFound(new { mensagem = $"Cliente com id {id} não encontrado." });

        return Ok(ToResponseDto(cliente));
    }

    /// <summary>Lista os pedidos de um cliente específico.</summary>
    [HttpGet("{id:int}/pedidos")]
    [ProducesResponseType(typeof(IEnumerable<PedidoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<PedidoResponseDto>>> GetPedidosDoCliente(int id)
    {
        var clienteExiste = await _context.Clientes.CountAsync(c => c.Id == id) > 0;
        if (!clienteExiste)
            return NotFound(new { mensagem = $"Cliente com id {id} não encontrado." });

        var pedidos = await _context.Pedidos
            .Include(p => p.Cliente)
            .Where(p => p.ClienteId == id)
            .OrderBy(p => p.Id)
            .ToListAsync();

        return Ok(pedidos.Select(PedidosController.ToResponseDto));
    }

    /// <summary>Cadastra um novo cliente.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClienteResponseDto>> CriarCliente(ClienteRequestDto dto)
    {
        var emailEmUso = await _context.Clientes.CountAsync(c => c.Email == dto.Email) > 0;
        if (emailEmUso)
            return BadRequest(new { mensagem = "Já existe um cliente cadastrado com este e-mail." });

        var cliente = new Cliente
        {
            Nome = dto.Nome,
            Email = dto.Email,
            Telefone = dto.Telefone,
            DataCadastro = DateTime.UtcNow
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, ToResponseDto(cliente));
    }

    /// <summary>Atualiza os dados de um cliente existente.</summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarCliente(int id, ClienteRequestDto dto)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound(new { mensagem = $"Cliente com id {id} não encontrado." });

        var emailEmUsoPorOutro = await _context.Clientes
            .CountAsync(c => c.Email == dto.Email && c.Id != id) > 0;
        if (emailEmUsoPorOutro)
            return BadRequest(new { mensagem = "Já existe um cliente cadastrado com este e-mail." });

        cliente.Nome = dto.Nome;
        cliente.Email = dto.Email;
        cliente.Telefone = dto.Telefone;

        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>Remove um cliente cadastrado.</summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExcluirCliente(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente is null)
            return NotFound(new { mensagem = $"Cliente com id {id} não encontrado." });

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    internal static ClienteResponseDto ToResponseDto(Cliente c) =>
        new(c.Id, c.Nome, c.Email, c.Telefone, c.DataCadastro);
}
