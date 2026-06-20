using Clinica.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinica.Infrastructure.Data.Configurations;

public class TactoVaginalConfiguration : IEntityTypeConfiguration<TactoVaginal>
{
    public void Configure(EntityTypeBuilder<TactoVaginal> builder)
    {
        builder.ToTable("TactosVaginales");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.AlturaPresentacion).HasMaxLength(50);
        builder.Property(x => x.MembranasOvulares).HasMaxLength(100);
        builder.Property(x => x.ColorLiquido).HasMaxLength(50);
        builder.Property(x => x.Pelvis).HasMaxLength(50);
        builder.Property(x => x.VariedadPresentacion).HasMaxLength(100);
    }
}