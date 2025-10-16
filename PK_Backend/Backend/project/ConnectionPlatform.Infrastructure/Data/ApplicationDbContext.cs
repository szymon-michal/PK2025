using Microsoft.EntityFrameworkCore;
using ConnectionPlatform.Core.Entities;

namespace ConnectionPlatform.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<UserProfilePhoto> UserProfilePhotos { get; set; }
    public DbSet<Interest> Interests { get; set; }
    public DbSet<UserInterests> UserInterests { get; set; }
    public DbSet<Skill> Skills { get; set; }
    public DbSet<UserSkill> UserSkills { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<FriendRequest> FriendRequests { get; set; }
    public DbSet<UserFriends> UserFriends { get; set; }
    public DbSet<UserBlocked> UserBlocked { get; set; }
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<Repository> Repositories { get; set; }
    public DbSet<RepositoryMetadata> RepositoryMetadata { get; set; }
    public DbSet<RepoEntry> RepoEntries { get; set; }
    public DbSet<RepoEntryData> RepoEntryData { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Email).HasColumnName("email").HasMaxLength(255).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();
            entity.Property(e => e.FirstName).HasColumnName("first_name").IsRequired();
            entity.Property(e => e.LastName).HasColumnName("last_name").IsRequired();
            entity.Property(e => e.Nick).HasColumnName("nick").IsRequired();
            entity.Property(e => e.Bio).HasColumnName("bio");
            entity.Property(e => e.Age).HasColumnName("age");
            entity.Property(e => e.IsActive).HasColumnName("is_active").HasDefaultValue(false);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.FcmToken).HasColumnName("fcmtoken");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Nick).IsUnique();
        });

        // UserProfilePhoto configuration
        modelBuilder.Entity<UserProfilePhoto>(entity =>
        {
            entity.ToTable("user_profile_photo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.FileName).HasColumnName("file_name").IsRequired();
            entity.Property(e => e.FileData).HasColumnName("file_data").IsRequired();
            entity.Property(e => e.Type).HasColumnName("type").IsRequired();
            entity.Property(e => e.UploadedAt).HasColumnName("uploaded_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.User)
                  .WithOne(u => u.ProfilePhoto)
                  .HasForeignKey<UserProfilePhoto>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Interest configuration
        modelBuilder.Entity<Interest>(entity =>
        {
            entity.ToTable("intrests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");

            entity.HasIndex(e => e.Name).IsUnique();
        });

        // UserInterests configuration
        modelBuilder.Entity<UserInterests>(entity =>
        {
            entity.ToTable("user_interests");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CategoryIds).HasColumnName("category_ids");

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Interests)
                  .HasForeignKey<UserInterests>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Skill configuration
        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Category).HasConversion<string>();
        });

        // UserSkill configuration
        modelBuilder.Entity<UserSkill>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.SkillId });
            entity.Property(e => e.Level).HasConversion<string>();

            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Skill)
                  .WithMany()
                  .HasForeignKey(e => e.SkillId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Category configuration
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).HasConversion<string>();
        });

        // FriendRequest configuration
        modelBuilder.Entity<FriendRequest>(entity =>
        {
            entity.ToTable("friend_requests");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.Message).HasColumnName("message");
            entity.Property(e => e.Status).HasColumnName("status").HasConversion<string>();
            entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Sender)
                  .WithMany()
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Receiver)
                  .WithMany()
                  .HasForeignKey(e => e.ReceiverId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SenderId, e.ReceiverId }).IsUnique();
        });

        // UserFriends configuration
        modelBuilder.Entity<UserFriends>(entity =>
        {
            entity.ToTable("user_friends");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Friends).HasColumnName("friends");

            entity.HasOne(e => e.User)
                  .WithOne(u => u.Friends)
                  .HasForeignKey<UserFriends>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // UserBlocked configuration
        modelBuilder.Entity<UserBlocked>(entity =>
        {
            entity.ToTable("user_blocked");
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.BlockedUsers).HasColumnName("blocked_users");

            entity.HasOne(e => e.User)
                  .WithOne(u => u.BlockedUsers)
                  .HasForeignKey<UserBlocked>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Conversation configuration
        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.ToTable("conversations");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.User1Id).HasColumnName("user1_id");
            entity.Property(e => e.User2Id).HasColumnName("user2_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.User1)
                  .WithMany()
                  .HasForeignKey(e => e.User1Id)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.User2)
                  .WithMany()
                  .HasForeignKey(e => e.User2Id)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(e => new { e.User1Id, e.User2Id }).IsUnique();
        });

        // Message configuration
        modelBuilder.Entity<Message>(entity =>
        {
            entity.ToTable("messages");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConversationId).HasColumnName("conversation_id");
            entity.Property(e => e.SenderId).HasColumnName("sender_id");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.MessageType).HasColumnName("message_type").HasConversion<string>();
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.SentAt).HasColumnName("sent_at").HasDefaultValueSql("now()");
            entity.Property(e => e.IsRead).HasColumnName("is_read").HasDefaultValue(false);

            entity.HasOne(e => e.Conversation)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(e => e.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Sender)
                  .WithMany(u => u.SentMessages)
                  .HasForeignKey(e => e.SenderId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Receiver)
                  .WithMany(u => u.ReceivedMessages)
                  .HasForeignKey(e => e.ReceiverId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Repository configuration
        modelBuilder.Entity<Repository>(entity =>
        {
            entity.ToTable("repositories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.Description).HasColumnName("description");

            entity.HasOne(e => e.User)
                  .WithMany(u => u.Repositories)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.UserId, e.Name }).IsUnique();
        });

        // RepositoryMetadata configuration
        modelBuilder.Entity<RepositoryMetadata>(entity =>
        {
            entity.ToTable("repository_metadata");
            entity.HasKey(e => e.RepositoryId);
            entity.Property(e => e.RepositoryId).HasColumnName("repository_id");
            entity.Property(e => e.TotalFiles).HasColumnName("total_files").HasDefaultValue(0);
            entity.Property(e => e.TotalFolders).HasColumnName("total_folders").HasDefaultValue(0);
            entity.Property(e => e.TotalSize).HasColumnName("total_size").HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");
            entity.Property(e => e.LastModified).HasColumnName("last_modified");
            entity.Property(e => e.License).HasColumnName("license");
            entity.Property(e => e.Visibility).HasColumnName("visibility").HasConversion<string>();

            entity.HasOne(e => e.Repository)
                  .WithOne(r => r.Metadata)
                  .HasForeignKey<RepositoryMetadata>(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RepoEntry configuration
        modelBuilder.Entity<RepoEntry>(entity =>
        {
            entity.ToTable("repo_entries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Name).HasColumnName("name").IsRequired();
            entity.Property(e => e.RepositoryId).HasColumnName("repository_id");
            entity.Property(e => e.ParentId).HasColumnName("parent_id");

            entity.HasOne(e => e.Repository)
                  .WithMany(r => r.Entries)
                  .HasForeignKey(e => e.RepositoryId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Parent)
                  .WithMany(p => p.Children)
                  .HasForeignKey(e => e.ParentId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // RepoEntryData configuration
        modelBuilder.Entity<RepoEntryData>(entity =>
        {
            entity.ToTable("repo_entries_data");
            entity.HasKey(e => e.EntryId);
            entity.Property(e => e.EntryId).HasColumnName("entry_id");
            entity.Property(e => e.IsDirectory).HasColumnName("is_directory").HasDefaultValue(false);
            entity.Property(e => e.Extension).HasColumnName("extension").HasConversion<string>();
            entity.Property(e => e.Content).HasColumnName("content");
            entity.Property(e => e.NumberOfLines).HasColumnName("number_of_lines");
            entity.Property(e => e.Size).HasColumnName("size").HasDefaultValue(0);
            entity.Property(e => e.LastModified).HasColumnName("last_modified").HasDefaultValueSql("now()");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("now()");

            entity.HasOne(e => e.Entry)
                  .WithOne(e => e.Data)
                  .HasForeignKey<RepoEntryData>(e => e.EntryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }
}