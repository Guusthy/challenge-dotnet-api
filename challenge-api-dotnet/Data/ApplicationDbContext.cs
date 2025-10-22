using challenge_api_dotnet.Models;
using Microsoft.EntityFrameworkCore;

namespace challenge_api_dotnet.Data;

public partial class ApplicationDbContext : DbContext
{
    public ApplicationDbContext()
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public virtual DbSet<Moto> Motos { get; set; }
    public virtual DbSet<Patio> Patios { get; set; }
    public virtual DbSet<Usuario> Usuarios { get; set; }
    public virtual DbSet<MarcadorFixo> MarcadoresFixos { get; set; }
    public virtual DbSet<MarcadorArucoMovel> MarcadoresArucoMoveis { get; set; }
    public virtual DbSet<Posicao> Posicoes { get; set; }
    public virtual DbSet<MedicaoPosicao> MedicoesPosicoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("RM556934");

        modelBuilder.HasSequence<int>("SEQ_PATIO_CH", "RM556934").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("SEQ_USUARIO_CH", "RM556934").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("SEQ_MOTO_CH", "RM556934").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("SEQ_POS_CH", "RM556934").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("SEQ_MARCADOR_F_CH", "RM556934").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("SEQ_MARCADOR_M_CH", "RM556934").StartsAt(1).IncrementsBy(1);
        modelBuilder.HasSequence<int>("SEQ_MEDICAO_CH", "RM556934").StartsAt(1).IncrementsBy(1);

