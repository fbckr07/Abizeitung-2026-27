using Frontend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Frontend.Data;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
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
    
    public DbSet<TeacherQuoteLike> TeacherQuoteLikes => Set<TeacherQuoteLike>();
    
    // Steckbriefe
    public DbSet<ProfileCategory> ProfileCategories => Set<ProfileCategory>();
    public DbSet<ProfileField> ProfileFields => Set<ProfileField>();
    public DbSet<ProfileFieldOption> ProfileFieldOptions => Set<ProfileFieldOption>();
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<ProfileValue> ProfileValues => Set<ProfileValue>();
    public DbSet<ProfileValueOption> ProfileValueOptions => Set<ProfileValueOption>();

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
        
        // Steckbriefe
        modelBuilder.Entity<ProfileCategory>(entity =>
        {
            entity.HasKey(c => c.Id);

            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).IsRequired().HasMaxLength(1000);
            entity.Property(c => c.SortOrder).IsRequired();
            entity.Property(c => c.IsActive).IsRequired().HasDefaultValue(true);

            // 1:n relationship with ProfileField
            entity.HasMany(c => c.Fields)
                .WithOne(f => f.Category)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Delete all fields when category is deleted
        });

        modelBuilder.Entity<ProfileField>(entity =>
        {
            entity.HasKey(f => f.Id);

            entity.Property(f => f.Label)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(f => f.Placeholder)
                .HasMaxLength(200);

            entity.Property(f => f.Type)
                .IsRequired()
                .HasConversion<string>()
                .HasMaxLength(30);

            entity.Property(f => f.IsRequired)
                .IsRequired();

            entity.Property(f => f.SortOrder)
                .IsRequired();

            entity.Property(f => f.CategoryId)
                .IsRequired();
            
            entity.HasOne(f => f.Category)
                .WithMany(c => c.Fields)
                .HasForeignKey(f => f.CategoryId)
                .OnDelete(DeleteBehavior.Cascade); // Delete all fields when category is deleted

            entity.HasMany(f => f.Options)
                .WithOne(o => o.Field)
                .HasForeignKey(o => o.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(f => new { f.CategoryId, f.SortOrder });
        });

        modelBuilder.Entity<ProfileFieldOption>(entity =>
        {
            entity.HasKey(o => o.Id);

            entity.Property(o => o.Value)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(o => o.Label)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(o => o.SortOrder)
                .IsRequired();

            entity.Property(o => o.FieldId)
                .IsRequired();

            entity.HasOne(o => o.Field)
                .WithMany(f => f.Options)
                .HasForeignKey(o => o.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(o => new { o.FieldId, o.Value }).IsUnique();
        });
        
        modelBuilder.Entity<StudentProfile>(entity =>
        {
            entity.HasKey(p => p.Id);
            
            entity.Property(u => u.StudentId)
                .IsRequired()
                .HasMaxLength(450);

            entity.Property(u => u.CreatedAt)
                .IsRequired();

            entity.Property(u => u.UpdatedAt);

            // genau 1 Steckbrief pro User
            entity.HasIndex(u => u.StudentId)
                .IsUnique();

            entity.HasOne<Student>()
                .WithOne()
                .HasForeignKey<StudentProfile>(p => p.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(s => s.Values)
                .WithOne(v => v.StudentProfile)
                .HasForeignKey(v => v.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileValue>(entity =>
        {
            entity.HasKey(v => v.Id);

            entity.Property(v => v.Value)
                .HasMaxLength(4000);

            entity.Property(v => v.StudentProfileId)
                .IsRequired();

            entity.Property(v => v.FieldId)
                .IsRequired();

            entity.HasOne(v => v.StudentProfile)
                .WithMany(u => u.Values)
                .HasForeignKey(v => v.StudentProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Field)
                .WithMany()
                .HasForeignKey(v => v.FieldId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(v => new { v.StudentProfileId, v.FieldId }).IsUnique();

            entity.HasMany(v => v.SelectedOptions)
                .WithOne()
                .HasForeignKey(vo => vo.ProfileValueId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileValueOption>(entity =>
        {
            entity.HasKey(vo => vo.Id);

            entity.Property(vo => vo.ProfileValueId)
                .IsRequired();

            entity.Property(vo => vo.FieldOptionId)
                .IsRequired();

            entity.HasOne<ProfileValue>()
                .WithMany(v => v.SelectedOptions)
                .HasForeignKey(vo => vo.ProfileValueId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<ProfileFieldOption>()
                .WithMany()
                .HasForeignKey(vo => vo.FieldOptionId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(vo => new { vo.ProfileValueId, vo.FieldOptionId }).IsUnique();
        });

        modelBuilder.Entity<TeacherQuoteLike>(entity =>
        {
            entity.HasIndex(i => new { i.TeacherQuoteId, i.StudentId })
                .IsUnique();

            entity.HasKey(l => l.Id);
            
            entity.Property(l => l.Id)
                .IsRequired()
                .ValueGeneratedOnAdd();

            entity.Property(l => l.TeacherQuoteId)
                .IsRequired();

            entity.Property(l => l.StudentId)
                .IsRequired();

            entity.Property(l => l.CreatedAt)
                .IsRequired();
            
            entity.HasIndex(l => new { l.TeacherQuoteId, l.StudentId })
                .IsUnique()
                .HasDatabaseName("IX_TeacherQuoteLikes_TeacherQuoteId_StudentId");
            
            // Zitat gelöscht -> Likes werden mitgelöscht
            entity.HasOne(l => l.TeacherQuote)
                .WithMany(q => q.Likes)
                .HasForeignKey(l => l.TeacherQuoteId)
                .OnDelete(DeleteBehavior.Cascade);
            
            // Schüler gelöscht -> Likes werden mitgelöscht
            entity.HasOne(l => l.Student)
                .WithMany()
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}