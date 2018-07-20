﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Recollectable.Data;

namespace Recollectable.Data.Migrations
{
    [DbContext(typeof(RecollectableContext))]
    partial class RecollectableContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Recollectable.Domain.Collectable", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CollectionId");

                    b.Property<Guid>("CountryId");

                    b.Property<string>("Description");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("CollectionId");

                    b.HasIndex("CountryId");

                    b.ToTable("Collectables");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Collectable");
                });

            modelBuilder.Entity("Recollectable.Domain.CollectableCondition", b =>
                {
                    b.Property<Guid>("ConditionId");

                    b.Property<Guid>("CollectableId");

                    b.Property<Guid>("CollectionId");

                    b.HasKey("ConditionId", "CollectableId", "CollectionId");

                    b.HasIndex("CollectableId");

                    b.HasIndex("CollectionId");

                    b.ToTable("CollectableCondition");
                });

            modelBuilder.Entity("Recollectable.Domain.Collection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("OwnerId");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId")
                        .IsUnique();

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("Recollectable.Domain.CollectorValue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double>("AU50Value");

                    b.Property<Guid>("CurrencyId");

                    b.Property<double>("F12Value");

                    b.Property<double>("G4Value");

                    b.Property<double>("MS60Value");

                    b.Property<double>("MS65Value");

                    b.Property<double>("PF63Value");

                    b.Property<double>("PF65Value");

                    b.Property<double>("VF20Value");

                    b.Property<double>("VG8Value");

                    b.Property<double>("XF40Value");

                    b.HasKey("Id");

                    b.HasIndex("CurrencyId")
                        .IsUnique();

                    b.ToTable("CollectorValues");
                });

            modelBuilder.Entity("Recollectable.Domain.Condition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Grade");

                    b.HasKey("Id");

                    b.ToTable("Condition");
                });

            modelBuilder.Entity("Recollectable.Domain.Country", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("Recollectable.Domain.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email");

                    b.Property<string>("FirstName");

                    b.Property<string>("LastName");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Recollectable.Domain.Currency", b =>
                {
                    b.HasBaseType("Recollectable.Domain.Collectable");

                    b.Property<string>("BackImagePath");

                    b.Property<int>("FaceValue");

                    b.Property<string>("FrontImagePath");

                    b.Property<string>("ObverseDescription");

                    b.Property<int>("ReleaseDate");

                    b.Property<string>("ReverseDescription");

                    b.Property<string>("Size");

                    b.Property<string>("Type");

                    b.ToTable("Currency");

                    b.HasDiscriminator().HasValue("Currency");
                });

            modelBuilder.Entity("Recollectable.Domain.Collectable", b =>
                {
                    b.HasOne("Recollectable.Domain.Collection", "Collection")
                        .WithMany("Collectables")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recollectable.Domain.Country", "Country")
                        .WithMany("Collectables")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recollectable.Domain.CollectableCondition", b =>
                {
                    b.HasOne("Recollectable.Domain.Collectable", "Collectable")
                        .WithMany("CollectableConditions")
                        .HasForeignKey("CollectableId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recollectable.Domain.Collection", "Collection")
                        .WithMany("CollectableConditions")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recollectable.Domain.Condition", "condition")
                        .WithMany("CollectableConditions")
                        .HasForeignKey("ConditionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recollectable.Domain.Collection", b =>
                {
                    b.HasOne("Recollectable.Domain.User", "Owner")
                        .WithOne("Collection")
                        .HasForeignKey("Recollectable.Domain.Collection", "OwnerId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recollectable.Domain.CollectorValue", b =>
                {
                    b.HasOne("Recollectable.Domain.Currency", "Currency")
                        .WithOne("CollectorValue")
                        .HasForeignKey("Recollectable.Domain.CollectorValue", "CurrencyId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}
