using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cp5Api.Models;

[Table("TB_CP5_CLIENTE")]
public class Cliente
{
    [Key]
    [Column("ID_CLIENTE")]
    public int Id { get; set; }

    [Column("NOME")]
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Column("EMAIL")]
    [Required]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [Column("TELEFONE")]
    [MaxLength(20)]
    public string? Telefone { get; set; }

    [Column("DATA_CADASTRO")]
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
