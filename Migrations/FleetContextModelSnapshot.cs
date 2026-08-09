using System;
using FleetCarePro.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace FleetCarePro.Migrations
{
    [DbContext(typeof(FleetContext))]
    partial class FleetContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618

            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.11")
                .HasAnnotation(
                    "Relational:MaxIdentifierLength",
                    128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            // باقي الـ Model Snapshot يتم توليده تلقائيًا
            // بواسطة EF Core بناءً على FleetContext.

#pragma warning restore 612, 618
        }
    }
}