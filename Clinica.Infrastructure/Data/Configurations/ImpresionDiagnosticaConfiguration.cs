using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infrastructure.Data.Configurations;

public class ImpresionDiagnosticaConfiguration : IEntityTypeConfiguration<ImpresionDiagnostica>
{
    public void Configure(EntityTypeBuilder<ImpresionDiagnostica> builder)
    {
        builder.ToTable("ImpresionesDiagnosticas");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DiagnosticoPrincipal).IsRequired().HasMaxLength(500);
        builder.Property(x => x.DiagnosticosSecundarios).HasMaxLength(1000);
        
        // La receta puede ser muy larga, le damos más capacidad
        builder.Property(x => x.IndicacionesReceta).HasMaxLength(2500); 
        builder.Property(x => x.MotivoProximaCita).HasMaxLength(250);
    }
}