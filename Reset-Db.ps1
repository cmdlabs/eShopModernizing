# This drops the database created by the eShop sample applications
# Helpful when switching between migrated variants that use EF vs EFCore, as the Identity mechanism for the CatalogItems table is different between the two.
sqlcmd -S "(localdb)\MSSQLLocalDB" -Q "DROP DATABASE [Microsoft.eShopOnContainers.Services.CatalogDb]"