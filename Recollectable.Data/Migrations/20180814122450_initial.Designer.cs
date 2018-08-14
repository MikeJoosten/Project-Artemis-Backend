﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Recollectable.Data;

namespace Recollectable.Data.Migrations
{
    [DbContext(typeof(RecollectableContext))]
    [Migration("20180814122450_initial")]
    partial class initial
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.1.1-rtm-30846")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("Recollectable.Domain.Entities.Collectable", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CollectorValueId");

                    b.Property<Guid>("CountryId");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("ReleaseDate")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.HasIndex("CollectorValueId");

                    b.HasIndex("CountryId");

                    b.ToTable("Collectables");

                    b.HasDiscriminator<string>("Discriminator").HasValue("Collectable");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Collection", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(25);

                    b.Property<Guid>("UserId");

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("Collections");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.CollectionCollectable", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("CollectableId");

                    b.Property<Guid>("CollectionId");

                    b.Property<Guid>("ConditionId");

                    b.HasKey("Id");

                    b.HasIndex("CollectableId");

                    b.HasIndex("CollectionId");

                    b.HasIndex("ConditionId");

                    b.ToTable("CollectionCollectables");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.CollectorValue", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<double?>("AU50");

                    b.Property<double?>("F12");

                    b.Property<double?>("G4");

                    b.Property<double?>("MS60");

                    b.Property<double?>("MS63");

                    b.Property<double?>("PF60");

                    b.Property<double?>("PF63");

                    b.Property<double?>("PF65");

                    b.Property<double?>("VF20");

                    b.Property<double?>("VG8");

                    b.Property<double?>("XF40");

                    b.HasKey("Id");

                    b.ToTable("CollectorValues");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Condition", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Grade")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("Conditions");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Country", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Description");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(50);

                    b.HasKey("Id");

                    b.ToTable("Countries");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.User", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(250);

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Currency", b =>
                {
                    b.HasBaseType("Recollectable.Domain.Entities.Collectable");

                    b.Property<string>("BackImagePath")
                        .HasMaxLength(250);

                    b.Property<string>("Designer")
                        .HasMaxLength(250);

                    b.Property<int>("FaceValue");

                    b.Property<string>("FrontImagePath")
                        .HasMaxLength(250);

                    b.Property<string>("HeadOfState")
                        .HasMaxLength(250);

                    b.Property<string>("ObverseDescription")
                        .HasMaxLength(250);

                    b.Property<string>("ReverseDescription")
                        .HasMaxLength(250);

                    b.Property<string>("Size")
                        .HasMaxLength(25);

                    b.Property<string>("Type")
                        .IsRequired()
                        .HasMaxLength(100);

                    b.ToTable("Currency");

                    b.HasDiscriminator().HasValue("Currency");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Banknote", b =>
                {
                    b.HasBaseType("Recollectable.Domain.Entities.Currency");

                    b.Property<string>("Color")
                        .HasMaxLength(250);

                    b.Property<string>("Signature")
                        .HasMaxLength(250);

                    b.Property<string>("Watermark")
                        .HasMaxLength(250);

                    b.ToTable("Banknote");

                    b.HasDiscriminator().HasValue("Banknote");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Coin", b =>
                {
                    b.HasBaseType("Recollectable.Domain.Entities.Currency");

                    b.Property<string>("EdgeLegend")
                        .HasMaxLength(100);

                    b.Property<string>("EdgeType")
                        .HasMaxLength(50);

                    b.Property<string>("Metal")
                        .HasMaxLength(50);

                    b.Property<string>("MintMark")
                        .HasMaxLength(50);

                    b.Property<int>("Mintage");

                    b.Property<string>("Note")
                        .HasMaxLength(250);

                    b.Property<string>("ObverseInscription")
                        .HasMaxLength(100);

                    b.Property<string>("ObverseLegend")
                        .HasMaxLength(100);

                    b.Property<string>("ReverseInscription")
                        .HasMaxLength(100);

                    b.Property<string>("ReverseLegend")
                        .HasMaxLength(100);

                    b.Property<string>("Subject")
                        .HasMaxLength(250);

                    b.Property<string>("Weight")
                        .HasMaxLength(25);

                    b.ToTable("Coin");

                    b.HasDiscriminator().HasValue("Coin");
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Collectable", b =>
                {
                    b.HasOne("Recollectable.Domain.Entities.CollectorValue", "CollectorValue")
                        .WithMany("Collectables")
                        .HasForeignKey("CollectorValueId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recollectable.Domain.Entities.Country", "Country")
                        .WithMany("Collectables")
                        .HasForeignKey("CountryId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.Collection", b =>
                {
                    b.HasOne("Recollectable.Domain.Entities.User", "User")
                        .WithMany("Collections")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Recollectable.Domain.Entities.CollectionCollectable", b =>
                {
                    b.HasOne("Recollectable.Domain.Entities.Collectable", "Collectable")
                        .WithMany("CollectionCollectables")
                        .HasForeignKey("CollectableId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recollectable.Domain.Entities.Collection", "Collection")
                        .WithMany("CollectionCollectables")
                        .HasForeignKey("CollectionId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Recollectable.Domain.Entities.Condition", "Condition")
                        .WithMany("CollectionCollectables")
                        .HasForeignKey("ConditionId")
                        .OnDelete(DeleteBehavior.Cascade);
                });
#pragma warning restore 612, 618
        }
    }
}