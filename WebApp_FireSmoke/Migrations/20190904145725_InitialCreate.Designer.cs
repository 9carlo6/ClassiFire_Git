﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using WebApp_FireSmoke.Models;

namespace WebApp_FireSmoke.Migrations
{
    [DbContext(typeof(WebAppContext))]
    [Migration("20190904145725_InitialCreate")]
    partial class InitialCreate
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "2.2.6-servicing-10079")
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("WebApp_FireSmoke.Models.ClassifiedImage", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Date");

                    b.Property<string>("FileType");

                    b.Property<double>("FireScoreClassification");

                    b.Property<string>("FireTypeClassification");

                    b.Property<string>("GeoJSON");

                    b.Property<string>("GeoPolygon");

                    b.Property<string>("Geolocalization");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<byte[]>("Photo");

                    b.Property<string>("PhotoName");

                    b.Property<double>("SmokeScoreClassification");

                    b.Property<string>("SmokeTypeClassification");

                    b.HasKey("Id");

                    b.ToTable("ClassifiedImages");
                });
#pragma warning restore 612, 618
        }
    }
}