        // Tabela: MOTO
        modelBuilder.Entity<Moto>(entity =>
        {
            entity.ToTable("MOTO");
            entity.HasKey(e => e.IdMoto);
            entity.HasIndex(e => e.Placa)
                .IsUnique()
                .HasFilter(null);

            entity.Property(e => e.IdMoto)
                .HasColumnName("ID_MOTO")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_MOTO_CH\".NEXTVAL");
            entity.Property(e => e.Placa).HasColumnName("PLACA").HasMaxLength(7).IsUnicode(false);
            entity.Property(e => e.Modelo).HasColumnName("MODELO").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(65).IsUnicode(false);
            entity.Property(e => e.DataCadastro)
                .HasColumnName("DATA_CADASTRO")
                .HasColumnType("DATE");
        });

        // Tabela: PATIO
        modelBuilder.Entity<Patio>(entity =>
        {
            entity.ToTable("PATIO");
            entity.HasKey(e => e.IdPatio);

            entity.Property(e => e.IdPatio)
                .HasColumnName("ID_PATIO")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_PATIO_CH\".NEXTVAL");
            entity.Property(e => e.Nome).HasColumnName("NOME").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Localizacao).HasColumnName("LOCALIZACAO").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Descricao).HasColumnName("DESCRICAO").HasMaxLength(255).IsUnicode(false);
        });

        // Tabela: USUARIO
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");
            entity.HasKey(e => e.IdUsuario);
            entity.HasIndex(e => e.Email).IsUnique();

            entity.Property(e => e.IdUsuario)
                .HasColumnName("ID_USUARIO")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_USUARIO_CH\".NEXTVAL");
            entity.Property(e => e.Nome).HasColumnName("NOME").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Email).HasColumnName("EMAIL").HasMaxLength(100).IsUnicode(false);
            entity.Property(e => e.Senha).HasColumnName("SENHA").HasMaxLength(255).IsUnicode(false);
            entity.Property(e => e.Status).HasColumnName("STATUS").HasMaxLength(20).IsUnicode(false)
                .HasDefaultValueSql("'ativo'");
            entity.Property(e => e.Tipo).HasColumnName("TIPO").HasMaxLength(20).IsUnicode(false);
            entity.Property(e => e.PatioIdPatio).HasColumnName("PATIO_ID_PATIO").HasColumnType("NUMBER(10)");

            // FK: Usuario → Patio
            entity.HasOne(d => d.PatioIdPatioNavigation)
                .WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.PatioIdPatio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("USUARIO_PATIO_FK");

            entity.HasCheckConstraint("USUARIO_TIPO_CHK", "\"TIPO\" IN (''USER'',''ADMIN'')");
        });

        // Tabela: MARCADOR_FIXO
        modelBuilder.Entity<MarcadorFixo>(entity =>
        {
            entity.ToTable("MARCADOR_FIXO");
            entity.HasKey(e => e.IdMarcadorArucoFixo);

            entity.Property(e => e.IdMarcadorArucoFixo)
                .HasColumnName("ID_MARCADOR_ARUCO_FIXO")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_MARCADOR_F_CH\".NEXTVAL");
            entity.Property(e => e.CodigoAruco).HasColumnName("CODIGO_ARUCO").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.XPos).HasColumnName("X_POS").HasColumnType("FLOAT(10)");
            entity.Property(e => e.YPos).HasColumnName("Y_POS").HasColumnType("FLOAT(10)");
            entity.Property(e => e.PatioIdPatio).HasColumnName("PATIO_ID_PATIO").HasColumnType("NUMBER(10)");

            // FK: MarcadorFixo → Patio
            entity.HasOne(d => d.PatioIdPatioNavigation)
                .WithMany(p => p.MarcadoresFixos)
                .HasForeignKey(d => d.PatioIdPatio)
                .HasConstraintName("SYS_C004862167");
        });

        // Tabela: MARCADOR_ARUCO_MOVEL
        modelBuilder.Entity<MarcadorArucoMovel>(entity =>
        {
            entity.ToTable("MARCADOR_ARUCO_MOVEL");
            entity.HasKey(e => e.IdMarcadorMovel);

            entity.Property(e => e.IdMarcadorMovel)
                .HasColumnName("ID_MARCADOR_MOVEL")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_MARCADOR_M_CH\".NEXTVAL");
            entity.Property(e => e.CodigoAruco).HasColumnName("CODIGO_ARUCO").HasMaxLength(50).IsUnicode(false);
            entity.Property(e => e.DataInstalacao).HasColumnName("DATA_INSTALACAO").HasColumnType("DATE");
            entity.Property(e => e.MotoIdMoto).HasColumnName("MOTO_ID_MOTO").HasColumnType("NUMBER(10)");

            // FK: MarcadorMovel → Moto
            entity.HasOne(d => d.MotoIdMotoNavigation)
                .WithMany(p => p.MarcadoresArucoMoveis)
                .HasForeignKey(d => d.MotoIdMoto)
                .HasConstraintName("SYS_C004862169");
        });

        // Tabela: POSICAO
        modelBuilder.Entity<Posicao>(entity =>
        {
            entity.ToTable("POSICAO");
            entity.HasKey(e => e.IdPosicao);

            entity.Property(e => e.IdPosicao)
                .HasColumnName("ID_POSICAO")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_POS_CH\".NEXTVAL");
            entity.Property(e => e.DataHora).HasColumnName("DATA_HORA").HasColumnType("DATE");
            entity.Property(e => e.XPos).HasColumnName("X_POS").HasColumnType("FLOAT(10)");
            entity.Property(e => e.YPos).HasColumnName("Y_POS").HasColumnType("FLOAT(10)");
            entity.Property(e => e.MotoIdMoto).HasColumnName("MOTO_ID_MOTO").HasColumnType("NUMBER(10)");
            entity.Property(e => e.PatioIdPatio).HasColumnName("PATIO_ID_PATIO").HasColumnType("NUMBER(10)");

            // FK: Posicao → Moto
            entity.HasOne(d => d.MotoIdMotoNavigation)
                .WithMany(p => p.Posicoes)
                .HasForeignKey(d => d.MotoIdMoto)
                .HasConstraintName("SYS_C004862164");

            // FK: Posicao → Patio
            entity.HasOne(d => d.PatioIdPatioNavigation)
                .WithMany(p => p.Posicoes)
                .HasForeignKey(d => d.PatioIdPatio)
                .HasConstraintName("SYS_C004862165");
        });

        // Tabela: MEDICAO_POSICAO
        modelBuilder.Entity<MedicaoPosicao>(entity =>
        {
            entity.ToTable("MEDICAO_POSICAO");
            entity.HasKey(e => e.IdMedicao);

            entity.Property(e => e.IdMedicao)
                .HasColumnName("ID_MEDICAO")
                .HasColumnType("NUMBER(10)")
                .ValueGeneratedOnAdd()
                .HasDefaultValueSql("\"RM556934\".\"SEQ_MEDICAO_CH\".NEXTVAL");
            entity.Property(e => e.DistanciaM).HasColumnName("DISTANCIA_M").HasColumnType("FLOAT(10)");
            entity.Property(e => e.PosicaoIdPosicao).HasColumnName("POSICAO_ID_POSICAO").HasColumnType("NUMBER(10)");
            entity.Property(e => e.MarcadorFixoIdMarcadorArucoFixo)
                .HasColumnName("MARCADOR_FIXO_ID_MARCADOR_ARUCO_FIXO").HasColumnType("NUMBER(10)");

            // FK: Medicao → Posicao
            entity.HasOne(d => d.PosicaoIdPosicaoNavigation)
                .WithMany(p => p.MedicoesPosicoes)
                .HasForeignKey(d => d.PosicaoIdPosicao)
                .HasConstraintName("SYS_C004862171");

            // FK: Medicao → MarcadorFixo
            entity.HasOne(d => d.MarcadorFixoIdMarcadorArucoFixoNavigation)
                .WithMany(p => p.MedicoesPosicoes)
                .HasForeignKey(d => d.MarcadorFixoIdMarcadorArucoFixo)
                .HasConstraintName("SYS_C004862172");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
