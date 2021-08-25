using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace eShopLegacyMVC.Models.Configurations
{
    public class CatalogItemConfiguration : IEntityTypeConfiguration<CatalogItem>
    {
        public void Configure(EntityTypeBuilder<CatalogItem> builder)
        {
            builder.ToTable("Catalog");
            builder.HasKey(ci => ci.Id);
            builder.Property(ci => ci.Id).ValueGeneratedNever().IsRequired();
            builder.Property(ci => ci.Name).IsRequired().HasMaxLength(50);
            builder.Property(ci => ci.Price).IsRequired();
            builder.Property(ci => ci.PictureFileName).IsRequired();
            builder.Ignore(ci => ci.PictureUri);
            builder.HasOne<CatalogBrand>(ci => ci.CatalogBrand).WithMany().HasForeignKey(ci => ci.CatalogBrandId);
            builder.HasOne<CatalogType>(ci => ci.CatalogType).WithMany().HasForeignKey(ci => ci.CatalogTypeId);
        }
    }
}
