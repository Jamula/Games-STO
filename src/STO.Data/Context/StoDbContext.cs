using Microsoft.EntityFrameworkCore;
using STO.Core.Models;

namespace STO.Data.Context;

public class StoDbContext : DbContext
{
    public StoDbContext(DbContextOptions<StoDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Character> Characters => Set<Character>();
    public DbSet<Item> Items => Set<Item>();
    public DbSet<Build> Builds => Set<Build>();
    public DbSet<BuildSlot> BuildSlots => Set<BuildSlot>();
    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<Reputation> Reputations => Set<Reputation>();
    public DbSet<Trait> Traits => Set<Trait>();
    public DbSet<CharacterTrait> CharacterTraits => Set<CharacterTrait>();
    public DbSet<AdmiraltyShip> AdmiraltyShips => Set<AdmiraltyShip>();
    public DbSet<DoffAssignment> DoffAssignments => Set<DoffAssignment>();
    public DbSet<ValuableItem> ValuableItems => Set<ValuableItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Account
        modelBuilder.Entity<Account>(e =>
        {
            e.HasIndex(a => a.Gamertag).IsUnique();
        });

        // Character
        modelBuilder.Entity<Character>(e =>
        {
            e.HasOne(c => c.Account)
                .WithMany(a => a.Characters)
                .HasForeignKey(c => c.AccountId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Build
        modelBuilder.Entity<Build>(e =>
        {
            e.HasOne(b => b.Character)
                .WithMany(c => c.Builds)
                .HasForeignKey(b => b.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // BuildSlot
        modelBuilder.Entity<BuildSlot>(e =>
        {
            e.HasOne(bs => bs.Build)
                .WithMany(b => b.Slots)
                .HasForeignKey(bs => bs.BuildId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(bs => bs.Item)
                .WithMany(i => i.BuildSlots)
                .HasForeignKey(bs => bs.ItemId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // InventoryItem
        modelBuilder.Entity<InventoryItem>(e =>
        {
            e.HasOne(ii => ii.Item)
                .WithMany(i => i.InventoryItems)
                .HasForeignKey(ii => ii.ItemId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ii => ii.Character)
                .WithMany(c => c.InventoryItems)
                .HasForeignKey(ii => ii.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Reputation — unique per character + faction
        modelBuilder.Entity<Reputation>(e =>
        {
            e.HasOne(r => r.Character)
                .WithMany(c => c.Reputations)
                .HasForeignKey(r => r.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(r => new { r.CharacterId, r.Faction }).IsUnique();
        });

        // CharacterTrait
        modelBuilder.Entity<CharacterTrait>(e =>
        {
            e.HasOne(ct => ct.Character)
                .WithMany(c => c.CharacterTraits)
                .HasForeignKey(ct => ct.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(ct => ct.Trait)
                .WithMany(t => t.CharacterTraits)
                .HasForeignKey(ct => ct.TraitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // AdmiraltyShip
        modelBuilder.Entity<AdmiraltyShip>(e =>
        {
            e.HasOne(a => a.Character)
                .WithMany(c => c.AdmiraltyShips)
                .HasForeignKey(a => a.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // DoffAssignment
        modelBuilder.Entity<DoffAssignment>(e =>
        {
            e.HasOne(d => d.Character)
                .WithMany(c => c.DoffAssignments)
                .HasForeignKey(d => d.CharacterId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ValuableItem
        modelBuilder.Entity<ValuableItem>(e =>
        {
            e.HasOne(v => v.Account)
                .WithMany(a => a.ValuableItems)
                .HasForeignKey(v => v.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(v => v.Character)
                .WithMany()
                .HasForeignKey(v => v.CharacterId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Item — unique name index for wiki items
        modelBuilder.Entity<Item>(e =>
        {
            e.HasIndex(i => i.Name);
        });

        // Trait — unique name index
        modelBuilder.Entity<Trait>(e =>
        {
            e.HasIndex(t => t.Name);
        });
    }
}
