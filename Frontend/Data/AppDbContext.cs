using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    
    public DbSet<Student> Students => Set<Student>();

    public DbSet<Teacher> Teacher => Set<Teacher>();

    public DbSet<StudentCategory> StudentCategories => Set<StudentCategory>();

    public DbSet<TeacherCategory> TeacherCategories => Set<TeacherCategory>();

    public DbSet<StudentVote> StudentVotes => Set<StudentVote>();

    public DbSet<TeacherVote> TeacherVotes => Set<TeacherVote>();

    public DbSet<TeacherQuote> TeacherQuotes => Set<TeacherQuote>();

    public DbSet<AdminUser> AdminUsers => Set<AdminUser>();
    
    public DbSet<Comment> Comments => Set<Comment>();
    
    public DbSet<Feedback> Feedbacks => Set<Feedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(k => k.Text).IsRequired().HasMaxLength(1024);
            entity.Property(k => k.Date).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(k => k.Confirmed).HasDefaultValue(false);

            entity.HasIndex(k => k.TargetId);
            entity.HasIndex(k => k.AuthorId);

            entity.HasOne<Student>()
                .WithMany()
                .HasForeignKey(k => k.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(k => k.Target)
                .WithMany(s => s.Comments)
                .HasForeignKey(k => k.TargetId)
                .OnDelete(DeleteBehavior.Cascade);

        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(s => s.Name).IsRequired().HasMaxLength(200);
            entity.Property(s => s.Gender).HasConversion<string>().HasMaxLength(20);
            entity.Property(s => s.LoginCode).IsRequired().HasMaxLength(6);
            entity.HasIndex(s => s.LoginCode).IsUnique();
            entity.Property(s => s.Course).HasMaxLength(64);
        });

        modelBuilder.Entity<Teacher>(entity =>
        {
            entity.Property(l => l.Name).IsRequired().HasMaxLength(200);
            entity.Property(l => l.Gender).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<StudentCategory>(entity =>
        {
            entity.Property(k => k.Gender).HasConversion<string>().HasMaxLength(20);
            entity.Property(k => k.QuestionText).IsRequired().HasMaxLength(500);
            entity.HasIndex(k => k.QuestionGroupId);
        });

        modelBuilder.Entity<TeacherCategory>(entity =>
        {
            entity.Property(k => k.Gender).HasConversion<string>().HasMaxLength(20);
            entity.Property(k => k.QuestionText).IsRequired().HasMaxLength(500);
            entity.HasIndex(k => k.QuestionGroupId);
        });

        modelBuilder.Entity<StudentVote>(entity =>
        {
            // Ein Vote pro Person pro Frage
            entity.HasIndex(v => new { KategorieSchuelerId = v.CategoryStudentId, v.VoterStudentId }).IsUnique();

            entity.HasOne(v => v.StudentCategory)
                .WithMany()
                .HasForeignKey(v => v.CategoryStudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.VoterStudent)
                .WithMany()
                .HasForeignKey(v => v.VoterStudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.VotedStudent)
                .WithMany()
                .HasForeignKey(v => v.VotedStudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.VotedStudent2)
                .WithMany()
                .HasForeignKey(v => v.VotedStudent2Id)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeacherVote>(entity =>
        {
            // Ein Vote pro Person pro Frage
            entity.HasIndex(v => new { KategorieLehrerId = v.CategoryTeacherId, v.VoterStudentId }).IsUnique();

            entity.HasOne(v => v.TeacherCategory)
                .WithMany()
                .HasForeignKey(v => v.CategoryTeacherId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.VoterStudent)
                .WithMany()
                .HasForeignKey(v => v.VoterStudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(v => v.VotedTeacher)
                .WithMany()
                .HasForeignKey(v => v.VotedTeacherId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<TeacherQuote>(entity =>
        {
            entity.Property(z => z.Quote).IsRequired().HasColumnType("text");
            entity.Property(z => z.Context).HasMaxLength(1000);
            entity.Property(z => z.IsReleased).HasDefaultValue(false);

            entity.HasOne(z => z.SubmittedByStudent)
                .WithMany()
                .HasForeignKey(z => z.SubmittedByStudentId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(z => z.Teacher)
                .WithMany()
                .HasForeignKey(z => z.TeacherId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<AdminUser>(entity =>
        {
            entity.Property(a => a.Username).IsRequired().HasMaxLength(100);
            entity.HasIndex(a => a.Username).IsUnique();
            entity.Property(a => a.PasswordHash).IsRequired();
        });
        
        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.Property(e => e.Text).HasMaxLength(1024);
            entity.Property(e => e.SentBy).HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired();
        });
    }
}