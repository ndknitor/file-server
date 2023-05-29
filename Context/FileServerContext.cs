﻿using System;
using System.Collections.Generic;
using FileServer.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace FileServer.Context;

public partial class FileServerContext : DbContext
{
    public FileServerContext(DbContextOptions<FileServerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AppFile> AppFile { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<AppFile>(entity =>
        {
            entity.HasKey(e => e.Name).HasName("PRIMARY");

            entity.Property(e => e.Name)
                .HasMaxLength(256)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
            entity.Property(e => e.Node)
                .IsRequired()
                .HasMaxLength(16)
                .UseCollation("utf8mb3_general_ci")
                .HasCharSet("utf8mb3");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}