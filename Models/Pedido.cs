using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cp5Api.Models;

[Table("TB_CP5_PEDIDO")]
public class Pedido
{
    [Key]
    [Column("ID_PEDIDO")]
    public int Id { get; set; }

    [Column("ID_CLIENTE")]
    public int ClienteId { get; set; }

    public Cliente? Cliente { get; set; }

    [Column("DESCRICAO")]
    [Required]
    [MaxLength(200)]
    public string Descricao { get; set; } = string.Empty;

    [Column("VALOR")]
    public decimal Valor { get; set; }

    [Column("STATUS")]
    public StatusPedido Status { get; set; } = StatusPedido.Pendente;

    [Column("DATA_PEDIDO")]
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
}
