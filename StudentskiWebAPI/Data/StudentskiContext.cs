using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using StudentskiWebAPI.Models;

namespace StudentskiWebAPI.Data;

public partial class StudentskiContext : DbContext
{
    public StudentskiContext(DbContextOptions<StudentskiContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Ispit> Ispits { get; set; }

    public virtual DbSet<IspitniRok> IspitniRoks { get; set; }

    public virtual DbSet<Predmet> Predmets { get; set; }

    public virtual DbSet<PrijavaIspitum> PrijavaIspita { get; set; }

    public virtual DbSet<Profesor> Profesors { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentPredmet> StudentPredmets { get; set; }

    public virtual DbSet<Zapisnik> Zapisniks { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Serbian_Latin_100_CI_AI");

        modelBuilder.Entity<Ispit>(entity =>
        {
            entity.HasKey(e => e.IdIspita).HasName("PK__ispit__29C6AD7FD83115DE");

            entity.ToTable("ispit");

            entity.Property(e => e.IdIspita).HasColumnName("ID_ISPITA");
            entity.Property(e => e.Datum).HasColumnName("DATUM");
            entity.Property(e => e.IdPredmeta).HasColumnName("ID_PREDMETA");
            entity.Property(e => e.IdRoka).HasColumnName("ID_ROKA");
        });

        modelBuilder.Entity<IspitniRok>(entity =>
        {
            entity.HasKey(e => e.IdRoka).HasName("PK__ispitni___C7D0FE726B6F9A8F");

            entity.ToTable("ispitni_rok");

            entity.Property(e => e.IdRoka).HasColumnName("ID_ROKA");
            entity.Property(e => e.Naziv)
                .HasMaxLength(15)
                .HasColumnName("NAZIV");
            entity.Property(e => e.SkolskaGod)
                .HasMaxLength(7)
                .HasColumnName("SKOLSKA_GOD");
            entity.Property(e => e.StatusRoka)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("STATUS_ROKA");
        });

        modelBuilder.Entity<Predmet>(entity =>
        {
            entity.HasKey(e => e.IdPredmeta).HasName("predmet_pk");

            entity.ToTable("predmet");

            entity.Property(e => e.IdPredmeta)
                .ValueGeneratedNever()
                .HasColumnName("ID_PREDMETA");
            entity.Property(e => e.Espb).HasColumnName("ESPB");
            entity.Property(e => e.IdProfesora).HasColumnName("ID_PROFESORA");
            entity.Property(e => e.Naziv)
                .HasMaxLength(50)
                .HasColumnName("NAZIV");
            entity.Property(e => e.Status)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("STATUS");
        });

        modelBuilder.Entity<PrijavaIspitum>(entity =>
        {
            entity.HasIndex(e => e.IdPredmeta, "IX_PrijavaIspita_ID_PREDMETA");

            entity.HasIndex(e => e.IdRoka, "IX_PrijavaIspita_ID_ROKA");

            entity.HasIndex(e => e.IdStudenta, "IX_PrijavaIspita_ID_STUDENTA");

            entity.Property(e => e.DatumPrijave).HasColumnName("DATUM_PRIJAVE");
            entity.Property(e => e.IdPredmeta).HasColumnName("ID_PREDMETA");
            entity.Property(e => e.IdRoka).HasColumnName("ID_ROKA");
            entity.Property(e => e.IdStudenta).HasColumnName("ID_STUDENTA");

            entity.HasOne(d => d.IdPredmetaNavigation).WithMany(p => p.PrijavaIspita).HasForeignKey(d => d.IdPredmeta);

            entity.HasOne(d => d.IdRokaNavigation).WithMany(p => p.PrijavaIspita).HasForeignKey(d => d.IdRoka);

            entity.HasOne(d => d.IdStudentaNavigation).WithMany(p => p.PrijavaIspita).HasForeignKey(d => d.IdStudenta);
        });

        modelBuilder.Entity<Profesor>(entity =>
        {
            entity.HasKey(e => e.IdProfesora).HasName("profesor_pk");

            entity.ToTable("profesor");

            entity.Property(e => e.IdProfesora)
                .ValueGeneratedNever()
                .HasColumnName("ID_PROFESORA");
            entity.Property(e => e.DatumZap).HasColumnName("DATUM_ZAP");
            entity.Property(e => e.Ime)
                .HasMaxLength(25)
                .HasColumnName("IME");
            entity.Property(e => e.Prezime)
                .HasMaxLength(50)
                .HasColumnName("PREZIME");
            entity.Property(e => e.Zvanje)
                .HasMaxLength(10)
                .IsFixedLength()
                .HasColumnName("ZVANJE");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.IdStudenta).HasName("PK__student__0FD28978D84A9F32");

            entity.ToTable("student");

            entity.Property(e => e.IdStudenta).HasColumnName("ID_STUDENTA");
            entity.Property(e => e.Broj).HasColumnName("BROJ");
            entity.Property(e => e.GodinaUpisa)
                .HasMaxLength(4)
                .HasColumnName("GODINA_UPISA");
            entity.Property(e => e.Ime)
                .HasMaxLength(25)
                .HasColumnName("IME");
            entity.Property(e => e.Prezime)
                .HasMaxLength(40)
                .HasColumnName("PREZIME");
            entity.Property(e => e.Smer)
                .HasMaxLength(5)
                .HasColumnName("SMER");
        });

        modelBuilder.Entity<StudentPredmet>(entity =>
        {
            entity.HasKey(e => new { e.IdStudenta, e.IdPredmeta, e.SkolskaGodina });

            entity.ToTable("student_predmet");

            entity.Property(e => e.IdStudenta).HasColumnName("ID_STUDENTA");
            entity.Property(e => e.IdPredmeta).HasColumnName("ID_PREDMETA");
            entity.Property(e => e.SkolskaGodina)
                .HasMaxLength(7)
                .HasColumnName("SKOLSKA_GODINA");
        });

        modelBuilder.Entity<Zapisnik>(entity =>
        {
            entity.HasKey(e => new { e.IdIspita, e.IdStudenta }).HasName("zapisnik_pk");

            entity.ToTable("zapisnik");

            entity.Property(e => e.IdIspita).HasColumnName("ID_ISPITA");
            entity.Property(e => e.IdStudenta).HasColumnName("ID_STUDENTA");
            entity.Property(e => e.Bodovi)
                .HasMaxLength(3)
                .HasColumnName("BODOVI");
            entity.Property(e => e.Ocena).HasColumnName("OCENA");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

   
}
